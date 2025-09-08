using System.Runtime.CompilerServices;
using System.Text.Json.Serialization.Metadata;
using Sqlx.Core;
using Sqlx.Core.Query;
using Sqlx.Core.Result;
using Sqlx.Postgres.Exceptions;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Query;

internal class PgExecutableQuery(string sql, IQueryExecutor queryExecutor) : IExecutableQuery
{
    private IQueryExecutor? _queryExecutor = queryExecutor;
    public string Query { get; } = sql;

    public PgParameterBuffer ParameterBuffer { get; } = new();
    
    public IQuery Bind(bool value)
    {
        return Encode<bool, PgBool>(value);
    }

    public IQuery Bind(bool? value)
    {
        return EncodeNullableStruct<bool, PgBool>(value);
    }

    public IQuery Bind(byte value)
    {
        return Encode<byte, PgChar>(value);
    }

    public IQuery Bind(byte? value)
    {
        return EncodeNullableStruct<byte, PgChar>(value);
    }

    public IQuery Bind(short value)
    {
        return Encode<short, PgShort>(value);
    }

    public IQuery Bind(short? value)
    {
        return EncodeNullableStruct<short, PgShort>(value);
    }

    public IQuery Bind(int value)
    {
        return Encode<int, PgInt>(value);
    }

    public IQuery Bind(int? value)
    {
        return EncodeNullableStruct<int, PgInt>(value);
    }

    public IQuery Bind(long value)
    {
        return Encode<long, PgLong>(value);
    }

    public IQuery Bind(long? value)
    {
        return EncodeNullableStruct<long, PgLong>(value);
    }

    public IQuery Bind(float value)
    {
        return Encode<float, PgFloat>(value);
    }

    public IQuery Bind(float? value)
    {
        return EncodeNullableStruct<float, PgFloat>(value);
    }

    public IQuery Bind(double value)
    {
        return Encode<double, PgDouble>(value);
    }

    public IQuery Bind(double? value)
    {
        return EncodeNullableStruct<double, PgDouble>(value);
    }

    public IQuery Bind(TimeOnly value)
    {
        return Encode<TimeOnly, PgTime>(value);
    }

    public IQuery Bind(TimeOnly? value)
    {
        return EncodeNullableStruct<TimeOnly, PgTime>(value);
    }

    public IQuery Bind(DateOnly value)
    {
        return Encode<DateOnly, PgDate>(value);
    }

    public IQuery Bind(DateOnly? value)
    {
        return EncodeNullableStruct<DateOnly, PgDate>(value);
    }

    public IQuery Bind(DateTime value)
    {
        return Encode<DateTime, PgDateTime>(value);
    }

    public IQuery Bind(DateTime? value)
    {
        return EncodeNullableStruct<DateTime, PgDateTime>(value);
    }

    public IQuery Bind(DateTimeOffset value)
    {
        return Encode<DateTimeOffset, PgDateTimeOffset>(value);
    }

    public IQuery Bind(DateTimeOffset? value)
    {
        return EncodeNullableStruct<DateTimeOffset, PgDateTimeOffset>(value);
    }

    public IQuery Bind(decimal value)
    {
        return Encode<decimal, PgDecimal>(value);
    }

