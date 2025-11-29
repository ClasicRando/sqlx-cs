using System.Runtime.CompilerServices;
using System.Text.Json.Serialization.Metadata;

namespace Sqlx.Core.Result;

/// <summary>
/// Database query result row that allows for decoding columns into specific types. This interface
/// just defines extraction of standard SQL types and database specific types can be found as
/// extension methods on this interface. Since nullability can be annotated on return types, there
/// are 2 methods per type where the simple method allows for extracting database nulls as
/// <c>null</c> and the other method defines a non-null result where database nulls initiate a
/// thrown exception.
/// </summary>
public interface IDataRow
{
    /// <param name="name">Column name to check</param>
    /// <returns>
    /// The 0-based index of the column name specified or -1 if the name does not exist
    /// </returns>
    int IndexOf(string name);

    /// <param name="index">0-based index of the column to check</param>
    /// <returns>True if the value found at the index is a DB null</returns>
    bool IsNull(int index);

    /// <summary>
    /// Extract a not-null <see cref="bool"/> value. The <c>BOOLEAN</c> type is not consistent
    /// across all databases so the driver specific implementation might vary.
    /// </summary>
    /// <param name="index">0-based index of the column to extract</param>
    /// <returns>Boolean value at the specified column</returns>
    /// <exception cref="Sqlx.Core.Exceptions.SqlxException">if the column value is null</exception>
    bool GetBooleanNotNull(int index);

    /// <summary>
    /// Extract a not-null <see cref="sbyte"/> value. This maps to the <c>TINYINT</c> type but can
    /// vary between database implementations since not all use <c>TINYINT</c>
    /// </summary>
    /// <param name="index">0-based index of the column to extract</param>
    /// <returns>Sbyte value at the specified column</returns>
    /// <exception cref="Sqlx.Core.Exceptions.SqlxException">if the column value is null</exception>
    sbyte GetByteNotNull(int index);

    /// <summary>
    /// Extract a not-null <see cref="short"/> value. This maps to the <c>SMALLINT</c> type but
    /// drivers can coerce larger values if the actual value fits into a <see cref="short"/>.
    /// </summary>
    /// <param name="index">0-based index of the column to extract</param>
    /// <returns>Short value at the specified column</returns>
    /// <exception cref="Sqlx.Core.Exceptions.SqlxException">if the column value is null</exception>
    short GetShortNotNull(int index);

    /// <summary>
    /// Extract a not-null <see cref="int"/> value. This maps to the <c>INTEGER</c> type but drivers
    /// can coerce larger values if the actual value fits into a <see cref="int"/>.
    /// </summary>
    /// <param name="index">0-based index of the column to extract</param>
    /// <returns>Int value at the specified column</returns>
    /// <exception cref="Sqlx.Core.Exceptions.SqlxException">if the column value is null</exception>
    int GetIntNotNull(int index);

    /// <summary>
    /// Extract a not-null <see cref="long"/> value. This maps to the <c>BIGINT</c> type.
    /// </summary>
    /// <param name="index">0-based index of the column to extract</param>
    /// <returns>Long value at the specified column</returns>
    /// <exception cref="Sqlx.Core.Exceptions.SqlxException">if the column value is null</exception>
    long GetLongNotNull(int index);

    /// <summary>
    /// Extract a not-null <see cref="float"/> value. This maps to the <c>REAL</c> type but drivers
    /// can coerce larger <see cref="double"/> values if the actual value fits into a
    /// <see cref="float"/>.
    /// </summary>
    /// <param name="index">0-based index of the column to extract</param>
    /// <returns>Float value at the specified column</returns>
    /// <exception cref="Sqlx.Core.Exceptions.SqlxException">if the column value is null</exception>
    float GetFloatNotNull(int index);

    /// <summary>
    /// Extract a not-null <see cref="double"/> value. This maps to the <c>DOUBLE PRECISION</c>
    /// type.
    /// </summary>
    /// <param name="index">0-based index of the column to extract</param>
    /// <returns>Double value at the specified column</returns>
    /// <exception cref="Sqlx.Core.Exceptions.SqlxException">if the column value is null</exception>
    double GetDoubleNotNull(int index);

