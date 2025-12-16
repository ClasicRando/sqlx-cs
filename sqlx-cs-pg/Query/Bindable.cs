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
/// <see cref="IBindable"/> instance is a <see cref="IPgBindable"/>.
/// </summary>
public static partial class Bindable
{
    /// <summary>
    /// Bind a <see cref="PgOid"/> value. This maps to the Postgres specific <c>OID</c> type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Type identifier value</param>
    [GeneratePgBindMethod]
    public static partial void Bind(this IBindable bindable, PgOid value);

    /// <summary>
    /// Bind a <see cref="PgOid"/> value. This maps to the Postgres specific <c>OID</c> type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Type identifier value</param>
    [GeneratePgBindMethod]
    public static partial void Bind(this IBindable bindable, PgOid? value);

    /// <summary>
    /// Bind a <see cref="PgTimeTz"/> value. This maps to the Postgres specific
    /// <c>TIME WITH TIME ZONE</c> type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Timezone aware time value</param>
    [GeneratePgBindMethod]
    public static partial void Bind(this IBindable bindable, PgTimeTz value);
    
    /// <summary>
    /// Bind a <see cref="PgTimeTz"/> value. This maps to the Postgres specific
    /// <c>TIME WITH TIME ZONE</c> type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Timezone aware time value</param>
    [GeneratePgBindMethod]
    public static partial void Bind(this IBindable bindable, PgTimeTz? value);
    
    /// <summary>
    /// Bind a <see cref="PgPoint"/> value. This maps to the Postgres specific <c>POINT</c> type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Point value</param>
    [GeneratePgBindMethod]
    public static partial void Bind(this IBindable bindable, PgPoint value);
    
    /// <summary>
    /// Bind a <see cref="PgPoint"/> value. This maps to the Postgres specific <c>POINT</c> type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Point value</param>
    [GeneratePgBindMethod]
    public static partial void Bind(this IBindable bindable, PgPoint? value);
    
    /// <summary>
    /// Bind a <see cref="PgLine"/> value. This maps to the Postgres specific <c>LINE</c> type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Line value</param>
    [GeneratePgBindMethod]
    public static partial void Bind(this IBindable bindable, PgLine value);
    
    /// <summary>
    /// Bind a <see cref="PgLine"/> value. This maps to the Postgres specific <c>LINE</c> type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Line value</param>
    [GeneratePgBindMethod]
    public static partial void Bind(this IBindable bindable, PgLine? value);
    
    /// <summary>
    /// Bind a <see cref="PgLineSegment"/> value. This maps to the Postgres specific <c>LSEG</c>
    /// type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Line segment value</param>
    [GeneratePgBindMethod]
    public static partial void Bind(this IBindable bindable, PgLineSegment value);
    
    /// <summary>
    /// Bind a <see cref="PgLineSegment"/> value. This maps to the Postgres specific <c>LSEG</c>
    /// type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Line segment value</param>
    [GeneratePgBindMethod]
    public static partial void Bind(this IBindable bindable, PgLineSegment? value);
    
    /// <summary>
    /// Bind a <see cref="PgBox"/> value. This maps to the Postgres specific <c>BOX</c> type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Box value</param>
    [GeneratePgBindMethod]
    public static partial void Bind(this IBindable bindable, PgBox value);
    
    /// <summary>
    /// Bind a <see cref="PgBox"/> value. This maps to the Postgres specific <c>BOX</c> type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Box value</param>
    [GeneratePgBindMethod]
    public static partial void Bind(this IBindable bindable, PgBox? value);
    
    /// <summary>
    /// Bind a <see cref="PgPath"/> value. This maps to the Postgres specific <c>PATH</c> type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Path value</param>
    [GeneratePgBindMethod]
    public static partial void Bind(this IBindable bindable, PgPath value);
    
