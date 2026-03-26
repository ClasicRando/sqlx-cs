using System.Collections;
using System.Net;
using System.Runtime.CompilerServices;
using Sqlx.Postgres.Generator.Result;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Result;

public static partial class DataRowExtensions
{
    extension(IPgDataRow pgDataRow)
    {
        /// <summary>
        /// Extract a value of type <typeparamref name="TValue"/> from this row using the type
        /// definition of <typeparamref name="TType"/>.
        /// </summary>
        /// <param name="name">column name to extract</param>
        /// <typeparam name="TValue">Return type</typeparam>
        /// <typeparam name="TType">Type definition</typeparam>
        /// <returns><typeparamref name="TValue"/> value at the specified column</returns>
        /// <exception cref="Sqlx.Core.Exceptions.SqlxException">
        /// if the column value is null or the column cannot be decoded as
        /// <typeparamref name="TType"/>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TValue GetPgNotNull<TValue, TType>(string name)
            where TType : IPgDbType<TValue>
            where TValue : notnull
        {
            return pgDataRow.GetPgNotNull<TValue, TType>(pgDataRow.IndexOf(name));
        }
        
        /// <summary>
        /// Extract a value of type <typeparamref name="TValue"/> from this row using the type
        /// definition of <typeparamref name="TType"/>. Allows for null return values and is
        /// specific to reference types. For value based types see
        /// <see cref="GetPgVal{TValue,TType}(IPgDataRow, int)"/>
        /// </summary>
        /// <param name="index">0-based index of the column to extract</param>
        /// <typeparam name="TValue">Return type</typeparam>
        /// <typeparam name="TType">Type definition</typeparam>
        /// <returns>
        /// <typeparamref name="TValue"/> value at the specified column or null if the DB value is
        /// null
        /// </returns>
        /// <exception cref="Sqlx.Core.Exceptions.SqlxException">
        /// if the column cannot be decoded as <typeparamref name="TType"/>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TValue? GetPgRef<TValue, TType>(int index)
            where TType : IPgDbType<TValue>
            where TValue : class
        {
            return pgDataRow.IsNull(index) ? null : pgDataRow.GetPgNotNull<TValue, TType>(index);
        }
        
        /// <summary>
        /// Extract a value of type <typeparamref name="TValue"/> from this row using the type
        /// definition of <typeparamref name="TType"/>. Allows for null return values and is
        /// specific to reference types. For value based types see
        /// <see cref="GetPgVal{TValue,TType}(IPgDataRow, string)"/>
        /// </summary>
        /// <param name="name">column name to extract</param>
        /// <typeparam name="TValue">Return type</typeparam>
        /// <typeparam name="TType">Type definition</typeparam>
        /// <returns>
        /// <typeparamref name="TValue"/> value at the specified column or null if the DB value is
        /// null
        /// </returns>
        /// <exception cref="Sqlx.Core.Exceptions.SqlxException">
        /// if the column cannot be decoded as <typeparamref name="TType"/>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TValue? GetPgRef<TValue, TType>(string name)
            where TType : IPgDbType<TValue>
            where TValue : class
        {
            return pgDataRow.GetPgRef<TValue, TType>(pgDataRow.IndexOf(name));
        }
        
        /// <summary>
        /// Extract a value of type <typeparamref name="TValue"/> from this row using the type
        /// definition of <typeparamref name="TType"/>. Allows for null return values and is
        /// specific to value types. For reference based types see
        /// <see cref="GetPgRef{TValue,TType}(IPgDataRow, int)"/>
        /// </summary>
        /// <param name="index">0-based index of the column to extract</param>
        /// <typeparam name="TValue">Return type</typeparam>
        /// <typeparam name="TType">Type definition</typeparam>
        /// <returns>
        /// <typeparamref name="TValue"/> value at the specified column or null if the DB value is
        /// null
        /// </returns>
        /// <exception cref="Sqlx.Core.Exceptions.SqlxException">
        /// if the column cannot be decoded as <typeparamref name="TType"/>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TValue? GetPgVal<TValue, TType>(int index)
            where TType : IPgDbType<TValue>
            where TValue : struct
        {
            return pgDataRow.IsNull(index) ? null : pgDataRow.GetPgNotNull<TValue, TType>(index);
        }
        
