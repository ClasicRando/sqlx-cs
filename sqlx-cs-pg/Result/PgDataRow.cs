using System.Buffers;
using System.Text.Json.Serialization.Metadata;
using Sqlx.Core;
using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Core.Result;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Result;

/// <summary>
/// <see cref="IDataRow"/> implementation for Postgres. Represents the bytes sent by the database
/// backend, the statement's metadata and the slices into the bytes that represent each column.
/// </summary>
internal sealed class PgDataRow : IDataRow
{
    private const int MaxStackSize = 256 / (sizeof(char) / sizeof(byte));
    private static readonly ArrayPool<char> CharArrayPool = ArrayPool<char>.Shared;
    
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
        
        if (PgJson<T>.DbType != columnData.ColumnMetadata.PgType
            && !PgJson<T>.IsCompatible(columnData.ColumnMetadata.PgType))
        {
            throw ColumnDecodeException.Create<T>(columnData.ColumnMetadata);
        }

        var bytes = _rowData.AsSpan()[columnData.Range.Start..columnData.Range.End];
        switch (columnData.ColumnMetadata.FormatCode)
        {
            case PgFormatCode.Text:
                char[]? rentedFromPool = null;
                var characterCount = Charsets.Default.GetCharCount(bytes);
                var chars = characterCount >= MaxStackSize
                    ? (rentedFromPool = CharArrayPool.Rent(characterCount))
                    : stackalloc char[characterCount];
                chars = chars[..characterCount];
                try
                {
                    Charsets.Default.GetChars(bytes, chars);
                    PgTextValue textValue = new(chars, ref columnData.ColumnMetadata);
                    return PgJson<T>.DecodeText(textValue, jsonTypeInfo);
                }
                finally
                {
                    if (rentedFromPool is not null)
                    {
                        CharArrayPool.Return(rentedFromPool);
                    }
                }
            case PgFormatCode.Binary:
                var buffer = new ReadBuffer(bytes);
                PgBinaryValue binaryValue = new(buffer, ref columnData.ColumnMetadata);
                return PgJson<T>.DecodeBytes(ref binaryValue, jsonTypeInfo);
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
        
        if (TType.DbType != columnData.ColumnMetadata.PgType
            && !TType.IsCompatible(columnData.ColumnMetadata.PgType))
        {
            throw ColumnDecodeException.Create<TResult>(columnData.ColumnMetadata);
        }

        var bytes = _rowData.AsSpan()[columnData.Range.Start..columnData.Range.End];
        switch (columnData.ColumnMetadata.FormatCode)
        {
            case PgFormatCode.Text:
                char[]? rentedFromPool = null;
                var characterCount = Charsets.Default.GetCharCount(bytes);
                var chars = characterCount >= MaxStackSize
                    ? (rentedFromPool = CharArrayPool.Rent(characterCount))
                    : stackalloc char[characterCount];
                chars = chars[..characterCount];
                try
                {
                    Charsets.Default.GetChars(bytes, chars);
                    PgTextValue textValue = new(chars, ref columnData.ColumnMetadata);
                    return TType.DecodeText(textValue);
                }
                finally
                {
                    if (rentedFromPool is not null)
                    {
                        CharArrayPool.Return(rentedFromPool);
                    }
                }
            case PgFormatCode.Binary:
                var buffer = new ReadBuffer(bytes);
                var value = new PgBinaryValue(buffer, ref columnData.ColumnMetadata);
                return TType.DecodeBytes(ref value);
            default:
                throw ColumnDecodeException.Create<TResult>(
                    columnData.ColumnMetadata,
                    $"Unexpected format code: {columnData.ColumnMetadata.FormatCode}");
        }
    }
}
