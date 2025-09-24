using System.Runtime.CompilerServices;
using System.Text.Json.Serialization.Metadata;
using Sqlx.Core;
using Sqlx.Core.Query;
using Sqlx.Core.Result;
using Sqlx.Postgres.Exceptions;
using Sqlx.Postgres.Generator.Query;
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

/// <summary>
/// Extensions class for Postgres specific binding to an <see cref="IQuery"/> instance. These
/// extension methods are included when you include the Postgres module and assume your
/// <see cref="IQuery"/> instance is a <see cref="PgExecutableQuery"/>.
/// </summary>
public static partial class ExecutableQueryExtensions
{
    /// <summary>
    /// Bind <see cref="PgTimeTz"/> parameter to query. This puts that value as the nth parameter in
    /// the parameterized query, where n is the current parameter as a 1-based index. This maps to
    /// the Postgres specific <c>TIME WITH TIME ZONE</c> type.
    /// </summary>
    /// <param name="query">Query to bind against</param>
    /// <param name="value">Timezone aware time value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod]
    public static partial IQuery Bind(this IQuery query, PgTimeTz value);
    
    /// <summary>
    /// Bind <see cref="PgTimeTz"/> parameter to query. This puts that value as the nth parameter in
    /// the parameterized query, where n is the current parameter as a 1-based index. This maps to
    /// the Postgres specific <c>TIME WITH TIME ZONE</c> type.
    /// </summary>
    /// <param name="query">Query to bind against</param>
    /// <param name="value">Timezone aware time value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod]
    public static partial IQuery Bind(this IQuery query, PgTimeTz? value);
    
    /// <summary>
    /// Bind <see cref="PgPoint"/> parameter to query. This puts that value as the nth parameter in
    /// the parameterized query, where n is the current parameter as a 1-based index. This maps to
    /// the PostGIS specific <c>POINT</c> type.
    /// </summary>
    /// <param name="query">Query to bind against</param>
    /// <param name="value">Point value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod]
    public static partial IQuery Bind(this IQuery query, PgPoint value);
    
    /// <summary>
    /// Bind <see cref="PgPoint"/> parameter to query. This puts that value as the nth parameter in
    /// the parameterized query, where n is the current parameter as a 1-based index. This maps to
    /// the PostGIS specific <c>POINT</c> type.
    /// </summary>
    /// <param name="query">Query to bind against</param>
    /// <param name="value">Point value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod]
    public static partial IQuery Bind(this IQuery query, PgPoint? value);
    
    /// <summary>
    /// Bind <see cref="PgLine"/> parameter to query. This puts that value as the nth parameter in
    /// the parameterized query, where n is the current parameter as a 1-based index. This maps to
    /// the PostGIS specific <c>LINE</c> type.
    /// </summary>
    /// <param name="query">Query to bind against</param>
    /// <param name="value">Line value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod]
    public static partial IQuery Bind(this IQuery query, PgLine value);
    
    /// <summary>
    /// Bind <see cref="PgLine"/> parameter to query. This puts that value as the nth parameter in
    /// the parameterized query, where n is the current parameter as a 1-based index. This maps to
    /// the PostGIS specific <c>LINE</c> type.
    /// </summary>
    /// <param name="query">Query to bind against</param>
    /// <param name="value">Line value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod]
    public static partial IQuery Bind(this IQuery query, PgLine? value);
    
    /// <summary>
    /// Bind <see cref="PgLineSegment"/> parameter to query. This puts that value as the nth
    /// parameter in the parameterized query, where n is the current parameter as a 1-based index.
    /// This maps to the PostGIS specific <c>LSEG</c> type.
    /// </summary>
    /// <param name="query">Query to bind against</param>
    /// <param name="value">Line segment value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod]
    public static partial IQuery Bind(this IQuery query, PgLineSegment value);
    
    /// <summary>
    /// Bind <see cref="PgLineSegment"/> parameter to query. This puts that value as the nth
    /// parameter in the parameterized query, where n is the current parameter as a 1-based index.
    /// This maps to the PostGIS specific <c>LSEG</c> type.
    /// </summary>
    /// <param name="query">Query to bind against</param>
    /// <param name="value">Line segment value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod]
    public static partial IQuery Bind(this IQuery query, PgLineSegment? value);
    
