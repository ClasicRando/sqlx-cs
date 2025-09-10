using System.Runtime.CompilerServices;
using System.Text.Json.Serialization.Metadata;
using Sqlx.Core;
using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Core.Result;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Exceptions;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Result;

internal sealed class PgDataRow : IDataRow
{
    private readonly byte[] _rowData;
    private readonly PgColumnMetadata[] _columnMetadata;
    private readonly Range?[] _columnValueSlices;
    
    public PgDataRow(byte[] rowData, PgColumnMetadata[] columnMetadata)
    {
        _rowData = rowData;
        _columnMetadata = columnMetadata;
        var buffer = new ReadBuffer(rowData);
        int columnCount = buffer.ReadShort();
        _columnValueSlices = new Range?[columnCount];
        for (var i = 0; i < columnCount; i++)
        {
            var length = buffer.ReadInt();
            if (length < 0)
            {
                _columnValueSlices[i] = null;
                continue;
            }
            _columnValueSlices[i] = buffer.Skip(length);
        }
    }
    
    public int IndexOf(string name)
    {
        for (var i = 0; i < _columnMetadata.Length; i++)
        {
            if (_columnMetadata[i].FieldName == name)
            {
                return i;
            }
        }

        return -1;
    }

    public bool? GetBoolean(int index)
    {
        return Decode<bool, PgBool>(index);
    }

    public sbyte? GetByte(int index)
    {
        return Decode<sbyte, PgChar>(index);
    }

    public short? GetShort(int index)
    {
        return Decode<short, PgShort>(index);
    }

    public int? GetInt(int index)
    {
        return Decode<int, PgInt>(index);
    }

    public long? GetLong(int index)
    {
        return Decode<long, PgLong>(index);
    }

    public float? GetFloat(int index)
    {
        return Decode<float, PgFloat>(index);
    }

    public double? GetDouble(int index)
    {
        return Decode<double, PgDouble>(index);
    }

    public TimeOnly? GetTime(int index)
    {
        return Decode<TimeOnly, PgTime>(index);
    }

    public DateOnly? GetDate(int index)
    {
        return Decode<DateOnly, PgDate>(index);
    }

    public DateTime? GetDateTime(int index)
    {
        return Decode<DateTime, PgDateTime>(index);
    }

    public DateTimeOffset? GetDateTimeOffset(int index)
    {
        return Decode<DateTimeOffset, PgDateTimeOffset>(index);
    }

    public decimal? GetDecimal(int index)
    {
        return Decode<decimal, PgDecimal>(index);
    }

    public byte[]? GetBytes(int index)
    {
        return Decode<byte[], PgBytea>(index);
    }

    public string? GetString(int index)
    {
        return Decode<string, PgString>(index);
    }

    public Guid? GetGuid(int index)
    {
        return Decode<Guid, PgUuid>(index);
    }

    public T? GetJson<T>(int index, JsonTypeInfo<T>? jsonTypeInfo = null) where T : notnull
    {
        ColumnData columnData = GetColumnData(index);
        return columnData.IsNull ? default : GetJsonInternal(columnData, jsonTypeInfo);
    }

    public bool GetBooleanNotNull(int index)
    {
        return DecodeNotNull<bool, PgBool>(index);
    }

    public sbyte GetByteNotNull(int index)
    {
        return DecodeNotNull<sbyte, PgChar>(index);
    }

    public short GetShortNotNull(int index)
    {
        return DecodeNotNull<short, PgShort>(index);
    }

    public int GetIntNotNull(int index)
    {
        return DecodeNotNull<int, PgInt>(index);
    }

    public long GetLongNotNull(int index)
    {
        return DecodeNotNull<long, PgLong>(index);
    }

    public float GetFloatNotNull(int index)
    {
        return DecodeNotNull<float, PgFloat>(index);
    }

    public double GetDoubleNotNull(int index)
    {
        return DecodeNotNull<double, PgDouble>(index);
    }

    public TimeOnly GetTimeNotNull(int index)
    {
        return DecodeNotNull<TimeOnly, PgTime>(index);
    }

    public DateOnly GetDateNotNull(int index)
    {
        return DecodeNotNull<DateOnly, PgDate>(index);
    }

    public DateTime GetDateTimeNotNull(int index)
    {
        return DecodeNotNull<DateTime, PgDateTime>(index);
    }

    public DateTimeOffset GetDateTimeOffsetNotNull(int index)
    {
        return DecodeNotNull<DateTimeOffset, PgDateTimeOffset>(index);
    }

    public decimal GetDecimalNotNull(int index)
    {
        return DecodeNotNull<decimal, PgDecimal>(index);
    }

    public byte[] GetBytesNotNull(int index)
    {
        return DecodeNotNull<byte[], PgBytea>(index);
    }

    public string GetStringNotNull(int index)
    {
        return DecodeNotNull<string, PgString>(index);
    }

    public Guid GetGuidNotNull(int index)
    {
        return DecodeNotNull<Guid, PgUuid>(index);
    }

    public T GetJsonNotNull<T>(int index, JsonTypeInfo<T>? jsonTypeInfo = null) where T : notnull
    {
        ColumnData columnData = GetColumnData(index);
        return columnData.IsNull
            ? throw new SqlxException($"Expected field #{index} to be non-null but found null")
            : GetJsonInternal(columnData, jsonTypeInfo);
    }

    private T GetJsonInternal<T>(ColumnData columnData, JsonTypeInfo<T>? jsonTypeInfo)
        where T : notnull
    {
        if (PgJson<T>.DbType != columnData.ColumnMetadata.PgType && !PgJson<T>.IsCompatible(columnData.ColumnMetadata.PgType))
        {
            throw ColumnDecodeError.Create<T>(columnData.ColumnMetadata);
        }

        var bytes = _rowData.AsSpan()[columnData.Range.Start..columnData.Range.End];
        switch (columnData.ColumnMetadata.FormatCode)
        {
            case PgFormatCode.Text:
                Span<char> chars = stackalloc char[Charsets.Default.GetCharCount(bytes)];
                Charsets.Default.GetChars(bytes, chars);
                return PgJson<T>.DecodeText(new PgTextValue(chars, columnData.ColumnMetadata), jsonTypeInfo);
            case PgFormatCode.Binary:
                var buffer = new ReadBuffer(bytes);
                return PgJson<T>.DecodeBytes(new PgBinaryValue(buffer, columnData.ColumnMetadata), jsonTypeInfo);
            default:
                throw ColumnDecodeError.Create<T>(
                    columnData.ColumnMetadata,
                    $"Unexpected format code: {columnData.ColumnMetadata.FormatCode}");
        }
    }

