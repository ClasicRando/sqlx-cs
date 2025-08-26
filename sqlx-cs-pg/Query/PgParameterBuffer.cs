using System.Text.Json.Serialization.Metadata;
using Sqlx.Core.Buffer;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Query;

internal class PgParameterBuffer : IDisposable
{
    private readonly WriteBuffer _buffer = new();
    private readonly List<PgType> _pgTypes = [];

    public short ParameterCount => (short)_pgTypes.Count;
    public ReadOnlyMemory<byte> Memory => _buffer.Memory;
    public IReadOnlyList<PgType> PgTypes => _pgTypes;
    
    public void Encode<T>(T? value) where T : notnull
    {
        switch (value)
        {
            case null:
                EncodeNull();
                break;
            case bool boolValue:
                EncodeValue<bool, PgBool>(boolValue);
                break;
            case short shortValue:
                EncodeValue<short, PgShort>(shortValue);
                break;
            case int intValue:
                EncodeValue<int, PgInt>(intValue);
                break;
            case long longValue:
                EncodeValue<long, PgLong>(longValue);
                break;
            case float floatValue:
                EncodeValue<float, PgFloat>(floatValue);
                break;
            case double doubleValue:
                EncodeValue<double, PgDouble>(doubleValue);
                break;
            case decimal decimalValue:
                EncodeValue<decimal, PgDecimal>(decimalValue);
                break;
            case string stringValue:
                EncodeValue<string, PgString>(stringValue);
                break;
            case byte[] bytes:
                EncodeValue<byte[], PgBytea>(bytes);
                break;
            case byte chr:
                EncodeValue<byte, PgChar>(chr);
                break;
            case DateOnly date:
                EncodeValue<DateOnly, PgDate>(date);
                break;
            case TimeOnly time:
                EncodeValue<TimeOnly, PgTime>(time);
                break;
            case PgTimeTz timeTz:
                EncodeValue<PgTimeTz, PgTimeTz>(timeTz);
                break;
            case DateTime dateTime:
                EncodeValue<DateTime, PgDateTime>(dateTime);
                break;
            case DateTimeOffset dateTimeOffset:
                EncodeValue<DateTimeOffset, PgDateTimeOffset>(dateTimeOffset);
                break;
            case Guid guid:
                EncodeValue<Guid, PgUuid>(guid);
                break;
            case PgPoint point:
                EncodeValue<PgPoint, PgPoint>(point);
                break;
            case PgLine line:
                EncodeValue<PgLine, PgLine>(line);
                break;
            case PgLineSegment lineSegment:
                EncodeValue<PgLineSegment, PgLineSegment>(lineSegment);
                break;
            case PgBox box:
                EncodeValue<PgBox, PgBox>(box);
                break;
            case PgPath path:
                EncodeValue<PgPath, PgPath>(path);
                break;
            case PgPolygon polygon:
                EncodeValue<PgPolygon, PgPolygon>(polygon);
                break;
            case PgCircle circle:
                EncodeValue<PgCircle, PgCircle>(circle);
                break;
            case PgInterval interval:
                EncodeValue<PgInterval, PgInterval>(interval);
                break;
            case TimeSpan timeSpan:
                EncodeValue<TimeSpan, TimeSpanType>(timeSpan);
                break;
            case PgMacAddress macAddress:
                EncodeValue<PgMacAddress, PgMacAddress>(macAddress);
                break;
            case PgMoney money:
                EncodeValue<PgMoney, PgMoney>(money);
                break;
            case PgInet inet:
                EncodeValue<PgInet, PgInet>(inet);
                break;
            case PgRange<long> int8Range:
                EncodeValue<PgRange<long>, PgRangeType<long, PgLong>>(int8Range);
                break;
            case PgRange<int> int4Range:
                EncodeValue<PgRange<int>, PgRangeType<int, PgInt>>(int4Range);
                break;
            case PgRange<DateOnly> dateRange:
                EncodeValue<PgRange<DateOnly>, PgRangeType<DateOnly, PgDate>>(dateRange);
                break;
            case PgRange<decimal> numericRange:
                EncodeValue<PgRange<decimal>, PgRangeType<decimal, PgDecimal>>(numericRange);
                break;
        }
    }

    public void EncodeNull()
    {
        _buffer.WriteInt(-1);
        _pgTypes.Add(PgType.Unspecified);
    }

    private void EncodeValue<TValue, TPgType>(TValue value)
        where TValue : notnull
        where TPgType : IPgDbType<TValue>
    {
        _buffer.WriteLengthPrefixed(false, buf => TPgType.Encode(value, buf));
        _pgTypes.Add(TPgType.DbType);
    }

    public void EncodeJsonValue<T>(T value, JsonTypeInfo<T>? typeInfo) where T : notnull
    {
        _buffer.WriteLengthPrefixed(false, buf => PgJson<T>.Encode(value, buf, typeInfo));
        _pgTypes.Add(PgJson<T>.DbType);
    }

    public void Dispose()
    {
        _buffer.Dispose();
    }
}
