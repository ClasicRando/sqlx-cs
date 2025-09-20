using Sqlx.Postgres.Message.Backend;

namespace Sqlx.Postgres.Message.Auth;

/// <summary>
/// Marker interface for contents of an authentication message sent from a Postgres backend.
/// Messages can either initiate the authentication process or continue the process with more
/// data/context to continue. Currently, only Cleartext, MD5 and SASL authentication are supported.
/// </summary>
internal interface IAuthMessage : IPgBackendMessage;
