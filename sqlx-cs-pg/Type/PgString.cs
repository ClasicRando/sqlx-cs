using System.Buffers;
using Sqlx.Core.Buffer;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

/// <summary>
/// <see cref="IPgDbType{T}"/> for <see cref="string"/> values. Maps to the
/// <c>TEXT</c>/<c>VARCHAR</c>/<c>NAME</c>/<c>XML</c>/<c>BPCHAR</c> types.
/// </summary>
internal abstract class PgString : IPgDbType<string>, IHasArrayType
{
    /// <inheritdoc cref="IPgDbType{T}.Encode"/>
    /// <summary>
    /// Simply writes the characters to the buffer using UTF8 encoding
    /// </summary>
    public static void Encode(string value, IBufferWriter<byte> buffer)
    {
        buffer.WriteString(value);
    }

    /// <inheritdoc cref="IPgDbType{T}.DecodeBytes"/>
    /// <summary>
    /// Read the entire byte buffer as UTF8 encoded characters
    /// </summary>
    public static string DecodeBytes(ref PgBinaryValue value)
    {
        return value.Buffer.ReadString();
    }

    /// <inheritdoc cref="IPgDbType{T}.DecodeText"/>
    /// <summary>
    /// Convert the characters into a new <see cref="string"/>
    /// </summary>
    public static string DecodeText(in PgTextValue value)
    {
        return new string(value.Chars);
    }
    
    public static PgTypeInfo DbType => PgTypeInfo.Text;

    public static PgTypeInfo ArrayDbType => PgTypeInfo.TextArray;

    public static bool IsCompatible(PgTypeInfo typeInfo)
    {
        return typeInfo == DbType
               || typeInfo == PgTypeInfo.Varchar
               || typeInfo == PgTypeInfo.Xml
               || typeInfo == PgTypeInfo.Name
               || typeInfo == PgTypeInfo.Bpchar;
    }
}