    /// <summary>
    /// Bind <see cref="PgBox"/> parameter to query. This puts that value as the nth parameter in
    /// the parameterized query, where n is the current parameter as a 1-based index. This maps to
    /// the PostGIS specific <c>BOX</c> type.
    /// </summary>
    /// <param name="query">Query to bind against</param>
    /// <param name="value">Box value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod]
    public static partial IQuery Bind(this IQuery query, PgBox value);
    
    /// <summary>
    /// Bind <see cref="PgBox"/> parameter to query. This puts that value as the nth parameter in
    /// the parameterized query, where n is the current parameter as a 1-based index. This maps to
    /// the PostGIS specific <c>BOX</c> type.
    /// </summary>
    /// <param name="query">Query to bind against</param>
    /// <param name="value">Box value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod]
    public static partial IQuery Bind(this IQuery query, PgBox? value);
    
    /// <summary>
    /// Bind <see cref="PgPath"/> parameter to query. This puts that value as the nth parameter in
    /// the parameterized query, where n is the current parameter as a 1-based index. This maps to
    /// the PostGIS specific <c>PATH</c> type.
    /// </summary>
    /// <param name="query">Query to bind against</param>
    /// <param name="value">Path value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod]
    public static partial IQuery Bind(this IQuery query, PgPath value);
    
    /// <summary>
    /// Bind <see cref="PgPath"/> parameter to query. This puts that value as the nth parameter in
    /// the parameterized query, where n is the current parameter as a 1-based index. This maps to
    /// the PostGIS specific <c>PATH</c> type.
    /// </summary>
    /// <param name="query">Query to bind against</param>
    /// <param name="value">Path value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod]
    public static partial IQuery Bind(this IQuery query, PgPath? value);
    
    /// <summary>
    /// Bind <see cref="PgPolygon"/> parameter to query. This puts that value as the nth parameter
    /// in the parameterized query, where n is the current parameter as a 1-based index. This maps
    /// to the PostGIS specific <c>POLYGON</c> type.
    /// </summary>
    /// <param name="query">Query to bind against</param>
    /// <param name="value">Polygon value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod]
    public static partial IQuery Bind(this IQuery query, PgPolygon value);
    
    /// <summary>
    /// Bind <see cref="PgPolygon"/> parameter to query. This puts that value as the nth parameter
    /// in the parameterized query, where n is the current parameter as a 1-based index. This maps
    /// to the PostGIS specific <c>POLYGON</c> type.
    /// </summary>
    /// <param name="query">Query to bind against</param>
    /// <param name="value">Polygon value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod]
    public static partial IQuery Bind(this IQuery query, PgPolygon? value);
    
    /// <summary>
    /// Bind <see cref="PgCircle"/> parameter to query. This puts that value as the nth parameter in
    /// the parameterized query, where n is the current parameter as a 1-based index. This maps to
    /// the PostGIS specific <c>CIRCLE</c> type.
    /// </summary>
    /// <param name="query">Query to bind against</param>
    /// <param name="value">Circle value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod]
    public static partial IQuery Bind(this IQuery query, PgCircle value);
    
    /// <summary>
    /// Bind <see cref="PgCircle"/> parameter to query. This puts that value as the nth parameter in
    /// the parameterized query, where n is the current parameter as a 1-based index. This maps to
    /// the PostGIS specific <c>CIRCLE</c> type.
    /// </summary>
    /// <param name="query">Query to bind against</param>
    /// <param name="value">Circle value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod]
    public static partial IQuery Bind(this IQuery query, PgCircle? value);
    
    /// <summary>
    /// Bind <see cref="PgInterval"/> parameter to query. This puts that value as the nth parameter
    /// in the parameterized query, where n is the current parameter as a 1-based index. This maps
    /// to the Postgres specific <c>INTERVAL</c> type.
    /// </summary>
    /// <param name="query">Query to bind against</param>
    /// <param name="value">Interval value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod]
    public static partial IQuery Bind(this IQuery query, PgInterval value);
    
