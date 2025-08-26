using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

public readonly record struct PgTimeTz(TimeOnly Time, int OffsetSeconds) : IPgDbType<PgTimeTz>
{
    private const int OffsetStart = 8;
    
    public static void Encode(PgTimeTz value, WriteBuffer buffer)
    {
        PgTime.Encode(value.Time, buffer);
        buffer.WriteInt(value.OffsetSeconds);
    }

    public static PgTimeTz DecodeBytes(PgBinaryValue value)
    {
        TimeOnly time = PgTime.DecodeBytes(value);
        var offsetSeconds = value.Buffer.ReadInt() * -1;
        return new PgTimeTz(time, offsetSeconds);
    }

    public static PgTimeTz DecodeText(PgTextValue value)
    {
        var offsetSeconds = FindOffset(value, value.ColumnMetadata);
        TimeOnly time = PgTime.DecodeText(value.Slice(..OffsetStart));
        return new PgTimeTz(time, offsetSeconds);
    }

    public static PgType DbType => PgType.Timetz;

    public static bool IsCompatible(PgType dbType)
    {
        return dbType.TypeOid == DbType.TypeOid;
    }

    public static PgType GetActualType(PgTimeTz value)
    {
        return DbType;
    }

    private static int FindOffset(ReadOnlySpan<char> chars, PgColumnMetadata columnMetadata)
    {
        if (chars.Length < OffsetStart)
        {
            return 0;
        }
        
        var offsetChar = chars[OffsetStart];
        if (offsetChar is 'Z')
        {
            return 0;
        }
        if (offsetChar is not ('+' or '-'))
        {
            throw ColumnDecodeError.Create<PgTimeTz>(
                columnMetadata,
                $"Invalid offset char: {chars}");
        }

        var offsetChars = chars[(OffsetStart + 1)..];
        Span<Range> splits = stackalloc Range[3];
        var rangeCount = offsetChars.Split(splits, ':');

        var factor = offsetChar == '+' ? 1 : -1;
        var offset = 0;
        var digitMultiplier = 2;
        for (var i = 0; i < rangeCount; i++)
        {
            if (!int.TryParse(offsetChars[splits[i]], null, out var result))
            {
                throw ColumnDecodeError.Create<PgTimeTz>(
                    columnMetadata,
                    $"Could not parse offset from '{chars}'");
            }
            offset += result * (int)Math.Pow(60.0, digitMultiplier--);
        }

        return offset * factor;
    }
}
