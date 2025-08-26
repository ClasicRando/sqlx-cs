using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Sqlx.Core.Types;

public static class Json
{
    public static void WriteToStream<T>(
        System.IO.Stream stream,
        T value,
        JsonTypeInfo<T>? typeInfo) where T : notnull
    {
        if (typeInfo is not null)
        {
            JsonSerializer.Serialize(stream, value, typeInfo);
            return;
        }

        JsonSerializer.Serialize(stream, value);
    }
    
    public static T FromBytes<T>(ReadOnlySpan<byte> bytes, JsonTypeInfo<T>? typeInfo = null)
        where T : notnull
    {
        T? result = typeInfo is null
            ? JsonSerializer.Deserialize<T>(bytes)
            : JsonSerializer.Deserialize(bytes, typeInfo);
        if (result is null)
        {
            throw new ArgumentException(
                "JSON deserialization returned null. Cannot create Json from null",
                nameof(bytes));
        }
        return result;
    }

    public static T FromChars<T>(ReadOnlySpan<char> chars, JsonTypeInfo<T>? typeInfo = null)
        where T : notnull
    {
        T? result = typeInfo is null
            ? JsonSerializer.Deserialize<T>(chars)
            : JsonSerializer.Deserialize(chars, typeInfo);
        if (result is null)
        {
            throw new ArgumentException(
                "JSON deserialization returned null. Cannot create Json from null",
                nameof(chars));
        }
        return result;
    }
}