        /// <summary>
        /// Extract a value of type <typeparamref name="TValue"/> from this row using the type
        /// definition of <typeparamref name="TType"/>. Allows for null return values and is
        /// specific to value types. For reference based types see
        /// <see cref="GetPgRef{TValue,TType}(IPgDataRow, string)"/>
        /// </summary>
        /// <param name="name">column name to extract</param>
        /// <typeparam name="TValue">Return type</typeparam>
        /// <typeparam name="TType">Type definition</typeparam>
        /// <returns>
        /// <typeparamref name="TValue"/> value at the specified column or null if the DB value is
        /// null
        /// </returns>
        /// <exception cref="Sqlx.Core.Exceptions.SqlxException">
        /// if the column cannot be decoded as <typeparamref name="TType"/>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TValue? GetPgVal<TValue, TType>(string name)
            where TType : IPgDbType<TValue>
            where TValue : struct
        {
            return pgDataRow.GetPgVal<TValue, TType>(pgDataRow.IndexOf(name));
        }
        
        /// <summary>
        /// Extract a value of type <typeparamref name="TType"/> from this row
        /// </summary>
        /// <param name="index">0-based index of the column to extract</param>
        /// <typeparam name="TType">Type definition</typeparam>
        /// <returns><typeparamref name="TType"/> value at the specified column</returns>
        /// <exception cref="Sqlx.Core.Exceptions.SqlxException">
        /// if the column value is null or the column cannot be decoded as
        /// <typeparamref name="TType"/>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TType GetPgNotNull<TType>(int index)
            where TType : IPgDbType<TType>
        {
            return pgDataRow.GetPgNotNull<TType, TType>(index);
        }
        
        /// <summary>
        /// Extract a value of type <typeparamref name="TType"/> from this row.
        /// </summary>
        /// <param name="name">column name to extract</param>
        /// <typeparam name="TType">Type definition</typeparam>
        /// <returns><typeparamref name="TType"/> value at the specified column</returns>
        /// <exception cref="Sqlx.Core.Exceptions.SqlxException">
        /// if the column value is null or the column cannot be decoded as
        /// <typeparamref name="TType"/>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TType GetPgNotNull<TType>(string name)
            where TType : IPgDbType<TType>
        {
            return pgDataRow.GetPgNotNull<TType, TType>(name);
        }
        
        /// <summary>
        /// Extract a value of type <typeparamref name="TType"/> from this row. Allows for null
        /// return values and is specific to reference types. For value based types see
        /// <see cref="GetPgVal{TType}(IPgDataRow, int)"/>
        /// </summary>
        /// <param name="index">0-based index of the column to extract</param>
        /// <typeparam name="TType">Type definition</typeparam>
        /// <returns>
        /// <typeparamref name="TType"/> value at the specified column or null if the DB value is
        /// null
        /// </returns>
        /// <exception cref="Sqlx.Core.Exceptions.SqlxException">
        /// if the column cannot be decoded as <typeparamref name="TType"/>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TType? GetPgRef<TType>(int index)
            where TType : class, IPgDbType<TType>
        {
            return pgDataRow.GetPgRef<TType, TType>(index);
        }
        
        /// <summary>
        /// Extract a value of type <typeparamref name="TType"/> from this row. Allows for null
        /// return values and is specific to reference types. For value based types see
        /// <see cref="GetPgVal{TType}(IPgDataRow, string)"/>
        /// </summary>
        /// <param name="name">column name to extract</param>
        /// <typeparam name="TType">Type definition</typeparam>
        /// <returns>
        /// <typeparamref name="TType"/> value at the specified column or null if the DB value is
        /// null
        /// </returns>
        /// <exception cref="Sqlx.Core.Exceptions.SqlxException">
        /// if the column cannot be decoded as <typeparamref name="TType"/>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TType? GetPgRef<TType>(string name)
            where TType : class, IPgDbType<TType>
        {
            return pgDataRow.GetPgRef<TType, TType>(name);
        }
        