    /// <summary>
    /// Bind <see cref="PgInterval"/> parameter to query. This puts that value as the nth parameter
    /// in the parameterized query, where n is the current parameter as a 1-based index. This maps
    /// to the Postgres specific <c>INTERVAL</c> type.
    /// </summary>
    /// <param name="query">Query to bind against</param>
    /// <param name="value">Interval value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod]
    public static partial IQuery Bind(this IQuery query, PgInterval? value);
    
    /// <summary>
    /// Bind <see cref="PgMacAddress"/> parameter to query. This puts that value as the nth
    /// parameter in the parameterized query, where n is the current parameter as a 1-based index.
    /// This maps to the Postgres specific <c>MACADDRESS</c> and <c>MACADDRESS8</c> types.
    /// </summary>
    /// <param name="query">Query to bind against</param>
    /// <param name="value">MAC Address value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod]
    public static partial IQuery Bind(this IQuery query, PgMacAddress value);
    
    /// <summary>
    /// Bind <see cref="PgMacAddress"/> parameter to query. This puts that value as the nth
    /// parameter in the parameterized query, where n is the current parameter as a 1-based index.
    /// This maps to the Postgres specific <c>MACADDRESS</c> and <c>MACADDRESS8</c> types.
    /// </summary>
    /// <param name="query">Query to bind against</param>
    /// <param name="value">MAC Address value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod]
    public static partial IQuery Bind(this IQuery query, PgMacAddress? value);
    
    /// <summary>
    /// Bind <see cref="PgMoney"/> parameter to query. This puts that value as the nth parameter
    /// in the parameterized query, where n is the current parameter as a 1-based index. This maps
    /// to the Postgres specific <c>MONEY</c> type.
    /// </summary>
    /// <param name="query">Query to bind against</param>
    /// <param name="value">Money value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod]
    public static partial IQuery Bind(this IQuery query, PgMoney value);
    
    /// <summary>
    /// Bind <see cref="PgMoney"/> parameter to query. This puts that value as the nth parameter
    /// in the parameterized query, where n is the current parameter as a 1-based index. This maps
    /// to the Postgres specific <c>MONEY</c> type.
    /// </summary>
    /// <param name="query">Query to bind against</param>
    /// <param name="value">Money value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod]
    public static partial IQuery Bind(this IQuery query, PgMoney? value);
    
    /// <summary>
    /// Bind <see cref="PgInet"/> parameter to query. This puts that value as the nth parameter in
    /// the parameterized query, where n is the current parameter as a 1-based index. This maps to
    /// the Postgres specific <c>INET</c> and <c>CIDR</c> types.
    /// </summary>
    /// <param name="query">Query to bind against</param>
    /// <param name="value">Network address value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod]
    public static partial IQuery Bind(this IQuery query, PgInet? value);
    
    /// <summary>
    /// Bind <see cref="PgRange{T}"/> of <see cref="long"/> parameter to query. This puts that value
    /// as the nth parameter in the parameterized query, where n is the current parameter as a
    /// 1-based index. This maps to the Postgres specific <c>INT8RANGE</c> type.
    /// </summary>
    /// <param name="query">Query to bind against</param>
    /// <param name="value">Long range value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod(Encoder = typeof(PgRangeType<long, PgLong>))]
    public static partial IQuery Bind(this IQuery query, PgRange<long>? value);
    
    /// <summary>
    /// Bind <see cref="PgRange{T}"/> of <see cref="int"/> parameter to query. This puts that value
    /// as the nth parameter in the parameterized query, where n is the current parameter as a
    /// 1-based index. This maps to the Postgres specific <c>INT4RANGE</c> type.
    /// </summary>
    /// <param name="query">Query to bind against</param>
    /// <param name="value">Int range value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod(Encoder = typeof(PgRangeType<int, PgInt>))]
    public static partial IQuery Bind(this IQuery query, PgRange<int>? value);
    
    /// <summary>
    /// Bind <see cref="PgRange{T}"/> of <see cref="DateOnly"/> parameter to query. This puts that
    /// value as the nth parameter in the parameterized query, where n is the current parameter as a
    /// 1-based index. This maps to the Postgres specific <c>DATERANGE</c> type.
    /// </summary>
    /// <param name="query">Query to bind against</param>
    /// <param name="value">Date range value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod(Encoder = typeof(PgRangeType<DateOnly, PgDate>))]
    public static partial IQuery Bind(this IQuery query, PgRange<DateOnly>? value);
    