    /// <summary>
    /// Extract a not-null <see cref="TimeOnly"/> value. This maps to the <c>TIME</c> type.
    /// </summary>
    /// <param name="index">0-based index of the column to extract</param>
    /// <returns>Time value at the specified column</returns>
    /// <exception cref="Sqlx.Core.Exceptions.SqlxException">if the column value is null</exception>
    TimeOnly GetTimeNotNull(int index);

    /// <summary>
    /// Extract a not-null <see cref="DateOnly"/> value. This maps to the <c>DATE</c> type.
    /// </summary>
    /// <param name="index">0-based index of the column to extract</param>
    /// <returns>Date value at the specified column</returns>
    /// <exception cref="Sqlx.Core.Exceptions.SqlxException">if the column value is null</exception>
    DateOnly GetDateNotNull(int index);

    /// <summary>
    /// Extract a not-null <see cref="DateTime"/> value. This maps to the <c>TIMESTAMP</c> type.
    /// </summary>
    /// <param name="index">0-based index of the column to extract</param>
    /// <returns>DateTime value at the specified column</returns>
    /// <exception cref="Sqlx.Core.Exceptions.SqlxException">if the column value is null</exception>
    DateTime GetDateTimeNotNull(int index);

    /// <summary>
    /// Extract a not-null <see cref="DateTimeOffset"/> value. This maps to the
    /// <c>TIMESTAMP WITH TIME ZONE</c> type.
    /// </summary>
    /// <param name="index">0-based index of the column to extract</param>
    /// <returns>DateTimeOffset value at the specified column</returns>
    /// <exception cref="Sqlx.Core.Exceptions.SqlxException">if the column value is null</exception>
    DateTimeOffset GetDateTimeOffsetNotNull(int index);

    /// <summary>
    /// Extract a not-null <see cref="decimal"/> value. This maps to the <c>DECIMAL</c> type.
    /// </summary>
    /// <param name="index">0-based index of the column to extract</param>
    /// <returns>Decimal value at the specified column</returns>
    /// <exception cref="Sqlx.Core.Exceptions.SqlxException">if the column value is null</exception>
    decimal GetDecimalNotNull(int index);

    /// <summary>
    /// Extract a not-null <c>byte[]</c> value. This maps to the <c>VARBINARY</c>/<c>BLOB</c> type.
    /// </summary>
    /// <param name="index">0-based index of the column to extract</param>
    /// <returns>Bytes value at the specified column</returns>
    /// <exception cref="Sqlx.Core.Exceptions.SqlxException">if the column value is null</exception>
    byte[] GetBytesNotNull(int index);

    /// <summary>
    /// Extract a not-null <see cref="string"/> value. This maps to the
    /// <c>VARCHAR</c>/<c>TEXT</c>/<c>CLOB</c> type.
    /// </summary>
    /// <param name="index">0-based index of the column to extract</param>
    /// <returns>String value at the specified column</returns>
    /// <exception cref="Sqlx.Core.Exceptions.SqlxException">if the column value is null</exception>
    string GetStringNotNull(int index);

    /// <summary>
    /// Extract a not-null <see cref="Guid"/> value. The <c>UUID</c>/<c>UNIQUEIDENTIFIER</c> type is
    /// not consistent across all databases so the driver specific implementation might vary.
    /// Generally it's either a built-in type or this method tries to parse 
    /// </summary>
    /// <param name="index">0-based index of the column to extract</param>
    /// <returns>Guid value at the specified column</returns>
    /// <exception cref="Sqlx.Core.Exceptions.SqlxException">if the column value is null</exception>
    Guid GetGuidNotNull(int index);

    /// <summary>
    /// Extract a not-null <typeparamref name="T"/> value after deserializing the inner value
    /// as JSON. Some databases have a JSON specific field type but other database drivers will
    /// treat string or byte[] fields as JSON compatible. When using this method, it's recommended
    /// to supply the <see cref="JsonTypeInfo{T}"/> parameter to aid deserialization.
    /// </summary>
    /// <param name="index">0-based index of the column to extract</param>
    /// <param name="jsonTypeInfo">optional JSON source generated deserialization metadata</param>
    /// <returns><c>T</c> value at the specified column</returns>
    /// <exception cref="Sqlx.Core.Exceptions.SqlxException">if the column value is null</exception>
    T GetJsonNotNull<T>(int index, JsonTypeInfo<T>? jsonTypeInfo = null) where T : notnull;
}