    private readonly ref struct ColumnData(
        bool isNull,
        Range range,
        PgColumnMetadata columnMetadata)
    {
        public bool IsNull { get; } = isNull;
        public Range Range { get; } = range;
        public PgColumnMetadata ColumnMetadata { get; } = columnMetadata;
    }

    private ColumnData GetColumnData(int index)
    {
        PgColumnMetadata columnMetadata = _columnMetadata[index];
        var sliceItem = _columnValueSlices[index];
        return sliceItem is not {} slice
            ? new ColumnData(true, new Range(), columnMetadata)
            : new ColumnData(false, slice, columnMetadata);
    }

    internal TResult? Decode<TResult, TType>(int index)
        where TResult : notnull
        where TType : IPgDbType<TResult>
    {
        ColumnData columnData = GetColumnData(index);
        return !columnData.IsNull ? Decode<TResult, TType>(columnData) : default;
    }

    internal TResult DecodeNotNull<TResult, TType>(int index)
        where TResult : notnull
        where TType : IPgDbType<TResult>
    {
        ColumnData columnData = GetColumnData(index);
        return !columnData.IsNull
            ? Decode<TResult, TType>(columnData)
            : throw new SqlxException($"Expected field #{index} to be non-null but found null");
    }

    private TResult Decode<TResult, TType>(ColumnData columnData)
        where TResult : notnull
        where TType : IPgDbType<TResult>
    {
        if (TType.DbType != columnData.ColumnMetadata.PgType && !TType.IsCompatible(columnData.ColumnMetadata.PgType))
        {
            throw ColumnDecodeError.Create<TResult>(columnData.ColumnMetadata);
        }

        var bytes = _rowData.AsSpan()[columnData.Range.Start..columnData.Range.End];
        switch (columnData.ColumnMetadata.FormatCode)
        {
            case PgFormatCode.Text:
                Span<char> chars = stackalloc char[Charsets.Default.GetCharCount(bytes)];
                Charsets.Default.GetChars(bytes, chars);
                return TType.DecodeText(new PgTextValue(chars, columnData.ColumnMetadata));
            case PgFormatCode.Binary:
                var buffer = new ReadBuffer(bytes);
                return TType.DecodeBytes(new PgBinaryValue(buffer, columnData.ColumnMetadata));
            default:
                throw ColumnDecodeError.Create<TResult>(
                    columnData.ColumnMetadata,
                    $"Unexpected format code: {columnData.ColumnMetadata.FormatCode}");
        }
    }
}

