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
    public static PgRange<DateOnly>? GetPgRangeDateOnly(this IDataRow dataRow, int index)
    {
        return GetPgDecodeInternal<PgRange<DateOnly>, PgRangeType<DateOnly, PgDate>>(dataRow, index);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgRange<DateOnly> GetPgRangeDateOnlyNotNull(this IDataRow dataRow, int index)
    {
        return GetPgDecodeNotNullInternal<PgRange<DateOnly>, PgRangeType<DateOnly, PgDate>>(
            dataRow,
            index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgRange<DateOnly>? GetPgRangeDateOnly(this IDataRow dataRow, string name)
    {
        return GetPgRangeDateOnly(dataRow, dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgRange<DateOnly> GetPgRangeDateOnlyNotNull(this IDataRow dataRow, string name)
    {
        return GetPgRangeDateOnlyNotNull(dataRow, dataRow.IndexOf(name));
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
}
