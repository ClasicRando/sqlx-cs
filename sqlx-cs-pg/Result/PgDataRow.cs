using System.Diagnostics.CodeAnalysis;
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

    public byte? GetByte(int index)
    {
        return Decode<byte, PgChar>(index);
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
        return GetJsonInternal(index, jsonTypeInfo);
    }

    public bool GetBooleanNotNull(int index)
    {
        return Decode<bool, PgBool>(index, throwIfNull: true);
    }

    public byte GetByteNotNull(int index)
    {
        return Decode<byte, PgChar>(index, throwIfNull: true);
    }

    public short GetShortNotNull(int index)
    {
        return Decode<short, PgShort>(index, throwIfNull: true);
    }

    public int GetIntNotNull(int index)
    {
        return Decode<int, PgInt>(index, throwIfNull: true);
    }

    public long GetLongNotNull(int index)
    {
        return Decode<long, PgLong>(index, throwIfNull: true);
    }

    public float GetFloatNotNull(int index)
    {
        return Decode<float, PgFloat>(index, throwIfNull: true);
    }

    public double GetDoubleNotNull(int index)
    {
        return Decode<double, PgDouble>(index, throwIfNull: true);
    }

    public TimeOnly GetTimeNotNull(int index)
    {
        return Decode<TimeOnly, PgTime>(index, throwIfNull: true);
    }

    public DateOnly GetDateNotNull(int index)
    {
        return Decode<DateOnly, PgDate>(index, throwIfNull: true);
    }

    public DateTime GetDateTimeNotNull(int index)
    {
        return Decode<DateTime, PgDateTime>(index, throwIfNull: true);
    }

    public DateTimeOffset GetDateTimeOffsetNotNull(int index)
    {
        return Decode<DateTimeOffset, PgDateTimeOffset>(index, throwIfNull: true);
    }

    public decimal GetDecimalNotNull(int index)
    {
        return Decode<decimal, PgDecimal>(index, throwIfNull: true);
    }

    public byte[] GetBytesNotNull(int index)
    {
        return Decode<byte[], PgBytea>(index, throwIfNull: true);
    }

    public string GetStringNotNull(int index)
    {
        return Decode<string, PgString>(index, throwIfNull: true);
    }

    public Guid GetGuidNotNull(int index)
    {
        return Decode<Guid, PgUuid>(index, throwIfNull: true);
    }

    public T GetJsonNotNull<T>(int index, JsonTypeInfo<T>? jsonTypeInfo = null) where T : notnull
    {
        return GetJsonInternal(index, jsonTypeInfo, throwIfNull: true);
    }

    private T? GetJsonInternal<T>(
        int index,
        JsonTypeInfo<T>? jsonTypeInfo,
        [DoesNotReturnIf(true)] bool throwIfNull = false) where T : notnull
    {
        ColumnData columnData = GetColumnData(index);
        if (columnData.IsNull)
        {
            return throwIfNull
                ? throw new SqlxException($"Expected field #{index} to be non-null but found null")
                : default;
        }

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

    internal TResult? Decode<TResult, TType>(
        int index,
        [DoesNotReturnIf(true)] bool throwIfNull = false)
        where TResult : notnull
        where TType : IPgDbType<TResult>
    {
        ColumnData columnData = GetColumnData(index);
        if (!columnData.IsNull)
        {
            return Decode<TResult, TType>(columnData);
        }
        
        return throwIfNull
            ? throw new SqlxException($"Expected field #{index} to be non-null but found null")
            : default;
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
        return GetPgDecodeSelfInternal<TDecode>(dataRow, index, throwIfNull: true);
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
        return GetPgDecodeNotNull<PgTimeTz>(dataRow, index);
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
        return GetPgDecodeNotNull<PgPoint>(dataRow, index);
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
        return GetPgDecodeNotNull<PgLine>(dataRow, index);
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
        return GetPgDecodeNotNull<PgLineSegment>(dataRow, index);
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
        return GetPgDecodeNotNull<PgBox>(dataRow, index);
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
        return GetPgDecodeNotNull<PgPath>(dataRow, index);
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
        return GetPgDecodeNotNull<PgCircle>(dataRow, index);
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
        return GetPgDecodeNotNull<PgPolygon>(dataRow, index);
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
        return GetPgDecodeNotNull<PgInterval>(dataRow, index);
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
        return GetPgDecodeNotNull<PgMacAddress>(dataRow, index);
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
        return GetPgDecodeNotNull<PgMoney>(dataRow, index);
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
        return GetPgDecodeNotNull<PgInet>(dataRow, index);
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
    public static PgRange<long>? GetPgInt8Range(this IDataRow dataRow, int index)
    {
        return GetPgDecodeInternal<PgRange<long>, PgRangeType<long, PgLong>>(dataRow, index);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgRange<long> GetPgInt8RangeNotNull(this IDataRow dataRow, int index)
    {
        return GetPgDecodeInternal<PgRange<long>, PgRangeType<long, PgLong>>(
            dataRow,
            index,
            throwIfNull: true);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgRange<long>? GetPgInt8Range(this IDataRow dataRow, string name)
    {
        return GetPgInt8Range(dataRow, dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgRange<long> GetPgInt8RangeNotNull(this IDataRow dataRow, string name)
    {
        return GetPgInt8RangeNotNull(dataRow, dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgRange<int>? GetPgInt4Range(this IDataRow dataRow, int index)
    {
        return GetPgDecodeInternal<PgRange<int>, PgRangeType<int, PgInt>>(dataRow, index);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgRange<int> GetPgInt4RangeNotNull(this IDataRow dataRow, int index)
    {
        return GetPgDecodeInternal<PgRange<int>, PgRangeType<int, PgInt>>(
            dataRow,
            index,
            throwIfNull: true);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgRange<int>? GetPgInt4Range(this IDataRow dataRow, string name)
    {
        return GetPgInt4Range(dataRow, dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgRange<int> GetPgInt4RangeNotNull(this IDataRow dataRow, string name)
    {
        return GetPgInt4RangeNotNull(dataRow, dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgRange<DateOnly>? GetPgDateRange(this IDataRow dataRow, int index)
    {
        return GetPgDecodeInternal<PgRange<DateOnly>, PgRangeType<DateOnly, PgDate>>(dataRow, index);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgRange<DateOnly> GetPgDateRangeNotNull(this IDataRow dataRow, int index)
    {
        return GetPgDecodeInternal<PgRange<DateOnly>, PgRangeType<DateOnly, PgDate>>(
            dataRow,
            index,
            throwIfNull: true);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgRange<DateOnly>? GetPgDateRange(this IDataRow dataRow, string name)
    {
        return GetPgDateRange(dataRow, dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgRange<DateOnly> GetPgDateRangeNotNull(this IDataRow dataRow, string name)
    {
        return GetPgDateRangeNotNull(dataRow, dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgRange<decimal>? GetPgNumericRange(this IDataRow dataRow, int index)
    {
        return GetPgDecodeInternal<PgRange<decimal>, PgRangeType<decimal, PgDecimal>>(dataRow, index);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgRange<decimal> GetPgNumericRangeNotNull(this IDataRow dataRow, int index)
    {
        return GetPgDecodeInternal<PgRange<decimal>, PgRangeType<decimal, PgDecimal>>(
            dataRow,
            index,
            throwIfNull: true);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgRange<decimal>? GetPgNumericRange(this IDataRow dataRow, string name)
    {
        return GetPgNumericRange(dataRow, dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PgRange<decimal> GetPgNumericRangeNotNull(this IDataRow dataRow, string name)
    {
        return GetPgNumericRangeNotNull(dataRow, dataRow.IndexOf(name));
    }
    
    private static TResult? GetPgDecodeInternal<TResult, TType>(
        IDataRow dataRow,
        int index,
        [DoesNotReturnIf(true)] bool throwIfNull = false)
        where TResult : notnull
        where TType : IPgDbType<TResult>
    {
        var pgDataRow = PgException.CheckIfIs<IDataRow, PgDataRow>(dataRow);
        return pgDataRow.Decode<TResult, TType>(index, throwIfNull);
    }
    
    private static TDecode? GetPgDecodeSelfInternal<TDecode>(
        IDataRow dataRow,
        int index,
        [DoesNotReturnIf(true)] bool throwIfNull = false)
        where TDecode : IPgDbType<TDecode>
    {
        return GetPgDecodeInternal<TDecode, TDecode>(dataRow, index, throwIfNull);
    }
}
