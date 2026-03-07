namespace Sqlx.Postgres.Message.Frontend;

/// <summary>
/// Target of a <see cref="PgFrontendMessageType.Close"/> and
/// <see cref="PgFrontendMessageType.Describe"/> message.
/// </summary>
internal enum MessageTarget
{
    PreparedStatement = (byte)'S',
    Portal = (byte)'P',
}
