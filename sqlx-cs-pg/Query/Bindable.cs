using System.Collections;
using System.Net;
using System.Runtime.CompilerServices;
using Sqlx.Core.Query;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Query;

/// <summary>
/// Extensions class for Postgres specific binding to an <see cref="IBindable"/> instance. These
/// extension methods are included when you include the Postgres module and assume your
/// <see cref="IBindable"/> instance is a <see cref="IPgBindable"/>.
/// </summary>
public static class Bindable
{
    extension(IPgBindable bindable)
    {
        /// <summary>
        /// Bind <typeparamref name="TType"/> parameter to query. This allows for any value that can
        /// be encoded using the type definition of <typeparamref name="TType"/> to be bound.
        /// </summary>
        /// <param name="value">Value to bind</param>
        /// <typeparam name="TType">DB Type definition to allow for encoding the value</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Bind<TType>(TType value)
            where TType : IPgDbType<TType>
        {
            bindable.Bind<TType, TType>(value);
        }

        /// <summary>
        /// Bind a nullable value type. This defers to <see cref="IBindable.BindNull"/> when the
        /// value is null and otherwise calls <see cref="IPgBindable.Bind{TValue,TType}"/>.
        /// </summary>
        /// <param name="value">Value to bind</param>
        /// <typeparam name="TValue">Type of the value</typeparam>
        /// <typeparam name="TType">Type used to encode the value</typeparam>
        public void BindVal<TValue, TType>(TValue? value)
            where TType : IPgDbType<TValue>
            where TValue : struct
        {
            if (!value.HasValue)
            {
                bindable.BindNull<TType>();
            }
            else
            {
                bindable.Bind<TValue, TType>(value.Value);
            }
        }

        /// <summary>
        /// Bind a nullable ref type. This defers to <see cref="IBindable.BindNull"/> when the value
        /// is null and otherwise calls <see cref="IPgBindable.Bind{TValue,TType}"/>.
        /// </summary>
        /// <param name="value">Value to bind</param>
        /// <typeparam name="TValue">Type of the value</typeparam>
        /// <typeparam name="TType">Type used to encode the value</typeparam>
        public void BindRef<TValue, TType>(TValue? value)
            where TType : IPgDbType<TValue>
            where TValue : class
        {
            if (value is null)
            {
                bindable.BindNull<TType>();
            }
            else
            {
                bindable.Bind<TValue, TType>(value);
            }
        }

        /// <summary>
        /// <para>
        /// Bind a <typeparamref name="TElement"/> array value. This allows for any array value that
        /// can be encoded using the type definition of <typeparamref name="TType"/> to be bound to
        /// a query.
        /// </para>
        /// <para>
        /// This differs from <see cref="Bindable.BindValArray{TElement,TType}"/> because the
        /// element type must be a class so that nullable vs default semantics can be handled
        /// correctly.
        /// </para>
        /// </summary>
        /// <param name="value">Array value to bind</param>
        /// <typeparam name="TElement">Array element type</typeparam>
        /// <typeparam name="TType">DB Type definition to allow for encoding the value</typeparam>
        /// <returns>This query instance for method chaining</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void BindRefArray<TElement, TType>(TElement?[]? value)
            where TType : IPgDbType<TElement>, IHasArrayType
            where TElement : class
        {
            bindable.BindRef<TElement?[], PgArrayTypeClass<TElement, TType>>(value);
        }

        /// <summary>
        /// <para>
        /// Bind a <typeparamref name="TElement"/> array value. This allows for any array value that
        /// can be encoded using the type definition of <typeparamref name="TType"/> to be bound to
        /// a query.
        /// </para>
        /// <para>
        /// This differs from <see cref="Bindable.BindRefArray{TElement,TType}"/> because the
        /// element type must be a struct so that nullable vs default semantics can be handled
        /// correctly.
        /// </para>
        /// </summary>
        /// <param name="value">Array value to bind</param>
        /// <typeparam name="TElement">Array element type</typeparam>
        /// <typeparam name="TType">DB Type definition to allow for encoding the value</typeparam>
        /// <returns>This query instance for method chaining</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void BindValArray<TElement, TType>(TElement?[]? value)
            where TType : IPgDbType<TElement>, IHasArrayType
            where TElement : struct
        {
            bindable.BindRef<TElement?[], PgArrayTypeStruct<TElement, TType>>(value);
        }

