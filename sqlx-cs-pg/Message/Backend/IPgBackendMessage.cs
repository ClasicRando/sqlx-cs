namespace Sqlx.Postgres.Message.Backend;

/// <summary>
/// Marker interface to associate a type as a message sent by the database backend. Implementors of
/// this interface should either be singleton messages with no contents or also implement the
/// <see cref="IPgBackendMessageDecoder{TMessage}"/> interface for itself to allow for decoding
/// a buffer into a new message instance.
/// </summary>
internal interface IPgBackendMessage;
