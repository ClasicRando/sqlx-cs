using System.Runtime.CompilerServices;
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
internal class PgExecutableQuery(string sql, IQueryExecutor queryExecutor) : IExecutableQuery
{
    private IQueryExecutor? _queryExecutor = queryExecutor;
    public string Query { get; } = sql;

    public PgParameterBuffer ParameterBuffer { get; } = new();
    
    public IQuery Bind(bool value)
    {
        return Encode<bool, PgBool>(value);
    }

    public IQuery Bind(sbyte value)
    {
        return Encode<sbyte, PgChar>(value);
    }

    public IQuery Bind(short value)
    {
        return Encode<short, PgShort>(value);
    }

    public IQuery Bind(int value)
    {
        return Encode<int, PgInt>(value);
    }

    public IQuery Bind(long value)
    {
        return Encode<long, PgLong>(value);
    }

    public IQuery Bind(float value)
    {
        return Encode<float, PgFloat>(value);
    }

    public IQuery Bind(double value)
    {
        return Encode<double, PgDouble>(value);
    }

    public IQuery Bind(TimeOnly value)
    {
        return Encode<TimeOnly, PgTime>(value);
    }

    public IQuery Bind(DateOnly value)
    {
        return Encode<DateOnly, PgDate>(value);
    }

    public IQuery Bind(DateTime value)
    {
        return Encode<DateTime, PgDateTime>(value);
    }

    public IQuery Bind(DateTimeOffset value)
    {
        return Encode<DateTimeOffset, PgDateTimeOffset>(value);
    }

    public IQuery Bind(decimal value)
    {
        return Encode<decimal, PgDecimal>(value);
    }

    public IQuery Bind(byte[]? value)
    {
        return EncodeNullableClass<byte[], PgBytea>(value);
    }

    public IQuery Bind(ReadOnlySpan<byte> value)
    {
        ParameterBuffer.EncodeBytes(value);
        return this;
    }

    public IQuery Bind(string? value)
    {
        return EncodeNullableClass<string, PgString>(value);
    }

    public IQuery Bind(ReadOnlySpan<char> value)
    {
        ParameterBuffer.EncodeChars(value);
        return this;
    }

    public IQuery Bind(Guid value)
    {
        return Encode<Guid, PgUuid>(value);
    }

    public IQuery BindJson<T>(T? value, JsonTypeInfo<T>? typeInfo = null) where T : notnull
    {
        if (value is null)
        {
            ParameterBuffer.EncodeNull();
            return this;
        }
        ParameterBuffer.EncodeJsonValue(value, typeInfo);
        return this;
    }

    public IQuery BindNull<T>() where T : notnull
    {
        return EncodeNull();
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal PgExecutableQuery Encode<TValue, TType>(TValue value)
        where TType : IPgDbType<TValue>
        where TValue : notnull
    {
        ParameterBuffer.EncodeValue<TValue, TType>(value);
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal PgExecutableQuery EncodeNullableClass<TValue, TType>(TValue? value)
        where TType : IPgDbType<TValue>
        where TValue : class
    {
        return value is null ? EncodeNull() : Encode<TValue, TType>(value);
    }

    private PgExecutableQuery EncodeNull()
    {
        ParameterBuffer.EncodeNull();
        return this;
    }
}