        /// <summary>
        /// Bind an <see cref="IPNetwork"/> value. This maps to the Postgres specific <c>INET</c>
        /// and <c>CIDR</c> types.
        /// </summary>
        /// <param name="value">Network address value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Bind(IPNetwork value)
        {
            bindable.Bind<IPNetwork, PgIpNetwork>(value);
        }

        /// <summary>
        /// Bind an <see cref="IPNetwork"/> value. This maps to the Postgres specific <c>INET</c>
        /// and <c>CIDR</c> types.
        /// </summary>
        /// <param name="value">Network address value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Bind(IPNetwork? value)
        {
            bindable.BindVal<IPNetwork, PgIpNetwork>(value);
        }

        /// <summary>
        /// Bind a <see cref="BitArray"/> value. This maps to the Postgres specific <c>VARBIT(n)</c>
        /// and <c>BIT(n)</c> types.
        /// </summary>
        /// <param name="value">Network address value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Bind(BitArray? value)
        {
            bindable.BindRef<BitArray, PgBitString>(value);
        }

        /// <summary>
        /// Bind a <see cref="PgRange{T}"/> of <see cref="long"/> value. This maps to the Postgres
        /// specific <c>INT8RANGE</c> type.
        /// </summary>
        /// <param name="value">Long range value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Bind(PgRange<long>? value)
        {
            bindable.BindRef<PgRange<long>, PgRangeType<long, PgLong>>(value);
        }

        /// <summary>
        /// Bind a <see cref="PgRange{T}"/> of <see cref="int"/> value. This maps to the Postgres
        /// specific <c>INT4RANGE</c> type.
        /// </summary>
        /// <param name="value">Int range value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Bind(PgRange<int>? value)
        {
            bindable.BindRef<PgRange<int>, PgRangeType<int, PgInt>>(value);
        }

        /// <summary>
        /// Bind a <see cref="PgRange{T}"/> of <see cref="DateOnly"/> value. This maps to the
        /// Postgres specific <c>DATERANGE</c> type.
        /// </summary>
        /// <param name="value">Date range value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Bind(PgRange<DateOnly>? value)
        {
            bindable.BindRef<PgRange<DateOnly>, PgRangeType<DateOnly, PgDate>>(value);
        }

        /// <summary>
        /// Bind a <see cref="PgRange{T}"/> of <see cref="DateTime"/> value. This maps to the
        /// Postgres specific <c>TSRANGE</c> type.
        /// </summary>
        /// <param name="value">Datetime range value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Bind(PgRange<DateTime>? value)
        {
            bindable.BindRef<PgRange<DateTime>, PgRangeType<DateTime, PgDateTime>>(value);
        }

        /// <summary>
        /// Bind a <see cref="PgRange{T}"/> of <see cref="DateTimeOffset"/> value. This maps to the
        /// Postgres specific <c>TSTZRANGE</c> type.
        /// </summary>
        /// <param name="value">Datetime offset range value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Bind(PgRange<DateTimeOffset>? value)
        {
            bindable
                .BindRef<PgRange<DateTimeOffset>, PgRangeType<DateTimeOffset, PgDateTimeOffset>>(
                    value);
        }

        /// <summary>
        /// Bind a <see cref="PgRange{T}"/> of <see cref="decimal"/> value. This maps to the
        /// Postgres specific <c>NUMRANGE</c> type.
        /// </summary>
        /// <param name="value">Decimal range value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Bind(PgRange<decimal>? value)
        {
            bindable.BindRef<PgRange<decimal>, PgRangeType<decimal, PgDecimal>>(value);
        }

        /// <summary>
        /// Bind a <see cref="bool"/> array value. This maps to the Postgres specific
        /// <c>BOOLEAN[]</c> types.
        /// </summary>
        /// <param name="value">Boolean array value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Bind(bool?[]? value)
        {
            bindable.BindValArray<bool, PgBool>(value);
        }

