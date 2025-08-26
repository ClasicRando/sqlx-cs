using Sqlx.Core.Buffer;

namespace Sqlx.Postgres.Message.Backend;

internal interface IPgBackendMessageDecoder<out TMessage> where TMessage : IPgBackendMessage
{
    internal static abstract TMessage Decode(ReadBuffer buffer);
}
