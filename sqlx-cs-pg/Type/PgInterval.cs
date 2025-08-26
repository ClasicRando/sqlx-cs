using Sqlx.Core.Buffer;
using Sqlx.Postgres.Exceptions;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

public readonly record struct PgInterval(int Months, int Days, long Microseconds)
    : IPgDbType<PgInterval>
{
    public TimeSpan ToTimeSpan()
    {
        if (Months > 0)
        {
            throw new ArgumentException("TimeSpan does not support a month value", nameof(Months));
        }
        return new TimeSpan(Microseconds * TimeSpan.TicksPerMicrosecond + Days * TimeSpan.TicksPerDay);
    }
    
    public static void Encode(PgInterval value, WriteBuffer buffer)
    {
        buffer.WriteLong(value.Microseconds);
        buffer.WriteInt(value.Days);
        buffer.WriteInt(value.Months);
    }

    public static PgInterval DecodeBytes(PgBinaryValue value)
    {
        var microSeconds = value.Buffer.ReadLong();
        var days = value.Buffer.ReadInt();
        var months = value.Buffer.ReadInt();
        return new PgInterval(months, days, microSeconds);
    }

    public static PgInterval DecodeText(PgTextValue value)
    {
        char currentChar;
        var currentNumber = 0;
        var isAfterT = false;
        var isFractionalSecond = false;
        var scale = 1;
        var year = 0;
        var month = 0;
        var week = 0;
        var day = 0;
        var hour = 0;
        var minute = 0;
        var second = 0;
        var millisecond = 0;
        var microsecond = 0;

        ReadOnlySpan<char>.Enumerator charEnumerator = value.Chars.GetEnumerator();
        while (!isFractionalSecond && charEnumerator.MoveNext())
        {
            currentChar = charEnumerator.Current;
            switch (currentChar)
            {
                case 'P':
                    break;
                case 'Y':
                    year = currentNumber * scale;
                    currentNumber = 0;
                    scale = 1;
                    break;
                case 'M':
                    if (isAfterT)
                    {
                        minute = currentNumber * scale;
                    }
                    else
                    {
                        month = currentNumber * scale;
                    }
                    currentNumber = 0;
                    scale = 1;
                    break;
                case 'W':
                    week = currentNumber * scale;
                    currentNumber = 0;
                    scale = 1;
                    break;
                case 'D':
                    day = currentNumber * scale;
                    currentNumber = 0;
                    scale = 1;
                    break;
                case 'T':
                    isAfterT = true;
                    break;
                case 'H':
                    hour = currentNumber * scale;
                    currentNumber = 0;
                    scale = 1;
                    break;
                case 'S':
                    second = currentNumber * scale;
                    currentNumber = 0;
                    break;
                case '+':
                    scale = 1;
                    break;
                case '-':
                    scale = -1;
                    break;
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    currentNumber = currentNumber * 10 + (currentChar - '0');
                    break;
                case '.':
                    second = currentNumber * scale;
                    currentNumber = 0;
                    isFractionalSecond = true;
                    break;
                default:
                    throw new PgException($"Unexpected character in interval. Interval: '{value}', char: '{currentChar}'");
            }
        }

        var secondFractionsParsed = 0;
        while (charEnumerator.MoveNext())
        {
            currentChar = charEnumerator.Current;
            switch (currentChar)
            {
                case 'S':
                    var padding = secondFractionsParsed % 3;
                    if (padding != 0)
                    {
                        for (var i = 0; i < 3 - padding; i++)
                        {
                            currentNumber *= 10;
                        }
                    }

                    if (secondFractionsParsed > 3)
                    {
                        microsecond = currentNumber * scale;
                    }
                    else
                    {
                        millisecond = currentNumber * scale;
                    }
                    break;
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    currentNumber = currentNumber * 10 + (currentChar - '0');
                    if (++secondFractionsParsed == 3)
                    {
                        millisecond = currentNumber * scale;
                        currentNumber = 0;
                    }
                    break;
            }
        }

        return new PgInterval(
            year * 12 + month,
            week * 7 + day,
            hour * MicrosecondsPerHour +
                minute * MicrosecondsPerMinute +
                second * MicrosecondsPerSecond +
                millisecond * MicrosecondsPerMillisecond +
                microsecond);
    }

    public static PgType DbType => PgType.Interval;

    public static bool IsCompatible(PgType dbType)
    {
        return dbType.TypeOid == DbType.TypeOid;
    }

    public static PgType GetActualType(PgInterval value)
    {
        return DbType;
    }

    private const long MinutesPerHour = 60L;
    private const long SecondsPerMinute = 60L;
    private const long MicrosecondsPerSecond = 1_000_000L;
    private const long MicrosecondsPerMillisecond = 1_000L;
    private const long MicrosecondsPerMinute = SecondsPerMinute * MicrosecondsPerSecond;
    private const long MicrosecondsPerHour = MinutesPerHour * MicrosecondsPerMinute;
}

internal abstract class TimeSpanType : IPgDbType<TimeSpan>
{
    public static void Encode(TimeSpan value, WriteBuffer buffer)
    {
        var interval = value.ToPgInterval();
        PgInterval.Encode(interval, buffer);
    }

    public static TimeSpan DecodeBytes(PgBinaryValue value)
    {
        return PgInterval.DecodeBytes(value).ToTimeSpan();
    }

    public static TimeSpan DecodeText(PgTextValue value)
    {
        return PgInterval.DecodeText(value).ToTimeSpan();
    }

    public static PgType DbType => PgType.Interval;

    public static bool IsCompatible(PgType dbType)
    {
        return dbType.TypeOid == DbType.TypeOid;
    }

    public static PgType GetActualType(TimeSpan value)
    {
        return DbType;
    }
}

public static class TimeSpanExtensions
{
    public static PgInterval ToPgInterval(this TimeSpan timeSpan)
    {
        return new PgInterval(
            0,
            timeSpan.Days,
            timeSpan.Seconds * 1_000_000 + timeSpan.Milliseconds * 1_000 + timeSpan.Microseconds);
    }
}
