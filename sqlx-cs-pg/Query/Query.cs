using System.Collections;
using System.Net;
using Sqlx.Core.Query;
using Sqlx.Postgres.Exceptions;
using Sqlx.Postgres.Generator.Query;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Query;

/// <summary>
/// Extensions class for Postgres specific binding to an <see cref="IBindable"/> instance. These
/// extension methods are included when you include the Postgres module and assume your
/// <see cref="IBindable"/> instance is a <see cref="PgExecutableQuery"/>.
/// </summary>
public static partial class Query
{
    /// <summary>
    /// Bind <see cref="PgOid"/> parameter to query. This puts that value as the nth parameter in
    /// the parameterized query, where n is the current parameter as a 1-based index. This maps to
    /// the Postgres specific <c>OID</c> type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Type identifier value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod]
    public static partial IBindable Bind(this IBindable bindable, PgOid value);

    /// <summary>
    /// Bind <see cref="PgOid"/> parameter to query. This puts that value as the nth parameter in
    /// the parameterized query, where n is the current parameter as a 1-based index. This maps to
    /// the Postgres specific <c>OID</c> type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Type identifier value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod]
    public static partial IBindable Bind(this IBindable bindable, PgOid? value);

    /// <summary>
    /// Bind <see cref="PgTimeTz"/> parameter to query. This puts that value as the nth parameter in
    /// the parameterized query, where n is the current parameter as a 1-based index. This maps to
    /// the Postgres specific <c>TIME WITH TIME ZONE</c> type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Timezone aware time value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod]
    public static partial IBindable Bind(this IBindable bindable, PgTimeTz value);
    
    /// <summary>
    /// Bind <see cref="PgTimeTz"/> parameter to query. This puts that value as the nth parameter in
    /// the parameterized query, where n is the current parameter as a 1-based index. This maps to
    /// the Postgres specific <c>TIME WITH TIME ZONE</c> type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Timezone aware time value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod]
    public static partial IBindable Bind(this IBindable bindable, PgTimeTz? value);
    
    /// <summary>
    /// Bind <see cref="PgPoint"/> parameter to query. This puts that value as the nth parameter in
    /// the parameterized query, where n is the current parameter as a 1-based index. This maps to
    /// the PostGIS specific <c>POINT</c> type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Point value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod]
    public static partial IBindable Bind(this IBindable bindable, PgPoint value);
    
    /// <summary>
    /// Bind <see cref="PgPoint"/> parameter to query. This puts that value as the nth parameter in
    /// the parameterized query, where n is the current parameter as a 1-based index. This maps to
    /// the PostGIS specific <c>POINT</c> type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Point value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod]
    public static partial IBindable Bind(this IBindable bindable, PgPoint? value);
    
    /// <summary>
    /// Bind <see cref="PgLine"/> parameter to query. This puts that value as the nth parameter in
    /// the parameterized query, where n is the current parameter as a 1-based index. This maps to
    /// the PostGIS specific <c>LINE</c> type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Line value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod]
    public static partial IBindable Bind(this IBindable bindable, PgLine value);
    
    /// <summary>
    /// Bind <see cref="PgLine"/> parameter to query. This puts that value as the nth parameter in
    /// the parameterized query, where n is the current parameter as a 1-based index. This maps to
    /// the PostGIS specific <c>LINE</c> type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Line value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod]
    public static partial IBindable Bind(this IBindable bindable, PgLine? value);
    
    /// <summary>
    /// Bind <see cref="PgLineSegment"/> parameter to query. This puts that value as the nth
    /// parameter in the parameterized query, where n is the current parameter as a 1-based index.
    /// This maps to the PostGIS specific <c>LSEG</c> type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Line segment value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod]
    public static partial IBindable Bind(this IBindable bindable, PgLineSegment value);
    
    /// <summary>
    /// Bind <see cref="PgLineSegment"/> parameter to query. This puts that value as the nth
    /// parameter in the parameterized query, where n is the current parameter as a 1-based index.
    /// This maps to the PostGIS specific <c>LSEG</c> type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Line segment value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod]
    public static partial IBindable Bind(this IBindable bindable, PgLineSegment? value);
    
    /// <summary>
    /// Bind <see cref="PgBox"/> parameter to query. This puts that value as the nth parameter in
    /// the parameterized query, where n is the current parameter as a 1-based index. This maps to
    /// the PostGIS specific <c>BOX</c> type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Box value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod]
    public static partial IBindable Bind(this IBindable bindable, PgBox value);
    
