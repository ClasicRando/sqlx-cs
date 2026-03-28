using System.Text.Json.Serialization.Metadata;
using Sqlx.Core.Buffer;
using Sqlx.Core.Result;
using Sqlx.Postgres.Connection;
using Sqlx.Postgres.Result;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Query;

/// <summary>
/// Default <see cref="IPgExecutableQuery"/> implementation for Postgres. Parameters are encoded
/// into a buffer using a <see cref="PgParameterWriter"/> and the query is executed using the
/// <see cref="PgConnection"/> supplied to the constructor.
/// </summary>
internal sealed class PgExecutableQuery : IPgExecutableQuery
{
    private bool _disposed;
    private PgConnection? _queryExecutor;
    private readonly ArrayBufferWriter _buffer;
    private readonly PgParameterWriter _parameterWriter;

    public PgExecutableQuery(string sql, PgConnection queryExecutor)
    {
        _queryExecutor = queryExecutor;
        Query = sql;
        _buffer = new ArrayBufferWriter();
        _parameterWriter = new PgParameterWriter(_buffer);
    }

    public string Query { get; }

    public short ParameterCount => _parameterWriter.ParameterCount;

    public IReadOnlyList<PgTypeInfo> ParameterPgTypes => _parameterWriter.PgTypes;

    public ReadOnlySpan<byte> EncodedParameters => _buffer.ReadableSpan;

    public void Bind(bool value)
    {
        BindPg<bool, PgBool>(value);
    }

    public void Bind(sbyte value)
    {
        BindPg<sbyte, PgChar>(value);
    }

    public void Bind(short value)
    {
        BindPg<short, PgShort>(value);
    }

    public void Bind(int value)
    {
        BindPg<int, PgInt>(value);
    }

    public void Bind(long value)
    {
        BindPg<long, PgLong>(value);
    }

    public void Bind(float value)
    {
        BindPg<float, PgFloat>(value);
    }

    public void Bind(double value)
    {
        BindPg<double, PgDouble>(value);
    }

    public void Bind(TimeOnly value)
    {
        BindPg<TimeOnly, PgTime>(value);
    }

    public void Bind(DateOnly value)
    {
        BindPg<DateOnly, PgDate>(value);
    }

    public void Bind(DateTime value)
    {
        BindPg<DateTime, PgDateTime>(value);
    }

    public void Bind(in DateTimeOffset value)
    {
        BindPg<DateTimeOffset, PgDateTimeOffset>(value);
    }

    public void Bind(decimal value)
    {
        BindPg<decimal, PgDecimal>(value);
    }

    public void Bind(byte[]? value)
    {
        if (value is null)
        {
            BindNull<PgBytea>();
        }
        else
        {
            BindPg<byte[], PgBytea>(value);
        }
    }

    public void Bind(in ReadOnlySpan<byte> value)
    {
        _parameterWriter.Bind(value);
    }

    public void Bind(string? value)
    {
        if (value is null)
        {
            BindNull<PgString>();
        }
        else
        {
            BindPg<string, PgString>(value);
        }
    }

    public void Bind(in ReadOnlySpan<char> value)
    {
        _parameterWriter.Bind(value);
    }

    public void Bind(in Guid value)
    {
        BindPg<Guid, PgUuid>(value);
    }

    public void BindJson<T>(T value, JsonTypeInfo<T>? typeInfo = null) where T : notnull
    {
        _parameterWriter.BindJson(value, typeInfo);
    }

    public void BindNull<T>() where T : notnull
    {
        _parameterWriter.BindNull<T>();
    }

    public void BindPg<TValue, TType>(TValue value)
        where TValue : notnull
        where TType : IPgDbType<TValue>
    {
        _parameterWriter.BindPg<TValue, TType>(value);
    }

    public Task<IAsyncResultSet<IPgDataRow>> ExecuteAsync(
        CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return _queryExecutor!.ExecuteQueryAsync(this, cancellationToken);
    }

    public void Dispose()
    {
        if (_disposed) return;

        _buffer.Dispose();
        _parameterWriter.Dispose();
        _queryExecutor = null;
        _disposed = true;
    }

    public ValueTask DisposeAsync()
    {
        Dispose();
        return ValueTask.CompletedTask;
    }
}