    /// <summary>
    /// Bind <see cref="PgRange{T}"/> of <see cref="DateTime"/> parameter to query. This puts that
    /// value as the nth parameter in the parameterized query, where n is the current parameter as a
    /// 1-based index. This maps to the Postgres specific <c>TSRANGE</c> type.
    /// </summary>
    /// <param name="query">Query to bind against</param>
    /// <param name="value">Datetime range value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod(Encoder = typeof(PgRangeType<DateTime, PgDateTime>))]
    public static partial IQuery Bind(this IQuery query, PgRange<DateTime>? value);
    
    /// <summary>
    /// Bind <see cref="PgRange{T}"/> of <see cref="DateTimeOffset"/> parameter to query. This puts
    /// that value as the nth parameter in the parameterized query, where n is the current parameter
    /// as a 1-based index. This maps to the Postgres specific <c>TSTZRANGE</c> type.
    /// </summary>
    /// <param name="query">Query to bind against</param>
    /// <param name="value">Datetime offset range value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod(Encoder = typeof(PgRangeType<DateTimeOffset, PgDateTimeOffset>))]
    public static partial IQuery Bind(this IQuery query, PgRange<DateTimeOffset>? value);
    
    /// <summary>
    /// Bind <see cref="PgRange{T}"/> of <see cref="decimal"/> parameter to query. This puts that
    /// value as the nth parameter in the parameterized query, where n is the current parameter as a
    /// 1-based index. This maps to the Postgres specific <c>NUMRANGE</c> type.
    /// </summary>
    /// <param name="query">Query to bind against</param>
    /// <param name="value">Decimal range value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod(Encoder = typeof(PgRangeType<decimal, PgDecimal>))]
    public static partial IQuery Bind(this IQuery query, PgRange<decimal>? value);
    
    /// <summary>
    /// Bind <see cref="bool"/> array parameter to query. This puts that value as the nth parameter
    /// in the parameterized query, where n is the current parameter as a 1-based index. This maps
    /// to the Postgres specific <c>BOOLEAN[]</c> types.
    /// </summary>
    /// <param name="query">Query to bind against</param>
    /// <param name="value">Boolean array value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod(Encoder = typeof(PgBool))]
    public static partial IQuery Bind(this IQuery query, bool?[]? value);
    
    /// <summary>
    /// Bind <see cref="short"/> array parameter to query. This puts that value as the nth parameter
    /// in the parameterized query, where n is the current parameter as a 1-based index. This maps
    /// to the Postgres specific <c>SMALLINT[]</c> type.
    /// </summary>
    /// <param name="query">Query to bind against</param>
    /// <param name="value">Short array value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod(Encoder = typeof(PgShort))]
    public static partial IQuery Bind(this IQuery query, short?[]? value);
    
    /// <summary>
    /// Bind <see cref="int"/> array parameter to query. This puts that value as the nth parameter
    /// in the parameterized query, where n is the current parameter as a 1-based index. This maps
    /// to the Postgres specific <c>INT[]</c> type.
    /// </summary>
    /// <param name="query">Query to bind against</param>
    /// <param name="value">Int array value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod(Encoder = typeof(PgInt))]
    public static partial IQuery Bind(this IQuery query, int?[]? value);
    
    /// <summary>
    /// Bind <see cref="long"/> array parameter to query. This puts that value as the nth parameter
    /// in the parameterized query, where n is the current parameter as a 1-based index. This maps
    /// to the Postgres specific <c>BIGINT[]</c> type.
    /// </summary>
    /// <param name="query">Query to bind against</param>
    /// <param name="value">Long array value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod(Encoder = typeof(PgLong))]
    public static partial IQuery Bind(this IQuery query, long?[]? value);
    
    /// <summary>
    /// Bind <see cref="float"/> array parameter to query. This puts that value as the nth parameter
    /// in the parameterized query, where n is the current parameter as a 1-based index. This maps
    /// to the Postgres specific <c>REAL[]</c> type.
    /// </summary>
    /// <param name="query">Query to bind against</param>
    /// <param name="value">Float array value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod(Encoder = typeof(PgFloat))]
    public static partial IQuery Bind(this IQuery query, float?[]? value);
    
