using System.Runtime.CompilerServices;
using Sqlx.Core.Result;
using Sqlx.Postgres.Exceptions;
using Sqlx.Postgres.Generator.Result;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Result;

public static partial class DataRowExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TDecode? GetPgDecode<TDecode>(this IDataRow dataRow, int index)
        where TDecode : IPgDbType<TDecode>
    {
        if (dataRow.IsNull(index))
        {
            return default;
        }
        PgDataRow pgDataRow = PgException.CheckIfIs<IDataRow, PgDataRow>(dataRow);
        return pgDataRow.DecodeNotNull<TDecode, TDecode>(index);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TDecode GetPgDecodeNotNull<TDecode>(this IDataRow dataRow, int index)
        where TDecode : IPgDbType<TDecode>
    {
        PgDataRow pgDataRow = PgException.CheckIfIs<IDataRow, PgDataRow>(dataRow);
        return pgDataRow.DecodeNotNull<TDecode, TDecode>(index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TDecode? GetPgDecode<TDecode>(this IDataRow dataRow, string name)
        where TDecode : IPgDbType<TDecode>
    {
        return GetPgDecode<TDecode>(dataRow, dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TDecode GetPgDecodeNotNull<TDecode>(this IDataRow dataRow, string name)
        where TDecode : IPgDbType<TDecode>
    {
        return GetPgDecodeNotNull<TDecode>(dataRow, dataRow.IndexOf(name));
    }

    [GeneratePgDecodeMethod]
    public static partial PgTimeTz? GetPgTimeTz(this IDataRow dataRow, int index);
    
    [GeneratePgDecodeMethod]
    public static partial PgTimeTz GetPgTimeTzNotNull(this IDataRow dataRow, int index);
    
    [GeneratePgDecodeMethod]
    public static partial PgTimeTz? GetPgTimeTz(this IDataRow dataRow, string name);
    
    [GeneratePgDecodeMethod]
    public static partial PgTimeTz GetPgTimeTzNotNull(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod]
    public static partial PgPoint? GetPgPoint(this IDataRow dataRow, int index);
    
    [GeneratePgDecodeMethod]
    public static partial PgPoint GetPgPointNotNull(this IDataRow dataRow, int index);
    
    [GeneratePgDecodeMethod]
    public static partial PgPoint? GetPgPoint(this IDataRow dataRow, string name);
    
    [GeneratePgDecodeMethod]
    public static partial PgPoint GetPgPointNotNull(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod]
    public static partial PgLine? GetPgLine(this IDataRow dataRow, int index);
    
    [GeneratePgDecodeMethod]
    public static partial PgLine GetPgLineNotNull(this IDataRow dataRow, int index);
    
    [GeneratePgDecodeMethod]
    public static partial PgLine? GetPgLine(this IDataRow dataRow, string name);
    
    [GeneratePgDecodeMethod]
    public static partial PgLine GetPgLineNotNull(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod]
    public static partial PgLineSegment? GetPgLineSegment(this IDataRow dataRow, int index);
    
    [GeneratePgDecodeMethod]
    public static partial PgLineSegment GetPgLineSegmentNotNull(this IDataRow dataRow, int index);
    
    [GeneratePgDecodeMethod]
    public static partial PgLineSegment? GetPgLineSegment(this IDataRow dataRow, string name);
    
    [GeneratePgDecodeMethod]
    public static partial PgLineSegment GetPgLineSegmentNotNull(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod]
    public static partial PgBox? GetPgBox(this IDataRow dataRow, int index);
    
    [GeneratePgDecodeMethod]
    public static partial PgBox GetPgBoxNotNull(this IDataRow dataRow, int index);
    
    [GeneratePgDecodeMethod]
    public static partial PgBox? GetPgBox(this IDataRow dataRow, string name);
    
    [GeneratePgDecodeMethod]
    public static partial PgBox GetPgBoxNotNull(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod]
    public static partial PgPath? GetPgPath(this IDataRow dataRow, int index);
    
    [GeneratePgDecodeMethod]
    public static partial PgPath GetPgPathNotNull(this IDataRow dataRow, int index);
    
    [GeneratePgDecodeMethod]
    public static partial PgPath? GetPgPath(this IDataRow dataRow, string name);
    
    [GeneratePgDecodeMethod]
    public static partial PgPath GetPgPathNotNull(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod]
    public static partial PgCircle? GetPgCircle(this IDataRow dataRow, int index);
    
    [GeneratePgDecodeMethod]
    public static partial PgCircle GetPgCircleNotNull(this IDataRow dataRow, int index);
    
    [GeneratePgDecodeMethod]
    public static partial PgCircle? GetPgCircle(this IDataRow dataRow, string name);
    
    [GeneratePgDecodeMethod]
    public static partial PgCircle GetPgCircleNotNull(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod]
    public static partial PgPolygon? GetPgPolygon(this IDataRow dataRow, int index);
    
    [GeneratePgDecodeMethod]
    public static partial PgPolygon GetPgPolygonNotNull(this IDataRow dataRow, int index);
    
    [GeneratePgDecodeMethod]
    public static partial PgPolygon? GetPgPolygon(this IDataRow dataRow, string name);
    
    [GeneratePgDecodeMethod]
    public static partial PgPolygon GetPgPolygonNotNull(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod]
    public static partial PgInterval? GetPgInterval(this IDataRow dataRow, int index);
    
    [GeneratePgDecodeMethod]
    public static partial PgInterval GetPgIntervalNotNull(this IDataRow dataRow, int index);
    
    [GeneratePgDecodeMethod]
    public static partial PgInterval? GetPgInterval(this IDataRow dataRow, string name);
    
    [GeneratePgDecodeMethod]
    public static partial PgInterval GetPgIntervalNotNull(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod]
    public static partial PgMacAddress? GetPgMacAddress(this IDataRow dataRow, int index);
    
    [GeneratePgDecodeMethod]
    public static partial PgMacAddress GetPgMacAddressNotNull(this IDataRow dataRow, int index);
    
    [GeneratePgDecodeMethod]
    public static partial PgMacAddress? GetPgMacAddress(this IDataRow dataRow, string name);
    
    [GeneratePgDecodeMethod]
    public static partial PgMacAddress GetPgMacAddressNotNull(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod]
    public static partial PgMoney? GetPgMoney(this IDataRow dataRow, int index);
    
    [GeneratePgDecodeMethod]
    public static partial PgMoney GetPgMoneyNotNull(this IDataRow dataRow, int index);
    
    [GeneratePgDecodeMethod]
    public static partial PgMoney? GetPgMoney(this IDataRow dataRow, string name);
    
    [GeneratePgDecodeMethod]
    public static partial PgMoney GetPgMoneyNotNull(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod]
    public static partial PgInet? GetPgInet(this IDataRow dataRow, int index);
    
    [GeneratePgDecodeMethod]
    public static partial PgInet GetPgInetNotNull(this IDataRow dataRow, int index);
    
    [GeneratePgDecodeMethod]
    public static partial PgInet? GetPgInet(this IDataRow dataRow, string name);
    
    [GeneratePgDecodeMethod]
    public static partial PgInet GetPgInetNotNull(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<long, PgLong>))]
    public static partial PgRange<long>? GetPgRangeLong(this IDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<long, PgLong>))]
    public static partial PgRange<long> GetPgRangeLongNotNull(this IDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<long, PgLong>))]
    public static partial PgRange<long>? GetPgRangeLong(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<long, PgLong>))]
    public static partial PgRange<long> GetPgRangeLongNotNull(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<int, PgInt>))]
    public static partial PgRange<int>? GetPgRangeInt(this IDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<int, PgInt>))]
    public static partial PgRange<int> GetPgRangeIntNotNull(this IDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<int, PgInt>))]
    public static partial PgRange<int>? GetPgRangeInt(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<int, PgInt>))]
    public static partial PgRange<int> GetPgRangeIntNotNull(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<DateOnly, PgDate>))]
    public static partial PgRange<DateOnly>? GetPgRangeDate(this IDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<DateOnly, PgDate>))]
    public static partial PgRange<DateOnly> GetPgRangeDateNotNull(this IDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<DateOnly, PgDate>))]
    public static partial PgRange<DateOnly>? GetPgRangeDate(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<DateOnly, PgDate>))]
    public static partial PgRange<DateOnly> GetPgRangeDateNotNull(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<DateTime, PgDateTime>))]
    public static partial PgRange<DateTime>? GetPgRangeDateTime(this IDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<DateTime, PgDateTime>))]
    public static partial PgRange<DateTime> GetPgRangeDateTimeNotNull(this IDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<DateTime, PgDateTime>))]
    public static partial PgRange<DateTime>? GetPgRangeDateTime(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<DateTime, PgDateTime>))]
    public static partial PgRange<DateTime> GetPgRangeDateTimeNotNull(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<DateTimeOffset, PgDateTimeOffset>))]
    public static partial PgRange<DateTimeOffset>? GetPgRangeDateTimeOffset(this IDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<DateTimeOffset, PgDateTimeOffset>))]
    public static partial PgRange<DateTimeOffset> GetPgRangeDateTimeOffsetNotNull(this IDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<DateTimeOffset, PgDateTimeOffset>))]
    public static partial PgRange<DateTimeOffset>? GetPgRangeDateTimeOffset(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<DateTimeOffset, PgDateTimeOffset>))]
    public static partial PgRange<DateTimeOffset> GetPgRangeDateTimeOffsetNotNull(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<decimal, PgDecimal>))]
    public static partial PgRange<decimal>? GetPgRangeDecimal(this IDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<decimal, PgDecimal>))]
    public static partial PgRange<decimal> GetPgRangeDecimalNotNull(this IDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<decimal, PgDecimal>))]
    public static partial PgRange<decimal>? GetPgRangeDecimal(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<decimal, PgDecimal>))]
    public static partial PgRange<decimal> GetPgRangeDecimalNotNull(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgBool))]
    public static partial bool?[]? GetPgBooleanArray(this IDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgBool))]
    public static partial bool?[] GetPgBooleanArrayNotNull(this IDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgBool))]
    public static partial bool?[]? GetPgBooleanArray(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgBool))]
    public static partial bool?[] GetPgBooleanArrayNotNull(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgChar))]
    public static partial sbyte?[]? GetPgCharArray(this IDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgChar))]
    public static partial sbyte?[] GetPgCharArrayNotNull(this IDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgChar))]
    public static partial sbyte?[]? GetPgCharArray(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgChar))]
    public static partial sbyte?[] GetPgCharArrayNotNull(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgShort))]
    public static partial short?[]? GetPgShortArray(this IDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgShort))]
    public static partial short?[] GetPgShortArrayNotNull(this IDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgShort))]
    public static partial short?[]? GetPgShortArray(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgShort))]
    public static partial short?[] GetPgShortArrayNotNull(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgInt))]
    public static partial int?[]? GetPgIntArray(this IDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgInt))]
    public static partial int?[] GetPgIntArrayNotNull(this IDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgInt))]
    public static partial int?[]? GetPgIntArray(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgInt))]
    public static partial int?[] GetPgIntArrayNotNull(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgLong))]
    public static partial long?[]? GetPgLongArray(this IDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgLong))]
    public static partial long?[] GetPgLongArrayNotNull(this IDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgLong))]
    public static partial long?[]? GetPgLongArray(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgLong))]
    public static partial long?[] GetPgLongArrayNotNull(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgFloat))]
    public static partial float?[]? GetPgFloatArray(this IDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgFloat))]
    public static partial float?[] GetPgFloatArrayNotNull(this IDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgFloat))]
    public static partial float?[]? GetPgFloatArray(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgFloat))]
    public static partial float?[] GetPgFloatArrayNotNull(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgDouble))]
    public static partial double?[]? GetPgDoubleArray(this IDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgDouble))]
    public static partial double?[] GetPgDoubleArrayNotNull(this IDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgDouble))]
    public static partial double?[]? GetPgDoubleArray(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgDouble))]
    public static partial double?[] GetPgDoubleArrayNotNull(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgTime))]
    public static partial TimeOnly?[]? GetPgTimeArray(this IDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgTime))]
    public static partial TimeOnly?[] GetPgTimeArrayNotNull(this IDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgTime))]
    public static partial TimeOnly?[]? GetPgTimeArray(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgTime))]
    public static partial TimeOnly?[] GetPgTimeArrayNotNull(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgDate))]
    public static partial DateOnly?[]? GetPgDateArray(this IDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgDate))]
    public static partial DateOnly?[] GetPgDateArrayNotNull(this IDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgDate))]
    public static partial DateOnly?[]? GetPgDateArray(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgDate))]
    public static partial DateOnly?[] GetPgDateArrayNotNull(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgDateTime))]
    public static partial DateTime?[]? GetPgDateTimeArray(this IDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgDateTime))]
    public static partial DateTime?[] GetPgDateTimeArrayNotNull(this IDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgDateTime))]
    public static partial DateTime?[]? GetPgDateTimeArray(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgDateTime))]
    public static partial DateTime?[] GetPgDateTimeArrayNotNull(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgDateTimeOffset))]
    public static partial DateTimeOffset?[]? GetPgDateTimeOffsetArray(this IDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgDateTimeOffset))]
    public static partial DateTimeOffset?[] GetPgDateTimeOffsetArrayNotNull(this IDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgDateTimeOffset))]
    public static partial DateTimeOffset?[]? GetPgDateTimeOffsetArray(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgDateTimeOffset))]
    public static partial DateTimeOffset?[] GetPgDateTimeOffsetArrayNotNull(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgDecimal))]
    public static partial decimal?[]? GetPgDecimalArray(this IDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgDecimal))]
    public static partial decimal?[] GetPgDecimalArrayNotNull(this IDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgDecimal))]
    public static partial decimal?[]? GetPgDecimalArray(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgDecimal))]
    public static partial decimal?[] GetPgDecimalArrayNotNull(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgBytea))]
    public static partial byte[]?[]? GetPgBytesArray(this IDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgBytea))]
    public static partial byte[]?[] GetPgBytesArrayNotNull(this IDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgBytea))]
    public static partial byte[]?[]? GetPgBytesArray(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgBytea))]
    public static partial byte[]?[] GetPgBytesArrayNotNull(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgString))]
    public static partial string?[]? GetPgStringArray(this IDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgString))]
    public static partial string?[] GetPgStringNotNull(this IDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgString))]
    public static partial string?[]? GetPgStringArray(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgString))]
    public static partial string?[] GetPgStringNotNull(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgUuid))]
    public static partial Guid?[]? GetPgGuidArray(this IDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgUuid))]
    public static partial Guid?[] GetPgGuidArrayNotNull(this IDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgUuid))]
    public static partial Guid?[]? GetPgGuidArray(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgUuid))]
    public static partial Guid?[] GetPgGuidArrayNotNull(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod]
    public static partial PgTimeTz?[]? GetPgTimeTzArray(this IDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgTimeTz))]
    public static partial PgTimeTz?[] GetPgTimeTzArrayNotNull(this IDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgTimeTz))]
    public static partial PgTimeTz?[]? GetPgTimeTzArray(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgTimeTz))]
    public static partial PgTimeTz?[] GetPgTimeTzArrayNotNull(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgPoint))]
    public static partial PgPoint?[]? GetPgPointArray(this IDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgPoint))]
    public static partial PgPoint?[] GetPgPointArrayNotNull(this IDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgPoint))]
    public static partial PgPoint?[]? GetPgPointArray(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgPoint))]
    public static partial PgPoint?[] GetPgPointArrayNotNull(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgLine))]
    public static partial PgLine?[]? GetPgLineArray(this IDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgLine))]
    public static partial PgLine?[] GetPgLineArrayNotNull(this IDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgLine))]
    public static partial PgLine?[]? GetPgLineArray(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgLine))]
    public static partial PgLine?[] GetPgLineArrayNotNull(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgLineSegment))]
    public static partial PgLineSegment?[]? GetPgLineSegmentArray(this IDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgLineSegment))]
    public static partial PgLineSegment?[] GetPgLineSegmentArrayNotNull(this IDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgLineSegment))]
    public static partial PgLineSegment?[]? GetPgLineSegmentArray(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgLineSegment))]
    public static partial PgLineSegment?[] GetPgLineSegmentArrayNotNull(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgBox))]
    public static partial PgBox?[]? GetPgBoxArray(this IDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgBox))]
    public static partial PgBox?[] GetPgBoxArrayNotNull(this IDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgBox))]
    public static partial PgBox?[]? GetPgBoxArray(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgBox))]
    public static partial PgBox?[] GetPgBoxArrayNotNull(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgPath))]
    public static partial PgPath?[]? GetPgPathArray(this IDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgPath))]
    public static partial PgPath?[] GetPgPathArrayNotNull(this IDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgPath))]
    public static partial PgPath?[]? GetPgPathArray(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgPath))]
    public static partial PgPath?[] GetPgPathArrayNotNull(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgCircle))]
    public static partial PgCircle?[]? GetPgCircleArray(this IDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgCircle))]
    public static partial PgCircle?[] GetPgCircleArrayNotNull(this IDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgCircle))]
    public static partial PgCircle?[]? GetPgCircleArray(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgCircle))]
    public static partial PgCircle?[] GetPgCircleArrayNotNull(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgPolygon))]
    public static partial PgPolygon?[]? GetPgPolygonArray(this IDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgPolygon))]
    public static partial PgPolygon?[] GetPgPolygonArrayNotNull(this IDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgPolygon))]
    public static partial PgPolygon?[]? GetPgPolygonArray(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgPolygon))]
    public static partial PgPolygon?[] GetPgPolygonArrayNotNull(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgInterval))]
    public static partial PgInterval?[]? GetPgIntervalArray(this IDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgInterval))]
    public static partial PgInterval?[] GetPgIntervalArrayNotNull(this IDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgInterval))]
    public static partial PgInterval?[]? GetPgIntervalArray(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgInterval))]
    public static partial PgInterval?[] GetPgIntervalArrayNotNull(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgMacAddress))]
    public static partial PgMacAddress?[]? GetPgMacAddressArray(this IDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgMacAddress))]
    public static partial PgMacAddress?[] GetPgMacAddressArrayNotNull(this IDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgMacAddress))]
    public static partial PgMacAddress?[]? GetPgMacAddressArray(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgMacAddress))]
    public static partial PgMacAddress?[] GetPgMacAddressArrayNotNull(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgMoney))]
    public static partial PgMoney?[]? GetPgMoneyArray(this IDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgMoney))]
    public static partial PgMoney?[] GetPgMoneyArrayNotNull(this IDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgMoney))]
    public static partial PgMoney?[]? GetPgMoneyArray(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgMoney))]
    public static partial PgMoney?[] GetPgMoneyArrayNotNull(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgInet))]
    public static partial PgInet?[]? GetPgInetArray(this IDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgInet))]
    public static partial PgInet?[] GetPgInetArrayNotNull(this IDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgInet))]
    public static partial PgInet?[]? GetPgInetArray(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgInet))]
    public static partial PgInet?[] GetPgInetArrayNotNull(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<long, PgLong>))]
    public static partial PgRange<long>?[]? GetPgRangeLongArray(this IDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<long, PgLong>))]
    public static partial PgRange<long>?[] GetPgRangeLongArrayNotNull(this IDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<long, PgLong>))]
    public static partial PgRange<long>?[]? GetPgRangeLongArray(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<long, PgLong>))]
    public static partial PgRange<long>?[] GetPgRangeLongArrayNotNull(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<int, PgInt>))]
    public static partial PgRange<int>?[]? GetPgRangeIntArray(this IDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<int, PgInt>))]
    public static partial PgRange<int>?[] GetPgRangeIntArrayNotNull(this IDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<int, PgInt>))]
    public static partial PgRange<int>?[]? GetPgRangeIntArray(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<int, PgInt>))]
    public static partial PgRange<int>?[] GetPgRangeIntArrayNotNull(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<DateOnly, PgDate>))]
    public static partial PgRange<DateOnly>?[]? GetPgRangeDateArray(this IDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<DateOnly, PgDate>))]
    public static partial PgRange<DateOnly>?[] GetPgRangeDateArrayNotNull(this IDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<DateOnly, PgDate>))]
    public static partial PgRange<DateOnly>?[]? GetPgRangeDateArray(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<DateOnly, PgDate>))]
    public static partial PgRange<DateOnly>?[] GetPgRangeDateArrayNotNull(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<DateTime, PgDateTime>))]
    public static partial PgRange<DateTime>?[]? GetPgRangeDateTimeArray(this IDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<DateTime, PgDateTime>))]
    public static partial PgRange<DateTime>?[] GetPgRangeDateTimeArrayNotNull(this IDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<DateTime, PgDateTime>))]
    public static partial PgRange<DateTime>?[]? GetPgRangeDateTimeArray(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<DateTime, PgDateTime>))]
    public static partial PgRange<DateTime>?[] GetPgRangeDateTimeArrayNotNull(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<DateTimeOffset, PgDateTimeOffset>))]
    public static partial PgRange<DateTimeOffset>?[]? GetPgRangeDateTimeOffsetArray(this IDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<DateTimeOffset, PgDateTimeOffset>))]
    public static partial PgRange<DateTimeOffset>?[] GetPgRangeDateTimeOffsetArrayNotNull(this IDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<DateTimeOffset, PgDateTimeOffset>))]
    public static partial PgRange<DateTimeOffset>?[]? GetPgRangeDateTimeOffsetArray(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<DateTimeOffset, PgDateTimeOffset>))]
    public static partial PgRange<DateTimeOffset>?[] GetPgRangeDateTimeOffsetArrayNotNull(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<decimal, PgDecimal>))]
    public static partial PgRange<decimal>?[]? GetPgRangeDecimalArray(this IDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<decimal, PgDecimal>))]
    public static partial PgRange<decimal>?[] GetPgRangeDecimalArrayNotNull(this IDataRow dataRow, int index);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<decimal, PgDecimal>))]
    public static partial PgRange<decimal>?[]? GetPgRangeDecimalArray(this IDataRow dataRow, string name);

    [GeneratePgDecodeMethod(Decoder = typeof(PgRangeType<decimal, PgDecimal>))]
    public static partial PgRange<decimal>?[] GetPgRangeDecimalArrayNotNull(this IDataRow dataRow, string name);
}