public static class DataRowExtensions
{
    extension(IDataRow dataRow)
    {
        /// <summary>
        /// Extract a possibly null <see cref="bool"/> value. The <c>BOOLEAN</c> type is not consistent
        /// across all databases so the driver specific implementation might vary.
        /// </summary>
        /// <param name="index">0-based index of the column to extract</param>
        /// <returns>Boolean value at the specified column or null if the DB value was null</returns>
        public bool? GetBoolean(int index)
        {
            if (dataRow.IsNull(index))
            {
                return null;
            }
            return dataRow.GetBooleanNotNull(index);
        }

        /// <summary>
        /// Extract a possibly null <see cref="sbyte"/> value. This maps to the <c>TINYINT</c> type but
        /// can vary between database implementations since not all use <c>TINYINT</c>
        /// </summary>
        /// <param name="index">0-based index of the column to extract</param>
        /// <returns>Sbyte value at the specified column or null if the DB value was null</returns>
        public sbyte? GetByte(int index)
        {
            if (dataRow.IsNull(index))
            {
                return null;
            }
            return dataRow.GetByteNotNull(index);
        }

        /// <summary>
        /// Extract a possibly null <see cref="short"/> value. This maps to the <c>SMALLINT</c> type but
        /// drivers can coerce larger values if the actual value fits into a <see cref="short"/>.
        /// </summary>
        /// <param name="index">0-based index of the column to extract</param>
        /// <returns>Short value at the specified column or null if the DB value was null</returns>
        public short? GetShort(int index)
        {
            if (dataRow.IsNull(index))
            {
                return null;
            }
            return dataRow.GetShortNotNull(index);
        }

        /// <summary>
        /// Extract a possibly null <see cref="int"/> value. This maps to the <c>INTEGER</c> type but
        /// drivers can coerce larger values if the actual value fits into a <see cref="int"/>.
        /// </summary>
        /// <param name="index">0-based index of the column to extract</param>
        /// <returns>Int value at the specified column or null if the DB value was null</returns>
        public int? GetInt(int index)
        {
            if (dataRow.IsNull(index))
            {
                return null;
            }
            return dataRow.GetIntNotNull(index);
        }

        /// <summary>
        /// Extract a possibly null <see cref="long"/> value. This maps to the <c>BIGINT</c> type.
        /// </summary>
        /// <param name="index">0-based index of the column to extract</param>
        /// <returns>Long value at the specified column or null if the DB value was null</returns>
        public long? GetLong(int index)
        {
            if (dataRow.IsNull(index))
            {
                return null;
            }
            return dataRow.GetLongNotNull(index);
        }

        /// <summary>
        /// Extract a possibly null <see cref="float"/> value. This maps to the <c>REAL</c> type but
        /// drivers can coerce larger <see cref="double"/> values if the actual value fits into a
        /// <see cref="float"/>.
        /// </summary>
        /// <param name="index">0-based index of the column to extract</param>
        /// <returns>Float value at the specified column or null if the DB value was null</returns>
        public float? GetFloat(int index)
        {
            if (dataRow.IsNull(index))
            {
                return null;
            }
            return dataRow.GetFloatNotNull(index);
        }

        /// <summary>
        /// Extract a possibly null <see cref="double"/> value. This maps to the <c>DOUBLE PRECISION</c>
        /// type.
        /// </summary>
        /// <param name="index">0-based index of the column to extract</param>
        /// <returns>Double value at the specified column or null if the DB value was null</returns>
        public double? GetDouble(int index)
        {
            if (dataRow.IsNull(index))
            {
                return null;
            }
            return dataRow.GetDoubleNotNull(index);
        }

        /// <summary>
        /// Extract a possibly null <see cref="TimeOnly"/> value. This maps to the <c>TIME</c> type.
        /// </summary>
        /// <param name="index">0-based index of the column to extract</param>
        /// <returns>Time value at the specified column or null if the DB value was null</returns>
        public TimeOnly? GetTime(int index)
        {
            if (dataRow.IsNull(index))
            {
                return null;
            }
            return dataRow.GetTimeNotNull(index);
        }

