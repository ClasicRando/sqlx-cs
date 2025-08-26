using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

public readonly record struct PgMacAddress(
    byte A,
    byte B,
    byte C,
    byte D,
    byte E,
    byte F,
    byte G,
    byte H) : IPgDbType<PgMacAddress>
{
    private const byte DefaultD = 0xFF;
    private const byte DefaultE = 0xFE;
    
    public PgMacAddress(
        byte A,
        byte B,
        byte C,
        byte F,
        byte G,
        byte H) : this(A, B, C, DefaultD, DefaultE, F, G, H)
    {
        IsMacAddress8 = false;
    }
    
    public bool IsMacAddress8 { get; } = true;

    public PgMacAddress ToMacAddr()
    {
        return new PgMacAddress(A, B, C, F, G, H);
    }

    public override string ToString()
    {
        return $"{A:X2}:{B:X2}:{C:X2}:{D:X2}:{E:X2}:{F:X2}:{G:X2}:{H:X2}";
    }

    public static void Encode(PgMacAddress value, WriteBuffer buffer)
    {
        buffer.WriteByte(value.A);
        buffer.WriteByte(value.B);
        buffer.WriteByte(value.C);
        if (value.IsMacAddress8)
        {
            buffer.WriteByte(value.D);
            buffer.WriteByte(value.E);
        }
        buffer.WriteByte(value.F);
        buffer.WriteByte(value.G);
        buffer.WriteByte(value.H);
    }

    public static PgMacAddress DecodeBytes(PgBinaryValue value)
    {
        var byteCount = value.Buffer.Remaining;
        if (byteCount != 6 && byteCount != 8)
        {
            throw ColumnDecodeError.Create<PgMacAddress>(
                value.ColumnMetadata,
                $"Expected 6 or 8 bytes. Found {byteCount}");
        }

        return new PgMacAddress(
            value.Buffer.ReadByte(),
            value.Buffer.ReadByte(),
            value.Buffer.ReadByte(),
            byteCount == 8 ? value.Buffer.ReadByte() : DefaultD,
            byteCount == 8 ? value.Buffer.ReadByte() : DefaultE,
            value.Buffer.ReadByte(),
            value.Buffer.ReadByte(),
            value.Buffer.ReadByte());
    }

    public static PgMacAddress DecodeText(PgTextValue value)
    {
        Span<Range> splits = stackalloc Range[8];
        var splitCount = value.Chars.Split(splits, ':');
        if (splitCount != 6 && splitCount != 8)
        {
            throw ColumnDecodeError.Create<PgMacAddress>(
                value.ColumnMetadata,
                $"Expected 6 or 8 address hex characters. Found {splitCount}");
        }

        Span<byte> bytes = stackalloc byte[splitCount];
        for (var i = 0; i < splitCount; i++)
        {
            Range rng = splits[i];
            if (rng.End.Value - rng.Start.Value != 2)
            {
                throw ColumnDecodeError.Create<PgMacAddress>(
                    value.ColumnMetadata,
                    $"Could not parse network location bytes from '{value}'");
            }

            bytes[i] = (byte)HexUtils.CharsToDigit<PgColumnMetadata>(
                value.Chars[rng],
                value.ColumnMetadata);
        }

        var index = 0;
        return new PgMacAddress(
            bytes[index++],
            bytes[index++],
            bytes[index++],
            bytes.Length == 8 ? bytes[index++] : DefaultD,
            bytes.Length == 8 ? bytes[index++] : DefaultE,
            bytes[index++],
            bytes[index++],
            bytes[index]);
    }

    public static PgType DbType => PgType.Macaddr;

    public static bool IsCompatible(PgType dbType)
    {
        return dbType.TypeOid == DbType.TypeOid || dbType.TypeOid == PgType.Macaddr8.TypeOid;
    }

    public static PgType GetActualType(PgMacAddress value)
    {
        return value.IsMacAddress8 ? PgType.Macaddr8 : PgType.Macaddr;
    }
}
