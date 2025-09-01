using System.Buffers;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Sqlx.Core.Types;

public static class Json
{
    public static void WriteToStream<T>(
        IBufferWriter<byte> stream,
        T value,
        JsonTypeInfo<T>? typeInfo) where T : notnull
    {
        var writer = new Utf8JsonWriter(stream);
        if (typeInfo is not null)
        {
            JsonSerializer.Serialize(writer, value, typeInfo);
            return;
        }

        JsonSerializer.Serialize(writer, value);
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
