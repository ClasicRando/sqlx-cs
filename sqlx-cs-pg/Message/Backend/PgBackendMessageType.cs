namespace Sqlx.Postgres.Message.Backend;

internal enum PgBackendMessageType : byte
{
    Authentication = (byte)'R',
    BackendDataKey = (byte)'K',
    BindComplete = (byte)'2',
    CloseComplete = (byte)'3',
    CommandComplete = (byte)'C',
    CopyData = (byte)'d',
    CopyDone = (byte)'c',
    CopyInResponse = (byte)'G',
    CopyOutResponse = (byte)'H',
    CopyBothResponse = (byte)'W',
    DataRow = (byte)'D',
    EmptyQueryResponse = (byte)'I',
    ErrorResponse = (byte)'E',
    FunctionCallResponse = (byte)'V',
    NegotiateProtocolVersion = (byte)'v',
    NoData = (byte)'n',
    NoticeResponse = (byte)'N',
    NotificationResponse = (byte)'A',
    ParameterDescription = (byte)'t',
    ParameterStatus = (byte)'S',
    ParseComplete = (byte)'1',
    PortalSuspend = (byte)'s',
    ReadyForQuery = (byte)'Z',
    RowDescription = (byte)'T',
}
