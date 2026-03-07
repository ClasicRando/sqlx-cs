namespace Sqlx.Postgres.Message.Backend;

#pragma warning disable CA1008
public enum PgBackendMessageType
#pragma warning restore CA1008
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