    /// <summary>
    /// Bind <see cref="PgBox"/> parameter to query. This puts that value as the nth parameter in
    /// the parameterized query, where n is the current parameter as a 1-based index. This maps to
    /// the PostGIS specific <c>BOX</c> type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Box value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod]
    public static partial IBindable Bind(this IBindable bindable, PgBox? value);
    
    /// <summary>
    /// Bind <see cref="PgPath"/> parameter to query. This puts that value as the nth parameter in
    /// the parameterized query, where n is the current parameter as a 1-based index. This maps to
    /// the PostGIS specific <c>PATH</c> type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Path value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod]
    public static partial IBindable Bind(this IBindable bindable, PgPath value);
    
    /// <summary>
    /// Bind <see cref="PgPath"/> parameter to query. This puts that value as the nth parameter in
    /// the parameterized query, where n is the current parameter as a 1-based index. This maps to
    /// the PostGIS specific <c>PATH</c> type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Path value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod]
    public static partial IBindable Bind(this IBindable bindable, PgPath? value);
    
    /// <summary>
    /// Bind <see cref="PgPolygon"/> parameter to query. This puts that value as the nth parameter
    /// in the parameterized query, where n is the current parameter as a 1-based index. This maps
    /// to the PostGIS specific <c>POLYGON</c> type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Polygon value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod]
    public static partial IBindable Bind(this IBindable bindable, PgPolygon value);
    
    /// <summary>
    /// Bind <see cref="PgPolygon"/> parameter to query. This puts that value as the nth parameter
    /// in the parameterized query, where n is the current parameter as a 1-based index. This maps
    /// to the PostGIS specific <c>POLYGON</c> type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Polygon value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod]
    public static partial IBindable Bind(this IBindable bindable, PgPolygon? value);
    
    /// <summary>
    /// Bind <see cref="PgCircle"/> parameter to query. This puts that value as the nth parameter in
    /// the parameterized query, where n is the current parameter as a 1-based index. This maps to
    /// the PostGIS specific <c>CIRCLE</c> type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Circle value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod]
    public static partial IBindable Bind(this IBindable bindable, PgCircle value);
    
    /// <summary>
    /// Bind <see cref="PgCircle"/> parameter to query. This puts that value as the nth parameter in
    /// the parameterized query, where n is the current parameter as a 1-based index. This maps to
    /// the PostGIS specific <c>CIRCLE</c> type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Circle value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod]
    public static partial IBindable Bind(this IBindable bindable, PgCircle? value);
    
    /// <summary>
    /// Bind <see cref="PgInterval"/> parameter to query. This puts that value as the nth parameter
    /// in the parameterized query, where n is the current parameter as a 1-based index. This maps
    /// to the Postgres specific <c>INTERVAL</c> type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Interval value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod]
    public static partial IBindable Bind(this IBindable bindable, PgInterval value);
    
    /// <summary>
    /// Bind <see cref="PgInterval"/> parameter to query. This puts that value as the nth parameter
    /// in the parameterized query, where n is the current parameter as a 1-based index. This maps
    /// to the Postgres specific <c>INTERVAL</c> type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Interval value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod]
    public static partial IBindable Bind(this IBindable bindable, PgInterval? value);
    
    /// <summary>
    /// Bind <see cref="PgMacAddress"/> parameter to query. This puts that value as the nth
    /// parameter in the parameterized query, where n is the current parameter as a 1-based index.
    /// This maps to the Postgres specific <c>MACADDRESS</c> types.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">MAC Address value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod]
    public static partial IBindable Bind(this IBindable bindable, PgMacAddress value);
    
    /// <summary>
    /// Bind <see cref="PgMacAddress"/> parameter to query. This puts that value as the nth
    /// parameter in the parameterized query, where n is the current parameter as a 1-based index.
    /// This maps to the Postgres specific <c>MACADDRESS</c> types.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">MAC Address value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod]
    public static partial IBindable Bind(this IBindable bindable, PgMacAddress? value);
    
    /// <summary>
    /// Bind <see cref="PgMacAddress8"/> parameter to query. This puts that value as the nth
    /// parameter in the parameterized query, where n is the current parameter as a 1-based index.
    /// This maps to the Postgres specific <c>MACADDRESS8</c> types.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">MAC Address value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod]
    public static partial IBindable Bind(this IBindable bindable, PgMacAddress8 value);
    
