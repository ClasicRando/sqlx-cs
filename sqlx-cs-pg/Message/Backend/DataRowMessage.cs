namespace Sqlx.Postgres.Message.Backend;

/// <summary>
/// This message is sent as part of a query result and represents a single row of the result.
/// Although the buffer contents are not parsed right away, the structure is the number of column
/// values as a <see cref="short"/> (possible to be zero) and then the column values (if present).
/// For each column the value can be decoded as the length of the column value in bytes as a
/// <see cref="short"/> (-1 is a special value for null) followed by the encoded value of column as
/// bytes with a size that equals the previous value. In the case of a null value (-1 length) there
/// are no bytes after the length.
/// </summary>
/// <param name="rowData"></param>
internal sealed class DataRowMessage(byte[] rowData)
    : IPgBackendDataMessage, IPgBackendMessageDecoder<DataRowMessage>
{
    public byte[] RowData { get; } = rowData;

    public static DataRowMessage Decode(ReadOnlySpan<byte> buffer)
    {
        return new DataRowMessage(buffer.ToArray());
    }
}
