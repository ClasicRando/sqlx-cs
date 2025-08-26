using System.Runtime.CompilerServices;
using System.Text.Json.Serialization.Metadata;
using Sqlx.Core;
using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Core.Result;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Result;

internal class PgDataRow : IDataRow
{
    public PgDataRow(byte[] rowData, PgColumnMetadata[] columnMetadata)
    {
        _rowData = rowData;
        _columnMetadata = columnMetadata;
        var buffer = new ReadBuffer(rowData);
        int columnCount = buffer.ReadShort();
        _columnValueSlices = new (int, int)?[columnCount];
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
    
    private readonly byte[] _rowData;
    private readonly PgColumnMetadata[] _columnMetadata;
    private readonly (int, int)?[] _columnValueSlices;
    
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

    public T? Get<T>(int index) where T : notnull
    {
        var sliceItem = _columnValueSlices[index];
        if (sliceItem is not {} slice)
        {
            return default;
        }

        PgColumnMetadata columnMetadata = _columnMetadata[index];
        if (typeof(T) == typeof(bool))
        {
            if (Decode<bool, PgBool>(slice, columnMetadata) is T result) return result;
        }
        else if (typeof(T) == typeof(short))
        {
            if (Decode<short, PgShort>(slice, columnMetadata) is T result) return result;
        }
        else if (typeof(T) == typeof(int))
        {
            if (Decode<int, PgInt>(slice, columnMetadata) is T result) return result;
        }
        else if (typeof(T) == typeof(long))
        {
            if (Decode<long, PgLong>(slice, columnMetadata) is T result) return result;
        }
        else if (typeof(T) == typeof(float))
        {
            if (Decode<float, PgFloat>(slice, columnMetadata) is T result) return result;
        }
        else if (typeof(T) == typeof(double))
        {
            if (Decode<double, PgDouble>(slice, columnMetadata) is T result) return result;
        }
        else if (typeof(T) == typeof(decimal))
        {
            if (Decode<decimal, PgDecimal>(slice, columnMetadata) is T result) return result;
        }
        else if (typeof(T) == typeof(string))
        {
            if (Decode<string, PgString>(slice, columnMetadata) is T result) return result;
        }
        else if (typeof(T) == typeof(byte[]))
        {
            if (Decode<byte[], PgBytea>(slice, columnMetadata) is T result) return result;
        }
        else if (typeof(T) == typeof(byte))
        {
            if (Decode<byte, PgChar>(slice, columnMetadata) is T result) return result;
        }
        else if (typeof(T) == typeof(DateOnly))
        {
            if (Decode<DateOnly, PgDate>(slice, columnMetadata) is T result) return result;
        }
        else if (typeof(T) == typeof(TimeOnly))
        {
            if (Decode<TimeOnly, PgTime>(slice, columnMetadata) is T result) return result;
        }
        else if (typeof(T) == typeof(PgTimeTz))
        {
            if (DecodeSelf<PgTimeTz>(slice, columnMetadata) is T result) return result;
        }
        else if (typeof(T) == typeof(DateTime))
        {
            if (Decode<DateTime, PgDateTime>(slice, columnMetadata) is T result) return result;
        }
        else if (typeof(T) == typeof(DateTimeOffset))
        {
            if (Decode<DateTimeOffset, PgDateTimeOffset>(slice, columnMetadata) is T result) return result;
        }
        else if (typeof(T) == typeof(Guid))
        {
            if (Decode<Guid, PgUuid>(slice, columnMetadata) is T result) return result;
        }
        else if (typeof(T) == typeof(PgPoint))
        {
            if (DecodeSelf<PgPoint>(slice, columnMetadata) is T result) return result;
        }
        else if (typeof(T) == typeof(PgLine))
        {
            if (DecodeSelf<PgLine>(slice, columnMetadata) is T result) return result;
        }
        else if (typeof(T) == typeof(PgLineSegment))
        {
            if (DecodeSelf<PgLineSegment>(slice, columnMetadata) is T result) return result;
        }
        else if (typeof(T) == typeof(PgBox))
        {
            if (DecodeSelf<PgBox>(slice, columnMetadata) is T result) return result;
        }
        else if (typeof(T) == typeof(PgPath))
        {
            if (DecodeSelf<PgPath>(slice, columnMetadata) is T result) return result;
        }
        else if (typeof(T) == typeof(PgCircle))
        {
            if (DecodeSelf<PgCircle>(slice, columnMetadata) is T result) return result;
        }
        else if (typeof(T) == typeof(PgPolygon))
        {
            if (DecodeSelf<PgPolygon>(slice, columnMetadata) is T result) return result;
        }
        else if (typeof(T) == typeof(PgInterval))
        {
            if (DecodeSelf<PgInterval>(slice, columnMetadata) is T result) return result;
        }
        else if (typeof(T) == typeof(TimeSpan))
        {
            if (Decode<TimeSpan, TimeSpanType>(slice, columnMetadata) is T result) return result;
        }
        else if (typeof(T) == typeof(PgMacAddress))
        {
            if (DecodeSelf<PgMacAddress>(slice, columnMetadata) is T result) return result;
        }
        else if (typeof(T) == typeof(PgMoney))
        {
            if (DecodeSelf<PgMoney>(slice, columnMetadata) is T result) return result;
        }
        else if (typeof(T) == typeof(PgInet))
        {
            if (DecodeSelf<PgInet>(slice, columnMetadata) is T result) return result;
        }
        else if (typeof(T) == typeof(PgRange<long>))
        {
            if (Decode<PgRange<long>, PgRangeType<long, PgLong>>(slice, columnMetadata) is T result) return result;
        }
        else if (typeof(T) == typeof(PgRange<int>))
        {
            if (Decode<PgRange<int>, PgRangeType<int, PgInt>>(slice, columnMetadata) is T result) return result;
        }
        else if (typeof(T) == typeof(PgRange<DateOnly>))
        {
            if (Decode<PgRange<DateOnly>, PgRangeType<DateOnly, PgDate>>(slice, columnMetadata) is T result) return result;
        }
        else if (typeof(T) == typeof(PgRange<decimal>))
        {
            if (Decode<PgRange<decimal>, PgRangeType<decimal, PgDecimal>>(slice, columnMetadata) is T result) return result;
        }
        throw ColumnDecodeError.Create<T>(columnMetadata);
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

    public T? GetJson<T>(int index, JsonTypeInfo<T>? jsonTypeInfo = null) where T : notnull
    {
        var sliceItem = _columnValueSlices[index];
        if (sliceItem is not {} slice)
        {
            return default;
        }

        PgColumnMetadata columnMetadata = _columnMetadata[index];
        if (PgJson<T>.DbType != columnMetadata.PgType && !PgJson<T>.IsCompatible(columnMetadata.PgType))
        {
            throw ColumnDecodeError.Create<T>(columnMetadata);
        }
        
        var bytes = _rowData.AsSpan()[slice.Item1..slice.Item2];
        switch (columnMetadata.FormatCode)
        {
            case PgFormatCode.Text:
                Span<char> chars = stackalloc char[Charsets.Default.GetCharCount(bytes)];
                Charsets.Default.GetChars(bytes, chars);
                return PgJson<T>.DecodeText(new PgTextValue(chars, columnMetadata), jsonTypeInfo);
            case PgFormatCode.Binary:
                var buffer = new ReadBuffer(bytes);
                return PgJson<T>.DecodeBytes(new PgBinaryValue(buffer, columnMetadata), jsonTypeInfo);
            default:
                throw ColumnDecodeError.Create<T>(
                    columnMetadata,
                    $"Unexpected format code: {columnMetadata.FormatCode}");
        }
    }

    internal TSelf? DecodeSelf<TSelf>(int index)
        where TSelf : IPgDbType<TSelf>
    {
        return Decode<TSelf, TSelf>(index);
    }

    private TResult? Decode<TResult, TType>(int index)
        where TResult : notnull
        where TType : IPgDbType<TResult>
    {
        var sliceItem = _columnValueSlices[index];
        if (sliceItem is not {} slice)
        {
            return default;
        }

        PgColumnMetadata columnMetadata = _columnMetadata[index];
        return Decode<TResult, TType>(slice, columnMetadata);
    }

    private TSelf DecodeSelf<TSelf>((int, int) slice, PgColumnMetadata columnMetadata)
        where TSelf : IPgDbType<TSelf>
    {
        return Decode<TSelf, TSelf>(slice, columnMetadata);
    }

    private TResult Decode<TResult, TType>((int, int) slice, PgColumnMetadata columnMetadata)
        where TResult : notnull
        where TType : IPgDbType<TResult>
    {
        if (TType.DbType != columnMetadata.PgType && !TType.IsCompatible(columnMetadata.PgType))
        {
            throw ColumnDecodeError.Create<TResult>(columnMetadata);
        }

        var bytes = _rowData.AsSpan()[slice.Item1..slice.Item2];
        switch (columnMetadata.FormatCode)
        {
            case PgFormatCode.Text:
                Span<char> chars = stackalloc char[Charsets.Default.GetCharCount(bytes)];
                Charsets.Default.GetChars(bytes, chars);
                return TType.DecodeText(new PgTextValue(chars, columnMetadata));
            case PgFormatCode.Binary:
                var buffer = new ReadBuffer(bytes);
                return TType.DecodeBytes(new PgBinaryValue(buffer, columnMetadata));
            default:
                throw ColumnDecodeError.Create<TResult>(
                    columnMetadata,
                    $"Unexpected format code: {columnMetadata.FormatCode}");
        }
    }
}

public static class DataRowExtensions
{
    public static TDecode? GetPgDecode<TDecode>(
        this IDataRow dataRow,
        int index) where TDecode : IPgDbType<TDecode>
    {
        if (dataRow is not PgDataRow pgDataRow)
        {
            throw new SqlxException($"Attempted to use {dataRow.GetType()} as if it were a PgDataRow");
        }
        return pgDataRow.DecodeSelf<TDecode>(index);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TDecode GetPgDecodeNotNull<TDecode>(
        this IDataRow dataRow,
        int index) where TDecode : IPgDbType<TDecode>
    {
        return GetPgDecode<TDecode>(dataRow, index)
               ?? throw new SqlxException($"Expected field #{index} to be non-null but found null");
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TDecode? GetPgDecode<TDecode>(
        this IDataRow dataRow,
        string name) where TDecode : IPgDbType<TDecode>
    {
        return GetPgDecode<TDecode>(dataRow, dataRow.IndexOf(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TDecode GetPgDecodeNotNull<TDecode>(
        this IDataRow dataRow,
        string name) where TDecode : IPgDbType<TDecode>
    {
        return GetPgDecode<TDecode>(dataRow, dataRow.IndexOf(name))
               ?? throw new SqlxException($"Expected field #{name} to be non-null but found null");
    }
}
