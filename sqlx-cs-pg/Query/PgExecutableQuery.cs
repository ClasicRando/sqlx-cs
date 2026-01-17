using System.Text.Json.Serialization.Metadata;
using Sqlx.Core;
using Sqlx.Core.Buffer;
using Sqlx.Core.Result;
using Sqlx.Postgres.Connection;
using Sqlx.Postgres.Exceptions;
using Sqlx.Postgres.Result;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Query;

/// <summary>
/// Default <see cref="IPgExecutableQuery"/> implementation for Postgres. Parameters are encoded
/// into a buffer using a <see cref="PgParameterWriter"/> and the query is executed using the
/// <see cref="PgConnection"/> supplied to the constructor.
/// </summary>
internal class PgExecutableQuery : IPgExecutableQuery
{
    private IPgQueryExecutor? _queryExecutor;
    private readonly PooledArrayBufferWriter _buffer;
    private readonly PgParameterWriter _parameterBuffer;

    public PgExecutableQuery(string sql, IPgQueryExecutor queryExecutor)
    {
        _queryExecutor = queryExecutor;
        Query = sql;
        _buffer = new PooledArrayBufferWriter();
        _parameterBuffer = new PgParameterWriter(_buffer);
    }

    public string Query { get; }

    public short ParameterCount => _parameterBuffer.ParameterCount;

    public IReadOnlyList<PgTypeInfo> ParameterPgTypes => _parameterBuffer.PgTypes;

    public ReadOnlySpan<byte> EncodedParameters => _buffer.ReadableSpan;

    public void Bind(bool value)
    {
        Bind<bool, PgBool>(value);
    }

    public void Bind(sbyte value)
    {
        Bind<sbyte, PgChar>(value);
    }

    public void Bind(short value)
    {
        Bind<short, PgShort>(value);
    }

    public void Bind(int value)
    {
        Bind<int, PgInt>(value);
    }

    public void Bind(long value)
    {
        Bind<long, PgLong>(value);
    }

    public void Bind(float value)
    {
        Bind<float, PgFloat>(value);
    }

    public void Bind(double value)
    {
        Bind<double, PgDouble>(value);
    }

    public void Bind(TimeOnly value)
    {
        Bind<TimeOnly, PgTime>(value);
    }

    public void Bind(DateOnly value)
    {
        Bind<DateOnly, PgDate>(value);
    }

    public void Bind(DateTime value)
    {
        Bind<DateTime, PgDateTime>(value);
    }

    public void Bind(DateTimeOffset value)
    {
        Bind<DateTimeOffset, PgDateTimeOffset>(value);
    }

    public void Bind(decimal value)
    {
        Bind<decimal, PgDecimal>(value);
    }
    
    public void Bind(byte[]? value)
    {
        this.BindRef<byte[], PgBytea>(value);
    }

    public void Bind(ReadOnlySpan<byte> value)
    {
        _parameterBuffer.Bind(value);
    }

    public void Bind(string? value)
    {
        this.BindRef<string, PgString>(value);
    }

    public void Bind(ReadOnlySpan<char> value)
    {
        _parameterBuffer.Bind(value);
    }

    public void Bind(Guid value)
    {
        Bind<Guid, PgUuid>(value);
    }

    public void BindJson<T>(T value, JsonTypeInfo<T>? typeInfo = null) where T : notnull
    {
        _parameterBuffer.BindJson(value, typeInfo);
    }

    public void BindNull<T>() where T : notnull
    {
        _parameterBuffer.BindNull<T>();
    }

    public void Bind<TValue, TType>(TValue value)
        where TValue : notnull
        where TType : IPgDbType<TValue>
    {
        _parameterBuffer.Bind<TValue, TType>(value);
    }

    public IAsyncEnumerable<Either<IPgDataRow, QueryResult>> ExecuteAsync(
        CancellationToken cancellationToken)
    {
        PgException.ThrowIfNull(_queryExecutor);
        return _queryExecutor.ExecuteQueryAsync(this, cancellationToken);
    }

    public void Dispose()
    {
        _buffer.Dispose();
        _parameterBuffer.Dispose();
        _queryExecutor = null;
    }
}
