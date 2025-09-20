using Sqlx.Core.Buffer;

namespace Sqlx.Postgres.Type;

/// <summary>
/// Defines an encoder for a Postgres type. Encoding is always in binary format that is expected by
/// the Postgres database server. When encoding parameters into a statement or within container
/// types, the size of the type must be encoded as well so types must be able to define what the
/// size of an encodable value is before encoding.
/// </summary>
/// <typeparam name="T">Type to encode</typeparam>
public interface IPgEncoder<in T> where T : notnull
{
    /// <summary>
    /// Encode the value into the buffer in the Postgres binary format
    /// </summary>
    /// <param name="value">Value to encode</param>
    /// <param name="buffer">Buffer to encode into</param>
    static abstract void Encode(T value, WriteBuffer buffer);
}
