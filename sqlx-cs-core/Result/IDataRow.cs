using System.Runtime.CompilerServices;
using System.Text.Json.Serialization.Metadata;

namespace Sqlx.Core.Result;

public interface IDataRow
{
    int IndexOf(string name);

    bool? GetBoolean(int index);

    sbyte? GetByte(int index);

    short? GetShort(int index);

    int? GetInt(int index);

    long? GetLong(int index);

    float? GetFloat(int index);

    double? GetDouble(int index);

    TimeOnly? GetTime(int index);

    DateOnly? GetDate(int index);

    DateTime? GetDateTime(int index);

    DateTimeOffset? GetDateTimeOffset(int index);

    decimal? GetDecimal(int index);

    byte[]? GetBytes(int index);

    string? GetString(int index);

    Guid? GetGuid(int index);

    T? GetJson<T>(int index, JsonTypeInfo<T>? jsonTypeInfo = null) where T : notnull;

    bool GetBooleanNotNull(int index);

    sbyte GetByteNotNull(int index);

    short GetShortNotNull(int index);

    int GetIntNotNull(int index);

    long GetLongNotNull(int index);

    float GetFloatNotNull(int index);

    double GetDoubleNotNull(int index);

    TimeOnly GetTimeNotNull(int index);

    DateOnly GetDateNotNull(int index);

    DateTime GetDateTimeNotNull(int index);

    DateTimeOffset GetDateTimeOffsetNotNull(int index);

    decimal GetDecimalNotNull(int index);

    byte[] GetBytesNotNull(int index);

    string GetStringNotNull(int index);

    Guid GetGuidNotNull(int index);

    T GetJsonNotNull<T>(int index, JsonTypeInfo<T>? jsonTypeInfo = null) where T : notnull;
}

public static class DataRowExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool? GetBoolean(this IDataRow dataRow, string name)
    {
        return dataRow.GetBoolean(dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static sbyte? GetByte(this IDataRow dataRow, string name)
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
    public static Guid? GetGuid(this IDataRow dataRow, string name)
    {
        return dataRow.GetGuid(dataRow.IndexOf(name));
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
    public static bool GetBooleanNotNull(this IDataRow dataRow, string name)
    {
        return dataRow.GetBooleanNotNull(dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static sbyte GetByteNotNull(this IDataRow dataRow, string name)
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
    public static Guid GetGuidNotNull(this IDataRow dataRow, string name)
    {
        return dataRow.GetGuidNotNull(dataRow.IndexOf(name));
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
