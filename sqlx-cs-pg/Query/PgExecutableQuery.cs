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

    public void Bind(in ReadOnlySpan<byte> value)
    {
        _parameterWriter.Bind(value);
    }

    public void Bind(in ReadOnlySpan<char> value)
    {
        _parameterWriter.Bind(value);
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
