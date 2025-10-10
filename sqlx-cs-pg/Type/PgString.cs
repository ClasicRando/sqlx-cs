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
    public static void Encode(string value, WriteBuffer buffer)
    {
        buffer.WriteString(value);
    }

    /// <inheritdoc cref="IPgDbType{T}.DecodeBytes"/>
    /// <summary>
    /// Read the entire byte buffer as UTF8 encoded characters
    /// </summary>
    public static string DecodeBytes(PgBinaryValue value)
    {
        return value.Buffer.ReadText();
    }

    /// <inheritdoc cref="IPgDbType{T}.DecodeText"/>
    /// <summary>
    /// Convert the characters into a new <see cref="string"/>
    /// </summary>
    public static string DecodeText(PgTextValue value)
    {
        return new string(value.Chars);
    }
    
    public static PgType DbType => PgType.Text;

    public static PgType ArrayDbType => PgType.TextArray;

    public static bool IsCompatible(PgType dbType)
    {
        return dbType.TypeOid == DbType.TypeOid
               || dbType.TypeOid == PgType.Varchar.TypeOid
               || dbType.TypeOid == PgType.Xml.TypeOid
               || dbType.TypeOid == PgType.Name.TypeOid
               || dbType.TypeOid == PgType.Bpchar.TypeOid;
    }

    public static PgType GetActualType(string value)
    {
        return DbType;
    }
}
