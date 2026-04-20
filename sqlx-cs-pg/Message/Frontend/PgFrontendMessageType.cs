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

    // Flush = (byte)'H',
    Parse = (byte)'P',
    Password = (byte)'p',
    Query = (byte)'Q',
    Sync = (byte)'S',
    Terminate = (byte)'X',
}
