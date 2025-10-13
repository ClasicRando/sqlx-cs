using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

/// <summary>
/// <para>
/// Postgres <c>INTERVAL</c> type represented as a number of months, days and microseconds. This
/// differs from <see cref="TimeSpan"/> since it has resolution of months.
/// </para>
/// <a href="https://www.postgresql.org/docs/current/datatype-geometric.html#DATATYPE-GEOMETRIC-POINTS">docs</a>
/// </summary>
public readonly record struct PgInterval(int Months, int Days, long Microseconds)
    : IPgDbType<PgInterval>, IHasArrayType
{
    /// <summary>
    /// Convert this interval to a <see cref="TimeSpan"/>
    /// </summary>
    /// <returns>
    /// A <see cref="TimeSpan"/> that represents the same duration as this interval
    /// </returns>
    /// <exception cref="ArgumentException">
    /// If this interval has a non-zero number of <see cref="Months"/>
    /// </exception>
    public TimeSpan ToTimeSpan()
    {
        return Months > 0
            ? throw new ArgumentException("TimeSpan does not support a month value", nameof(Months))
            : new TimeSpan(Microseconds * TimeSpan.TicksPerMicrosecond + Days * TimeSpan.TicksPerDay);
    }
    
    /// <inheritdoc cref="IPgDbType{T}.Encode"/>
    /// <summary>
    /// <para>
    /// Writes 3 values of the <see cref="PgInterval"/> to represent the interval:
    /// <list type="number">
    ///     <item><see cref="long"/> - whole micro seconds of the time portion</item>
    ///     <item><see cref="int"/> - number of days</item>
    ///     <item><see cref="int"/> - number of total months</item>
    /// </list>
    /// </para>
    /// <a href="https://github.com/postgres/postgres/blob/874d817baa160ca7e68bee6ccc9fc1848c56e750/src/backend/utils/adt/timestamp.c#L1007">pg source code</a>
    /// </summary>
    public static void Encode(PgInterval value, WriteBuffer buffer)
    {
        buffer.WriteLong(value.Microseconds);
        buffer.WriteInt(value.Days);
        buffer.WriteInt(value.Months);
    }

    /// <inheritdoc cref="IPgDbType{T}.DecodeBytes"/>
    /// <summary>
    /// <para>
    /// Reads 3 values to create a <see cref="PgInterval"/>:
    /// <list type="number">
    ///     <item><see cref="long"/> - whole micro seconds of the time portion</item>
    ///     <item><see cref="int"/> - number of days</item>
    ///     <item><see cref="int"/> - number of total months</item>
    /// </list>
    /// </para>
    /// <a href="https://github.com/postgres/postgres/blob/874d817baa160ca7e68bee6ccc9fc1848c56e750/src/backend/utils/adt/timestamp.c#L1032">pg source code</a>
    /// </summary>
    public static PgInterval DecodeBytes(ref PgBinaryValue value)
    {
        var microSeconds = value.Buffer.ReadLong();
        var days = value.Buffer.ReadInt();
        var months = value.Buffer.ReadInt();
        return new PgInterval(months, days, microSeconds);
    }

    /// <inheritdoc cref="IPgDbType{T}.DecodeText"/>
    /// <summary>
    /// <para>
    /// Attempt to parse the characters into a <see cref="PgInterval"/>. The expected format is
    /// ISO-8601 (this is the interval format specified when connecting to the database).
    /// </para>
    /// <a href="https://github.com/postgres/postgres/blob/874d817baa160ca7e68bee6ccc9fc1848c56e750/src/backend/utils/adt/timestamp.c#L983">pg source code</a>
    /// </summary>
    /// <exception cref="ColumnDecodeException">
    /// If either characters are not a valid ISO-8601 interval
    /// </exception>
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
                    throw ColumnDecodeException.Create<PgInterval>(
                        value.ColumnMetadata,
                        $"Unexpected character in interval. Interval: '{value}', char: '{currentChar}'");
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

    public static PgType ArrayDbType => PgType.IntervalArray;

    public static bool IsCompatible(PgType dbType)
    {
        return dbType == DbType;
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

public static class TimeSpanExtensions
{
    /// <summary>
    /// Convert this <see cref="TimeSpan"/> to a <see cref="PgInterval"/>. Captures the number of
    /// whole days and microseconds outside of those days.
    /// </summary>
    /// <param name="timeSpan">This time span</param>
    /// <returns>
    /// A <see cref="PgInterval"/> that represents the same duration as this <see cref="TimeSpan"/>
    /// </returns>
    public static PgInterval ToPgInterval(this TimeSpan timeSpan)
    {
        var days = timeSpan.Days;
        var ticksWithoutDays = timeSpan.Ticks - (TimeSpan.TicksPerDay * days);
        return new PgInterval(0, days, ticksWithoutDays / TimeSpan.TicksPerMicrosecond);
    }
}