        /// <summary>
        /// Bind a <see cref="short"/> array value. This maps to the Postgres specific
        /// <c>SMALLINT[]</c> type.
        /// </summary>
        /// <param name="value">Short array value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Bind(short?[]? value)
        {
            bindable.BindValArray<short, PgShort>(value);
        }

        /// <summary>
        /// Bind a <see cref="int"/> array value. This maps to the Postgres specific
        /// <c>INT[]</c> type.
        /// </summary>
        /// <param name="value">Int array value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Bind(int?[]? value)
        {
            bindable.BindValArray<int, PgInt>(value);
        }

        /// <summary>
        /// Bind a <see cref="long"/> array value. This maps to the Postgres specific
        /// <c>BIGINT[]</c> type.
        /// </summary>
        /// <param name="value">Long array value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Bind(long?[]? value)
        {
            bindable.BindValArray<long, PgLong>(value);
        }

        /// <summary>
        /// Bind a <see cref="float"/> array value. This maps to the Postgres specific
        /// <c>REAL[]</c> type.
        /// </summary>
        /// <param name="value">Float array value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Bind(float?[]? value)
        {
            bindable.BindValArray<float, PgFloat>(value);
        }

        /// <summary>
        /// Bind a <see cref="double"/> array value. This maps to the Postgres specific
        /// <c>DOUBLE PRECISION[]</c> type.
        /// </summary>
        /// <param name="value">Double array value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Bind(double?[]? value)
        {
            bindable.BindValArray<double, PgDouble>(value);
        }

        /// <summary>
        /// Bind a <see cref="TimeOnly"/> array value. This maps to the Postgres specific
        /// <c>TIME[]</c> type.
        /// </summary>
        /// <param name="value">Time array value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Bind(TimeOnly?[]? value)
        {
            bindable.BindValArray<TimeOnly, PgTime>(value);
        }

        /// <summary>
        /// Bind a <see cref="DateOnly"/> array value. This maps to the Postgres specific
        /// <c>DATE[]</c> type.
        /// </summary>
        /// <param name="value">Date array value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Bind(DateOnly?[]? value)
        {
            bindable.BindValArray<DateOnly, PgDate>(value);
        }

        /// <summary>
        /// Bind a <see cref="DateTime"/> array value. This maps to the Postgres specific
        /// <c>TIMESTAMP[]</c> type.
        /// </summary>
        /// <param name="value">Datetime array value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Bind(DateTime?[]? value)
        {
            bindable.BindValArray<DateTime, PgDateTime>(value);
        }

        /// <summary>
        /// Bind a <see cref="DateTimeOffset"/> array value. This maps to the Postgres specific
        /// <c>TIMESTAMP WITH TIME ZONE[]</c> type.
        /// </summary>
        /// <param name="value">Datetime offset array value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Bind(DateTimeOffset?[]? value)
        {
            bindable.BindValArray<DateTimeOffset, PgDateTimeOffset>(value);
        }

        /// <summary>
        /// Bind a <see cref="decimal"/> array value. This maps to the Postgres specific
        /// <c>DECIMAL[]</c> type.
        /// </summary>
        /// <param name="value">Decimal array value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Bind(decimal?[]? value)
        {
            bindable.BindValArray<decimal, PgDecimal>(value);
        }

        /// <summary>
        /// Bind an array of <see cref="byte"/> arrays value. This maps to the Postgres specific
        /// <c>BYTEA[]</c> type.
        /// </summary>
        /// <param name="value">Byte array value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Bind(byte[]?[]? value)
        {
            bindable.BindRefArray<byte[], PgBytea>(value);
        }

        /// <summary>
        /// Bind a <see cref="string"/> array value. This maps to the Postgres specific
        /// <c>TEXT[]</c> type (and it's compatible array types).
        /// </summary>
        /// <param name="value">String array value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Bind(string?[]? value)
        {
            bindable.BindRefArray<string, PgString>(value);
        }

        /// <summary>
        /// Bind a <see cref="Guid"/> array value. This maps to the Postgres specific
        /// <c>UUID[]</c> type.
        /// </summary>
        /// <param name="value">Guid array value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Bind(Guid?[]? value)
        {
            bindable.BindValArray<Guid, PgUuid>(value);
        }
    }
}