        /// <summary>
        /// Extract a value of type <typeparamref name="TType"/> from this row. Allows for null
        /// return values and is specific to value types. For reference based types see
        /// <see cref="GetPgRef{TValue,TType}(IPgDataRow, int)"/>
        /// </summary>
        /// <param name="index">0-based index of the column to extract</param>
        /// <typeparam name="TType">Type definition</typeparam>
        /// <returns>
        /// <typeparamref name="TType"/> value at the specified column or null if the DB value is
        /// null
        /// </returns>
        /// <exception cref="Sqlx.Core.Exceptions.SqlxException">
        /// if the column cannot be decoded as <typeparamref name="TType"/>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TType? GetPgVal<TType>(int index)
            where TType : struct, IPgDbType<TType>
        {
            return pgDataRow.GetPgVal<TType, TType>(index);
        }
        
        /// <summary>
        /// Extract a value of type <typeparamref name="TType"/> from this row. Allows for null
        /// return values and is specific to value types. For reference based types see
        /// <see cref="GetPgRef{TValue,TType}(IPgDataRow, string)"/>
        /// </summary>
        /// <param name="name">column name to extract</param>
        /// <typeparam name="TType">Type definition</typeparam>
        /// <returns>
        /// <typeparamref name="TType"/> value at the specified column or null if the DB value is
        /// null
        /// </returns>
        /// <exception cref="Sqlx.Core.Exceptions.SqlxException">
        /// if the column cannot be decoded as <typeparamref name="TType"/>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TType? GetPgVal<TType>(string name)
            where TType : struct, IPgDbType<TType>
        {
            return pgDataRow.GetPgVal<TType, TType>(name);
        }
        
        /// <summary>
        /// Extract an array value of type <typeparamref name="TType"/> from this row. Array
        /// elements type must be reference type. For value type elements see
        /// <see cref="GetPgValArrayNotNull{TType}(IPgDataRow,int)"/>.
        /// </summary>
        /// <param name="index">0-based index of the column to extract</param>
        /// <typeparam name="TType">Type definition</typeparam>
        /// <returns>
        /// <typeparamref name="TType"/> array of nullable elements at the specified column
        /// </returns>
        /// <exception cref="Sqlx.Core.Exceptions.SqlxException">
        /// if the column cannot be decoded as <typeparamref name="TType"/>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TType?[] GetPgRefArrayNotNull<TType>(int index)
            where TType : class, IPgDbType<TType>, IHasArrayType
        {
            return pgDataRow.GetPgNotNull<TType?[], PgArrayTypeClass<TType, TType>>(index);
        }
        
        /// <summary>
        /// Extract an array value of type <typeparamref name="TType"/> from this row. Array
        /// elements must be reference types. For value based types see
        /// <see cref="GetPgValArrayNotNull{TType}(IPgDataRow, string)"/>
        /// </summary>
        /// <param name="name">column name to extract</param>
        /// <typeparam name="TType">Type definition</typeparam>
        /// <returns>
        /// <typeparamref name="TType"/> array of nullable elements at the specified column
        /// </returns>
        /// <exception cref="Sqlx.Core.Exceptions.SqlxException">
        /// if the column cannot be decoded as <typeparamref name="TType"/>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TType?[] GetPgRefArrayNotNull<TType>(string name)
            where TType : class, IPgDbType<TType>, IHasArrayType
        {
            return pgDataRow.GetPgRefArrayNotNull<TType>(pgDataRow.IndexOf(name));
        }
        