    /// <summary>
    /// Bind a <see cref="PgPath"/> value. This maps to the Postgres specific <c>PATH</c> type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Path value</param>
    [GeneratePgBindMethod]
    public static partial void Bind(this IBindable bindable, PgPath? value);
    
    /// <summary>
    /// Bind a <see cref="PgPolygon"/> value. This maps to the Postgres specific <c>POLYGON</c>
    /// type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Polygon value</param>
    [GeneratePgBindMethod]
    public static partial void Bind(this IBindable bindable, PgPolygon value);
    
    /// <summary>
    /// Bind a <see cref="PgPolygon"/> value. This maps to the Postgres specific <c>POLYGON</c>
    /// type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Polygon value</param>
    [GeneratePgBindMethod]
    public static partial void Bind(this IBindable bindable, PgPolygon? value);
    
    /// <summary>
    /// Bind a <see cref="PgCircle"/> value. This maps to the Postgres specific <c>CIRCLE</c> type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Circle value</param>
    /// <returns>This query instance for method chaining</returns>
    [GeneratePgBindMethod]
    public static partial void Bind(this IBindable bindable, PgCircle value);
    
    /// <summary>
    /// Bind a <see cref="PgCircle"/> value. This maps to the Postgres specific <c>CIRCLE</c> type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Circle value</param>
    [GeneratePgBindMethod]
    public static partial void Bind(this IBindable bindable, PgCircle? value);
    
    /// <summary>
    /// Bind a <see cref="PgInterval"/> value. This maps to the Postgres specific <c>INTERVAL</c>
    /// type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Interval value</param>
    [GeneratePgBindMethod]
    public static partial void Bind(this IBindable bindable, PgInterval value);
    
    /// <summary>
    /// Bind a <see cref="PgInterval"/> value. This maps to the Postgres specific <c>INTERVAL</c>
    /// type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Interval value</param>
    [GeneratePgBindMethod]
    public static partial void Bind(this IBindable bindable, PgInterval? value);
    
    /// <summary>
    /// Bind a <see cref="PgMacAddress"/> value. This maps to the Postgres specific
    /// <c>MACADDRESS</c> type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">MAC Address value</param>
    [GeneratePgBindMethod]
    public static partial void Bind(this IBindable bindable, PgMacAddress value);
    
    /// <summary>
    /// Bind a <see cref="PgMacAddress"/> value. This maps to the Postgres specific
    /// <c>MACADDRESS</c> type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">MAC Address value</param>
    [GeneratePgBindMethod]
    public static partial void Bind(this IBindable bindable, PgMacAddress? value);
    
    /// <summary>
    /// Bind a <see cref="PgMacAddress8"/> value. This maps to the Postgres specific
    /// <c>MACADDRESS8</c> type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">MAC Address value</param>
    [GeneratePgBindMethod]
    public static partial void Bind(this IBindable bindable, PgMacAddress8 value);
    
    /// <summary>
    /// Bind a <see cref="PgMacAddress8"/> value. This maps to the Postgres specific
    /// <c>MACADDRESS8</c> type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">MAC Address value</param>
    [GeneratePgBindMethod]
    public static partial void Bind(this IBindable bindable, PgMacAddress8? value);
    
    /// <summary>
    /// Bind a <see cref="PgMoney"/> value. This maps to the Postgres specific <c>MONEY</c> type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Money value</param>
    [GeneratePgBindMethod]
    public static partial void Bind(this IBindable bindable, PgMoney value);
    
    /// <summary>
    /// Bind a <see cref="PgMoney"/> value. This maps to the Postgres specific <c>MONEY</c> type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Money value</param>
    [GeneratePgBindMethod]
    public static partial void Bind(this IBindable bindable, PgMoney? value);
    
    /// <summary>
    /// Bind a <see cref="PgInet"/> value. This maps to the Postgres specific <c>INET</c> and
    /// <c>CIDR</c> types.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Network address value</param>
    [GeneratePgBindMethod]
    public static partial void Bind(this IBindable bindable, PgInet value);
    
