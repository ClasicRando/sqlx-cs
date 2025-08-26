using Sqlx.Core.Buffer;

namespace Sqlx.Postgres.Message.Backend;

internal sealed class DataRowMessage(byte[] rowData) : IPgBackendMessage, IPgBackendMessageDecoder<DataRowMessage>
{
    public byte[] RowData { get; } = rowData;
    
    public static DataRowMessage Decode(ReadBuffer buffer)
    {
        return new DataRowMessage(buffer.ReadBytes());
    }
}
