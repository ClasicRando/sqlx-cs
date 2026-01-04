using System.Buffers;
using Sqlx.Core.Buffer;

namespace Sqlx.Postgres.Message.Backend;

/// <summary>
/// Message sent to signal that a query flow is done and the backend is ready for another query. The
/// only exception is when the client is pipelining queries and each query should be treated as an
/// isolated unit. In that case this message will be sent for every sync and a new query could be
/// executed but the client should wait until the pipeline is done.
/// </summary>
internal record ReadyForQueryMessage(TransactionStatus TransactionStatus)
    : IPgBackendMessage, IPgBackendMessageDecoder<ReadyForQueryMessage>
{
    public static ReadyForQueryMessage Decode(ReadOnlySequence<byte> buffer)
    {
        return new ReadyForQueryMessage((TransactionStatus)buffer.ReadByte());
    }
}