        /// <summary>
        /// Extract a possibly null <see cref="DateOnly"/> value. This maps to the <c>DATE</c> type.
        /// </summary>
        /// <param name="index">0-based index of the column to extract</param>
        /// <returns>Date value at the specified column or null if the DB value was null</returns>
        public DateOnly? GetDate(int index)
        {
            if (dataRow.IsNull(index))
            {
                return null;
            }
            return dataRow.GetDateNotNull(index);
        }

        /// <summary>
        /// Extract a possibly null <see cref="DateTime"/> value. This maps to the <c>TIMESTAMP</c>
        /// type.
        /// </summary>
        /// <param name="index">0-based index of the column to extract</param>
        /// <returns>DateTime value at the specified column or null if the DB value was null</returns>
        public DateTime? GetDateTime(int index)
        {
            if (dataRow.IsNull(index))
            {
                return null;
            }
            return dataRow.GetDateTimeNotNull(index);
        }

        /// <summary>
        /// Extract a possibly null <see cref="DateTimeOffset"/> value. This maps to the
        /// <c>TIMESTAMP WITH TIME ZONE</c> type.
        /// </summary>
        /// <param name="index">0-based index of the column to extract</param>
        /// <returns>
        /// DateTimeOffset value at the specified column or null if the DB value was null
        /// </returns>
        public DateTimeOffset? GetDateTimeOffset(int index)
        {
            if (dataRow.IsNull(index))
            {
                return null;
            }
            return dataRow.GetDateTimeOffsetNotNull(index);
        }

        /// <summary>
        /// Extract a possibly null <see cref="decimal"/> value. This maps to the <c>DECIMAL</c> type.
        /// </summary>
        /// <param name="index">0-based index of the column to extract</param>
        /// <returns>Decimal value at the specified column or null if the DB value was null</returns>
        public decimal? GetDecimal(int index)
        {
            if (dataRow.IsNull(index))
            {
                return null;
            }
            return dataRow.GetDecimalNotNull(index);
        }

        /// <summary>
        /// Extract a possibly null <c>byte[]</c> value. This maps to the
        /// <c>VARBINARY</c>/<c>BLOB</c> type.
        /// </summary>
        /// <param name="index">0-based index of the column to extract</param>
        /// <returns>Bytes value at the specified column or null if the DB value was null</returns>
        public byte[]? GetBytes(int index)
        {
            if (dataRow.IsNull(index))
            {
                return null;
            }
            return dataRow.GetBytesNotNull(index);
        }

        /// <summary>
        /// Extract a possibly null <see cref="string"/> value. This maps to the
        /// <c>VARCHAR</c>/<c>TEXT</c>/<c>CLOB</c> type.
        /// </summary>
        /// <param name="index">0-based index of the column to extract</param>
        /// <returns>String value at the specified column or null if the DB value was null</returns>
        public string? GetString(int index)
        {
            if (dataRow.IsNull(index))
            {
                return null;
            }
            return dataRow.GetStringNotNull(index);
        }

        /// <summary>
        /// Extract a possibly null <see cref="Guid"/> value. The <c>UUID</c>/<c>UNIQUEIDENTIFIER</c>
        /// type is not consistent across all databases so the driver specific implementation might
        /// vary. Generally it's either a built-in type or this method tries to interpret bytes or a
        /// string as a <see cref="Guid"/>
        /// </summary>
        /// <param name="index">0-based index of the column to extract</param>
        /// <returns>Guid value at the specified column or null if the DB value was null</returns>
        public Guid? GetGuid(int index)
        {
            if (dataRow.IsNull(index))
            {
                return null;
            }
            return dataRow.GetGuidNotNull(index);
        }