    /// <summary>
    /// Bind <see cref="PgMacAddress8"/> parameter to query. This puts that value as the nth
    /// parameter in the parameterized query, where n is the current parameter as a 1-based index.
    /// This maps to the Postgres specific <c>MACADDRESS8</c> types.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">MAC Address value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod]
    public static partial IBindable Bind(this IBindable bindable, PgMacAddress8? value);
    
    /// <summary>
    /// Bind <see cref="PgMoney"/> parameter to query. This puts that value as the nth parameter
    /// in the parameterized query, where n is the current parameter as a 1-based index. This maps
    /// to the Postgres specific <c>MONEY</c> type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Money value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod]
    public static partial IBindable Bind(this IBindable bindable, PgMoney value);
    
    /// <summary>
    /// Bind <see cref="PgMoney"/> parameter to query. This puts that value as the nth parameter
    /// in the parameterized query, where n is the current parameter as a 1-based index. This maps
    /// to the Postgres specific <c>MONEY</c> type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Money value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod]
    public static partial IBindable Bind(this IBindable bindable, PgMoney? value);
    
    /// <summary>
    /// Bind <see cref="PgInet"/> parameter to query. This puts that value as the nth parameter in
    /// the parameterized query, where n is the current parameter as a 1-based index. This maps to
    /// the Postgres specific <c>INET</c> and <c>CIDR</c> types.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Network address value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod]
    public static partial IBindable Bind(this IBindable bindable, PgInet? value);
    
    /// <summary>
    /// Bind <see cref="IPNetwork"/> parameter to query. This puts that value as the nth parameter
    /// in the parameterized query, where n is the current parameter as a 1-based index. This maps
    /// to the Postgres specific <c>INET</c> and <c>CIDR</c> types.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Network address value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod(Encoder = typeof(PgIpNetwork))]
    public static partial IBindable Bind(this IBindable bindable, IPNetwork? value);
    
    /// <summary>
    /// Bind <see cref="BitArray"/> parameter to query. This puts that value as the nth parameter
    /// in the parameterized query, where n is the current parameter as a 1-based index. This maps
    /// to the Postgres specific <c>VARBIT(n)</c> and <c>BIT(n)</c> types.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Network address value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod(Encoder = typeof(PgBitString))]
    public static partial IBindable Bind(this IBindable bindable, BitArray? value);
    
    /// <summary>
    /// Bind <see cref="PgRange{T}"/> of <see cref="long"/> parameter to query. This puts that value
    /// as the nth parameter in the parameterized query, where n is the current parameter as a
    /// 1-based index. This maps to the Postgres specific <c>INT8RANGE</c> type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Long range value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod(Encoder = typeof(PgRangeType<long, PgLong>))]
    public static partial IBindable Bind(this IBindable bindable, PgRange<long>? value);
    
    /// <summary>
    /// Bind <see cref="PgRange{T}"/> of <see cref="int"/> parameter to query. This puts that value
    /// as the nth parameter in the parameterized query, where n is the current parameter as a
    /// 1-based index. This maps to the Postgres specific <c>INT4RANGE</c> type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Int range value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod(Encoder = typeof(PgRangeType<int, PgInt>))]
    public static partial IBindable Bind(this IBindable bindable, PgRange<int>? value);
    
    /// <summary>
    /// Bind <see cref="PgRange{T}"/> of <see cref="DateOnly"/> parameter to query. This puts that
    /// value as the nth parameter in the parameterized query, where n is the current parameter as a
    /// 1-based index. This maps to the Postgres specific <c>DATERANGE</c> type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Date range value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod(Encoder = typeof(PgRangeType<DateOnly, PgDate>))]
    public static partial IBindable Bind(this IBindable bindable, PgRange<DateOnly>? value);
    
    /// <summary>
    /// Bind <see cref="PgRange{T}"/> of <see cref="DateTime"/> parameter to query. This puts that
    /// value as the nth parameter in the parameterized query, where n is the current parameter as a
    /// 1-based index. This maps to the Postgres specific <c>TSRANGE</c> type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Datetime range value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod(Encoder = typeof(PgRangeType<DateTime, PgDateTime>))]
    public static partial IBindable Bind(this IBindable bindable, PgRange<DateTime>? value);
    