    /// <summary>
    /// Bind <see cref="double"/> array parameter to query. This puts that value as the nth
    /// parameter in the parameterized query, where n is the current parameter as a 1-based index.
    /// This maps to the Postgres specific <c>DOUBLE PRECISION[]</c> type.
    /// </summary>
    /// <param name="query">Query to bind against</param>
    /// <param name="value">Double array value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod(Encoder = typeof(PgDouble))]
    public static partial IQuery Bind(this IQuery query, double?[]? value);
    
    /// <summary>
    /// Bind <see cref="TimeOnly"/> array parameter to query. This puts that value as the nth
    /// parameter in the parameterized query, where n is the current parameter as a 1-based index.
    /// This maps to the Postgres specific <c>TIME[]</c> type.
    /// </summary>
    /// <param name="query">Query to bind against</param>
    /// <param name="value">Time array value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod(Encoder = typeof(PgTime))]
    public static partial IQuery Bind(this IQuery query, TimeOnly?[]? value);
    
    /// <summary>
    /// Bind <see cref="DateOnly"/> array parameter to query. This puts that value as the nth
    /// parameter in the parameterized query, where n is the current parameter as a 1-based index.
    /// This maps to the Postgres specific <c>DATE[]</c> type.
    /// </summary>
    /// <param name="query">Query to bind against</param>
    /// <param name="value">Date array value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod(Encoder = typeof(PgDate))]
    public static partial IQuery Bind(this IQuery query, DateOnly?[]? value);
    
    /// <summary>
    /// Bind <see cref="DateTime"/> array parameter to query. This puts that value as the nth
    /// parameter in the parameterized query, where n is the current parameter as a 1-based index.
    /// This maps to the Postgres specific <c>TIMESTAMP[]</c> type.
    /// </summary>
    /// <param name="query">Query to bind against</param>
    /// <param name="value">Datetime array value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod(Encoder = typeof(PgDateTime))]
    public static partial IQuery Bind(this IQuery query, DateTime?[]? value);
    
    /// <summary>
    /// Bind <see cref="DateTimeOffset"/> array parameter to query. This puts that value as the nth
    /// parameter in the parameterized query, where n is the current parameter as a 1-based index.
    /// This maps to the Postgres specific <c>TIMESTAMP WITH TIME ZONE[]</c> type.
    /// </summary>
    /// <param name="query">Query to bind against</param>
    /// <param name="value">Datetime offset array value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod(Encoder = typeof(PgDateTimeOffset))]
    public static partial IQuery Bind(this IQuery query, DateTimeOffset?[]? value);
    
    /// <summary>
    /// Bind <see cref="decimal"/> array parameter to query. This puts that value as the nth
    /// parameter in the parameterized query, where n is the current parameter as a 1-based index.
    /// This maps to the Postgres specific <c>DECIMAL[]</c> type.
    /// </summary>
    /// <param name="query">Query to bind against</param>
    /// <param name="value">Decimal array value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod(Encoder = typeof(PgDecimal))]
    public static partial IQuery Bind(this IQuery query, decimal?[]? value);
    
    /// <summary>
    /// Bind <see cref="byte"/> array parameter to query. This puts that value as the nth parameter
    /// in the parameterized query, where n is the current parameter as a 1-based index. This maps
    /// to the Postgres specific <c>BYTEA[]</c> type.
    /// </summary>
    /// <param name="query">Query to bind against</param>
    /// <param name="value">Byte array value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod(Encoder = typeof(PgBytea))]
    public static partial IQuery Bind(this IQuery query, byte[]?[]? value);
    
    /// <summary>
    /// Bind <see cref="string"/> array parameter to query. This puts that value as the nth
    /// parameter in the parameterized query, where n is the current parameter as a 1-based index.
    /// This maps to the Postgres specific <c>TEXT[]</c> type.
    /// </summary>
    /// <param name="query">Query to bind against</param>
    /// <param name="value">String array value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod(Encoder = typeof(PgString))]
    public static partial IQuery Bind(this IQuery query, string?[]? value);
    