        /// <summary>
        /// Extract a possibly null <typeparamref name="T"/> value after deserializing the inner value
        /// as JSON. Some databases have a JSON specific field type but other database drivers will
        /// treat string or byte[] fields as JSON compatible. When using this method, it's recommended
        /// to supply the <see cref="JsonTypeInfo{T}"/> parameter to aid deserialization.
        /// </summary>
        /// <param name="index">0-based index of the column to extract</param>
        /// <param name="jsonTypeInfo">optional JSON source generated deserialization metadata</param>
        /// <returns>
        /// <c>T</c> value at the specified column or default if the DB value was null
        /// </returns>
        public T? GetJson<T>(
            int index,
            JsonTypeInfo<T>? jsonTypeInfo = null)
            where T : notnull
        {
            return dataRow.IsNull(index) ? default : dataRow.GetJsonNotNull(index, jsonTypeInfo);
        }

        /// <summary>
        /// Extract a possibly null <see cref="bool"/> value. The <c>BOOLEAN</c> type is not consistent
        /// across all databases so the driver specific implementation might vary.
        /// </summary>
        /// <param name="name">name of the column to extract</param>
        /// <returns>Boolean value at the specified column or null if the DB value was null</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool? GetBoolean(string name)
        {
            return GetBoolean(dataRow, dataRow.IndexOf(name));
        }

        /// <summary>
        /// Extract a possibly null <see cref="sbyte"/> value. This maps to the <c>TINYINT</c> type but
        /// can vary between database implementations since not all use <c>TINYINT</c>
        /// </summary>
        /// <param name="name">name of the column to extract</param>
        /// <returns>Sbyte value at the specified column or null if the DB value was null</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sbyte? GetByte(string name)
        {
            return GetByte(dataRow, dataRow.IndexOf(name));
        }

        /// <summary>
        /// Extract a possibly null <see cref="short"/> value. This maps to the <c>SMALLINT</c> type but
        /// drivers can coerce larger values if the actual value fits into a <see cref="short"/>.
        /// </summary>
        /// <param name="name">name of the column to extract</param>
        /// <returns>Short value at the specified column or null if the DB value was null</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short? GetShort(string name)
        {
            return GetShort(dataRow, dataRow.IndexOf(name));
        }

        /// <summary>
        /// Extract a possibly null <see cref="int"/> value. This maps to the <c>INTEGER</c> type but
        /// drivers can coerce larger values if the actual value fits into a <see cref="int"/>.
        /// </summary>
        /// <param name="name">name of the column to extract</param>
        /// <returns>Int value at the specified column or null if the DB value was null</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int? GetInt(string name)
        {
            return GetInt(dataRow, dataRow.IndexOf(name));
        }

        /// <summary>
        /// Extract a possibly null <see cref="long"/> value. This maps to the <c>BIGINT</c> type.
        /// </summary>
        /// <param name="name">name of the column to extract</param>
        /// <returns>Long value at the specified column or null if the DB value was null</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long? GetLong(string name)
        {
            return GetLong(dataRow, dataRow.IndexOf(name));
        }

        /// <summary>
        /// Extract a possibly null <see cref="float"/> value. This maps to the <c>REAL</c> type but
        /// drivers can coerce larger <see cref="double"/> values if the actual value fits into a
        /// <see cref="float"/>.
        /// </summary>
        /// <param name="name">name of the column to extract</param>
        /// <returns>Float value at the specified column or null if the DB value was null</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float? GetFloat(string name)
        {
            return GetFloat(dataRow, dataRow.IndexOf(name));
        }

        /// <summary>
        /// Extract a possibly null <see cref="double"/> value. This maps to the <c>DOUBLE PRECISION</c>
        /// type.
        /// </summary>
        /// <param name="name">name of the column to extract</param>
        /// <returns>Double value at the specified column or null if the DB value was null</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double? GetDouble(string name)
        {
            return GetDouble(dataRow, dataRow.IndexOf(name));
        }

        /// <summary>
        /// Extract a possibly null <see cref="TimeOnly"/> value. This maps to the <c>TIME</c> type.
        /// </summary>
        /// <param name="name">name of the column to extract</param>
        /// <returns>Time value at the specified column or null if the DB value was null</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TimeOnly? GetTime(string name)
        {
            return GetTime(dataRow, dataRow.IndexOf(name));
        }

        /// <summary>
        /// Extract a possibly null <see cref="DateOnly"/> value. This maps to the <c>DATE</c> type.
        /// </summary>
        /// <param name="name">name of the column to extract</param>
        /// <returns>Date value at the specified column or null if the DB value was null</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DateOnly? GetDate(string name)
        {
            return GetDate(dataRow, dataRow.IndexOf(name));
        }