    /// <summary>
    /// Bind <see cref="PgRange{T}"/> of <see cref="DateTimeOffset"/> parameter to query. This puts
    /// that value as the nth parameter in the parameterized query, where n is the current parameter
    /// as a 1-based index. This maps to the Postgres specific <c>TSTZRANGE</c> type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Datetime offset range value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod(Encoder = typeof(PgRangeType<DateTimeOffset, PgDateTimeOffset>))]
    public static partial IBindable Bind(this IBindable bindable, PgRange<DateTimeOffset>? value);
    
    /// <summary>
    /// Bind <see cref="PgRange{T}"/> of <see cref="decimal"/> parameter to query. This puts that
    /// value as the nth parameter in the parameterized query, where n is the current parameter as a
    /// 1-based index. This maps to the Postgres specific <c>NUMRANGE</c> type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Decimal range value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod(Encoder = typeof(PgRangeType<decimal, PgDecimal>))]
    public static partial IBindable Bind(this IBindable bindable, PgRange<decimal>? value);
    
    /// <summary>
    /// Bind <see cref="bool"/> array parameter to query. This puts that value as the nth parameter
    /// in the parameterized query, where n is the current parameter as a 1-based index. This maps
    /// to the Postgres specific <c>BOOLEAN[]</c> types.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Boolean array value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod(Encoder = typeof(PgBool))]
    public static partial IBindable Bind(this IBindable bindable, bool?[]? value);
    
    /// <summary>
    /// Bind <see cref="short"/> array parameter to query. This puts that value as the nth parameter
    /// in the parameterized query, where n is the current parameter as a 1-based index. This maps
    /// to the Postgres specific <c>SMALLINT[]</c> type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Short array value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod(Encoder = typeof(PgShort))]
    public static partial IBindable Bind(this IBindable bindable, short?[]? value);
    
    /// <summary>
    /// Bind <see cref="int"/> array parameter to query. This puts that value as the nth parameter
    /// in the parameterized query, where n is the current parameter as a 1-based index. This maps
    /// to the Postgres specific <c>INT[]</c> type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Int array value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod(Encoder = typeof(PgInt))]
    public static partial IBindable Bind(this IBindable bindable, int?[]? value);
    
    /// <summary>
    /// Bind <see cref="long"/> array parameter to query. This puts that value as the nth parameter
    /// in the parameterized query, where n is the current parameter as a 1-based index. This maps
    /// to the Postgres specific <c>BIGINT[]</c> type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Long array value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod(Encoder = typeof(PgLong))]
    public static partial IBindable Bind(this IBindable bindable, long?[]? value);
    
    /// <summary>
    /// Bind <see cref="float"/> array parameter to query. This puts that value as the nth parameter
    /// in the parameterized query, where n is the current parameter as a 1-based index. This maps
    /// to the Postgres specific <c>REAL[]</c> type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Float array value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod(Encoder = typeof(PgFloat))]
    public static partial IBindable Bind(this IBindable bindable, float?[]? value);
    
    /// <summary>
    /// Bind <see cref="double"/> array parameter to query. This puts that value as the nth
    /// parameter in the parameterized query, where n is the current parameter as a 1-based index.
    /// This maps to the Postgres specific <c>DOUBLE PRECISION[]</c> type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Double array value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod(Encoder = typeof(PgDouble))]
    public static partial IBindable Bind(this IBindable bindable, double?[]? value);
    
    /// <summary>
    /// Bind <see cref="TimeOnly"/> array parameter to query. This puts that value as the nth
    /// parameter in the parameterized query, where n is the current parameter as a 1-based index.
    /// This maps to the Postgres specific <c>TIME[]</c> type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Time array value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod(Encoder = typeof(PgTime))]
    public static partial IBindable Bind(this IBindable bindable, TimeOnly?[]? value);
    
    /// <summary>
    /// Bind <see cref="DateOnly"/> array parameter to query. This puts that value as the nth
    /// parameter in the parameterized query, where n is the current parameter as a 1-based index.
    /// This maps to the Postgres specific <c>DATE[]</c> type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Date array value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod(Encoder = typeof(PgDate))]
    public static partial IBindable Bind(this IBindable bindable, DateOnly?[]? value);
    
    /// <summary>
    /// Bind <see cref="DateTime"/> array parameter to query. This puts that value as the nth
    /// parameter in the parameterized query, where n is the current parameter as a 1-based index.
    /// This maps to the Postgres specific <c>TIMESTAMP[]</c> type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Datetime array value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod(Encoder = typeof(PgDateTime))]
    public static partial IBindable Bind(this IBindable bindable, DateTime?[]? value);
    
