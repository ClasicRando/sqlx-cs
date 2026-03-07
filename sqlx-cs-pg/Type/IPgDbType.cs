using System.Buffers;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

/// <summary>
/// Postgres database type defining the type information as well as the encoding and decoding of the
/// type. These are all static abstract methods/properties since the behaviour is about the type
/// and not associated with a specific instance of the type. Generally this is implemented by type
/// <typeparamref name="T"/> itself but for types already defined, an abstract class is defined that
/// will be a proxy for that classes type information. 
/// </summary>
/// <typeparam name="T">Type that is linked to a Postgres database type</typeparam>
public interface IPgDbType<T> where T : notnull
{
    /// <summary>
    /// Encode the value into the buffer in the Postgres binary format
    /// </summary>
    /// <param name="value">Value to encode</param>
    /// <param name="buffer">Buffer to encode into</param>
    static abstract void Encode(T value, IBufferWriter<byte> buffer);
    
    /// <param name="value">Binary encoded value sent from the database</param>
    /// <returns>
    /// A new instance of <typeparamref name="T"/> decoded from the binary encoded value
    /// </returns>
    static abstract T DecodeBytes(ref PgBinaryValue value);

    /// <param name="value">Text encoded value sent from the database</param>
    /// <returns>
    /// A new instance of <typeparamref name="T"/> decoded from the text encoded value
    /// </returns>
    static abstract T DecodeText(in PgTextValue value);
    
    /// <summary>
    /// <see cref="PgTypeInfo"/> definition for this type
    /// </summary>
    static abstract PgTypeInfo DbType { get; }

    /// <param name="typeInfo">Database type to check</param>
    /// <returns>True if the provided type is compatible with this type for decoding</returns>
    static abstract bool IsCompatible(PgTypeInfo typeInfo);
}
