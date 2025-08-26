using Sqlx.Core.Buffer;

namespace Sqlx.Postgres.Message.Frontend;

internal interface IPgFrontendMessage
{
    internal void Encode(WriteBuffer buffer);
}
