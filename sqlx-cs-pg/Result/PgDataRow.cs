using System.Runtime.CompilerServices;
using System.Text.Json.Serialization.Metadata;
using Sqlx.Core;
using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Core.Result;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Exceptions;
using Sqlx.Postgres.Generator.Result;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Result;

internal sealed class PgDataRow : IDataRow
{
    private readonly byte[] _rowData;
    private readonly PgStatementMetadata _statementMetadata;
    private readonly Range?[] _columnValueSlices;
    
    public PgDataRow(byte[] rowData, PgStatementMetadata statementMetadata)
    {
        _rowData = rowData;
        _statementMetadata = statementMetadata;
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
        return _statementMetadata.IndexOfFieldName(name);
    }

    public bool IsNull(int index)
    {
        return _columnValueSlices[index] is null;
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
        if (columnData.IsNull)
        {
            throw new SqlxException($"Expected field #{index} to be non-null but found null");
        }
        
        if (PgJson<T>.DbType != columnData.ColumnMetadata.PgType && !PgJson<T>.IsCompatible(columnData.ColumnMetadata.PgType))
        {
            throw ColumnDecodeException.Create<T>(columnData.ColumnMetadata);
        }

        var bytes = _rowData.AsSpan()[columnData.Range.Start..columnData.Range.End];
        switch (columnData.ColumnMetadata.FormatCode)
        {
            case PgFormatCode.Text:
                Span<char> chars = stackalloc char[Charsets.Default.GetCharCount(bytes)];
                Charsets.Default.GetChars(bytes, chars);
                PgTextValue textValue = new(chars, ref columnData.ColumnMetadata);
                return PgJson<T>.DecodeText(textValue, jsonTypeInfo);
            case PgFormatCode.Binary:
                var buffer = new ReadBuffer(bytes);
                PgBinaryValue binaryValue = new(buffer, ref columnData.ColumnMetadata);
                return PgJson<T>.DecodeBytes(binaryValue, jsonTypeInfo);
            default:
                throw ColumnDecodeException.Create<T>(
                    columnData.ColumnMetadata,
                    $"Unexpected format code: {columnData.ColumnMetadata.FormatCode}");
        }
    }

    private readonly ref struct ColumnData(
        bool isNull,
        Range range,
        ref PgColumnMetadata columnMetadata)
    {
        public readonly bool IsNull = isNull;
        public readonly Range Range = range;
        public readonly ref PgColumnMetadata ColumnMetadata = ref columnMetadata;
    }

    private ColumnData GetColumnData(int index)
    {
        ref PgColumnMetadata columnMetadata = ref _statementMetadata[index];
        var sliceItem = _columnValueSlices[index];
        return sliceItem is not {} slice
            ? new ColumnData(true, new Range(), ref columnMetadata)
            : new ColumnData(false, slice, ref columnMetadata);
    }

    internal TResult DecodeNotNull<TResult, TType>(int index)
        where TResult : notnull
        where TType : IPgDbType<TResult>
    {
        ColumnData columnData = GetColumnData(index);
        if (columnData.IsNull)
        {
            throw new SqlxException($"Expected field #{index} to be non-null but found null");
        }
        
        if (TType.DbType.TypeOid != columnData.ColumnMetadata.PgType.TypeOid
            && !TType.IsCompatible(columnData.ColumnMetadata.PgType))
        {
            throw ColumnDecodeException.Create<TResult>(columnData.ColumnMetadata);
        }

        var bytes = _rowData.AsSpan()[columnData.Range.Start..columnData.Range.End];
        switch (columnData.ColumnMetadata.FormatCode)
        {
            case PgFormatCode.Text:
                Span<char> chars = stackalloc char[Charsets.Default.GetCharCount(bytes)];
                Charsets.Default.GetChars(bytes, chars);
                return TType.DecodeText(new PgTextValue(chars, ref columnData.ColumnMetadata));
            case PgFormatCode.Binary:
                var buffer = new ReadBuffer(bytes);
                return TType.DecodeBytes(new PgBinaryValue(buffer, ref columnData.ColumnMetadata));
            default:
                throw ColumnDecodeException.Create<TResult>(
                    columnData.ColumnMetadata,
                    $"Unexpected format code: {columnData.ColumnMetadata.FormatCode}");
        }
    }
}

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