    public IQuery Bind(decimal? value)
    {
        return EncodeNullableStruct<decimal, PgDecimal>(value);
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

    public IQuery Bind(Guid? value)
    {
        return EncodeNullableStruct<Guid, PgUuid>(value);
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

    public IQuery BindOutParameter<T>() where T : notnull
    {
        ParameterBuffer.EncodeNull();
        return this;
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal PgExecutableQuery EncodeNullableStruct<TValue, TType>(TValue? value)
        where TType : IPgDbType<TValue>
        where TValue : struct
    {
        return value.HasValue ? Encode<TValue, TType>(value.Value) : EncodeNull();
    }

    private PgExecutableQuery EncodeNull()
    {
        ParameterBuffer.EncodeNull();
        return this;
    }
}

public static class ExecutableQueryExtensions
{
    public static IQuery Bind(this IQuery query, PgTimeTz value)
    {
        return BindPg<PgTimeTz, PgTimeTz>(query, value);
    }
    
    public static IQuery Bind(this IQuery query, PgTimeTz? value)
    {
        return BindPgNullableStruct<PgTimeTz, PgTimeTz>(query, value);
    }

    public static IQuery Bind(this IQuery query, PgPoint value)
    {
        return BindPg<PgPoint, PgPoint>(query, value);
    }
    
    public static IQuery Bind(this IQuery query, PgPoint? value)
    {
        return BindPgNullableStruct<PgPoint, PgPoint>(query, value);
    }

    public static IQuery Bind(this IQuery query, PgLine value)
    {
        return BindPg<PgLine, PgLine>(query, value);
    }
    
    public static IQuery Bind(this IQuery query, PgLine? value)
    {
        return BindPgNullableStruct<PgLine, PgLine>(query, value);
    }

    public static IQuery Bind(this IQuery query, PgLineSegment value)
    {
        return BindPg<PgLineSegment, PgLineSegment>(query, value);
    }
    
    public static IQuery Bind(this IQuery query, PgLineSegment? value)
    {
        return BindPgNullableStruct<PgLineSegment, PgLineSegment>(query, value);
    }

    public static IQuery Bind(this IQuery query, PgBox value)
    {
        return BindPg<PgBox, PgBox>(query, value);
    }
    
    public static IQuery Bind(this IQuery query, PgBox? value)
    {
        return BindPgNullableStruct<PgBox, PgBox>(query, value);
    }

    public static IQuery Bind(this IQuery query, PgPath value)
    {
        return BindPg<PgPath, PgPath>(query, value);
    }
    
    public static IQuery Bind(this IQuery query, PgPath? value)
    {
        return BindPgNullableStruct<PgPath, PgPath>(query, value);
    }

    public static IQuery Bind(this IQuery query, PgPolygon value)
    {
        return BindPg<PgPolygon, PgPolygon>(query, value);
    }
    
    public static IQuery Bind(this IQuery query, PgPolygon? value)
    {
        return BindPgNullableStruct<PgPolygon, PgPolygon>(query, value);
    }

    public static IQuery Bind(this IQuery query, PgCircle value)
    {
        return BindPg<PgCircle, PgCircle>(query, value);
    }
    
    public static IQuery Bind(this IQuery query, PgCircle? value)
    {
        return BindPgNullableStruct<PgCircle, PgCircle>(query, value);
    }

    public static IQuery Bind(this IQuery query, PgInterval value)
    {
        return BindPg<PgInterval, PgInterval>(query, value);
    }
    
    public static IQuery Bind(this IQuery query, PgInterval? value)
    {
        return BindPgNullableStruct<PgInterval, PgInterval>(query, value);
    }

    public static IQuery Bind(this IQuery query, PgMacAddress value)
    {
        return BindPg<PgMacAddress, PgMacAddress>(query, value);
    }
    
    public static IQuery Bind(this IQuery query, PgMacAddress? value)
    {
        return BindPgNullableStruct<PgMacAddress, PgMacAddress>(query, value);
    }

    public static IQuery Bind(this IQuery query, PgMoney value)
    {
        return BindPg<PgMoney, PgMoney>(query, value);
    }
    
    public static IQuery Bind(this IQuery query, PgMoney? value)
    {
        return BindPgNullableStruct<PgMoney, PgMoney>(query, value);
    }
    
    public static IQuery Bind(this IQuery query, PgInet? value)
    {
        return BindPgNullableClass<PgInet, PgInet>(query, value);
    }

    public static IQuery Bind(this IQuery query, PgRange<long>? value)
    {
        return BindPgNullableClass<PgRange<long>, PgRangeType<long, PgLong>>(query, value);
    }
    
    public static IQuery Bind(this IQuery query, PgRange<int>? value)
    {
        return BindPgNullableClass<PgRange<int>, PgRangeType<int, PgInt>>(query, value);
    }

    public static IQuery Bind(this IQuery query, PgRange<DateOnly>? value)
    {
        return BindPgNullableClass<PgRange<DateOnly>, PgRangeType<DateOnly, PgDate>>(query, value);
    }

    public static IQuery Bind(this IQuery query, PgRange<DateTime>? value)
    {
        return BindPgNullableClass<PgRange<DateTime>, PgRangeType<DateTime, PgDateTime>>(query, value);
    }

    public static IQuery Bind(this IQuery query, PgRange<DateTimeOffset>? value)
    {
        return BindPgNullableClass<PgRange<DateTimeOffset>, PgRangeType<DateTimeOffset, PgDateTimeOffset>>(query, value);
    }
    
    public static IQuery Bind(this IQuery query, PgRange<decimal>? value)
    {
        return BindPgNullableClass<PgRange<decimal>, PgRangeType<decimal, PgDecimal>>(query, value);
    }

    public static IQuery Bind(this IQuery query, bool?[]? value)
    {
        return BindPgArrayStruct<bool, PgBool>(query, value);
    }

    public static IQuery Bind(this IQuery query, byte?[]? value)
    {
        return BindPgArrayStruct<byte, PgChar>(query, value);
    }

    public static IQuery Bind(this IQuery query, short?[]? value)
    {
        return BindPgArrayStruct<short, PgShort>(query, value);
    }
    
    public static IQuery Bind(this IQuery query, int?[]? value)
    {
        return BindPgArrayStruct<int, PgInt>(query, value);
    }

    public static IQuery Bind(this IQuery query, long?[]? value)
    {
        return BindPgArrayStruct<long, PgLong>(query, value);
    }

    public static IQuery Bind(this IQuery query, float?[]? value)
    {
        return BindPgArrayStruct<float, PgFloat>(query, value);
    }

    public static IQuery Bind(this IQuery query, double?[]? value)
    {
        return BindPgArrayStruct<double, PgDouble>(query, value);
    }

    public static IQuery Bind(this IQuery query, TimeOnly?[]? value)
    {
        return BindPgArrayStruct<TimeOnly, PgTime>(query, value);
    }

    public static IQuery Bind(this IQuery query, DateOnly?[]? value)
    {
        return BindPgArrayStruct<DateOnly, PgDate>(query, value);
    }

    public static IQuery Bind(this IQuery query, DateTime?[]? value)
    {
        return BindPgArrayStruct<DateTime, PgDateTime>(query, value);
    }

    public static IQuery Bind(this IQuery query, DateTimeOffset?[]? value)
    {
        return BindPgArrayStruct<DateTimeOffset, PgDateTimeOffset>(query, value);
    }

    public static IQuery Bind(this IQuery query, decimal?[]? value)
    {
        return BindPgArrayStruct<decimal, PgDecimal>(query, value);
    }

    public static IQuery Bind(this IQuery query, byte[]?[]? value)
    {
        return BindPgArrayClass<byte[], PgBytea>(query, value);
    }

    public static IQuery Bind(this IQuery query, string?[]? value)
    {
        return BindPgArrayClass<string, PgString>(query, value);
    }

    public static IQuery Bind(this IQuery query, Guid?[]? value)
    {
        return BindPgArrayStruct<Guid, PgUuid>(query, value);
    }

    public static IQuery BindPg<TValue, TType>(this IQuery query, TValue value)
        where TType : IPgDbType<TValue>
        where TValue : notnull
    {
        var pgExecutableQuery = PgException.CheckIfIs<IQuery, PgExecutableQuery>(query);
        return pgExecutableQuery.Encode<TValue, TType>(value);
    }

    public static IQuery BindPgNullableStruct<TValue, TType>(
        this IQuery query,
        TValue? value)
        where TType : IPgDbType<TValue>
        where TValue : struct
    {
        var pgExecutableQuery = PgException.CheckIfIs<IQuery, PgExecutableQuery>(query);
        return pgExecutableQuery.EncodeNullableStruct<TValue, TType>(value);
    }

    public static IQuery BindPgNullableClass<TValue, TType>(
        this IQuery query,
        TValue? value)
        where TType : IPgDbType<TValue>
        where TValue : class
    {
        var pgExecutableQuery = PgException.CheckIfIs<IQuery, PgExecutableQuery>(query);
        return pgExecutableQuery.EncodeNullableClass<TValue, TType>(value);
    }

    public static IQuery BindPgArrayStruct<TElement, TType>(
        this IQuery query,
        TElement?[]? value)
        where TType : IPgDbType<TElement>
        where TElement : struct
    {
        var pgExecutableQuery = PgException.CheckIfIs<IQuery, PgExecutableQuery>(query);
        return pgExecutableQuery.EncodeNullableClass<TElement?[], PgArrayTypeStruct<TElement, TType>>(value);
    }

    public static IQuery BindPgArrayClass<TElement, TType>(
        this IQuery query,
        TElement?[]? value)
        where TType : IPgDbType<TElement>
        where TElement : class
    {
        var pgExecutableQuery = PgException.CheckIfIs<IQuery, PgExecutableQuery>(query);
        return pgExecutableQuery.EncodeNullableClass<TElement?[], PgArrayTypeClass<TElement, TType>>(value);
    }
}