    /// <summary>
    /// Bind a <see cref="PgInet"/> value. This maps to the Postgres specific <c>INET</c> and
    /// <c>CIDR</c> types.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Network address value</param>
    [GeneratePgBindMethod]
    public static partial void Bind(this IBindable bindable, PgInet? value);
    
    /// <summary>
    /// Bind an <see cref="IPNetwork"/> value. This maps to the Postgres specific <c>INET</c> and
    /// <c>CIDR</c> types.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Network address value</param>
    [GeneratePgBindMethod(Encoder = typeof(PgIpNetwork))]
    public static partial void Bind(this IBindable bindable, IPNetwork value);
    
    /// <summary>
    /// Bind an <see cref="IPNetwork"/> value. This maps to the Postgres specific <c>INET</c> and
    /// <c>CIDR</c> types.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Network address value</param>
    [GeneratePgBindMethod(Encoder = typeof(PgIpNetwork))]
    public static partial void Bind(this IBindable bindable, IPNetwork? value);
    
    /// <summary>
    /// Bind a <see cref="BitArray"/> value. This maps to the Postgres specific <c>VARBIT(n)</c> and
    /// <c>BIT(n)</c> types.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Network address value</param>
    [GeneratePgBindMethod(Encoder = typeof(PgBitString))]
    public static partial void Bind(this IBindable bindable, BitArray? value);
    
    /// <summary>
    /// Bind a <see cref="PgRange{T}"/> of <see cref="long"/> value. This maps to the Postgres
    /// specific <c>INT8RANGE</c> type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Long range value</param>
    [GeneratePgBindMethod(Encoder = typeof(PgRangeType<long, PgLong>))]
    public static partial void Bind(this IBindable bindable, PgRange<long>? value);
    
    /// <summary>
    /// Bind a <see cref="PgRange{T}"/> of <see cref="int"/> value. This maps to the Postgres
    /// specific <c>INT4RANGE</c> type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Int range value</param>
    [GeneratePgBindMethod(Encoder = typeof(PgRangeType<int, PgInt>))]
    public static partial void Bind(this IBindable bindable, PgRange<int>? value);
    
    /// <summary>
    /// Bind a <see cref="PgRange{T}"/> of <see cref="DateOnly"/> value. This maps to the Postgres
    /// specific <c>DATERANGE</c> type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Date range value</param>
    [GeneratePgBindMethod(Encoder = typeof(PgRangeType<DateOnly, PgDate>))]
    public static partial void Bind(this IBindable bindable, PgRange<DateOnly>? value);
    
    /// <summary>
    /// Bind a <see cref="PgRange{T}"/> of <see cref="DateTime"/> value. This maps to the Postgres
    /// specific <c>TSRANGE</c> type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Datetime range value</param>
    [GeneratePgBindMethod(Encoder = typeof(PgRangeType<DateTime, PgDateTime>))]
    public static partial void Bind(this IBindable bindable, PgRange<DateTime>? value);
    
    /// <summary>
    /// Bind a <see cref="PgRange{T}"/> of <see cref="DateTimeOffset"/> value. This maps to the
    /// Postgres specific <c>TSTZRANGE</c> type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Datetime offset range value</param>
    [GeneratePgBindMethod(Encoder = typeof(PgRangeType<DateTimeOffset, PgDateTimeOffset>))]
    public static partial void Bind(this IBindable bindable, PgRange<DateTimeOffset>? value);
    
    /// <summary>
    /// Bind a <see cref="PgRange{T}"/> of <see cref="decimal"/> value. This maps to the Postgres
    /// specific <c>NUMRANGE</c> type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Decimal range value</param>
    [GeneratePgBindMethod(Encoder = typeof(PgRangeType<decimal, PgDecimal>))]
    public static partial void Bind(this IBindable bindable, PgRange<decimal>? value);
    
