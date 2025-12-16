using System.Text.Json.Serialization.Metadata;
using Sqlx.Core;
using Sqlx.Core.Query;
using Sqlx.Core.Result;
using Sqlx.Postgres.Exceptions;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Query;

/// <summary>
/// <see cref="IExecutableQuery"/> implementation for Postgres. Parameters are encoded into a
/// <see cref="PgParameterBuffer"/> and the query is executed using the <see cref="IQueryExecutor"/>
/// supplied to the constructor.
/// </summary>
internal class PgExecutableQuery(string sql, IQueryExecutor queryExecutor) : IExecutableQuery, IPgBindable
{
    private IQueryExecutor? _queryExecutor = queryExecutor;
    public string Query { get; } = sql;

    public PgParameterBuffer ParameterBuffer { get; } = new();
    

    
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

    public void Bind(DateTimeOffset value)
    {
        BindPg<DateTimeOffset, PgDateTimeOffset>(value);
    }

    public void Bind(decimal value)
    {
        BindPg<decimal, PgDecimal>(value);
    }
    
    public void Bind(byte[]? value)
    {
        this.BindPgNullableClass<byte[], PgBytea>(value);
    }

    public void Bind(ReadOnlySpan<byte> value)
    {
        ParameterBuffer.EncodeBytes(value);
    }

    public void Bind(string? value)
    {
        this.BindPgNullableClass<string, PgString>(value);
    }

    public void Bind(ReadOnlySpan<char> value)
    {
        ParameterBuffer.EncodeChars(value);
    }

    public void Bind(Guid value)
    {
        BindPg<Guid, PgUuid>(value);
    }

    public void BindJson<T>(T value, JsonTypeInfo<T>? typeInfo = null) where T : notnull
    {
        ParameterBuffer.EncodeJsonValue(value, typeInfo);
    }

    public void BindNull<T>() where T : notnull
    {
        ParameterBuffer.EncodeNull();
    }

    public void BindPg<TValue, TType>(TValue value)
        where TValue : notnull
        where TType : IPgDbType<TValue>
    {
        ParameterBuffer.EncodeValue<TValue, TType>(value);
    }

    public Task<IAsyncEnumerable<Either<IDataRow, QueryResult>>> Execute(
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
