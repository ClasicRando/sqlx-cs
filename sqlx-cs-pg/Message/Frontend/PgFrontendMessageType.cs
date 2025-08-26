namespace Sqlx.Postgres.Message.Frontend;

internal enum PgFrontendMessageType : byte
{
    Bind = (byte)'B',
    Close = (byte)'C',
    CopyData = (byte)'d',
    CopyDone = (byte)'c',
    CopyFail = (byte)'f',
    Describe = (byte)'D',
    Execute = (byte)'E',
    Flush = (byte)'H',
    FunctionCall = (byte)'F',
    NegotiateProtocolVersion = (byte)'v',
    Parse = (byte)'P',
    Password = (byte)'p',
    Query = (byte)'Q',
    SaslResponse = (byte)'p',
    Sync = (byte)'S',
    Terminate = (byte)'X',
}
