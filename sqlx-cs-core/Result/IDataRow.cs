using System.Runtime.CompilerServices;
using System.Text.Json.Serialization.Metadata;
using Sqlx.Core.Exceptions;

namespace Sqlx.Core.Result;

public interface IDataRow
{
    public int IndexOf(string name);

    public T? Get<T>(int index) where T : notnull;

    public bool? GetBoolean(int index);

    public byte? GetByte(int index);

    public short? GetShort(int index);

    public int? GetInt(int index);

    public long? GetLong(int index);

    public float? GetFloat(int index);

    public double? GetDouble(int index);

    public TimeOnly? GetTime(int index);

    public DateOnly? GetDate(int index);

    public DateTime? GetDateTime(int index);

    public DateTimeOffset? GetDateTimeOffset(int index);

    public decimal? GetDecimal(int index);

    public byte[]? GetBytes(int index);

    public string? GetString(int index);

    public T? GetJson<T>(int index, JsonTypeInfo<T>? jsonTypeInfo = null) where T : notnull;
}

public static class DataRowExtensions
{
    public static T GetNotNull<T>(this IDataRow dataRow, int index) where T : notnull
    {
        return dataRow.Get<T>(index)
               ?? throw new SqlxException($"Expected field #{index} to be non-null but found null");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T? Get<T>(this IDataRow dataRow, string name) where T : notnull
    {
        return dataRow.Get<T>(dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T GetNotNull<T>(this IDataRow dataRow, string name) where T : notnull
    {
        return GetNotNull<T>(dataRow, dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool? GetBoolean(this IDataRow dataRow, string name)
    {
        return dataRow.GetBoolean(dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte? GetByte(this IDataRow dataRow, string name)
    {
        return dataRow.GetByte(dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static short? GetShort(this IDataRow dataRow, string name)
    {
        return dataRow.GetShort(dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int? GetInt(this IDataRow dataRow, string name)
    {
        return dataRow.GetInt(dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long? GetLong(this IDataRow dataRow, string name)
    {
        return dataRow.GetLong(dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float? GetFloat(this IDataRow dataRow, string name)
    {
        return dataRow.GetFloat(dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double? GetDouble(this IDataRow dataRow, string name)
    {
        return dataRow.GetDouble(dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TimeOnly? GetTime(this IDataRow dataRow, string name)
    {
        return dataRow.GetTime(dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateOnly? GetDate(this IDataRow dataRow, string name)
    {
        return dataRow.GetDate(dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTime? GetDateTime(this IDataRow dataRow, string name)
    {
        return dataRow.GetDateTime(dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTimeOffset? GetDateTimeOffset(this IDataRow dataRow, string name)
    {
        return dataRow.GetDateTimeOffset(dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static decimal? GetDecimal(this IDataRow dataRow, string name)
    {
        return dataRow.GetDecimal(dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[]? GetBytes(this IDataRow dataRow, string name)
    {
        return dataRow.GetBytes(dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string? GetString(this IDataRow dataRow, string name)
    {
        return dataRow.GetString(dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T? GetJson<T>(
        this IDataRow dataRow,
        string name,
        JsonTypeInfo<T>? jsonTypeInfo = null)
        where T : notnull
    {
        return dataRow.GetJson(dataRow.IndexOf(name), jsonTypeInfo);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool GetBooleanNotNull(this IDataRow dataRow, int index)
    {
        return dataRow.GetBoolean(index)
               ?? throw new SqlxException($"Expected field #{index} to be non-null but found null");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte GetByteNotNull(this IDataRow dataRow, int index)
    {
        return dataRow.GetByte(index)
               ?? throw new SqlxException($"Expected field #{index} to be non-null but found null");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static short GetShortNotNull(this IDataRow dataRow, int index)
    {
        return dataRow.GetShort(index)
               ?? throw new SqlxException($"Expected field #{index} to be non-null but found null");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetIntNotNull(this IDataRow dataRow, int index)
    {
        return dataRow.GetInt(index)
               ?? throw new SqlxException($"Expected field #{index} to be non-null but found null");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long GetLongNotNull(this IDataRow dataRow, int index)
    {
        return dataRow.GetLong(index)
               ?? throw new SqlxException($"Expected field #{index} to be non-null but found null");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float GetFloatNotNull(this IDataRow dataRow, int index)
    {
        return dataRow.GetFloat(index)
               ?? throw new SqlxException($"Expected field #{index} to be non-null but found null");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double GetDoubleNotNull(this IDataRow dataRow, int index)
    {
        return dataRow.GetDouble(index)
               ?? throw new SqlxException($"Expected field #{index} to be non-null but found null");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TimeOnly GetTimeNotNull(this IDataRow dataRow, int index)
    {
        return dataRow.GetTime(index)
               ?? throw new SqlxException($"Expected field #{index} to be non-null but found null");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateOnly GetDateNotNull(this IDataRow dataRow, int index)
    {
        return dataRow.GetDate(index)
               ?? throw new SqlxException($"Expected field #{index} to be non-null but found null");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTime GetDateTimeNotNull(this IDataRow dataRow, int index)
    {
        return dataRow.GetDateTime(index)
               ?? throw new SqlxException($"Expected field #{index} to be non-null but found null");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTimeOffset GetDateTimeOffsetNotNull(this IDataRow dataRow, int index)
    {
        return dataRow.GetDateTimeOffset(index)
               ?? throw new SqlxException($"Expected field #{index} to be non-null but found null");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static decimal GetDecimalNotNull(this IDataRow dataRow, int index)
    {
        return dataRow.GetDecimal(index)
               ?? throw new SqlxException($"Expected field #{index} to be non-null but found null");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[] GetBytesNotNull(this IDataRow dataRow, int index)
    {
        return dataRow.GetBytes(index)
               ?? throw new SqlxException($"Expected field #{index} to be non-null but found null");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetStringNotNull(this IDataRow dataRow, int index)
    {
        return dataRow.GetString(index)
               ?? throw new SqlxException($"Expected field #{index} to be non-null but found null");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T GetJsonNotNull<T>(
        this IDataRow dataRow,
        int index,
        JsonTypeInfo<T>? jsonTypeInfo = null) where T : notnull
    {
        return dataRow.GetJson(index, jsonTypeInfo)
               ?? throw new SqlxException($"Expected field #{index} to be non-null but found null");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool GetBooleanNotNull(this IDataRow dataRow, string name)
    {
        return dataRow.GetBooleanNotNull(dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte GetByteNotNull(this IDataRow dataRow, string name)
    {
        return dataRow.GetByteNotNull(dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static short GetShortNotNull(this IDataRow dataRow, string name)
    {
        return dataRow.GetShortNotNull(dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetIntNotNull(this IDataRow dataRow, string name)
    {
        return dataRow.GetIntNotNull(dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long GetLongNotNull(this IDataRow dataRow, string name)
    {
        return dataRow.GetLongNotNull(dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float GetFloatNotNull(this IDataRow dataRow, string name)
    {
        return dataRow.GetFloatNotNull(dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double GetDoubleNotNull(this IDataRow dataRow, string name)
    {
        return dataRow.GetDoubleNotNull(dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TimeOnly GetTimeNotNull(this IDataRow dataRow, string name)
    {
        return dataRow.GetTimeNotNull(dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateOnly GetDateNotNull(this IDataRow dataRow, string name)
    {
        return dataRow.GetDateNotNull(dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTime GetDateTimeNotNull(this IDataRow dataRow, string name)
    {
        return dataRow.GetDateTimeNotNull(dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTimeOffset GetDateTimeOffsetNotNull(this IDataRow dataRow, string name)
    {
        return dataRow.GetDateTimeOffsetNotNull(dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static decimal GetDecimalNotNull(this IDataRow dataRow, string name)
    {
        return dataRow.GetDecimalNotNull(dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[] GetBytesNotNull(this IDataRow dataRow, string name)
    {
        return dataRow.GetBytesNotNull(dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetStringNotNull(this IDataRow dataRow, string name)
    {
        return dataRow.GetStringNotNull(dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T GetJsonNotNull<T>(
        this IDataRow dataRow,
        string name,
        JsonTypeInfo<T>? jsonTypeInfo = null) where T : notnull
    {
        return dataRow.GetJsonNotNull(dataRow.IndexOf(name), jsonTypeInfo);
    }
}
