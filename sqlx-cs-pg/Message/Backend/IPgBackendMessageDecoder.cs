using Sqlx.Core.Buffer;

namespace Sqlx.Postgres.Message.Backend;

/// <summary>
/// Implementors provide a static factory method to decode a supplied buffer into a new instance of
/// the message type. Generally implementors should define self as the message type.
/// </summary>
/// <typeparam name="TMessage">Backend message type</typeparam>
internal interface IPgBackendMessageDecoder<out TMessage> where TMessage : IPgBackendMessage
{
    /// <summary>
    /// Decode the supplied buffer into the message type
    /// </summary>
    /// <param name="buffer">Message contents buffer</param>
    /// <returns>A new instance of the message type</returns>
    internal static abstract TMessage Decode(ReadBuffer buffer);
}