        /// <summary>
        /// Extract an array value of type <typeparamref name="TType"/> from this row. Array
        /// elements must be value types. For reference based types see
        /// <see cref="GetPgRefArrayNotNull{TType}(IPgDataRow, int)"/>
        /// </summary>
        /// <param name="index">0-based index of the column to extract</param>
        /// <typeparam name="TType">Type definition</typeparam>
        /// <returns>
        /// <typeparamref name="TType"/> array of nullable elements at the specified column
        /// </returns>
        /// <exception cref="Sqlx.Core.Exceptions.SqlxException">
        /// if the column cannot be decoded as <typeparamref name="TType"/>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TType?[] GetPgValArrayNotNull<TType>(int index)
            where TType : struct, IPgDbType<TType>, IHasArrayType
        {
            return pgDataRow.GetPgNotNull<TType?[], PgArrayTypeStruct<TType, TType>>(index);
        }
        
        /// <summary>
        /// Extract an array value of type <typeparamref name="TType"/> from this row. Array
        /// elements must be value types. For reference based types see
        /// <see cref="GetPgRefArrayNotNull{TType}(IPgDataRow, string)"/>
        /// </summary>
        /// <param name="name">column name to extract</param>
        /// <typeparam name="TType">Type definition</typeparam>
        /// <returns>
        /// <typeparamref name="TType"/> array of nullable elements at the specified column
        /// </returns>
        /// <exception cref="Sqlx.Core.Exceptions.SqlxException">
        /// if the column cannot be decoded as <typeparamref name="TType"/>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TType?[] GetPgValArrayNotNull<TType>(string name)
            where TType : struct, IPgDbType<TType>, IHasArrayType
        {
            return pgDataRow.GetPgValArrayNotNull<TType>(pgDataRow.IndexOf(name));
        }
        
        /// <summary>
        /// Extract an array value of type <typeparamref name="TType"/> from this row. Allows for
        /// null return values and the array's element type must be a reference type. For value type
        /// elements see
        /// <see cref="GetPgValArray{TType}(IPgDataRow,int)"/>.
        /// </summary>
        /// <param name="index">0-based index of the column to extract</param>
        /// <typeparam name="TType">Type definition</typeparam>
        /// <returns>
        /// <typeparamref name="TType"/> array of nullable elements at the specified column or null
        /// if the DB value is null
        /// </returns>
        /// <exception cref="Sqlx.Core.Exceptions.SqlxException">
        /// if the column cannot be decoded as <typeparamref name="TType"/>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TType?[]? GetPgRefArray<TType>(int index)
            where TType : class, IPgDbType<TType>, IHasArrayType
        {
            return pgDataRow.GetPgRef<TType?[], PgArrayTypeClass<TType, TType>>(index);
        }
        
        /// <summary>
        /// Extract an array value of type <typeparamref name="TType"/> from this row. Allows for
        /// null return values and the array's element type must be a reference type. For value type
        /// types see <see cref="GetPgValArray{TType}(IPgDataRow, string)"/>
        /// </summary>
        /// <param name="name">column name to extract</param>
        /// <typeparam name="TType">Type definition</typeparam>
        /// <returns>
        /// <typeparamref name="TType"/> array of nullable elements at the specified column or null
        /// if the DB value is null
        /// </returns>
        /// <exception cref="Sqlx.Core.Exceptions.SqlxException">
        /// if the column cannot be decoded as <typeparamref name="TType"/>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TType?[]? GetPgRefArray<TType>(string name)
            where TType : class, IPgDbType<TType>, IHasArrayType
        {
            return pgDataRow.GetPgRefArray<TType>(pgDataRow.IndexOf(name));
        }
        
