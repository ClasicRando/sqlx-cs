namespace Sqlx.Postgres.Message.Backend;

/// <summary>
/// Message sent from the server as the contents of a row copied from the server. Depending on the
/// copy format used to initialize the <c>COPY TO</c> action, the data might represent text or
/// binary encoded data.
/// </summary>
internal sealed class CopyDataMessage(byte[] data) : IPgBackendDataMessage
{
    public byte[] Data { get; } = data;
}