    /// <summary>
    /// Bind <see cref="DateTimeOffset"/> array parameter to query. This puts that value as the nth
    /// parameter in the parameterized query, where n is the current parameter as a 1-based index.
    /// This maps to the Postgres specific <c>TIMESTAMP WITH TIME ZONE[]</c> type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Datetime offset array value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod(Encoder = typeof(PgDateTimeOffset))]
    public static partial IBindable Bind(this IBindable bindable, DateTimeOffset?[]? value);
    
    /// <summary>
    /// Bind <see cref="decimal"/> array parameter to query. This puts that value as the nth
    /// parameter in the parameterized query, where n is the current parameter as a 1-based index.
    /// This maps to the Postgres specific <c>DECIMAL[]</c> type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Decimal array value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod(Encoder = typeof(PgDecimal))]
    public static partial IBindable Bind(this IBindable bindable, decimal?[]? value);
    
    /// <summary>
    /// Bind <see cref="byte"/> array parameter to query. This puts that value as the nth parameter
    /// in the parameterized query, where n is the current parameter as a 1-based index. This maps
    /// to the Postgres specific <c>BYTEA[]</c> type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Byte array value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod(Encoder = typeof(PgBytea))]
    public static partial IBindable Bind(this IBindable bindable, byte[]?[]? value);
    
    /// <summary>
    /// Bind <see cref="string"/> array parameter to query. This puts that value as the nth
    /// parameter in the parameterized query, where n is the current parameter as a 1-based index.
    /// This maps to the Postgres specific <c>TEXT[]</c> type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">String array value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod(Encoder = typeof(PgString))]
    public static partial IBindable Bind(this IBindable bindable, string?[]? value);
    
    /// <summary>
    /// Bind <see cref="Guid"/> array parameter to query. This puts that value as the nth
    /// parameter in the parameterized query, where n is the current parameter as a 1-based index.
    /// This maps to the Postgres specific <c>UUID[]</c> type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Guid array value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod(Encoder = typeof(PgUuid))]
    public static partial IBindable Bind(this IBindable bindable, Guid?[]? value);

    extension(IBindable bindable)
    {
        /// <summary>
        /// Bind <typeparamref name="TType"/> parameter to query. This puts that value as the nth
        /// parameter in the parameterized query, where n is the current parameter as a 1-based index.
        /// This allows for any value that can be encoded using the type definition of
        /// <typeparamref name="TType"/> to be bound to a query.
        /// </summary>
        /// <param name="value">Value to bind to the query</param>
        /// <typeparam name="TType">DB Type definition to allow for encoding the value</typeparam>
        /// <returns>This query instance for method chaining</returns>
        public IBindable BindPg<TType>(TType value)
            where TType : IPgDbType<TType>
        {
            IPgBindable pgBindable = PgException.CheckIfIs<IBindable, IPgBindable>(bindable);
            return pgBindable.BindPg<TType, TType>(value);
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
        /// <param name="value">Array value to bind</param>
        /// <typeparam name="TElement">Array element type</typeparam>
        /// <typeparam name="TType">DB Type definition to allow for encoding the value</typeparam>
        /// <returns>This query instance for method chaining</returns>
        public IBindable BindPgArrayStruct<TElement, TType>(TElement?[]? value)
            where TType : IPgDbType<TElement>, IHasArrayType
            where TElement : struct
        {
            IPgBindable pgBindable = PgException.CheckIfIs<IBindable, IPgBindable>(bindable);
            return pgBindable.BindPgNullableClass<TElement?[], PgArrayTypeStruct<TElement, TType>>(value);
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
        /// <param name="value">Array value to bind</param>
        /// <typeparam name="TElement">Array element type</typeparam>
        /// <typeparam name="TType">DB Type definition to allow for encoding the value</typeparam>
        /// <returns>This query instance for method chaining</returns>
        public IBindable BindPgArrayClass<TElement, TType>(TElement?[]? value)
            where TType : IPgDbType<TElement>, IHasArrayType
            where TElement : class
        {
            IPgBindable pgBindable = PgException.CheckIfIs<IBindable, IPgBindable>(bindable);
            return pgBindable.BindPgNullableClass<TElement?[], PgArrayTypeClass<TElement, TType>>(value);
        }
    }
}
