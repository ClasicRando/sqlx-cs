using Sqlx.Core.Buffer;

namespace Sqlx.Postgres.Message.Backend;

internal sealed class ReadyForQueryMessage(TransactionStatus transactionStatus) : IPgBackendMessage, IPgBackendMessageDecoder<ReadyForQueryMessage>
{
    internal TransactionStatus TransactionStatus { get; } = transactionStatus;
    
    public static ReadyForQueryMessage Decode(ReadBuffer buffer)
    {
        return new ReadyForQueryMessage((TransactionStatus)buffer.ReadByte());
    }
}
