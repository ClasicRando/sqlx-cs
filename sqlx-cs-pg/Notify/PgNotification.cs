namespace Sqlx.Postgres.Notify;

public record PgNotification(int ProcessId, string ChannelName, string Payload);