    /// <summary>
    /// Bind <see cref="Guid"/> array parameter to query. This puts that value as the nth
    /// parameter in the parameterized query, where n is the current parameter as a 1-based index.
    /// This maps to the Postgres specific <c>UUID[]</c> type.
    /// </summary>
    /// <param name="query">Query to bind against</param>
    /// <param name="value">Guid array value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod(Encoder = typeof(PgUuid))]
    public static partial IQuery Bind(this IQuery query, Guid?[]? value);

    /// <summary>
    /// Bind <typeparamref name="TValue"/> parameter to query. This puts that value as the nth
    /// parameter in the parameterized query, where n is the current parameter as a 1-based index.
    /// This allows for any value that can be encoded using the type definition of
    /// <typeparamref name="TType"/> to be bound to a query.
    /// </summary>
    /// <param name="query">Query to bind against</param>
    /// <param name="value">Value to bind to the query</param>
    /// <typeparam name="TValue">Value type to bind</typeparam>
    /// <typeparam name="TType">DB Type definition to allow for encoding the value</typeparam>
    /// <returns>This query instance for method chaining</returns>
    public static IQuery BindPg<TValue, TType>(this IQuery query, TValue value)
        where TType : IPgDbType<TValue>
        where TValue : notnull
    {
        var pgExecutableQuery = PgException.CheckIfIs<IQuery, PgExecutableQuery>(query);
        return pgExecutableQuery.Encode<TValue, TType>(value);
    }

    /// <summary>
    /// <para>
    /// Bind <typeparamref name="TElement"/> array parameter to query. This puts that value as the
    /// nth parameter in the parameterized query, where n is the current parameter as a 1-based
    /// index. This allows for any array value that can be encoded using the type definition of
    /// <typeparamref name="TType"/> to be bound to a query.
    /// </para>
    /// <para>
    /// This differs from <see cref="BindPgArrayClass"/> because the element type must be a struct
    /// so that nullable vs default semantics can be handled correctly.
    /// </para>
    /// </summary>
    /// <param name="query">Query to bind against</param>
    /// <param name="value">Array value to bind</param>
    /// <typeparam name="TElement">Array element type</typeparam>
    /// <typeparam name="TType">DB Type definition to allow for encoding the value</typeparam>
    /// <returns>This query instance for method chaining</returns>
    public static IQuery BindPgArrayStruct<TElement, TType>(
        this IQuery query,
        TElement?[]? value)
        where TType : IPgDbType<TElement>, IHasArrayType
        where TElement : struct
    {
        var pgExecutableQuery = PgException.CheckIfIs<IQuery, PgExecutableQuery>(query);
        return pgExecutableQuery.EncodeNullableClass<TElement?[], PgArrayTypeStruct<TElement, TType>>(value);
    }

    /// <summary>
    /// <para>
    /// Bind <typeparamref name="TElement"/> array parameter to query. This puts that value as the
    /// nth parameter in the parameterized query, where n is the current parameter as a 1-based
    /// index. This allows for any array value that can be encoded using the type definition of
    /// <typeparamref name="TType"/> to be bound to a query.
    /// </para>
    /// <para>
    /// This differs from <see cref="BindPgArrayStruct"/> because the element type must be a class
    /// so that nullable vs default semantics can be handled correctly.
    /// </para>
    /// </summary>
    /// <param name="query">Query to bind against</param>
    /// <param name="value">Array value to bind</param>
    /// <typeparam name="TElement">Array element type</typeparam>
    /// <typeparam name="TType">DB Type definition to allow for encoding the value</typeparam>
    /// <returns>This query instance for method chaining</returns>
    public static IQuery BindPgArrayClass<TElement, TType>(
        this IQuery query,
        TElement?[]? value)
        where TType : IPgDbType<TElement>, IHasArrayType
        where TElement : class
    {
        var pgExecutableQuery = PgException.CheckIfIs<IQuery, PgExecutableQuery>(query);
        return pgExecutableQuery.EncodeNullableClass<TElement?[], PgArrayTypeClass<TElement, TType>>(value);
    }
}
