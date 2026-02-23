using Sqlx.Core.Buffer;
using Sqlx.Core.Result;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Exceptions;
using Sqlx.Postgres.Message.Auth;
using Sqlx.Postgres.Message.Backend;
using Sqlx.Postgres.Result;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Connector;

public partial class PgConnector
{
    internal async ValueTask<PgBackendMessageType> ReceiveNextMessageType(CancellationToken cancellationToken)
    {
        var format = await _asyncConnector.ReadByteAsync(cancellationToken).ConfigureAwait(false);
        return (PgBackendMessageType)format;
    }

    internal async ValueTask<int> ReceiveNextMessageSize(CancellationToken cancellationToken)
    {
        var size = await _asyncConnector.ReadIntAsync(cancellationToken).ConfigureAwait(false) - 4;
        await _asyncConnector.EnsureBufferFilled(size, cancellationToken).ConfigureAwait(false);
        return size;
    }

    internal void AdvanceReadBuffer(int size)
    {
        _asyncConnector.AdvanceBufferPosition(size);
    }
    
    internal PgColumnMetadata[] ReceiveRowDescriptionMessage(int size)
    {
        var buffer = _asyncConnector.ReadBuffer[..size];
        var columnCount = buffer.ReadShort();
        var metadata = new PgColumnMetadata[columnCount];
        for (var i = 0; i < columnCount; i++)
        {
            metadata[i] = new PgColumnMetadata(
                FieldName: buffer.ReadCString(),
                TableOid: buffer.ReadInt(),
                ColumnAttribute: buffer.ReadShort(),
                TypeInfo: PgTypeInfo.FromOid(new PgOid(buffer.ReadUInt())),
                DataTypeSize: buffer.ReadShort(),
                TypeModifier: buffer.ReadInt(),
                FormatCode: (PgFormatCode)buffer.ReadShort());
        }

        AdvanceReadBuffer(size);
        
        return metadata;
    }

    private byte[] ReceiveCopyDataMessage(int size)
    {
        var bytes = _asyncConnector.ReadBuffer[..size].ToArray();
        AdvanceReadBuffer(size);
        return bytes;
    }

    internal PgDataRow ReceiveRowDataMessage(int size, PgStatementMetadata pgStatementMetadata)
    {
        var buffer = _asyncConnector.ReadBuffer[..size];
        var dataRow = new PgDataRow(ref buffer, pgStatementMetadata);
        AdvanceReadBuffer(size);
        return dataRow;
    }

    internal QueryResult ReceiveQueryResult(int size)
    {
        var buffer = _asyncConnector.ReadBuffer[..size];
        var message = buffer.ReadCString();
        var rowCount = ExtractRowCount(message);
        AdvanceReadBuffer(size);
        return new QueryResult(rowCount, message);
    }

    /// <summary>
    /// Messages are in a format of a command keyword, followed by the rows count (except for INSERT
    /// which always has 0 before the row count) to extract the row count iterate backwards until
    /// we find a non-digit character and parse that span.
    /// </summary>
    /// <param name="message">Message to parse</param>
    /// <returns>Row count or -1 if parsing fails</returns>
    private static long ExtractRowCount(ReadOnlySpan<char> message)
    {
        var i = message.Length - 1;
        for (; i >= 0; i--)
        {
            if (char.IsDigit(message[i])) continue;
            
            i++;
            break;
        }

        return long.TryParse(message[i..], out var rowCount) ? rowCount : 0;
    }

    internal void HandleReadyForQueryMessage(int size)
    {
        var buffer = _asyncConnector.ReadBuffer[..size];
        var status = (TransactionStatus)buffer.ReadByte();
        AdvanceReadBuffer(size);
        HandleReadyForQuery(status);
    }

    private T ReceiveMessage<T>(int size) where T : IPgBackendMessage, IPgBackendMessageDecoder<T>
    {
        var buffer = _asyncConnector.ReadBuffer[..size];
        T message = T.Decode(buffer);
        AdvanceReadBuffer(size);
        return message;
    }

    private async Task<IAuthMessage> ReceiveAuthMessage(CancellationToken cancellationToken)
    {
        PgBackendMessageType messageType =
            await ReceiveNextMessageType(cancellationToken).ConfigureAwait(false);
        var size = await ReceiveNextMessageSize(cancellationToken).ConfigureAwait(false);
        
        cancellationToken.ThrowIfCancellationRequested();

        if (messageType is not PgBackendMessageType.Authentication)
        {
            AdvanceReadBuffer(size);
            throw new PgException($"Expected an Authentication message but found {messageType}");
        }
        
        var buffer = _asyncConnector.ReadBuffer[..size];
        IAuthMessage message = AuthenticationMessage.Decode(buffer);
        AdvanceReadBuffer(size);
        return message;
    }

    private async Task<T> ReceiveAuthMessageAs<T>(CancellationToken cancellationToken)
        where T : IAuthMessage
    {
        IAuthMessage message = await ReceiveAuthMessage(cancellationToken).ConfigureAwait(false);
        if (message is not T result)
        {
            throw new PgException(
                $"Expected message to be {typeof(T)} but found {message.GetType()}");
        }

        return result;
    }
}