public static class DataRowExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TDecode? GetPgDecode<TDecode>(this IDataRow dataRow, int index)
        where TDecode : IPgDbType<TDecode>
    {
        return GetPgDecodeSelfInternal<TDecode>(dataRow, index);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TDecode GetPgDecodeNotNull<TDecode>(this IDataRow dataRow, int index)
        where TDecode : IPgDbType<TDecode>
    {
        return GetPgDecodeSelfNotNullInternal<TDecode>(dataRow, index);
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
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgTimeTz? GetPgTimeTz(this IDataRow dataRow, int index)
    {
        return GetPgDecode<PgTimeTz>(dataRow, index);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgTimeTz GetPgTimeTzNotNull(this IDataRow dataRow, int index)
    {
        return GetPgDecodeSelfNotNullInternal<PgTimeTz>(dataRow, index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgTimeTz? GetPgTimeTz(this IDataRow dataRow, string name)
    {
        return GetPgTimeTz(dataRow, dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgTimeTz GetPgTimeTzNotNull(this IDataRow dataRow, string name)
    {
        return GetPgTimeTzNotNull(dataRow, dataRow.IndexOf(name));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgPoint? GetPgPoint(this IDataRow dataRow, int index)
    {
        return GetPgDecode<PgPoint>(dataRow, index);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgPoint GetPgPointNotNull(this IDataRow dataRow, int index)
    {
        return GetPgDecodeSelfNotNullInternal<PgPoint>(dataRow, index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgPoint? GetPgPoint(this IDataRow dataRow, string name)
    {
        return GetPgPoint(dataRow, dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgPoint GetPgPointNotNull(this IDataRow dataRow, string name)
    {
        return GetPgPointNotNull(dataRow, dataRow.IndexOf(name));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgLine? GetPgLine(this IDataRow dataRow, int index)
    {
        return GetPgDecode<PgLine>(dataRow, index);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgLine GetPgLineNotNull(this IDataRow dataRow, int index)
    {
        return GetPgDecodeSelfNotNullInternal<PgLine>(dataRow, index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgLine? GetPgLine(this IDataRow dataRow, string name)
    {
        return GetPgLine(dataRow, dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgLine GetPgLineNotNull(this IDataRow dataRow, string name)
    {
        return GetPgLineNotNull(dataRow, dataRow.IndexOf(name));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgLineSegment? GetPgLineSegment(this IDataRow dataRow, int index)
    {
        return GetPgDecode<PgLineSegment>(dataRow, index);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgLineSegment GetPgLineSegmentNotNull(this IDataRow dataRow, int index)
    {
        return GetPgDecodeSelfNotNullInternal<PgLineSegment>(dataRow, index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgLineSegment? GetPgLineSegment(this IDataRow dataRow, string name)
    {
        return GetPgLineSegment(dataRow, dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgLineSegment GetPgLineSegmentNotNull(this IDataRow dataRow, string name)
    {
        return GetPgLineSegmentNotNull(dataRow, dataRow.IndexOf(name));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgBox? GetPgBox(this IDataRow dataRow, int index)
    {
        return GetPgDecode<PgBox>(dataRow, index);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgBox GetPgBoxNotNull(this IDataRow dataRow, int index)
    {
        return GetPgDecodeSelfNotNullInternal<PgBox>(dataRow, index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgBox? GetPgBox(this IDataRow dataRow, string name)
    {
        return GetPgBox(dataRow, dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgBox GetPgBoxNotNull(this IDataRow dataRow, string name)
    {
        return GetPgBoxNotNull(dataRow, dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgPath? GetPgPath(this IDataRow dataRow, int index)
    {
        return GetPgDecode<PgPath>(dataRow, index);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgPath GetPgPathNotNull(this IDataRow dataRow, int index)
    {
        return GetPgDecodeSelfNotNullInternal<PgPath>(dataRow, index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgPath? GetPgPath(this IDataRow dataRow, string name)
    {
        return GetPgPath(dataRow, dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgPath GetPgPathNotNull(this IDataRow dataRow, string name)
    {
        return GetPgPathNotNull(dataRow, dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgCircle? GetPgCircle(this IDataRow dataRow, int index)
    {
        return GetPgDecode<PgCircle>(dataRow, index);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgCircle GetPgCircleNotNull(this IDataRow dataRow, int index)
    {
        return GetPgDecodeSelfNotNullInternal<PgCircle>(dataRow, index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgCircle? GetPgCircle(this IDataRow dataRow, string name)
    {
        return GetPgCircle(dataRow, dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgCircle GetPgCircleNotNull(this IDataRow dataRow, string name)
    {
        return GetPgCircleNotNull(dataRow, dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgPolygon? GetPgPolygon(this IDataRow dataRow, int index)
    {
        return GetPgDecode<PgPolygon>(dataRow, index);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgPolygon GetPgPolygonNotNull(this IDataRow dataRow, int index)
    {
        return GetPgDecodeSelfNotNullInternal<PgPolygon>(dataRow, index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgPolygon? GetPgPolygon(this IDataRow dataRow, string name)
    {
        return GetPgPolygon(dataRow, dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgPolygon GetPgPolygonNotNull(this IDataRow dataRow, string name)
    {
        return GetPgPolygonNotNull(dataRow, dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgInterval? GetPgInterval(this IDataRow dataRow, int index)
    {
        return GetPgDecode<PgInterval>(dataRow, index);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgInterval GetPgIntervalNotNull(this IDataRow dataRow, int index)
    {
        return GetPgDecodeSelfNotNullInternal<PgInterval>(dataRow, index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgInterval? GetPgInterval(this IDataRow dataRow, string name)
    {
        return GetPgInterval(dataRow, dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgInterval GetPgIntervalNotNull(this IDataRow dataRow, string name)
    {
        return GetPgIntervalNotNull(dataRow, dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgMacAddress? GetPgMacAddress(this IDataRow dataRow, int index)
    {
        return GetPgDecode<PgMacAddress>(dataRow, index);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgMacAddress GetPgMacAddressNotNull(this IDataRow dataRow, int index)
    {
        return GetPgDecodeSelfNotNullInternal<PgMacAddress>(dataRow, index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgMacAddress? GetPgMacAddress(this IDataRow dataRow, string name)
    {
        return GetPgMacAddress(dataRow, dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgMacAddress GetPgMacAddressNotNull(this IDataRow dataRow, string name)
    {
        return GetPgMacAddressNotNull(dataRow, dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgMoney? GetPgMoney(this IDataRow dataRow, int index)
    {
        return GetPgDecode<PgMoney>(dataRow, index);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgMoney GetPgMoneyNotNull(this IDataRow dataRow, int index)
    {
        return GetPgDecodeSelfNotNullInternal<PgMoney>(dataRow, index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgMoney? GetPgMoney(this IDataRow dataRow, string name)
    {
        return GetPgMoney(dataRow, dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgMoney GetPgMoneyNotNull(this IDataRow dataRow, string name)
    {
        return GetPgMoneyNotNull(dataRow, dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgInet? GetPgInet(this IDataRow dataRow, int index)
    {
        return GetPgDecode<PgInet>(dataRow, index);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgInet GetPgInetNotNull(this IDataRow dataRow, int index)
    {
        return GetPgDecodeSelfNotNullInternal<PgInet>(dataRow, index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgInet? GetPgInet(this IDataRow dataRow, string name)
    {
        return GetPgInet(dataRow, dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgInet GetPgInetNotNull(this IDataRow dataRow, string name)
    {
        return GetPgInetNotNull(dataRow, dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgRange<long>? GetPgRangeLong(this IDataRow dataRow, int index)
    {
        return GetPgDecodeInternal<PgRange<long>, PgRangeType<long, PgLong>>(dataRow, index);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgRange<long> GetPgRangeLongNotNull(this IDataRow dataRow, int index)
    {
        return GetPgDecodeNotNullInternal<PgRange<long>, PgRangeType<long, PgLong>>(
            dataRow,
            index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgRange<long>? GetPgRangeLong(this IDataRow dataRow, string name)
    {
        return GetPgRangeLong(dataRow, dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgRange<long> GetPgRangeLongNotNull(this IDataRow dataRow, string name)
    {
        return GetPgRangeLongNotNull(dataRow, dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgRange<int>? GetPgRangeInt(this IDataRow dataRow, int index)
    {
        return GetPgDecodeInternal<PgRange<int>, PgRangeType<int, PgInt>>(dataRow, index);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgRange<int> GetPgRangeIntNotNull(this IDataRow dataRow, int index)
    {
        return GetPgDecodeNotNullInternal<PgRange<int>, PgRangeType<int, PgInt>>(
            dataRow,
            index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgRange<int>? GetPgRangeInt(this IDataRow dataRow, string name)
    {
        return GetPgRangeInt(dataRow, dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgRange<int> GetPgRangeIntNotNull(this IDataRow dataRow, string name)
    {
        return GetPgRangeIntNotNull(dataRow, dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgRange<DateOnly>? GetPgRangeDate(this IDataRow dataRow, int index)
    {
        return GetPgDecodeInternal<PgRange<DateOnly>, PgRangeType<DateOnly, PgDate>>(dataRow, index);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgRange<DateOnly> GetPgRangeDateNotNull(this IDataRow dataRow, int index)
    {
        return GetPgDecodeNotNullInternal<PgRange<DateOnly>, PgRangeType<DateOnly, PgDate>>(
            dataRow,
            index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgRange<DateOnly>? GetPgRangeDate(this IDataRow dataRow, string name)
    {
        return GetPgRangeDate(dataRow, dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgRange<DateOnly> GetPgRangeDateOnlyNotNull(this IDataRow dataRow, string name)
    {
        return GetPgRangeDateNotNull(dataRow, dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgRange<DateTime>? GetPgRangeDateTime(this IDataRow dataRow, int index)
    {
        return GetPgDecodeInternal<PgRange<DateTime>, PgRangeType<DateTime, PgDateTime>>(dataRow, index);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgRange<DateTime> GetPgRangeDateTimeNotNull(this IDataRow dataRow, int index)
    {
        return GetPgDecodeNotNullInternal<PgRange<DateTime>, PgRangeType<DateTime, PgDateTime>>(
            dataRow,
            index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgRange<DateTime>? GetPgRangeDateTime(this IDataRow dataRow, string name)
    {
        return GetPgRangeDateTime(dataRow, dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgRange<DateTime> GetPgRangeDateTimeNotNull(this IDataRow dataRow, string name)
    {
        return GetPgRangeDateTimeNotNull(dataRow, dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgRange<DateTimeOffset>? GetPgRangeDateTimeOffset(this IDataRow dataRow, int index)
    {
        return GetPgDecodeInternal<PgRange<DateTimeOffset>, PgRangeType<DateTimeOffset, PgDateTimeOffset>>(dataRow, index);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgRange<DateTimeOffset> GetPgRangeDateTimeOffsetNotNull(this IDataRow dataRow, int index)
    {
        return GetPgDecodeNotNullInternal<PgRange<DateTimeOffset>, PgRangeType<DateTimeOffset, PgDateTimeOffset>>(
            dataRow,
            index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgRange<DateTimeOffset>? GetPgRangeDateTimeOffset(this IDataRow dataRow, string name)
    {
        return GetPgRangeDateTimeOffset(dataRow, dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgRange<DateTimeOffset> GetPgRangeDateTimeOffsetNotNull(this IDataRow dataRow, string name)
    {
        return GetPgRangeDateTimeOffsetNotNull(dataRow, dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgRange<decimal>? GetPgRangeDecimal(this IDataRow dataRow, int index)
    {
        return GetPgDecodeInternal<PgRange<decimal>, PgRangeType<decimal, PgDecimal>>(dataRow, index);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgRange<decimal> GetPgRangeDecimalNotNull(this IDataRow dataRow, int index)
    {
        return GetPgDecodeNotNullInternal<PgRange<decimal>, PgRangeType<decimal, PgDecimal>>(
            dataRow,
            index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgRange<decimal>? GetPgRangeDecimal(this IDataRow dataRow, string name)
    {
        return GetPgRangeDecimal(dataRow, dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgRange<decimal> GetPgRangeDecimalNotNull(this IDataRow dataRow, string name)
    {
        return GetPgRangeDecimalNotNull(dataRow, dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool?[]? GetPgBooleanArray(this IDataRow dataRow, int index)
    {
        return GetPgDecodeArrayStructInternal<bool, PgBool>(dataRow, index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool?[] GetPgBooleanArrayNotNull(this IDataRow dataRow, int index)
    {
        return GetPgDecodeArrayStructNotNullInternal<bool, PgBool>(dataRow, index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool?[]? GetPgBooleanArray(this IDataRow dataRow, string name)
    {
        return GetPgBooleanArray(dataRow, dataRow.IndexOf(name));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool?[] GetPgBooleanArrayNotNull(this IDataRow dataRow, string name)
    {
        return GetPgBooleanArrayNotNull(dataRow, dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static sbyte?[]? GetPgCharArray(this IDataRow dataRow, int index)
    {
        return GetPgDecodeArrayStructInternal<sbyte, PgChar>(dataRow, index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static sbyte?[] GetPgCharArrayNotNull(this IDataRow dataRow, int index)
    {
        return GetPgDecodeArrayStructNotNullInternal<sbyte, PgChar>(dataRow, index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static sbyte?[]? GetPgCharArray(this IDataRow dataRow, string name)
    {
        return GetPgCharArray(dataRow, dataRow.IndexOf(name));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static sbyte?[] GetPgCharArrayNotNull(this IDataRow dataRow, string name)
    {
        return GetPgCharArrayNotNull(dataRow, dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static short?[]? GetPgShortArray(this IDataRow dataRow, int index)
    {
        return GetPgDecodeArrayStructInternal<short, PgShort>(dataRow, index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static short?[] GetPgShortArrayNotNull(this IDataRow dataRow, int index)
    {
        return GetPgDecodeArrayStructNotNullInternal<short, PgShort>(dataRow, index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static short?[]? GetPgShortArray(this IDataRow dataRow, string name)
    {
        return GetPgShortArray(dataRow, dataRow.IndexOf(name));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static short?[] GetPgShortArrayNotNull(this IDataRow dataRow, string name)
    {
        return GetPgShortArrayNotNull(dataRow, dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int?[]? GetPgIntArray(this IDataRow dataRow, int index)
    {
        return GetPgDecodeArrayStructInternal<int, PgInt>(dataRow, index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int?[] GetPgIntArrayNotNull(this IDataRow dataRow, int index)
    {
        return GetPgDecodeArrayStructNotNullInternal<int, PgInt>(dataRow, index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int?[]? GetPgIntArray(this IDataRow dataRow, string name)
    {
        return GetPgIntArray(dataRow, dataRow.IndexOf(name));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int?[] GetPgIntArrayNotNull(this IDataRow dataRow, string name)
    {
        return GetPgIntArrayNotNull(dataRow, dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long?[]? GetPgLongArray(this IDataRow dataRow, int index)
    {
        return GetPgDecodeArrayStructInternal<long, PgLong>(dataRow, index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long?[] GetPgLongArrayNotNull(this IDataRow dataRow, int index)
    {
        return GetPgDecodeArrayStructNotNullInternal<long, PgLong>(dataRow, index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long?[]? GetPgLongArray(this IDataRow dataRow, string name)
    {
        return GetPgLongArray(dataRow, dataRow.IndexOf(name));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long?[] GetPgLongArrayNotNull(this IDataRow dataRow, string name)
    {
        return GetPgLongArrayNotNull(dataRow, dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float?[]? GetPgFloatArray(this IDataRow dataRow, int index)
    {
        return GetPgDecodeArrayStructInternal<float, PgFloat>(dataRow, index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float?[] GetPgFloatArrayNotNull(this IDataRow dataRow, int index)
    {
        return GetPgDecodeArrayStructNotNullInternal<float, PgFloat>(dataRow, index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float?[]? GetPgFloatArray(this IDataRow dataRow, string name)
    {
        return GetPgFloatArray(dataRow, dataRow.IndexOf(name));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float?[] GetPgFloatArrayNotNull(this IDataRow dataRow, string name)
    {
        return GetPgFloatArrayNotNull(dataRow, dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double?[]? GetPgDoubleArray(this IDataRow dataRow, int index)
    {
        return GetPgDecodeArrayStructInternal<double, PgDouble>(dataRow, index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double?[] GetPgDoubleArrayNotNull(this IDataRow dataRow, int index)
    {
        return GetPgDecodeArrayStructNotNullInternal<double, PgDouble>(dataRow, index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double?[]? GetPgDoubleArray(this IDataRow dataRow, string name)
    {
        return GetPgDoubleArray(dataRow, dataRow.IndexOf(name));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double?[] GetPgDoubleArrayNotNull(this IDataRow dataRow, string name)
    {
        return GetPgDoubleArrayNotNull(dataRow, dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TimeOnly?[]? GetPgTimeArray(this IDataRow dataRow, int index)
    {
        return GetPgDecodeArrayStructInternal<TimeOnly, PgTime>(dataRow, index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TimeOnly?[] GetPgTimeArrayNotNull(this IDataRow dataRow, int index)
    {
        return GetPgDecodeArrayStructNotNullInternal<TimeOnly, PgTime>(dataRow, index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TimeOnly?[]? GetPgTimeArray(this IDataRow dataRow, string name)
    {
        return GetPgTimeArray(dataRow, dataRow.IndexOf(name));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TimeOnly?[] GetPgTimeArrayNotNull(this IDataRow dataRow, string name)
    {
        return GetPgTimeArrayNotNull(dataRow, dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateOnly?[]? GetPgDateArray(this IDataRow dataRow, int index)
    {
        return GetPgDecodeArrayStructInternal<DateOnly, PgDate>(dataRow, index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateOnly?[] GetPgDateArrayNotNull(this IDataRow dataRow, int index)
    {
        return GetPgDecodeArrayStructNotNullInternal<DateOnly, PgDate>(dataRow, index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateOnly?[]? GetPgDateArray(this IDataRow dataRow, string name)
    {
        return GetPgDateArray(dataRow, dataRow.IndexOf(name));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateOnly?[] GetPgDateArrayNotNull(this IDataRow dataRow, string name)
    {
        return GetPgDateArrayNotNull(dataRow, dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTime?[]? GetPgDateTimeArray(this IDataRow dataRow, int index)
    {
        return GetPgDecodeArrayStructInternal<DateTime, PgDateTime>(dataRow, index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTime?[] GetPgDateTimeArrayNotNull(this IDataRow dataRow, int index)
    {
        return GetPgDecodeArrayStructNotNullInternal<DateTime, PgDateTime>(dataRow, index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTime?[]? GetPgDateTimeArray(this IDataRow dataRow, string name)
    {
        return GetPgDateTimeArray(dataRow, dataRow.IndexOf(name));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTime?[] GetPgDateTimeArrayNotNull(this IDataRow dataRow, string name)
    {
        return GetPgDateTimeArrayNotNull(dataRow, dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTimeOffset?[]? GetPgDateTimeOffsetArray(this IDataRow dataRow, int index)
    {
        return GetPgDecodeArrayStructInternal<DateTimeOffset, PgDateTimeOffset>(dataRow, index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTimeOffset?[] GetPgDateTimeOffsetArrayNotNull(this IDataRow dataRow, int index)
    {
        return GetPgDecodeArrayStructNotNullInternal<DateTimeOffset, PgDateTimeOffset>(dataRow, index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTimeOffset?[]? GetPgDateTimeOffsetArray(this IDataRow dataRow, string name)
    {
        return GetPgDateTimeOffsetArray(dataRow, dataRow.IndexOf(name));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTimeOffset?[] GetPgDateTimeOffsetArrayNotNull(this IDataRow dataRow, string name)
    {
        return GetPgDateTimeOffsetArrayNotNull(dataRow, dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static decimal?[]? GetPgDecimalArray(this IDataRow dataRow, int index)
    {
        return GetPgDecodeArrayStructInternal<decimal, PgDecimal>(dataRow, index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static decimal?[] GetPgDecimalArrayNotNull(this IDataRow dataRow, int index)
    {
        return GetPgDecodeArrayStructNotNullInternal<decimal, PgDecimal>(dataRow, index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static decimal?[]? GetPgDecimalArray(this IDataRow dataRow, string name)
    {
        return GetPgDecimalArray(dataRow, dataRow.IndexOf(name));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static decimal?[] GetPgDecimalArrayNotNull(this IDataRow dataRow, string name)
    {
        return GetPgDecimalArrayNotNull(dataRow, dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[]?[]? GetPgBytesArray(this IDataRow dataRow, int index)
    {
        return GetPgDecodeArrayClassInternal<byte[], PgBytea>(dataRow, index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[]?[] GetPgBytesArrayNotNull(this IDataRow dataRow, int index)
    {
        return GetPgDecodeArrayClassNotNullInternal<byte[], PgBytea>(dataRow, index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[]?[]? GetPgBytesArray(this IDataRow dataRow, string name)
    {
        return GetPgBytesArray(dataRow, dataRow.IndexOf(name));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[]?[] GetPgBytesArrayNotNull(this IDataRow dataRow, string name)
    {
        return GetPgBytesArrayNotNull(dataRow, dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string?[]? GetPgStringArray(this IDataRow dataRow, int index)
    {
        return GetPgDecodeArrayClassInternal<string, PgString>(dataRow, index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string?[] GetPgStringNotNull(this IDataRow dataRow, int index)
    {
        return GetPgDecodeArrayClassNotNullInternal<string, PgString>(dataRow, index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string?[]? GetPgStringArray(this IDataRow dataRow, string name)
    {
        return GetPgStringArray(dataRow, dataRow.IndexOf(name));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string?[] GetPgStringNotNull(this IDataRow dataRow, string name)
    {
        return GetPgStringNotNull(dataRow, dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Guid?[]? GetPgGuidArray(this IDataRow dataRow, int index)
    {
        return GetPgDecodeArrayStructInternal<Guid, PgUuid>(dataRow, index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Guid?[] GetPgGuidArrayNotNull(this IDataRow dataRow, int index)
    {
        return GetPgDecodeArrayStructNotNullInternal<Guid, PgUuid>(dataRow, index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Guid?[]? GetPgGuidArray(this IDataRow dataRow, string name)
    {
        return GetPgGuidArray(dataRow, dataRow.IndexOf(name));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Guid?[] GetPgGuidArrayNotNull(this IDataRow dataRow, string name)
    {
        return GetPgGuidArrayNotNull(dataRow, dataRow.IndexOf(name));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgTimeTz?[]? GetPgTimeTzArray(this IDataRow dataRow, int index)
    {
        return GetPgDecodeArrayStructInternal<PgTimeTz, PgTimeTz>(dataRow, index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgTimeTz?[] GetPgTimeTzArrayNotNull(this IDataRow dataRow, int index)
    {
        return GetPgDecodeArrayStructNotNullInternal<PgTimeTz, PgTimeTz>(dataRow, index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgTimeTz?[]? GetPgTimeTzArray(this IDataRow dataRow, string name)
    {
        return GetPgTimeTzArray(dataRow, dataRow.IndexOf(name));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgTimeTz?[] GetPgTimeTzArrayNotNull(this IDataRow dataRow, string name)
    {
        return GetPgTimeTzArrayNotNull(dataRow, dataRow.IndexOf(name));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgPoint?[]? GetPgPointArray(this IDataRow dataRow, int index)
    {
        return GetPgDecodeArrayStructInternal<PgPoint, PgPoint>(dataRow, index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgPoint?[] GetPgPointArrayNotNull(this IDataRow dataRow, int index)
    {
        return GetPgDecodeArrayStructNotNullInternal<PgPoint, PgPoint>(dataRow, index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgPoint?[]? GetPgPointArray(this IDataRow dataRow, string name)
    {
        return GetPgPointArray(dataRow, dataRow.IndexOf(name));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgPoint?[] GetPgPointArrayNotNull(this IDataRow dataRow, string name)
    {
        return GetPgPointArrayNotNull(dataRow, dataRow.IndexOf(name));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgLine?[]? GetPgLineArray(this IDataRow dataRow, int index)
    {
        return GetPgDecodeArrayStructInternal<PgLine, PgLine>(dataRow, index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgLine?[] GetPgLineArrayNotNull(this IDataRow dataRow, int index)
    {
        return GetPgDecodeArrayStructNotNullInternal<PgLine, PgLine>(dataRow, index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgLine?[]? GetPgLineArray(this IDataRow dataRow, string name)
    {
        return GetPgLineArray(dataRow, dataRow.IndexOf(name));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgLine?[] GetPgLineArrayNotNull(this IDataRow dataRow, string name)
    {
        return GetPgLineArrayNotNull(dataRow, dataRow.IndexOf(name));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgLineSegment?[]? GetPgLineSegmentArray(this IDataRow dataRow, int index)
    {
        return GetPgDecodeArrayStructInternal<PgLineSegment, PgLineSegment>(dataRow, index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgLineSegment?[] GetPgLineSegmentArrayNotNull(this IDataRow dataRow, int index)
    {
        return GetPgDecodeArrayStructNotNullInternal<PgLineSegment, PgLineSegment>(dataRow, index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgLineSegment?[]? GetPgLineSegmentArray(this IDataRow dataRow, string name)
    {
        return GetPgLineSegmentArray(dataRow, dataRow.IndexOf(name));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgLineSegment?[] GetPgLineSegmentArrayNotNull(this IDataRow dataRow, string name)
    {
        return GetPgLineSegmentArrayNotNull(dataRow, dataRow.IndexOf(name));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgBox?[]? GetPgBoxArray(this IDataRow dataRow, int index)
    {
        return GetPgDecodeArrayStructInternal<PgBox, PgBox>(dataRow, index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgBox?[] GetPgBoxArrayNotNull(this IDataRow dataRow, int index)
    {
        return GetPgDecodeArrayStructNotNullInternal<PgBox, PgBox>(dataRow, index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgBox?[]? GetPgBoxArray(this IDataRow dataRow, string name)
    {
        return GetPgBoxArray(dataRow, dataRow.IndexOf(name));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgBox?[] GetPgBoxArrayNotNull(this IDataRow dataRow, string name)
    {
        return GetPgBoxArrayNotNull(dataRow, dataRow.IndexOf(name));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgPath?[]? GetPgPathArray(this IDataRow dataRow, int index)
    {
        return GetPgDecodeArrayStructInternal<PgPath, PgPath>(dataRow, index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgPath?[] GetPgPathArrayNotNull(this IDataRow dataRow, int index)
    {
        return GetPgDecodeArrayStructNotNullInternal<PgPath, PgPath>(dataRow, index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgPath?[]? GetPgPathArray(this IDataRow dataRow, string name)
    {
        return GetPgPathArray(dataRow, dataRow.IndexOf(name));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgPath?[] GetPgPathArrayNotNull(this IDataRow dataRow, string name)
    {
        return GetPgPathArrayNotNull(dataRow, dataRow.IndexOf(name));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgCircle?[]? GetPgCircleArray(this IDataRow dataRow, int index)
    {
        return GetPgDecodeArrayStructInternal<PgCircle, PgCircle>(dataRow, index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgCircle?[] GetPgCircleArrayNotNull(this IDataRow dataRow, int index)
    {
        return GetPgDecodeArrayStructNotNullInternal<PgCircle, PgCircle>(dataRow, index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgCircle?[]? GetPgCircleArray(this IDataRow dataRow, string name)
    {
        return GetPgCircleArray(dataRow, dataRow.IndexOf(name));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgCircle?[] GetPgCircleArrayNotNull(this IDataRow dataRow, string name)
    {
        return GetPgCircleArrayNotNull(dataRow, dataRow.IndexOf(name));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgPolygon?[]? GetPgPolygonArray(this IDataRow dataRow, int index)
    {
        return GetPgDecodeArrayStructInternal<PgPolygon, PgPolygon>(dataRow, index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgPolygon?[] GetPgPolygonArrayNotNull(this IDataRow dataRow, int index)
    {
        return GetPgDecodeArrayStructNotNullInternal<PgPolygon, PgPolygon>(dataRow, index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgPolygon?[]? GetPgPolygonArray(this IDataRow dataRow, string name)
    {
        return GetPgPolygonArray(dataRow, dataRow.IndexOf(name));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgPolygon?[] GetPgPolygonArrayNotNull(this IDataRow dataRow, string name)
    {
        return GetPgPolygonArrayNotNull(dataRow, dataRow.IndexOf(name));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgInterval?[]? GetPgIntervalArray(this IDataRow dataRow, int index)
    {
        return GetPgDecodeArrayStructInternal<PgInterval, PgInterval>(dataRow, index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgInterval?[] GetPgIntervalArrayNotNull(this IDataRow dataRow, int index)
    {
        return GetPgDecodeArrayStructNotNullInternal<PgInterval, PgInterval>(dataRow, index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgInterval?[]? GetPgIntervalArray(this IDataRow dataRow, string name)
    {
        return GetPgIntervalArray(dataRow, dataRow.IndexOf(name));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgInterval?[] GetPgIntervalArrayNotNull(this IDataRow dataRow, string name)
    {
        return GetPgIntervalArrayNotNull(dataRow, dataRow.IndexOf(name));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgMacAddress?[]? GetPgMacAddressArray(this IDataRow dataRow, int index)
    {
        return GetPgDecodeArrayStructInternal<PgMacAddress, PgMacAddress>(dataRow, index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgMacAddress?[] GetPgMacAddressArrayNotNull(this IDataRow dataRow, int index)
    {
        return GetPgDecodeArrayStructNotNullInternal<PgMacAddress, PgMacAddress>(dataRow, index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgMacAddress?[]? GetPgMacAddressArray(this IDataRow dataRow, string name)
    {
        return GetPgMacAddressArray(dataRow, dataRow.IndexOf(name));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgMacAddress?[] GetPgMacAddressArrayNotNull(this IDataRow dataRow, string name)
    {
        return GetPgMacAddressArrayNotNull(dataRow, dataRow.IndexOf(name));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgMoney?[]? GetPgMoneyArray(this IDataRow dataRow, int index)
    {
        return GetPgDecodeArrayStructInternal<PgMoney, PgMoney>(dataRow, index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgMoney?[] GetPgMoneyArrayNotNull(this IDataRow dataRow, int index)
    {
        return GetPgDecodeArrayStructNotNullInternal<PgMoney, PgMoney>(dataRow, index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgMoney?[]? GetPgMoneyArray(this IDataRow dataRow, string name)
    {
        return GetPgMoneyArray(dataRow, dataRow.IndexOf(name));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgMoney?[] GetPgMoneyArrayNotNull(this IDataRow dataRow, string name)
    {
        return GetPgMoneyArrayNotNull(dataRow, dataRow.IndexOf(name));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgInet?[]? GetPgInetArray(this IDataRow dataRow, int index)
    {
        return GetPgDecodeArrayClassInternal<PgInet, PgInet>(dataRow, index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgInet?[] GetPgInetArrayNotNull(this IDataRow dataRow, int index)
    {
        return GetPgDecodeArrayClassNotNullInternal<PgInet, PgInet>(dataRow, index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgInet?[]? GetPgInetArray(this IDataRow dataRow, string name)
    {
        return GetPgInetArray(dataRow, dataRow.IndexOf(name));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgInet?[] GetPgInetArrayNotNull(this IDataRow dataRow, string name)
    {
        return GetPgInetArrayNotNull(dataRow, dataRow.IndexOf(name));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgRange<long>?[]? GetPgRangeLongArray(this IDataRow dataRow, int index)
    {
        return GetPgDecodeArrayClassInternal<PgRange<long>, PgRangeType<long, PgLong>>(dataRow, index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgRange<long>?[] GetPgRangeLongArrayNotNull(this IDataRow dataRow, int index)
    {
        return GetPgDecodeArrayClassNotNullInternal<PgRange<long>, PgRangeType<long, PgLong>>(dataRow, index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgRange<long>?[]? GetPgRangeLongArray(this IDataRow dataRow, string name)
    {
        return GetPgRangeLongArray(dataRow, dataRow.IndexOf(name));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgRange<long>?[] GetPgRangeLongArrayNotNull(this IDataRow dataRow, string name)
    {
        return GetPgRangeLongArrayNotNull(dataRow, dataRow.IndexOf(name));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgRange<int>?[]? GetPgRangeIntArray(this IDataRow dataRow, int index)
    {
        return GetPgDecodeArrayClassInternal<PgRange<int>, PgRangeType<int, PgInt>>(dataRow, index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgRange<int>?[] GetPgRangeIntArrayNotNull(this IDataRow dataRow, int index)
    {
        return GetPgDecodeArrayClassNotNullInternal<PgRange<int>, PgRangeType<int, PgInt>>(dataRow, index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgRange<int>?[]? GetPgRangeIntArray(this IDataRow dataRow, string name)
    {
        return GetPgRangeIntArray(dataRow, dataRow.IndexOf(name));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgRange<int>?[] GetPgRangeIntArrayNotNull(this IDataRow dataRow, string name)
    {
        return GetPgRangeIntArrayNotNull(dataRow, dataRow.IndexOf(name));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgRange<DateOnly>?[]? GetPgRangeDateArray(this IDataRow dataRow, int index)
    {
        return GetPgDecodeArrayClassInternal<PgRange<DateOnly>, PgRangeType<DateOnly, PgDate>>(dataRow, index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgRange<DateOnly>?[] GetPgRangeDateArrayNotNull(this IDataRow dataRow, int index)
    {
        return GetPgDecodeArrayClassNotNullInternal<PgRange<DateOnly>, PgRangeType<DateOnly, PgDate>>(dataRow, index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgRange<DateOnly>?[]? GetPgRangeDateArray(this IDataRow dataRow, string name)
    {
        return GetPgRangeDateArray(dataRow, dataRow.IndexOf(name));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgRange<DateOnly>?[] GetPgRangeDateArrayNotNull(this IDataRow dataRow, string name)
    {
        return GetPgRangeDateArrayNotNull(dataRow, dataRow.IndexOf(name));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgRange<DateTime>?[]? GetPgRangeDateTimeArray(this IDataRow dataRow, int index)
    {
        return GetPgDecodeArrayClassInternal<PgRange<DateTime>, PgRangeType<DateTime, PgDateTime>>(dataRow, index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgRange<DateTime>?[] GetPgRangeDateTimeArrayNotNull(this IDataRow dataRow, int index)
    {
        return GetPgDecodeArrayClassNotNullInternal<PgRange<DateTime>, PgRangeType<DateTime, PgDateTime>>(dataRow, index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgRange<DateTime>?[]? GetPgRangeDateTimeArray(this IDataRow dataRow, string name)
    {
        return GetPgRangeDateTimeArray(dataRow, dataRow.IndexOf(name));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgRange<DateTime>?[] GetPgRangeDateTimeArrayNotNull(this IDataRow dataRow, string name)
    {
        return GetPgRangeDateTimeArrayNotNull(dataRow, dataRow.IndexOf(name));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgRange<DateTimeOffset>?[]? GetPgRangeDateTimeOffsetArray(this IDataRow dataRow, int index)
    {
        return GetPgDecodeArrayClassInternal<PgRange<DateTimeOffset>, PgRangeType<DateTimeOffset, PgDateTimeOffset>>(dataRow, index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgRange<DateTimeOffset>?[] GetPgRangeDateTimeOffsetArrayNotNull(this IDataRow dataRow, int index)
    {
        return GetPgDecodeArrayClassNotNullInternal<PgRange<DateTimeOffset>, PgRangeType<DateTimeOffset, PgDateTimeOffset>>(dataRow, index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgRange<DateTimeOffset>?[]? GetPgRangeDateTimeOffsetArray(this IDataRow dataRow, string name)
    {
        return GetPgRangeDateTimeOffsetArray(dataRow, dataRow.IndexOf(name));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgRange<DateTimeOffset>?[] GetPgRangeDateTimeOffsetArrayNotNull(this IDataRow dataRow, string name)
    {
        return GetPgRangeDateTimeOffsetArrayNotNull(dataRow, dataRow.IndexOf(name));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgRange<decimal>?[]? GetPgRangeDecimalArray(this IDataRow dataRow, int index)
    {
        return GetPgDecodeArrayClassInternal<PgRange<decimal>, PgRangeType<decimal, PgDecimal>>(dataRow, index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgRange<decimal>?[] GetPgRangeDecimalArrayNotNull(this IDataRow dataRow, int index)
    {
        return GetPgDecodeArrayClassNotNullInternal<PgRange<decimal>, PgRangeType<decimal, PgDecimal>>(dataRow, index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgRange<decimal>?[]? GetPgRangeDecimalArray(this IDataRow dataRow, string name)
    {
        return GetPgRangeDecimalArray(dataRow, dataRow.IndexOf(name));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgRange<decimal>?[] GetPgRangeDecimalArrayNotNull(this IDataRow dataRow, string name)
    {
        return GetPgRangeDecimalArrayNotNull(dataRow, dataRow.IndexOf(name));
    }
    
    private static TResult? GetPgDecodeInternal<TResult, TType>(IDataRow dataRow, int index)
        where TResult : notnull
        where TType : IPgDbType<TResult>
    {
        var pgDataRow = PgException.CheckIfIs<IDataRow, PgDataRow>(dataRow);
        return pgDataRow.Decode<TResult, TType>(index);
    }
    
    private static TResult GetPgDecodeNotNullInternal<TResult, TType>(IDataRow dataRow, int index)
        where TResult : notnull
        where TType : IPgDbType<TResult>
    {
        var pgDataRow = PgException.CheckIfIs<IDataRow, PgDataRow>(dataRow);
        return pgDataRow.DecodeNotNull<TResult, TType>(index);
    }
    
    private static TDecode? GetPgDecodeSelfInternal<TDecode>(IDataRow dataRow, int index)
        where TDecode : IPgDbType<TDecode>
    {
        return GetPgDecodeInternal<TDecode, TDecode>(dataRow, index);
    }
    
    private static TDecode GetPgDecodeSelfNotNullInternal<TDecode>(
        IDataRow dataRow,
        int index)
        where TDecode : IPgDbType<TDecode>
    {
        return GetPgDecodeNotNullInternal<TDecode, TDecode>(dataRow, index);
    }
    
    public static TElement?[]? GetPgDecodeArrayStructInternal<TElement, TType>(
        IDataRow dataRow,
        int index)
        where TType : IPgDbType<TElement>, IHasArrayType
        where TElement : struct
    {
        return GetPgDecodeInternal<TElement?[], PgArrayTypeStruct<TElement, TType>>(dataRow, index);
    }
    
    public static TElement?[] GetPgDecodeArrayStructNotNullInternal<TElement, TType>(
        IDataRow dataRow,
        int index)
        where TType : IPgDbType<TElement>, IHasArrayType
        where TElement : struct
    {
        return GetPgDecodeNotNullInternal<TElement?[], PgArrayTypeStruct<TElement, TType>>(
            dataRow,
            index);
    }
    
    public static TElement?[]? GetPgDecodeArrayClassInternal<TElement, TType>(
        IDataRow dataRow,
        int index)
        where TType : IPgDbType<TElement>, IHasArrayType
        where TElement : class
    {
        return GetPgDecodeInternal<TElement?[], PgArrayTypeClass<TElement, TType>>(dataRow, index);
    }
    
    public static TElement?[] GetPgDecodeArrayClassNotNullInternal<TElement, TType>(
        IDataRow dataRow,
        int index)
        where TType : IPgDbType<TElement>, IHasArrayType
        where TElement : class
    {
        return GetPgDecodeNotNullInternal<TElement?[], PgArrayTypeClass<TElement, TType>>(
            dataRow,
            index);
    }
}