    /// <summary>
    /// Bind a <see cref="bool"/> array value. This maps to the Postgres specific <c>BOOLEAN[]</c>
    /// types.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Boolean array value</param>
    [GeneratePgBindMethod(Encoder = typeof(PgBool))]
    public static partial void Bind(this IBindable bindable, bool?[]? value);
    
    /// <summary>
    /// Bind a <see cref="short"/> array value. This maps to the Postgres specific <c>SMALLINT[]</c>
    /// type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Short array value</param>
    [GeneratePgBindMethod(Encoder = typeof(PgShort))]
    public static partial void Bind(this IBindable bindable, short?[]? value);
    
    /// <summary>
    /// Bind a <see cref="int"/> array value. This maps to the Postgres specific <c>INT[]</c> type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Int array value</param>
    [GeneratePgBindMethod(Encoder = typeof(PgInt))]
    public static partial void Bind(this IBindable bindable, int?[]? value);
    
    /// <summary>
    /// Bind a <see cref="long"/> array value. This maps to the Postgres specific <c>BIGINT[]</c>
    /// type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Long array value</param>
    [GeneratePgBindMethod(Encoder = typeof(PgLong))]
    public static partial void Bind(this IBindable bindable, long?[]? value);
    
    /// <summary>
    /// Bind a <see cref="float"/> array value. This maps to the Postgres specific <c>REAL[]</c>
    /// type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Float array value</param>
    [GeneratePgBindMethod(Encoder = typeof(PgFloat))]
    public static partial void Bind(this IBindable bindable, float?[]? value);
    
    /// <summary>
    /// Bind a <see cref="double"/> array value. This maps to the Postgres specific
    /// <c>DOUBLE PRECISION[]</c> type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Double array value</param>
    [GeneratePgBindMethod(Encoder = typeof(PgDouble))]
    public static partial void Bind(this IBindable bindable, double?[]? value);
    
    /// <summary>
    /// Bind a <see cref="TimeOnly"/> array value. This maps to the Postgres specific <c>TIME[]</c>
    /// type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Time array value</param>
    [GeneratePgBindMethod(Encoder = typeof(PgTime))]
    public static partial void Bind(this IBindable bindable, TimeOnly?[]? value);
    
    /// <summary>
    /// Bind a <see cref="DateOnly"/> array value. This maps to the Postgres specific <c>DATE[]</c>
    /// type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Date array value</param>
    [GeneratePgBindMethod(Encoder = typeof(PgDate))]
    public static partial void Bind(this IBindable bindable, DateOnly?[]? value);
    
    /// <summary>
    /// Bind a <see cref="DateTime"/> array value. This maps to the Postgres specific
    /// <c>TIMESTAMP[]</c> type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Datetime array value</param>
    [GeneratePgBindMethod(Encoder = typeof(PgDateTime))]
    public static partial void Bind(this IBindable bindable, DateTime?[]? value);
    
    /// <summary>
    /// Bind a <see cref="DateTimeOffset"/> array value. This maps to the Postgres specific
    /// <c>TIMESTAMP WITH TIME ZONE[]</c> type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Datetime offset array value</param>
    [GeneratePgBindMethod(Encoder = typeof(PgDateTimeOffset))]
    public static partial void Bind(this IBindable bindable, DateTimeOffset?[]? value);
    
    /// <summary>
    /// Bind a <see cref="decimal"/> array value. This maps to the Postgres specific
    /// <c>DECIMAL[]</c> type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Decimal array value</param>
    [GeneratePgBindMethod(Encoder = typeof(PgDecimal))]
    public static partial void Bind(this IBindable bindable, decimal?[]? value);
    
    /// <summary>
    /// Bind an array of <see cref="byte"/> arrays value. This maps to the Postgres specific
    /// <c>BYTEA[]</c> type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Byte array value</param>
    [GeneratePgBindMethod(Encoder = typeof(PgBytea))]
    public static partial void Bind(this IBindable bindable, byte[]?[]? value);
    