        /// <summary>
        /// Extract a possibly null <see cref="DateTime"/> value. This maps to the <c>TIMESTAMP</c>
        /// type.
        /// </summary>
        /// <param name="name">name of the column to extract</param>
        /// <returns>DateTime value at the specified column or null if the DB value was null</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DateTime? GetDateTime(string name)
        {
            return GetDateTime(dataRow, dataRow.IndexOf(name));
        }

        /// <summary>
        /// Extract a possibly null <see cref="DateTimeOffset"/> value. This maps to the
        /// <c>TIMESTAMP WITH TIME ZONE</c> type.
        /// </summary>
        /// <param name="name">name of the column to extract</param>
        /// <returns>
        /// DateTimeOffset value at the specified column or null if the DB value was null
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DateTimeOffset? GetDateTimeOffset(string name)
        {
            return GetDateTimeOffset(dataRow, dataRow.IndexOf(name));
        }

        /// <summary>
        /// Extract a possibly null <see cref="decimal"/> value. This maps to the <c>DECIMAL</c> type.
        /// </summary>
        /// <param name="name">name of the column to extract</param>
        /// <returns>Decimal value at the specified column or null if the DB value was null</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public decimal? GetDecimal(string name)
        {
            return GetDecimal(dataRow, dataRow.IndexOf(name));
        }

        /// <summary>
        /// Extract a possibly null <c>byte[]</c> value. This maps to the
        /// <c>VARBINARY</c>/<c>BLOB</c> type.
        /// </summary>
        /// <param name="name">name of the column to extract</param>
        /// <returns>Bytes value at the specified column or null if the DB value was null</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte[]? GetBytes(string name)
        {
            return GetBytes(dataRow, dataRow.IndexOf(name));
        }

        /// <summary>
        /// Extract a possibly null <see cref="string"/> value. This maps to the
        /// <c>VARCHAR</c>/<c>TEXT</c>/<c>CLOB</c> type.
        /// </summary>
        /// <param name="name">name of the column to extract</param>
        /// <returns>String value at the specified column or null if the DB value was null</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string? GetString(string name)
        {
            return GetString(dataRow, dataRow.IndexOf(name));
        }

        /// <summary>
        /// Extract a possibly null <see cref="Guid"/> value. The <c>UUID</c>/<c>UNIQUEIDENTIFIER</c>
        /// type is not consistent across all databases so the driver specific implementation might
        /// vary. Generally it's either a built-in type or this method tries to parse 
        /// </summary>
        /// <param name="name">name of the column to extract</param>
        /// <returns>Guid value at the specified column or null if the DB value was null</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Guid? GetGuid(string name)
        {
            return GetGuid(dataRow, dataRow.IndexOf(name));
        }

        /// <summary>
        /// Extract a possibly null <typeparamref name="T"/> value after deserializing the inner value
        /// as JSON. Some databases have a JSON specific field type but other database drivers will
        /// treat string or byte[] fields as JSON compatible. When using this method, it's recommended
        /// to supply the <see cref="JsonTypeInfo{T}"/> parameter to aid deserialization.
        /// </summary>
        /// <param name="name">name of the column to extract</param>
        /// <param name="jsonTypeInfo">optional JSON source generated deserialization metadata</param>
        /// <returns><c>T</c> value at the specified column or null if the DB value was null</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T? GetJson<T>(
            string name,
            JsonTypeInfo<T>? jsonTypeInfo = null)
            where T : notnull
        {
            return GetJson(dataRow, dataRow.IndexOf(name), jsonTypeInfo);
        }

        /// <summary>
        /// Extract a not-null <see cref="bool"/> value. The <c>BOOLEAN</c> type is not consistent
        /// across all databases so the driver specific implementation might vary.
        /// </summary>
        /// <param name="name">name of the column to extract</param>
        /// <returns>Boolean value at the specified column</returns>
        /// <exception cref="Sqlx.Core.Exceptions.SqlxException">if the column value is null</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool GetBooleanNotNull(string name)
        {
            return dataRow.GetBooleanNotNull(dataRow.IndexOf(name));
        }

