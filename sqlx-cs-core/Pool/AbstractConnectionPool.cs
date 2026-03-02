using System.Diagnostics.CodeAnalysis;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using Sqlx.Core.Config;
using Sqlx.Core.Exceptions;

namespace Sqlx.Core.Pool;

/// <summary>
/// Base of connection pools. Uses a channel to provide idle connections to consumers, managing
/// invalid connections and idle connection cleanup in the background.
/// </summary>
/// <typeparam name="TConnection">Underlining connection type</typeparam>
/// <typeparam name="TSelf">Type of self</typeparam>
public abstract class AbstractConnectionPool<TConnection, TSelf> : IAsyncDisposable, IDisposable
    where TConnection : class, IPooledConnection
    where TSelf : AbstractConnectionPool<TConnection, TSelf>
{
    public PoolOptions PoolOptions { get; }

    private bool _disposed;
    private readonly TimeSpan _connectTimeout;
    private readonly ILogger<TSelf> _logger;
    private readonly ChannelReader<TConnection?> _idleConnectionReader;
    private readonly ChannelWriter<TConnection?> _idleConnectionWriter;
    private readonly TConnection?[] _connections;

    private volatile int _connectionCount;
    private volatile int _idleConnectionCount;
    private readonly CancellationTokenSource _idleCleanupCts = new();
    private readonly PeriodicTimer _idleCleanupTimer;
    private readonly Task _idleCleanupTask;
    private volatile bool _idleTimerEnabled;

    internal AbstractConnectionPool(
        PoolOptions poolOptions,
        TimeSpan connectTimeout,
        ILogger<TSelf> logger)
    {
        PoolOptions = poolOptions;
        _connectTimeout = connectTimeout;
        _logger = logger;
        var idleChannel = Channel.CreateUnbounded<TConnection?>();
        _idleConnectionReader = idleChannel.Reader;
        _idleConnectionWriter = idleChannel.Writer;
        _connections = new TConnection[poolOptions.MaxConnections];
        _idleCleanupTimer = new PeriodicTimer(poolOptions.IdleCleanupInterval);
        _idleCleanupTask = Task.Run(() => IdleCleanupActionAsync(_idleCleanupCts.Token));
    }

    ~AbstractConnectionPool() => Dispose(false);

    internal ValueTask<TConnection> AcquireStreamAsync(CancellationToken cancellationToken)
    {
        return TryGetIdleStream(out TConnection? stream)
            ? ValueTask.FromResult(stream)
            : CoreAsync(cancellationToken);

        async ValueTask<TConnection> CoreAsync(CancellationToken ct)
        {
            using var acquireTimeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            acquireTimeoutCts.CancelAfter(_connectTimeout);
            CancellationToken acquireTimeoutToken = acquireTimeoutCts.Token;

            do
            {
                TConnection? s =
                    await OpenNewStreamAsync(acquireTimeoutToken).ConfigureAwait(false);
                if (s is not null)
                {
                    return s;
                }

                try
                {
                    s = await _idleConnectionReader.ReadAsync(acquireTimeoutToken)
                        .ConfigureAwait(false);
                    if (VerifyActiveStream(s))
                    {
                        return s;
                    }
                }
                catch (OperationCanceledException)
                {
                    ct.ThrowIfCancellationRequested();
                    throw new SqlxException(
                        "The connection pool is exhausted and the new connection open operation " +
                        "timed out. You can either increase the pool size to greater than " +
                        $"{PoolOptions.MaxConnections}, increate the timeout to greater than " +
                        $"{_connectTimeout} or make sure that no connection are " +
                        "being leaked.");
                }
                catch (ChannelClosedException)
                {
                    throw new SqlxException("Connection pool has been closed");
                }

                if (TryGetIdleStream(out s))
                {
                    return s;
                }
            } while (true);
        }
    }

    internal void Return(TConnection stream)
    {
        if (stream.Status is ConnectionStatus.Broken)
        {
            CloseStream(stream);
            return;
        }

        Interlocked.Increment(ref _idleConnectionCount);
        _idleConnectionWriter.TryWrite(stream);
    }

    private bool TryGetIdleStream([NotNullWhen(true)] out TConnection? stream)
    {
        while (_idleConnectionReader.TryRead(out stream))
        {
            if (VerifyActiveStream(stream))
            {
                return true;
            }
        }

        return false;
    }

    protected abstract TConnection CreateNewConnection();

    private async ValueTask<TConnection?> OpenNewStreamAsync(CancellationToken cancellationToken)
    {
        var currentStreamCount = _connectionCount;
        while (currentStreamCount < PoolOptions.MaxConnections)
        {
            if (Interlocked.CompareExchange(
                    ref _connectionCount,
                    currentStreamCount + 1,
                    currentStreamCount) != currentStreamCount)
            {
                currentStreamCount = _connectionCount;
                continue;
            }

            try
            {
                TConnection stream = CreateNewConnection();
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(_connectTimeout);
                await stream.OpenAsync(cts.Token).ConfigureAwait(false);

                var i = 0;
                for (; i < PoolOptions.MaxConnections; i++)
                {
                    if (Interlocked.CompareExchange(ref _connections[i], stream, null) == null)
                    {
                        break;
                    }
                }

                if (i == PoolOptions.MaxConnections)
                {
                    throw new SqlxException(
                        "Tried to create another connection after reaching max connection count. " +
                        "Please report bug.");
                }

                if (currentStreamCount >= PoolOptions.MinIdleConnections)
                {
                    UpdateIdleCleanupTimer();
                }

                return stream;
            }
            catch
            {
                Interlocked.Decrement(ref _connectionCount);
                _idleConnectionWriter.TryWrite(null);
                UpdateIdleCleanupTimer();
                throw;
            }
        }

        return null;
    }

    private bool VerifyActiveStream([NotNullWhen(true)] TConnection? stream)
    {
        if (stream is null)
        {
            return false;
        }

        Interlocked.Decrement(ref _idleConnectionCount);

        if (stream.Status is ConnectionStatus.Broken)
        {
            CloseStream(stream);
            return false;
        }

        if (PoolOptions.MaxLifetime == Timeout.InfiniteTimeSpan
            || DateTime.UtcNow <= stream.LastOpenTimestamp + PoolOptions.MaxLifetime)
        {
            return true;
        }

        _logger.LogConnectionExceededMaxLifeTime(
            SqlxConfig.DetailedLoggingLevel,
            PoolOptions.MaxLifetime,
            stream.Id);
        CloseStream(stream);
        return false;

    }

    [SuppressMessage(
        "Design",
        "CA1031:Do not catch general exception types",
        Justification =
            "Exceptions while disposing of a stream do not really matter as to what caused them " +
            "and should not be propagated")]
    private void CloseStream(TConnection stream)
    {
        try
        {
            stream.Dispose();
        }
        catch (Exception e)
        {
            _logger.LogErrorClosingConnection(SqlxConfig.DetailedLoggingLevel, stream.Id, e);
        }

        var i = 0;
        for (; i < PoolOptions.MaxConnections; i++)
        {
            if (Interlocked.CompareExchange(ref _connections[i], null, stream) == stream)
            {
                break;
            }
        }

        if (i == PoolOptions.MaxConnections)
        {
            return;
        }

        var streamCount = Interlocked.Decrement(ref _connectionCount);
        _idleConnectionWriter.TryWrite(null);

        if (streamCount == PoolOptions.MinIdleConnections)
        {
            UpdateIdleCleanupTimer();
        }
    }

    private void UpdateIdleCleanupTimer()
    {
        var streamCount = _connectionCount;
        if (streamCount > PoolOptions.MinIdleConnections && !_idleTimerEnabled)
        {
            _idleTimerEnabled = true;
            _idleCleanupTimer.Period = PoolOptions.IdleKeepAliveInterval;
        }
        else if (streamCount <= PoolOptions.MinIdleConnections && _idleTimerEnabled)
        {
            _idleTimerEnabled = false;
            _idleCleanupTimer.Period = Timeout.InfiniteTimeSpan;
        }
    }

    private async Task IdleCleanupActionAsync(CancellationToken cancellationToken)
    {
        var sampleSize = DivideRoundingUp(
            (int)PoolOptions.IdleTimeout.TotalMilliseconds,
            (int)PoolOptions.IdleCleanupInterval.TotalMilliseconds);
        var medianIndex = DivideRoundingUp(sampleSize, 2) - 1;
        var samples = new List<int>(sampleSize);
        try
        {
            while (!cancellationToken.IsCancellationRequested
                   && await _idleCleanupTimer.WaitForNextTickAsync(cancellationToken)
                       .ConfigureAwait(false))
            {
                var idleStreamCount = _idleConnectionCount;
                samples.Add(idleStreamCount);

                if (samples.Count < sampleSize)
                {
                    continue;
                }

                samples.Sort();
                var idleRemoveCount = samples[medianIndex];

                while (idleRemoveCount > 0
                       && _connectionCount > PoolOptions.MinIdleConnections
                       && _idleConnectionReader.TryRead(out TConnection? stream)
                       && stream is not null)
                {
                    if (!VerifyActiveStream(stream) || !_idleConnectionWriter.TryWrite(stream))
                    {
                        CloseStream(stream);
                    }

                    idleRemoveCount--;
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private static int DivideRoundingUp(int value, int divisor) => 1 + (value - 1) / divisor;

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore().ConfigureAwait(false);
        Dispose(false);
        GC.SuppressFinalize(this);
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        await _idleCleanupCts.CancelAsync().ConfigureAwait(false);
        await _idleCleanupTask.ConfigureAwait(false);
        _idleCleanupCts.Dispose();
        _idleCleanupTimer.Dispose();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;
        if (!disposing) return;

        _idleCleanupCts.Cancel();
        _idleCleanupTask.GetAwaiter().GetResult();
        _idleCleanupCts.Dispose();
        _idleCleanupTimer.Dispose();

        _disposed = true;
    }
}
