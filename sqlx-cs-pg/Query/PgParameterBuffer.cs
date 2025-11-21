using System.Text.Json.Serialization.Metadata;
using Sqlx.Core;
using Sqlx.Core.Buffer;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Query;

/// <summary>
/// Buffer storing binary encoded parameters. Parameters are added to an instance of this class and
/// the encoded values can be accessed using <see cref="Span"/>. Along with the encoded values,
/// the <see cref="PgTypeInfo"/>s can be accessed for each parameter added.
/// </summary>
internal sealed class PgParameterBuffer : IDisposable
{
    private readonly WriteBuffer _buffer = new();
    private readonly List<PgTypeInfo> _pgTypes = [];

    /// <summary>
    /// Number of parameters encoded
    /// </summary>
    public short ParameterCount => (short)_pgTypes.Count;
    /// <summary>
    /// Encoded parameters values in binary format as the raw bytes
    /// </summary>
    public ReadOnlySpan<byte> Span => _buffer.ReadableSpan;
    /// <summary>
    /// <see cref="PgTypeInfo"/>s encoded into this buffer (in order of encoding)
    /// </summary>
    public IReadOnlyList<PgTypeInfo> PgTypes => _pgTypes;

    /// <summary>
    /// Encode a null value. Currently, this specifies the parameter type as
    /// <see cref="PgTypeInfo.Unspecified"/>.
    /// </summary>
    public void EncodeNull()
    {
        _buffer.WriteInt(-1);
        _pgTypes.Add(PgTypeInfo.Unspecified);
    }

    /// <summary>
    /// Encode the supplied value using the specified encoder type.
    /// </summary>
    /// <param name="value">Value to encode</param>
    /// <typeparam name="TValue">Type of the value</typeparam>
    /// <typeparam name="TPgType">Type of the encoder (could be itself)</typeparam>
    public void EncodeValue<TValue, TPgType>(TValue value)
        where TValue : notnull
        where TPgType : IPgDbType<TValue>
    {
        var startingPosition = _buffer.StartWritingLengthPrefixed();
        TPgType.Encode(value, _buffer);
        _buffer.FinishWritingLengthPrefixed(startingPosition, includeLength: false);
        _pgTypes.Add(TPgType.DbType);
    }

    /// <summary>
    /// Specialized method for encoding a raw span of bytes
    /// </summary>
    /// <param name="bytes">Bytes to encode</param>
    public void EncodeBytes(ReadOnlySpan<byte> bytes)
    {
        _buffer.WriteInt(bytes.Length);
        _buffer.WriteBytes(bytes);
        _pgTypes.Add(PgBytea.DbType);
    }

    /// <summary>
    /// Specialized method for encoding a raw span of chars
    /// </summary>
    /// <param name="chars">Chars to encode</param>
    public void EncodeChars(ReadOnlySpan<char> chars)
    {
        _buffer.WriteInt(Charsets.Default.GetByteCount(chars));
        _buffer.WriteString(chars);
        _pgTypes.Add(PgString.DbType);
    }

    /// <summary>
    /// Specialized method for encoding a value as JSON
    /// </summary>
    /// <param name="value">Value to encode as JSON</param>
    /// <param name="typeInfo">Optional type info to use when serializing the value</param>
    /// <typeparam name="T">Value type to encode</typeparam>
    public void EncodeJsonValue<T>(T value, JsonTypeInfo<T>? typeInfo) where T : notnull
    {
        var startingPosition = _buffer.StartWritingLengthPrefixed();
        PgJson<T>.Encode(value, _buffer, typeInfo);
        _buffer.FinishWritingLengthPrefixed(startingPosition, includeLength: false);
        _pgTypes.Add(PgJson<T>.DbType);
    }

    public void Dispose()
    {
        _buffer.Dispose();
    }
}