        /// <summary>
        /// Extract a not-null <see cref="sbyte"/> value. This maps to the <c>TINYINT</c> type but can
        /// vary between database implementations since not all use <c>TINYINT</c>
        /// </summary>
        /// <param name="name">name of the column to extract</param>
        /// <returns>Sbyte value at the specified column</returns>
        /// <exception cref="Sqlx.Core.Exceptions.SqlxException">if the column value is null</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sbyte GetByteNotNull(string name)
        {
            return dataRow.GetByteNotNull(dataRow.IndexOf(name));
        }

        /// <summary>
        /// Extract a not-null <see cref="short"/> value. This maps to the <c>SMALLINT</c> type but
        /// drivers can coerce larger values if the actual value fits into a <see cref="short"/>.
        /// </summary>
        /// <param name="name">name of the column to extract</param>
        /// <returns>Short value at the specified column</returns>
        /// <exception cref="Sqlx.Core.Exceptions.SqlxException">if the column value is null</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short GetShortNotNull(string name)
        {
            return dataRow.GetShortNotNull(dataRow.IndexOf(name));
        }

        /// <summary>
        /// Extract a not-null <see cref="int"/> value. This maps to the <c>INTEGER</c> type but drivers
        /// can coerce larger values if the actual value fits into a <see cref="int"/>.
        /// </summary>
        /// <param name="name">name of the column to extract</param>
        /// <returns>Int value at the specified column</returns>
        /// <exception cref="Sqlx.Core.Exceptions.SqlxException">if the column value is null</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetIntNotNull(string name)
        {
            return dataRow.GetIntNotNull(dataRow.IndexOf(name));
        }

        /// <summary>
        /// Extract a not-null <see cref="long"/> value. This maps to the <c>BIGINT</c> type.
        /// </summary>
        /// <param name="name">name of the column to extract</param>
        /// <returns>Long value at the specified column</returns>
        /// <exception cref="Sqlx.Core.Exceptions.SqlxException">if the column value is null</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long GetLongNotNull(string name)
        {
            return dataRow.GetLongNotNull(dataRow.IndexOf(name));
        }

        /// <summary>
        /// Extract a not-null <see cref="float"/> value. This maps to the <c>REAL</c> type but drivers
        /// can coerce larger <see cref="double"/> values if the actual value fits into a
        /// <see cref="float"/>.
        /// </summary>
        /// <param name="name">name of the column to extract</param>
        /// <returns>Float value at the specified column</returns>
        /// <exception cref="Sqlx.Core.Exceptions.SqlxException">if the column value is null</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetFloatNotNull(string name)
        {
            return dataRow.GetFloatNotNull(dataRow.IndexOf(name));
        }

        /// <summary>
        /// Extract a not-null <see cref="double"/> value. This maps to the <c>DOUBLE PRECISION</c>
        /// type.
        /// </summary>
        /// <param name="name">name of the column to extract</param>
        /// <returns>Double value at the specified column</returns>
        /// <exception cref="Sqlx.Core.Exceptions.SqlxException">if the column value is null</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double GetDoubleNotNull(string name)
        {
            return dataRow.GetDoubleNotNull(dataRow.IndexOf(name));
        }

        /// <summary>
        /// Extract a not-null <see cref="TimeOnly"/> value. This maps to the <c>TIME</c> type.
        /// </summary>
        /// <param name="name">name of the column to extract</param>
        /// <returns>Time value at the specified column</returns>
        /// <exception cref="Sqlx.Core.Exceptions.SqlxException">if the column value is null</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TimeOnly GetTimeNotNull(string name)
        {
            return dataRow.GetTimeNotNull(dataRow.IndexOf(name));
        }

        /// <summary>
        /// Extract a not-null <see cref="DateOnly"/> value. This maps to the <c>DATE</c> type.
        /// </summary>
        /// <param name="name">name of the column to extract</param>
        /// <returns>Date value at the specified column</returns>
        /// <exception cref="Sqlx.Core.Exceptions.SqlxException">if the column value is null</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DateOnly GetDateNotNull(string name)
        {
            return dataRow.GetDateNotNull(dataRow.IndexOf(name));
        }

