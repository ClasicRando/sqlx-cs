using System.Text.Json.Serialization.Metadata;
using Sqlx.Core;
using Sqlx.Core.Result;
using Sqlx.Postgres.Connection;
using Sqlx.Postgres.Exceptions;
using Sqlx.Postgres.Result;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Query;

/// <summary>
/// <see cref="IPgExecutableQuery"/> implementation for Postgres. Parameters are encoded into a
/// <see cref="PgParameterBuffer"/> and the query is executed using the <see cref="PgConnection"/>
/// supplied to the constructor.
/// </summary>
internal class PgExecutableQuery(string sql, IPgQueryExecutor queryExecutor) : IPgExecutableQuery
{
    private IPgQueryExecutor? _queryExecutor = queryExecutor;
    public string Query { get; } = sql;

    public PgParameterBuffer ParameterBuffer { get; } = new();

    public int ParameterCount => ParameterBuffer.ParameterCount;

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
        ParameterBuffer.EncodeBytes(value);
    }

    public void Bind(string? value)
    {
        this.BindRef<string, PgString>(value);
    }

    public void Bind(ReadOnlySpan<char> value)
    {
        ParameterBuffer.EncodeChars(value);
    }

    public void Bind(Guid value)
    {
        Bind<Guid, PgUuid>(value);
    }

    public void BindJson<T>(T value, JsonTypeInfo<T>? typeInfo = null) where T : notnull
    {
        ParameterBuffer.EncodeJsonValue(value, typeInfo);
    }

    public void BindNull<T>() where T : notnull
    {
        ParameterBuffer.EncodeNull();
    }

    public void Bind<TValue, TType>(TValue value)
        where TValue : notnull
        where TType : IPgDbType<TValue>
    {
        ParameterBuffer.EncodeValue<TValue, TType>(value);
    }

    public Task<IAsyncEnumerable<Either<IPgDataRow, QueryResult>>> Execute(
        CancellationToken cancellationToken)
    {
        PgException.ThrowIfNull(_queryExecutor);
        return _queryExecutor.ExecuteQuery(this, cancellationToken);
    }

    public void Dispose()
    {
        ParameterBuffer.Dispose();
        _queryExecutor = null;
    }
}
