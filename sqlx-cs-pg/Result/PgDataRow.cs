using System.Buffers;
using System.Text.Json.Serialization.Metadata;
using Sqlx.Core;
using Sqlx.Core.Buffer;
using Sqlx.Core.Column;
using Sqlx.Core.Exceptions;
using Sqlx.Core.Result;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Result;

/// <summary>
/// <see cref="IDataRow"/> implementation for Postgres. Represents the bytes sent by the database
/// backend, the statement's metadata and the slices into the bytes that represent each column.
/// </summary>
internal sealed class PgDataRow : IPgDataRow
{
    private const int MaxStackSize = 256 / (sizeof(char) / sizeof(byte));
    private static readonly ArrayPool<char> CharArrayPool = ArrayPool<char>.Shared;

    private bool _isDisposed;
    private ReadOnlyMemory<byte> _rowData;
    private readonly PgStatementMetadata _statementMetadata;
    private readonly short _columnCount;

    public PgDataRow(ReadOnlyMemory<byte> rowData, PgStatementMetadata statementMetadata)
    {
        _rowData = rowData;
        _statementMetadata = statementMetadata;
        var temp = rowData.Span;
        _columnCount = temp.ReadShort();
    }

    public int ColumnCount => _columnCount;

    public int IndexOf(string name)
    {
        CheckDisposed();
        return _statementMetadata.IndexOfFieldName(name);
    }

    public IColumnMetadata GetColumnMetadata(int index)
    {
        CheckDisposed();
        CheckValidIndex(index);
        return _statementMetadata[index];
    }

    public bool IsNull(int index)
    {
        CheckDisposed();
        return GetColumnData(index).IsNull;
    }

    public bool GetBooleanNotNull(int index)
    {
        return GetPgNotNull<bool, PgBool>(index);
    }

    public sbyte GetByteNotNull(int index)
    {
        return GetPgNotNull<sbyte, PgChar>(index);
    }

    public short GetShortNotNull(int index)
    {
        return GetPgNotNull<short, PgShort>(index);
    }

    public int GetIntNotNull(int index)
    {
        return GetPgNotNull<int, PgInt>(index);
    }

    public long GetLongNotNull(int index)
    {
        return GetPgNotNull<long, PgLong>(index);
    }

    public float GetFloatNotNull(int index)
    {
        return GetPgNotNull<float, PgFloat>(index);
    }

    public double GetDoubleNotNull(int index)
    {
        return GetPgNotNull<double, PgDouble>(index);
    }

    public TimeOnly GetTimeNotNull(int index)
    {
        return GetPgNotNull<TimeOnly, PgTime>(index);
    }

    public DateOnly GetDateNotNull(int index)
    {
        return GetPgNotNull<DateOnly, PgDate>(index);
    }

    public DateTime GetDateTimeNotNull(int index)
    {
        return GetPgNotNull<DateTime, PgDateTime>(index);
    }

    public DateTimeOffset GetDateTimeOffsetNotNull(int index)
    {
        return GetPgNotNull<DateTimeOffset, PgDateTimeOffset>(index);
    }

    public decimal GetDecimalNotNull(int index)
    {
        return GetPgNotNull<decimal, PgDecimal>(index);
    }

    public byte[] GetBytesNotNull(int index)
    {
        return GetPgNotNull<byte[], PgBytea>(index);
    }

    public string GetStringNotNull(int index)
    {
        return GetPgNotNull<string, PgString>(index);
    }

    public Guid GetGuidNotNull(int index)
    {
        return GetPgNotNull<Guid, PgUuid>(index);
    }

    public T GetJsonNotNull<T>(int index, JsonTypeInfo<T>? jsonTypeInfo = null) where T : notnull
    {
        CheckDisposed();
        ColumnData columnData = GetColumnData(index);
        if (columnData.IsNull)
        {
            throw new SqlxException($"Expected field #{index} to be non-null but found null");
        }

        ref readonly PgColumnMetadata columnMetadata = ref columnData.ColumnMetadata;
        if (PgJson<T>.DbType != columnMetadata.TypeInfo
            && !PgJson<T>.IsCompatible(columnMetadata.TypeInfo))
        {
            throw ColumnDecodeException.Create<T, PgColumnMetadata>(columnMetadata);
        }

        var bytes = columnData.Data;
        switch (columnMetadata.FormatCode)
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
                    PgTextValue textValue = new(chars, columnMetadata);
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
                PgBinaryValue binaryValue = new(bytes, columnMetadata);
                return PgJson<T>.DecodeBytes(ref binaryValue, jsonTypeInfo);
            default:
                throw ColumnDecodeException.Create<T, PgColumnMetadata>(
                    columnData.ColumnMetadata,
                    $"Unexpected format code: {columnData.ColumnMetadata.FormatCode}");
        }
    }

    private readonly ref struct ColumnData(
        bool isNull,
        ReadOnlySpan<byte> data,
        in PgColumnMetadata columnMetadata)
    {
        public readonly bool IsNull = isNull;
        public readonly ReadOnlySpan<byte> Data = data;
        public readonly ref readonly PgColumnMetadata ColumnMetadata = ref columnMetadata;
    }

    private ColumnData GetColumnData(int index)
    {
        CheckValidIndex(index);
        ref PgColumnMetadata columnMetadata = ref _statementMetadata[index];
        var span = GetColumnSpan(index, out var isNull);
        return new ColumnData(isNull, span, in columnMetadata);
    }

    private ReadOnlySpan<byte> GetColumnSpan(int index, out bool isNull)
    {
        var span = _rowData.Span[2..];
        for (var i = 0; i < _columnCount; i++)
        {
            var length = span.ReadInt();

            if (i == index)
            {
                isNull = length < 0;
                return isNull
                    ? default
                    : span[..length];
            }
            
            if (length < 0)
            {
                continue;
            }

            span.Skip(length);
        }

        throw new InvalidOperationException("Attempted to capture ");
    }

    public TResult GetPgNotNull<TResult, TType>(int index)
        where TType : IPgDbType<TResult>
        where TResult : notnull
    {
        CheckDisposed();
        ColumnData columnData = GetColumnData(index);
        if (columnData.IsNull)
        {
            throw new SqlxException($"Expected field #{index} to be non-null but found null");
        }
        
        ref readonly PgColumnMetadata columnMetadata = ref columnData.ColumnMetadata;
        if (TType.DbType != columnMetadata.TypeInfo
            && !TType.IsCompatible(columnMetadata.TypeInfo))
        {
            throw ColumnDecodeException.Create<TResult, PgColumnMetadata>(columnMetadata);
        }

        var bytes = columnData.Data;
        switch (columnMetadata.FormatCode)
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
                    PgTextValue textValue = new(chars, columnMetadata);
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
                var value = new PgBinaryValue(bytes, columnMetadata);
                return TType.DecodeBytes(ref value);
            default:
                throw ColumnDecodeException.Create<TResult, PgColumnMetadata>(
                    columnData.ColumnMetadata,
                    $"Unexpected format code: {columnData.ColumnMetadata.FormatCode}");
        }
    }

    private void CheckValidIndex(int index)
    {
        if (index >= 0 && index < _columnCount) return;
        throw new ArgumentOutOfRangeException(
            nameof(index),
            $"Invalid index. Must be between 0..{_columnCount - 1}");
    }

    private void CheckDisposed() => ObjectDisposedException.ThrowIf(_isDisposed, typeof(PgDataRow));

    public void Dispose()
    {
        if (_isDisposed) return;

        _isDisposed = true;
        
        _rowData = default;
    }
}
