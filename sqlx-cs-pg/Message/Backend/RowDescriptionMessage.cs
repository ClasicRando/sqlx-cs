using Sqlx.Core.Buffer;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Message.Backend;

/// <summary>
/// Message sent by the backend to describe the rows returned by a simple query or prepared
/// statement. This message is sent in response to a describe request of a prepared statement sent
/// by the client or packaged into the response of a simple query to describe the result. 
/// </summary>
internal record RowDescriptionMessage(PgColumnMetadata[] ColumnMetadata)
    : IPgBackendMessage, IPgBackendMessageDecoder<RowDescriptionMessage>
{
    public static RowDescriptionMessage Decode(ReadBuffer buffer)
    {
        var columnCount = buffer.ReadShort();
        var metadata = new PgColumnMetadata[columnCount];
        for (var i = 0; i < columnCount; i++)
        {
            metadata[i] = new PgColumnMetadata(
                FieldName: buffer.ReadCString(),
                TableOid: buffer.ReadInt(),
                ColumnAttribute: buffer.ReadShort(),
                PgTypeInfo: PgTypeInfo.FromOid(buffer.ReadInt()),
                DataTypeSize: buffer.ReadShort(),
                TypeModifier: buffer.ReadInt(),
                FormatCode: (PgFormatCode)buffer.ReadShort());
        }

        return new RowDescriptionMessage(metadata);
    }
}
