using Sqlx.Core.Buffer;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Message.Backend;

internal sealed class RowDescriptionMessage(PgColumnMetadata[] columnMetadata) : IPgBackendMessage, IPgBackendMessageDecoder<RowDescriptionMessage>
{
    internal PgColumnMetadata[] ColumnMetadata { get; } = columnMetadata;
    
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
                PgType: new PgType(buffer.ReadInt()),
                DataTypeSize: buffer.ReadShort(),
                TypeModifier: buffer.ReadInt(),
                FormatCode: (PgFormatCode)buffer.ReadShort());
        }

        return new RowDescriptionMessage(metadata);
    }
}
