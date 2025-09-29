using Sqlx.Core.Buffer;
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
    static abstract void Encode(T value, WriteBuffer buffer);
    
    /// <param name="value">Binary encoded value sent from the database</param>
    /// <returns>
    /// A new instance of <typeparamref name="T"/> decoded from the binary encoded value
    /// </returns>
    static abstract T DecodeBytes(PgBinaryValue value);

    /// <param name="value">Text encoded value sent from the database</param>
    /// <returns>
    /// A new instance of <typeparamref name="T"/> decoded from the text encoded value
    /// </returns>
    static abstract T DecodeText(PgTextValue value);
    
    /// <summary>
    /// <see cref="PgType"/> definition for this type 
    /// </summary>
    static abstract PgType DbType { get; }

    /// <param name="dbType">Database type to check</param>
    /// <returns>True if the provided type is compatible with this type for decoding</returns>
    static abstract bool IsCompatible(PgType dbType);

    /// <summary>
    /// Get the actual <see cref="PgType"/> for the specified value. Generally this method returns
    /// <see cref="DbType"/> but in some types such as <see cref="PgMacAddress"/> <c>MACADDRESS</c>
    /// and <c>MACADDRESS8</c> are represented with the same CLR type and the contents of an
    /// instance will inform the actual database type.
    /// </summary>
    /// <param name="value">Value to check for actual type</param>
    /// <returns>The final type of the value</returns>
    static abstract PgType GetActualType(T value);
}