        /// <summary>
        /// Extract an array value of type <typeparamref name="TType"/> from this row. Allows for
        /// null return values and the array's element type must be a value type. For reference
        /// based types see <see cref="GetPgRefArray{TType}(IPgDataRow, int)"/>
        /// </summary>
        /// <param name="index">0-based index of the column to extract</param>
        /// <typeparam name="TType">Type definition</typeparam>
        /// <returns>
        /// <typeparamref name="TType"/> array of nullable elements at the specified column or null
        /// if the DB value is null
        /// </returns>
        /// <exception cref="Sqlx.Core.Exceptions.SqlxException">
        /// if the column cannot be decoded as <typeparamref name="TType"/>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TType?[]? GetPgValArray<TType>(int index)
            where TType : struct, IPgDbType<TType>, IHasArrayType
        {
            return pgDataRow.GetPgRef<TType?[], PgArrayTypeStruct<TType, TType>>(index);
        }
        
        /// <summary>
        /// Extract an array value of type <typeparamref name="TType"/> from this row. Allows for
        /// null return values and the array's element type must be a value type. For reference
        /// based types see <see cref="GetPgRefArray{TType}(IPgDataRow, string)"/>
        /// </summary>
        /// <param name="name">column name to extract</param>
        /// <typeparam name="TType">Type definition</typeparam>
        /// <returns>
        /// <typeparamref name="TType"/> array of nullable elements at the specified column or null
        /// if the DB value is null
        /// </returns>
        /// <exception cref="Sqlx.Core.Exceptions.SqlxException">
        /// if the column cannot be decoded as <typeparamref name="TType"/>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TType?[]? GetPgValArray<TType>(string name)
            where TType : struct, IPgDbType<TType>, IHasArrayType
        {
            return pgDataRow.GetPgValArray<TType>(pgDataRow.IndexOf(name));
        }
    }
    
    [GeneratePgDecodeMethod(Decoder = typeof(PgIpNetwork))]
    public static partial IPNetwork? GetIpNetwork(this IPgDataRow dataRow, int index);
    
    [GeneratePgDecodeMethod(Decoder = typeof(PgIpNetwork))]
    public static partial IPNetwork GetIpNetworkNotNull(this IPgDataRow dataRow, int index);
    
    [GeneratePgDecodeMethod(Decoder = typeof(PgIpNetwork))]
    public static partial IPNetwork? GetIpNetwork(this IPgDataRow dataRow, string name);
    
