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
    /// <summary>
    /// Extract a possibly null <see cref="bool"/> value. The <c>BOOLEAN</c> type is not consistent
    /// across all databases so the driver specific implementation might vary.
    /// </summary>
    /// <param name="dataRow">data row to extract a value from</param>
    /// <param name="index">0-based index of the column to extract</param>
    /// <returns>Boolean value at the specified column or null if the DB value was null</returns>
    public static bool? GetBoolean(this IDataRow dataRow, int index)
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
    /// <param name="dataRow">data row to extract a value from</param>
    /// <param name="index">0-based index of the column to extract</param>
    /// <returns>Sbyte value at the specified column or null if the DB value was null</returns>
    public static sbyte? GetByte(this IDataRow dataRow, int index)
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
    /// <param name="dataRow">data row to extract a value from</param>
    /// <param name="index">0-based index of the column to extract</param>
    /// <returns>Short value at the specified column or null if the DB value was null</returns>
    public static short? GetShort(this IDataRow dataRow, int index)
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
    /// <param name="dataRow">data row to extract a value from</param>
    /// <param name="index">0-based index of the column to extract</param>
    /// <returns>Int value at the specified column or null if the DB value was null</returns>
    public static int? GetInt(this IDataRow dataRow, int index)
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
    /// <param name="dataRow">data row to extract a value from</param>
    /// <param name="index">0-based index of the column to extract</param>
    /// <returns>Long value at the specified column or null if the DB value was null</returns>
    public static long? GetLong(this IDataRow dataRow, int index)
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
    /// <param name="dataRow">data row to extract a value from</param>
    /// <param name="index">0-based index of the column to extract</param>
    /// <returns>Float value at the specified column or null if the DB value was null</returns>
    public static float? GetFloat(this IDataRow dataRow, int index)
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
    /// <param name="dataRow">data row to extract a value from</param>
    /// <param name="index">0-based index of the column to extract</param>
    /// <returns>Double value at the specified column or null if the DB value was null</returns>
    public static double? GetDouble(this IDataRow dataRow, int index)
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
    /// <param name="dataRow">data row to extract a value from</param>
    /// <param name="index">0-based index of the column to extract</param>
    /// <returns>Time value at the specified column or null if the DB value was null</returns>
    public static TimeOnly? GetTime(this IDataRow dataRow, int index)
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
    /// <param name="dataRow">data row to extract a value from</param>
    /// <param name="index">0-based index of the column to extract</param>
    /// <returns>Date value at the specified column or null if the DB value was null</returns>
    public static DateOnly? GetDate(this IDataRow dataRow, int index)
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
    /// <param name="dataRow">data row to extract a value from</param>
    /// <param name="index">0-based index of the column to extract</param>
    /// <returns>DateTime value at the specified column or null if the DB value was null</returns>
    public static DateTime? GetDateTime(this IDataRow dataRow, int index)
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
    /// <param name="dataRow">data row to extract a value from</param>
    /// <param name="index">0-based index of the column to extract</param>
    /// <returns>
    /// DateTimeOffset value at the specified column or null if the DB value was null
    /// </returns>
    public static DateTimeOffset? GetDateTimeOffset(this IDataRow dataRow, int index)
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
    /// <param name="dataRow">data row to extract a value from</param>
    /// <param name="index">0-based index of the column to extract</param>
    /// <returns>Decimal value at the specified column or null if the DB value was null</returns>
    public static decimal? GetDecimal(this IDataRow dataRow, int index)
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
    /// <param name="dataRow">data row to extract a value from</param>
    /// <param name="index">0-based index of the column to extract</param>
    /// <returns>Bytes value at the specified column or null if the DB value was null</returns>
    public static byte[]? GetBytes(this IDataRow dataRow, int index)
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
    /// <param name="dataRow">data row to extract a value from</param>
    /// <param name="index">0-based index of the column to extract</param>
    /// <returns>String value at the specified column or null if the DB value was null</returns>
    public static string? GetString(this IDataRow dataRow, int index)
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
    /// <param name="dataRow">data row to extract a value from</param>
    /// <param name="index">0-based index of the column to extract</param>
    /// <returns>Guid value at the specified column or null if the DB value was null</returns>
    public static Guid? GetGuid(this IDataRow dataRow, int index)
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
    /// <param name="dataRow">data row to extract a value from</param>
    /// <param name="index">0-based index of the column to extract</param>
    /// <param name="jsonTypeInfo">optional JSON source generated deserialization metadata</param>
    /// <returns>
    /// <c>T</c> value at the specified column or default if the DB value was null
    /// </returns>
    public static T? GetJson<T>(
        this IDataRow dataRow,
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
    /// <param name="dataRow">data row to extract a value from</param>
    /// <param name="name">name of the column to extract</param>
    /// <returns>Boolean value at the specified column or null if the DB value was null</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool? GetBoolean(this IDataRow dataRow, string name)
    {
        return GetBoolean(dataRow, dataRow.IndexOf(name));
    }

    /// <summary>
    /// Extract a possibly null <see cref="sbyte"/> value. This maps to the <c>TINYINT</c> type but
    /// can vary between database implementations since not all use <c>TINYINT</c>
    /// </summary>
    /// <param name="dataRow">data row to extract a value from</param>
    /// <param name="name">name of the column to extract</param>
    /// <returns>Sbyte value at the specified column or null if the DB value was null</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static sbyte? GetByte(this IDataRow dataRow, string name)
    {
        return GetByte(dataRow, dataRow.IndexOf(name));
    }

    /// <summary>
    /// Extract a possibly null <see cref="short"/> value. This maps to the <c>SMALLINT</c> type but
    /// drivers can coerce larger values if the actual value fits into a <see cref="short"/>.
    /// </summary>
    /// <param name="dataRow">data row to extract a value from</param>
    /// <param name="name">name of the column to extract</param>
    /// <returns>Short value at the specified column or null if the DB value was null</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static short? GetShort(this IDataRow dataRow, string name)
    {
        return GetShort(dataRow, dataRow.IndexOf(name));
    }

    /// <summary>
    /// Extract a possibly null <see cref="int"/> value. This maps to the <c>INTEGER</c> type but
    /// drivers can coerce larger values if the actual value fits into a <see cref="int"/>.
    /// </summary>
    /// <param name="dataRow">data row to extract a value from</param>
    /// <param name="name">name of the column to extract</param>
    /// <returns>Int value at the specified column or null if the DB value was null</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int? GetInt(this IDataRow dataRow, string name)
    {
        return GetInt(dataRow, dataRow.IndexOf(name));
    }

    /// <summary>
    /// Extract a possibly null <see cref="long"/> value. This maps to the <c>BIGINT</c> type.
    /// </summary>
    /// <param name="dataRow">data row to extract a value from</param>
    /// <param name="name">name of the column to extract</param>
    /// <returns>Long value at the specified column or null if the DB value was null</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long? GetLong(this IDataRow dataRow, string name)
    {
        return GetLong(dataRow, dataRow.IndexOf(name));
    }

    /// <summary>
    /// Extract a possibly null <see cref="float"/> value. This maps to the <c>REAL</c> type but
    /// drivers can coerce larger <see cref="double"/> values if the actual value fits into a
    /// <see cref="float"/>.
    /// </summary>
    /// <param name="dataRow">data row to extract a value from</param>
    /// <param name="name">name of the column to extract</param>
    /// <returns>Float value at the specified column or null if the DB value was null</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float? GetFloat(this IDataRow dataRow, string name)
    {
        return GetFloat(dataRow, dataRow.IndexOf(name));
    }

    /// <summary>
    /// Extract a possibly null <see cref="double"/> value. This maps to the <c>DOUBLE PRECISION</c>
    /// type.
    /// </summary>
    /// <param name="dataRow">data row to extract a value from</param>
    /// <param name="name">name of the column to extract</param>
    /// <returns>Double value at the specified column or null if the DB value was null</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double? GetDouble(this IDataRow dataRow, string name)
    {
        return GetDouble(dataRow, dataRow.IndexOf(name));
    }

    /// <summary>
    /// Extract a possibly null <see cref="TimeOnly"/> value. This maps to the <c>TIME</c> type.
    /// </summary>
    /// <param name="dataRow">data row to extract a value from</param>
    /// <param name="name">name of the column to extract</param>
    /// <returns>Time value at the specified column or null if the DB value was null</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TimeOnly? GetTime(this IDataRow dataRow, string name)
    {
        return GetTime(dataRow, dataRow.IndexOf(name));
    }

    /// <summary>
    /// Extract a possibly null <see cref="DateOnly"/> value. This maps to the <c>DATE</c> type.
    /// </summary>
    /// <param name="dataRow">data row to extract a value from</param>
    /// <param name="name">name of the column to extract</param>
    /// <returns>Date value at the specified column or null if the DB value was null</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateOnly? GetDate(this IDataRow dataRow, string name)
    {
        return GetDate(dataRow, dataRow.IndexOf(name));
    }

    /// <summary>
    /// Extract a possibly null <see cref="DateTime"/> value. This maps to the <c>TIMESTAMP</c>
    /// type.
    /// </summary>
    /// <param name="dataRow">data row to extract a value from</param>
    /// <param name="name">name of the column to extract</param>
    /// <returns>DateTime value at the specified column or null if the DB value was null</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTime? GetDateTime(this IDataRow dataRow, string name)
    {
        return GetDateTime(dataRow, dataRow.IndexOf(name));
    }

    /// <summary>
    /// Extract a possibly null <see cref="DateTimeOffset"/> value. This maps to the
    /// <c>TIMESTAMP WITH TIME ZONE</c> type.
    /// </summary>
    /// <param name="dataRow">data row to extract a value from</param>
    /// <param name="name">name of the column to extract</param>
    /// <returns>
    /// DateTimeOffset value at the specified column or null if the DB value was null
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTimeOffset? GetDateTimeOffset(this IDataRow dataRow, string name)
    {
        return GetDateTimeOffset(dataRow, dataRow.IndexOf(name));
    }

    /// <summary>
    /// Extract a possibly null <see cref="decimal"/> value. This maps to the <c>DECIMAL</c> type.
    /// </summary>
    /// <param name="dataRow">data row to extract a value from</param>
    /// <param name="name">name of the column to extract</param>
    /// <returns>Decimal value at the specified column or null if the DB value was null</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static decimal? GetDecimal(this IDataRow dataRow, string name)
    {
        return GetDecimal(dataRow, dataRow.IndexOf(name));
    }

    /// <summary>
    /// Extract a possibly null <c>byte[]</c> value. This maps to the
    /// <c>VARBINARY</c>/<c>BLOB</c> type.
    /// </summary>
    /// <param name="dataRow">data row to extract a value from</param>
    /// <param name="name">name of the column to extract</param>
    /// <returns>Bytes value at the specified column or null if the DB value was null</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[]? GetBytes(this IDataRow dataRow, string name)
    {
        return GetBytes(dataRow, dataRow.IndexOf(name));
    }

    /// <summary>
    /// Extract a possibly null <see cref="string"/> value. This maps to the
    /// <c>VARCHAR</c>/<c>TEXT</c>/<c>CLOB</c> type.
    /// </summary>
    /// <param name="dataRow">data row to extract a value from</param>
    /// <param name="name">name of the column to extract</param>
    /// <returns>String value at the specified column or null if the DB value was null</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string? GetString(this IDataRow dataRow, string name)
    {
        return GetString(dataRow, dataRow.IndexOf(name));
    }

    /// <summary>
    /// Extract a possibly null <see cref="Guid"/> value. The <c>UUID</c>/<c>UNIQUEIDENTIFIER</c>
    /// type is not consistent across all databases so the driver specific implementation might
    /// vary. Generally it's either a built-in type or this method tries to parse 
    /// </summary>
    /// <param name="dataRow">data row to extract a value from</param>
    /// <param name="name">name of the column to extract</param>
    /// <returns>Guid value at the specified column or null if the DB value was null</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Guid? GetGuid(this IDataRow dataRow, string name)
    {
        return GetGuid(dataRow, dataRow.IndexOf(name));
    }

    /// <summary>
    /// Extract a possibly null <typeparamref name="T"/> value after deserializing the inner value
    /// as JSON. Some databases have a JSON specific field type but other database drivers will
    /// treat string or byte[] fields as JSON compatible. When using this method, it's recommended
    /// to supply the <see cref="JsonTypeInfo{T}"/> parameter to aid deserialization.
    /// </summary>
    /// <param name="dataRow">data row to extract a value from</param>
    /// <param name="name">name of the column to extract</param>
    /// <param name="jsonTypeInfo">optional JSON source generated deserialization metadata</param>
    /// <returns><c>T</c> value at the specified column or null if the DB value was null</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T? GetJson<T>(
        this IDataRow dataRow,
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
    /// <param name="dataRow">data row to extract a value from</param>
    /// <param name="name">name of the column to extract</param>
    /// <returns>Boolean value at the specified column</returns>
    /// <exception cref="Sqlx.Core.Exceptions.SqlxException">if the column value is null</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool GetBooleanNotNull(this IDataRow dataRow, string name)
    {
        return dataRow.GetBooleanNotNull(dataRow.IndexOf(name));
    }

    /// <summary>
    /// Extract a not-null <see cref="sbyte"/> value. This maps to the <c>TINYINT</c> type but can
    /// vary between database implementations since not all use <c>TINYINT</c>
    /// </summary>
    /// <param name="dataRow">data row to extract a value from</param>
    /// <param name="name">name of the column to extract</param>
    /// <returns>Sbyte value at the specified column</returns>
    /// <exception cref="Sqlx.Core.Exceptions.SqlxException">if the column value is null</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static sbyte GetByteNotNull(this IDataRow dataRow, string name)
    {
        return dataRow.GetByteNotNull(dataRow.IndexOf(name));
    }

    /// <summary>
    /// Extract a not-null <see cref="short"/> value. This maps to the <c>SMALLINT</c> type but
    /// drivers can coerce larger values if the actual value fits into a <see cref="short"/>.
    /// </summary>
    /// <param name="dataRow">data row to extract a value from</param>
    /// <param name="name">name of the column to extract</param>
    /// <returns>Short value at the specified column</returns>
    /// <exception cref="Sqlx.Core.Exceptions.SqlxException">if the column value is null</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static short GetShortNotNull(this IDataRow dataRow, string name)
    {
        return dataRow.GetShortNotNull(dataRow.IndexOf(name));
    }

    /// <summary>
    /// Extract a not-null <see cref="int"/> value. This maps to the <c>INTEGER</c> type but drivers
    /// can coerce larger values if the actual value fits into a <see cref="int"/>.
    /// </summary>
    /// <param name="dataRow">data row to extract a value from</param>
    /// <param name="name">name of the column to extract</param>
    /// <returns>Int value at the specified column</returns>
    /// <exception cref="Sqlx.Core.Exceptions.SqlxException">if the column value is null</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetIntNotNull(this IDataRow dataRow, string name)
    {
        return dataRow.GetIntNotNull(dataRow.IndexOf(name));
    }

    /// <summary>
    /// Extract a not-null <see cref="long"/> value. This maps to the <c>BIGINT</c> type.
    /// </summary>
    /// <param name="dataRow">data row to extract a value from</param>
    /// <param name="name">name of the column to extract</param>
    /// <returns>Long value at the specified column</returns>
    /// <exception cref="Sqlx.Core.Exceptions.SqlxException">if the column value is null</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long GetLongNotNull(this IDataRow dataRow, string name)
    {
        return dataRow.GetLongNotNull(dataRow.IndexOf(name));
    }

    /// <summary>
    /// Extract a not-null <see cref="float"/> value. This maps to the <c>REAL</c> type but drivers
    /// can coerce larger <see cref="double"/> values if the actual value fits into a
    /// <see cref="float"/>.
    /// </summary>
    /// <param name="dataRow">data row to extract a value from</param>
    /// <param name="name">name of the column to extract</param>
    /// <returns>Float value at the specified column</returns>
    /// <exception cref="Sqlx.Core.Exceptions.SqlxException">if the column value is null</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float GetFloatNotNull(this IDataRow dataRow, string name)
    {
        return dataRow.GetFloatNotNull(dataRow.IndexOf(name));
    }

    /// <summary>
    /// Extract a not-null <see cref="double"/> value. This maps to the <c>DOUBLE PRECISION</c>
    /// type.
    /// </summary>
    /// <param name="dataRow">data row to extract a value from</param>
    /// <param name="name">name of the column to extract</param>
    /// <returns>Double value at the specified column</returns>
    /// <exception cref="Sqlx.Core.Exceptions.SqlxException">if the column value is null</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double GetDoubleNotNull(this IDataRow dataRow, string name)
    {
        return dataRow.GetDoubleNotNull(dataRow.IndexOf(name));
    }

    /// <summary>
    /// Extract a not-null <see cref="TimeOnly"/> value. This maps to the <c>TIME</c> type.
    /// </summary>
    /// <param name="dataRow">data row to extract a value from</param>
    /// <param name="name">name of the column to extract</param>
    /// <returns>Time value at the specified column</returns>
    /// <exception cref="Sqlx.Core.Exceptions.SqlxException">if the column value is null</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TimeOnly GetTimeNotNull(this IDataRow dataRow, string name)
    {
        return dataRow.GetTimeNotNull(dataRow.IndexOf(name));
    }

    /// <summary>
    /// Extract a not-null <see cref="DateOnly"/> value. This maps to the <c>DATE</c> type.
    /// </summary>
    /// <param name="dataRow">data row to extract a value from</param>
    /// <param name="name">name of the column to extract</param>
    /// <returns>Date value at the specified column</returns>
    /// <exception cref="Sqlx.Core.Exceptions.SqlxException">if the column value is null</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateOnly GetDateNotNull(this IDataRow dataRow, string name)
    {
        return dataRow.GetDateNotNull(dataRow.IndexOf(name));
    }

    /// <summary>
    /// Extract a not-null <see cref="DateTime"/> value. This maps to the <c>TIMESTAMP</c> type.
    /// </summary>
    /// <param name="dataRow">data row to extract a value from</param>
    /// <param name="name">name of the column to extract</param>
    /// <returns>DateTime value at the specified column</returns>
    /// <exception cref="Sqlx.Core.Exceptions.SqlxException">if the column value is null</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTime GetDateTimeNotNull(this IDataRow dataRow, string name)
    {
        return dataRow.GetDateTimeNotNull(dataRow.IndexOf(name));
    }

    /// <summary>
    /// Extract a not-null <see cref="DateTimeOffset"/> value. This maps to the
    /// <c>TIMESTAMP WITH TIME ZONE</c> type.
    /// </summary>
    /// <param name="dataRow">data row to extract a value from</param>
    /// <param name="name">name of the column to extract</param>
    /// <returns>DateTimeOffset value at the specified column</returns>
    /// <exception cref="Sqlx.Core.Exceptions.SqlxException">if the column value is null</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTimeOffset GetDateTimeOffsetNotNull(this IDataRow dataRow, string name)
    {
        return dataRow.GetDateTimeOffsetNotNull(dataRow.IndexOf(name));
    }

    /// <summary>
    /// Extract a not-null <see cref="decimal"/> value. This maps to the <c>DECIMAL</c> type.
    /// </summary>
    /// <param name="dataRow">data row to extract a value from</param>
    /// <param name="name">name of the column to extract</param>
    /// <returns>Decimal value at the specified column</returns>
    /// <exception cref="Sqlx.Core.Exceptions.SqlxException">if the column value is null</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static decimal GetDecimalNotNull(this IDataRow dataRow, string name)
    {
        return dataRow.GetDecimalNotNull(dataRow.IndexOf(name));
    }

    /// <summary>
    /// Extract a not-null <c>byte[]</c> value. This maps to the <c>VARBINARY</c>/<c>BLOB</c> type.
    /// </summary>
    /// <param name="dataRow">data row to extract a value from</param>
    /// <param name="name">name of the column to extract</param>
    /// <returns>Bytes value at the specified column</returns>
    /// <exception cref="Sqlx.Core.Exceptions.SqlxException">if the column value is null</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[] GetBytesNotNull(this IDataRow dataRow, string name)
    {
        return dataRow.GetBytesNotNull(dataRow.IndexOf(name));
    }

    /// <summary>
    /// Extract a not-null <see cref="string"/> value. This maps to the
    /// <c>VARCHAR</c>/<c>TEXT</c>/<c>CLOB</c> type.
    /// </summary>
    /// <param name="dataRow">data row to extract a value from</param>
    /// <param name="name">name of the column to extract</param>
    /// <returns>String value at the specified column</returns>
    /// <exception cref="Sqlx.Core.Exceptions.SqlxException">if the column value is null</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetStringNotNull(this IDataRow dataRow, string name)
    {
        return dataRow.GetStringNotNull(dataRow.IndexOf(name));
    }

    /// <summary>
    /// Extract a not-null <see cref="Guid"/> value. The <c>UUID</c>/<c>UNIQUEIDENTIFIER</c> type is
    /// not consistent across all databases so the driver specific implementation might vary.
    /// Generally it's either a built-in type or this method tries to parse 
    /// </summary>
    /// <param name="dataRow">data row to extract a value from</param>
    /// <param name="name">name of the column to extract</param>
    /// <returns>Guid value at the specified column</returns>
    /// <exception cref="Sqlx.Core.Exceptions.SqlxException">if the column value is null</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Guid GetGuidNotNull(this IDataRow dataRow, string name)
    {
        return dataRow.GetGuidNotNull(dataRow.IndexOf(name));
    }

    /// <summary>
    /// Extract a not-null <typeparamref name="T"/> value after deserializing the inner value
    /// as JSON. Some databases have a JSON specific field type but other database drivers will
    /// treat string or byte[] fields as JSON compatible. When using this method, it's recommended
    /// to supply the <see cref="JsonTypeInfo{T}"/> parameter to aid deserialization.
    /// </summary>
    /// <param name="dataRow">data row to extract a value from</param>
    /// <param name="name">name of the column to extract</param>
    /// <param name="jsonTypeInfo">optional JSON source generated deserialization metadata</param>
    /// <returns><c>T</c> value at the specified column</returns>
    /// <exception cref="Sqlx.Core.Exceptions.SqlxException">if the column value is null</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T GetJsonNotNull<T>(
        this IDataRow dataRow,
        string name,
        JsonTypeInfo<T>? jsonTypeInfo = null) where T : notnull
    {
        return dataRow.GetJsonNotNull(dataRow.IndexOf(name), jsonTypeInfo);
    }
}
