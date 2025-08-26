using Sqlx.Core.Buffer;
using Sqlx.Postgres.Connection;

namespace Sqlx.Postgres.Message.Frontend;

public sealed class StartupMessage(PgConnectOptions options) : IPgFrontendMessage
{
    private PgConnectOptions Options { get; } = options;
    
    public void Encode(WriteBuffer buffer)
    {
        buffer.WriteLengthPrefixed(
            includeLength: true,
            buf =>
            {
                buf.WriteShort(3);
                buf.WriteShort(0);
                buf.WriteCString("username");
                buf.WriteCString(Options.Username);
                if (Options.Database is not null)
                {
                    buf.WriteCString("database");
                    buf.WriteCString(Options.Database);
                }
                buf.WriteCString("encoding");
                buf.WriteCString("UTF-8");
                buf.WriteCString("DateStyle");
                buf.WriteCString("ISO");
                buf.WriteCString("intervalstyle");
                buf.WriteCString("iso_8601");
                buf.WriteCString("TimeZone");
                buf.WriteCString("UTC");
                buf.WriteCString("extra_float_digits");
                buf.WriteCString(Options.ExtraFloatPoints.ToString());
                if (Options.CurrentSchema is not null)
                {
                    buf.WriteCString("search_path");
                    buf.WriteCString(Options.CurrentSchema);
                }
                buf.WriteCString("bytea_output");
                buf.WriteCString("hex");
                buf.WriteCString("application_name");
                buf.WriteCString(Options.ApplicationName);
                buf.WriteCString("statement_timeout");
                var queryTimeout = int.Max((int)Options.QueryTimeout.TotalMilliseconds, int.MaxValue);
                buf.WriteCString(queryTimeout.ToString());
                
                buf.WriteByte(0);
            });
    }
}