    [GeneratePgDecodeMethod(Decoder = typeof(PgIpNetwork))]
    public static partial IPNetwork GetIpNetworkNotNull(this IPgDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgBitString))]
    public static partial BitArray? GetBitArray(this IPgDataRow dataRow, int index);
    
    [GeneratePgDecodeMethod(Decoder = typeof(PgBitString))]
    public static partial BitArray GetBitArrayNotNull(this IPgDataRow dataRow, int index);
    
    [GeneratePgDecodeMethod(Decoder = typeof(PgBitString))]
    public static partial BitArray? GetBitArray(this IPgDataRow dataRow, string name);
    
    [GeneratePgDecodeMethod(Decoder = typeof(PgBitString))]
    public static partial BitArray GetBitArrayNotNull(this IPgDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<long, PgLong>))]
    public static partial PgRange<long>? GetPgRangeLong(this IPgDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<long, PgLong>))]
    public static partial PgRange<long> GetPgRangeLongNotNull(this IPgDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<long, PgLong>))]
    public static partial PgRange<long>? GetPgRangeLong(this IPgDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<long, PgLong>))]
    public static partial PgRange<long> GetPgRangeLongNotNull(this IPgDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<int, PgInt>))]
    public static partial PgRange<int>? GetPgRangeInt(this IPgDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<int, PgInt>))]
    public static partial PgRange<int> GetPgRangeIntNotNull(this IPgDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<int, PgInt>))]
    public static partial PgRange<int>? GetPgRangeInt(this IPgDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<int, PgInt>))]
    public static partial PgRange<int> GetPgRangeIntNotNull(this IPgDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<DateOnly, PgDate>))]
    public static partial PgRange<DateOnly>? GetPgRangeDate(this IPgDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<DateOnly, PgDate>))]
    public static partial PgRange<DateOnly> GetPgRangeDateNotNull(this IPgDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<DateOnly, PgDate>))]
    public static partial PgRange<DateOnly>? GetPgRangeDate(this IPgDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<DateOnly, PgDate>))]
    public static partial PgRange<DateOnly> GetPgRangeDateNotNull(this IPgDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<DateTime, PgDateTime>))]
    public static partial PgRange<DateTime>? GetPgRangeDateTime(this IPgDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<DateTime, PgDateTime>))]
    public static partial PgRange<DateTime> GetPgRangeDateTimeNotNull(this IPgDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<DateTime, PgDateTime>))]
    public static partial PgRange<DateTime>? GetPgRangeDateTime(this IPgDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<DateTime, PgDateTime>))]
    public static partial PgRange<DateTime> GetPgRangeDateTimeNotNull(this IPgDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<DateTimeOffset, PgDateTimeOffset>))]
    public static partial PgRange<DateTimeOffset>? GetPgRangeDateTimeOffset(this IPgDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<DateTimeOffset, PgDateTimeOffset>))]
    public static partial PgRange<DateTimeOffset> GetPgRangeDateTimeOffsetNotNull(this IPgDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<DateTimeOffset, PgDateTimeOffset>))]
    public static partial PgRange<DateTimeOffset>? GetPgRangeDateTimeOffset(this IPgDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<DateTimeOffset, PgDateTimeOffset>))]
    public static partial PgRange<DateTimeOffset> GetPgRangeDateTimeOffsetNotNull(this IPgDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<decimal, PgDecimal>))]
    public static partial PgRange<decimal>? GetPgRangeDecimal(this IPgDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<decimal, PgDecimal>))]
    public static partial PgRange<decimal> GetPgRangeDecimalNotNull(this IPgDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<decimal, PgDecimal>))]
    public static partial PgRange<decimal>? GetPgRangeDecimal(this IPgDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<decimal, PgDecimal>))]
    public static partial PgRange<decimal> GetPgRangeDecimalNotNull(this IPgDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgBool))]
    public static partial bool?[]? GetBooleanArray(this IPgDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgBool))]
    public static partial bool?[] GetBooleanArrayNotNull(this IPgDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgBool))]
    public static partial bool?[]? GetBooleanArray(this IPgDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgBool))]
    public static partial bool?[] GetBooleanArrayNotNull(this IPgDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgChar))]
    public static partial sbyte?[]? GetByteArray(this IPgDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgChar))]
    public static partial sbyte?[] GetByteArrayNotNull(this IPgDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgChar))]
    public static partial sbyte?[]? GetByteArray(this IPgDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgChar))]
    public static partial sbyte?[] GetByteArrayNotNull(this IPgDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgShort))]
    public static partial short?[]? GetShortArray(this IPgDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgShort))]
    public static partial short?[] GetShortArrayNotNull(this IPgDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgShort))]
    public static partial short?[]? GetShortArray(this IPgDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgShort))]
    public static partial short?[] GetShortArrayNotNull(this IPgDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgInt))]
    public static partial int?[]? GetIntArray(this IPgDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgInt))]
    public static partial int?[] GetIntArrayNotNull(this IPgDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgInt))]
    public static partial int?[]? GetIntArray(this IPgDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgInt))]
    public static partial int?[] GetIntArrayNotNull(this IPgDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgLong))]
    public static partial long?[]? GetLongArray(this IPgDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgLong))]
    public static partial long?[] GetLongArrayNotNull(this IPgDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgLong))]
    public static partial long?[]? GetLongArray(this IPgDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgLong))]
    public static partial long?[] GetLongArrayNotNull(this IPgDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgFloat))]
    public static partial float?[]? GetFloatArray(this IPgDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgFloat))]
    public static partial float?[] GetFloatArrayNotNull(this IPgDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgFloat))]
    public static partial float?[]? GetFloatArray(this IPgDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgFloat))]
    public static partial float?[] GetFloatArrayNotNull(this IPgDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgDouble))]
    public static partial double?[]? GetDoubleArray(this IPgDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgDouble))]
    public static partial double?[] GetDoubleArrayNotNull(this IPgDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgDouble))]
    public static partial double?[]? GetDoubleArray(this IPgDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgDouble))]
    public static partial double?[] GetDoubleArrayNotNull(this IPgDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgTime))]
    public static partial TimeOnly?[]? GetTimeArray(this IPgDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgTime))]
    public static partial TimeOnly?[] GetTimeArrayNotNull(this IPgDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgTime))]
    public static partial TimeOnly?[]? GetTimeArray(this IPgDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgTime))]
    public static partial TimeOnly?[] GetTimeArrayNotNull(this IPgDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgDate))]
    public static partial DateOnly?[]? GetDateArray(this IPgDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgDate))]
    public static partial DateOnly?[] GetDateArrayNotNull(this IPgDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgDate))]
    public static partial DateOnly?[]? GetDateArray(this IPgDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgDate))]
    public static partial DateOnly?[] GetDateArrayNotNull(this IPgDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgDateTime))]
    public static partial DateTime?[]? GetDateTimeArray(this IPgDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgDateTime))]
    public static partial DateTime?[] GetDateTimeArrayNotNull(this IPgDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgDateTime))]
    public static partial DateTime?[]? GetDateTimeArray(this IPgDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgDateTime))]
    public static partial DateTime?[] GetDateTimeArrayNotNull(this IPgDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgDateTimeOffset))]
    public static partial DateTimeOffset?[]? GetDateTimeOffsetArray(this IPgDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgDateTimeOffset))]
    public static partial DateTimeOffset?[] GetDateTimeOffsetArrayNotNull(this IPgDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgDateTimeOffset))]
    public static partial DateTimeOffset?[]? GetDateTimeOffsetArray(this IPgDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgDateTimeOffset))]
    public static partial DateTimeOffset?[] GetDateTimeOffsetArrayNotNull(this IPgDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgDecimal))]
    public static partial decimal?[]? GetDecimalArray(this IPgDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgDecimal))]
    public static partial decimal?[] GetDecimalArrayNotNull(this IPgDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgDecimal))]
    public static partial decimal?[]? GetDecimalArray(this IPgDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgDecimal))]
    public static partial decimal?[] GetDecimalArrayNotNull(this IPgDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgBytea))]
    public static partial byte[]?[]? GetBytesArray(this IPgDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgBytea))]
    public static partial byte[]?[] GetBytesArrayNotNull(this IPgDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgBytea))]
    public static partial byte[]?[]? GetBytesArray(this IPgDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgBytea))]
    public static partial byte[]?[] GetBytesArrayNotNull(this IPgDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgString))]
    public static partial string?[]? GetStringArray(this IPgDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgString))]
    public static partial string?[] GetStringArrayNotNull(this IPgDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgString))]
    public static partial string?[]? GetStringArray(this IPgDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgString))]
    public static partial string?[] GetStringArrayNotNull(this IPgDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgUuid))]
    public static partial Guid?[]? GetGuidArray(this IPgDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgUuid))]
    public static partial Guid?[] GetGuidArrayNotNull(this IPgDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgUuid))]
    public static partial Guid?[]? GetGuidArray(this IPgDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgUuid))]
    public static partial Guid?[] GetGuidArrayNotNull(this IPgDataRow dataRow, string name);
    
    [GeneratePgDecodeMethod(Decoder = typeof(PgIpNetwork))]
    public static partial IPNetwork?[]? GetIpNetworkArray(this IPgDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgIpNetwork))]
    public static partial IPNetwork?[] GetIpNetworkArrayNotNull(this IPgDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgIpNetwork))]
    public static partial IPNetwork?[]? GetIpNetworkArray(this IPgDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgIpNetwork))]
    public static partial IPNetwork?[] GetIpNetworkArrayNotNull(this IPgDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgBitString))]
    public static partial BitArray?[]? GetBitArrayArray(this IPgDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgBitString))]
    public static partial BitArray?[] GetBitArrayArrayNotNull(this IPgDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgBitString))]
    public static partial BitArray?[]? GetBitArrayArray(this IPgDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgBitString))]
    public static partial BitArray?[] GetBitArrayArrayNotNull(this IPgDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<long, PgLong>))]
    public static partial PgRange<long>?[]? GetPgRangeLongArray(this IPgDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<long, PgLong>))]
    public static partial PgRange<long>?[] GetPgRangeLongArrayNotNull(this IPgDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<long, PgLong>))]
    public static partial PgRange<long>?[]? GetPgRangeLongArray(this IPgDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<long, PgLong>))]
    public static partial PgRange<long>?[] GetPgRangeLongArrayNotNull(this IPgDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<int, PgInt>))]
    public static partial PgRange<int>?[]? GetPgRangeIntArray(this IPgDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<int, PgInt>))]
    public static partial PgRange<int>?[] GetPgRangeIntArrayNotNull(this IPgDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<int, PgInt>))]
    public static partial PgRange<int>?[]? GetPgRangeIntArray(this IPgDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<int, PgInt>))]
    public static partial PgRange<int>?[] GetPgRangeIntArrayNotNull(this IPgDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<DateOnly, PgDate>))]
    public static partial PgRange<DateOnly>?[]? GetPgRangeDateArray(this IPgDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<DateOnly, PgDate>))]
    public static partial PgRange<DateOnly>?[] GetPgRangeDateArrayNotNull(this IPgDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<DateOnly, PgDate>))]
    public static partial PgRange<DateOnly>?[]? GetPgRangeDateArray(this IPgDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<DateOnly, PgDate>))]
    public static partial PgRange<DateOnly>?[] GetPgRangeDateArrayNotNull(this IPgDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<DateTime, PgDateTime>))]
    public static partial PgRange<DateTime>?[]? GetPgRangeDateTimeArray(this IPgDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<DateTime, PgDateTime>))]
    public static partial PgRange<DateTime>?[] GetPgRangeDateTimeArrayNotNull(this IPgDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<DateTime, PgDateTime>))]
    public static partial PgRange<DateTime>?[]? GetPgRangeDateTimeArray(this IPgDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<DateTime, PgDateTime>))]
    public static partial PgRange<DateTime>?[] GetPgRangeDateTimeArrayNotNull(this IPgDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<DateTimeOffset, PgDateTimeOffset>))]
    public static partial PgRange<DateTimeOffset>?[]? GetPgRangeDateTimeOffsetArray(this IPgDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<DateTimeOffset, PgDateTimeOffset>))]
    public static partial PgRange<DateTimeOffset>?[] GetPgRangeDateTimeOffsetArrayNotNull(this IPgDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<DateTimeOffset, PgDateTimeOffset>))]
    public static partial PgRange<DateTimeOffset>?[]? GetPgRangeDateTimeOffsetArray(this IPgDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<DateTimeOffset, PgDateTimeOffset>))]
    public static partial PgRange<DateTimeOffset>?[] GetPgRangeDateTimeOffsetArrayNotNull(this IPgDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<decimal, PgDecimal>))]
    public static partial PgRange<decimal>?[]? GetPgRangeDecimalArray(this IPgDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<decimal, PgDecimal>))]
    public static partial PgRange<decimal>?[] GetPgRangeDecimalArrayNotNull(this IPgDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<decimal, PgDecimal>))]
    public static partial PgRange<decimal>?[]? GetPgRangeDecimalArray(this IPgDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<decimal, PgDecimal>))]
    public static partial PgRange<decimal>?[] GetPgRangeDecimalArrayNotNull(this IPgDataRow dataRow, string name);
}