        /// <summary>
        /// Extract a not-null <see cref="DateTime"/> value. This maps to the <c>TIMESTAMP</c> type.
        /// </summary>
        /// <param name="name">name of the column to extract</param>
        /// <returns>DateTime value at the specified column</returns>
        /// <exception cref="Sqlx.Core.Exceptions.SqlxException">if the column value is null</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DateTime GetDateTimeNotNull(string name)
        {
            return dataRow.GetDateTimeNotNull(dataRow.IndexOf(name));
        }

        /// <summary>
        /// Extract a not-null <see cref="DateTimeOffset"/> value. This maps to the
        /// <c>TIMESTAMP WITH TIME ZONE</c> type.
        /// </summary>
        /// <param name="name">name of the column to extract</param>
        /// <returns>DateTimeOffset value at the specified column</returns>
        /// <exception cref="Sqlx.Core.Exceptions.SqlxException">if the column value is null</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DateTimeOffset GetDateTimeOffsetNotNull(string name)
        {
            return dataRow.GetDateTimeOffsetNotNull(dataRow.IndexOf(name));
        }

        /// <summary>
        /// Extract a not-null <see cref="decimal"/> value. This maps to the <c>DECIMAL</c> type.
        /// </summary>
        /// <param name="name">name of the column to extract</param>
        /// <returns>Decimal value at the specified column</returns>
        /// <exception cref="Sqlx.Core.Exceptions.SqlxException">if the column value is null</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public decimal GetDecimalNotNull(string name)
        {
            return dataRow.GetDecimalNotNull(dataRow.IndexOf(name));
        }

        /// <summary>
        /// Extract a not-null <c>byte[]</c> value. This maps to the <c>VARBINARY</c>/<c>BLOB</c> type.
        /// </summary>
        /// <param name="name">name of the column to extract</param>
        /// <returns>Bytes value at the specified column</returns>
        /// <exception cref="Sqlx.Core.Exceptions.SqlxException">if the column value is null</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte[] GetBytesNotNull(string name)
        {
            return dataRow.GetBytesNotNull(dataRow.IndexOf(name));
        }

        /// <summary>
        /// Extract a not-null <see cref="string"/> value. This maps to the
        /// <c>VARCHAR</c>/<c>TEXT</c>/<c>CLOB</c> type.
        /// </summary>
        /// <param name="name">name of the column to extract</param>
        /// <returns>String value at the specified column</returns>
        /// <exception cref="Sqlx.Core.Exceptions.SqlxException">if the column value is null</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetStringNotNull(string name)
        {
            return dataRow.GetStringNotNull(dataRow.IndexOf(name));
        }

        /// <summary>
        /// Extract a not-null <see cref="Guid"/> value. The <c>UUID</c>/<c>UNIQUEIDENTIFIER</c> type is
        /// not consistent across all databases so the driver specific implementation might vary.
        /// Generally it's either a built-in type or this method tries to parse 
        /// </summary>
        /// <param name="name">name of the column to extract</param>
        /// <returns>Guid value at the specified column</returns>
        /// <exception cref="Sqlx.Core.Exceptions.SqlxException">if the column value is null</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Guid GetGuidNotNull(string name)
        {
            return dataRow.GetGuidNotNull(dataRow.IndexOf(name));
        }

        /// <summary>
        /// Extract a not-null <typeparamref name="T"/> value after deserializing the inner value
        /// as JSON. Some databases have a JSON specific field type but other database drivers will
        /// treat string or byte[] fields as JSON compatible. When using this method, it's recommended
        /// to supply the <see cref="JsonTypeInfo{T}"/> parameter to aid deserialization.
        /// </summary>
        /// <param name="name">name of the column to extract</param>
        /// <param name="jsonTypeInfo">optional JSON source generated deserialization metadata</param>
        /// <returns><c>T</c> value at the specified column</returns>
        /// <exception cref="Sqlx.Core.Exceptions.SqlxException">if the column value is null</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetJsonNotNull<T>(
            string name,
            JsonTypeInfo<T>? jsonTypeInfo = null) where T : notnull
        {
            return dataRow.GetJsonNotNull(dataRow.IndexOf(name), jsonTypeInfo);
        }
    }
}