    /// <summary>
    /// Bind a <see cref="string"/> array value. This maps to the Postgres specific
    /// <c>TEXT[]</c> type (and it's compatible array types).
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">String array value</param>
    [GeneratePgBindMethod(Encoder = typeof(PgString))]
    public static partial void Bind(this IBindable bindable, string?[]? value);
    
    /// <summary>
    /// Bind a <see cref="Guid"/> array value. This maps to the Postgres specific <c>UUID[]</c>
    /// type.
    /// </summary>
    /// <param name="bindable">Object to bind against</param>
    /// <param name="value">Guid array value</param>
    [GeneratePgBindMethod(Encoder = typeof(PgUuid))]
    public static partial void Bind(this IBindable bindable, Guid?[]? value);

    extension(IBindable bindable)
    {
        /// <summary>
        /// Bind <typeparamref name="TType"/> parameter to query. This allows for any value that can
        /// be encoded using the type definition of <typeparamref name="TType"/> to be bound.
        /// </summary>
        /// <param name="value">Value to bind</param>
        /// <typeparam name="TType">DB Type definition to allow for encoding the value</typeparam>
        public void BindPg<TType>(TType value)
            where TType : IPgDbType<TType>
        {
            PgException.CheckIfIs<IBindable, IPgBindable>(bindable)
                .BindPg<TType, TType>(value);
        }

        /// <summary>
        /// <para>
        /// Bind <typeparamref name="TElement"/> array parameter to query. This puts that value as the
        /// nth parameter in the parameterized query, where n is the current parameter as a 1-based
        /// index. This allows for any array value that can be encoded using the type definition of
        /// <typeparamref name="TType"/> to be bound to a query.
        /// </para>
        /// <para>
        /// This differs from <see cref="Bindable.BindPgValArray{TElement,TType}"/> because the element type must be a class
        /// so that nullable vs default semantics can be handled correctly.
        /// </para>
        /// </summary>
        /// <param name="value">Array value to bind</param>
        /// <typeparam name="TElement">Array element type</typeparam>
        /// <typeparam name="TType">DB Type definition to allow for encoding the value</typeparam>
        /// <returns>This query instance for method chaining</returns>
        public void BindPgRefArray<TElement, TType>(TElement?[]? value)
            where TType : IPgDbType<TElement>, IHasArrayType
            where TElement : class
        {
            PgException.CheckIfIs<IBindable, IPgBindable>(bindable)
                .BindPgNullableClass<TElement?[], PgArrayTypeClass<TElement, TType>>(value);
        }
        
        /// <summary>
        /// <para>
        /// Bind <typeparamref name="TElement"/> array parameter to query. This puts that value as the
        /// nth parameter in the parameterized query, where n is the current parameter as a 1-based
        /// index. This allows for any array value that can be encoded using the type definition of
        /// <typeparamref name="TType"/> to be bound to a query.
        /// </para>
        /// <para>
        /// This differs from <see cref="Bindable.BindPgRefArray{TElement,TType}"/> because the element type must be a struct
        /// so that nullable vs default semantics can be handled correctly.
        /// </para>
        /// </summary>
        /// <param name="value">Array value to bind</param>
        /// <typeparam name="TElement">Array element type</typeparam>
        /// <typeparam name="TType">DB Type definition to allow for encoding the value</typeparam>
        /// <returns>This query instance for method chaining</returns>
        public void BindPgValArray<TElement, TType>(TElement?[]? value)
            where TType : IPgDbType<TElement>, IHasArrayType
            where TElement : struct
        {
            PgException.CheckIfIs<IBindable, IPgBindable>(bindable)
                .BindPgNullableClass<TElement?[], PgArrayTypeStruct<TElement, TType>>(value);
        }
    }
}
