using System.Buffers;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Sqlx.Core.Types;

public static class Json
{
    /// <summary>
    /// Write the value as JSON to the supplied buffer. Although the type info parameter is
    /// optional, it's recommended to supply the type info to not require runtime magic code to
    /// serialize the value to JSON.
    /// </summary>
    /// <param name="buffer">buffer to write the JSON value to</param>
    /// <param name="value">object to write as JSON</param>
    /// <param name="typeInfo">optional type info</param>
    /// <typeparam name="T">value type to be serialized</typeparam>
    public static void WriteToBuffer<T>(
        IBufferWriter<byte> buffer,
        T value,
        JsonTypeInfo<T>? typeInfo) where T : notnull
    {
        using var writer = new Utf8JsonWriter(buffer);
        if (typeInfo is not null)
        {
            JsonSerializer.Serialize(writer, value, typeInfo);
            return;
        }

        JsonSerializer.Serialize(writer, value);
    }
    
    /// <summary>
    /// Deserialize the supplied bytes as JSON into <typeparamref name="T"/>. Although the type info
    /// parameter is optional, it's recommend to supply the type info to not require runtime magic
    /// code to deserialize the value from JSON bytes.
    /// </summary>
    /// <param name="bytes">JSON value as UTF-8 bytes</param>
    /// <param name="typeInfo">optional type info</param>
    /// <typeparam name="T">output type to deserialize to</typeparam>
    /// <returns>a new instance of <typeparamref name="T"/> from deserializing JSON</returns>
    /// <exception cref="ArgumentException">if the JSON deserializing return null</exception>
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

    /// <summary>
    /// Deserialize the supplied chars as JSON into <typeparamref name="T"/>. Although the type info
    /// parameter is optional, it's recommend to supply the type info to not require runtime magic
    /// code to deserialize the value from JSON chars.
    /// </summary>
    /// <param name="chars">JSON value as UTF-8 character</param>
    /// <param name="typeInfo">optional type info</param>
    /// <typeparam name="T">output type to deserialize to</typeparam>
    /// <returns>a new instance of <typeparamref name="T"/> from deserializing JSON</returns>
    /// <exception cref="ArgumentException">if the JSON deserializing return null</exception>
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
