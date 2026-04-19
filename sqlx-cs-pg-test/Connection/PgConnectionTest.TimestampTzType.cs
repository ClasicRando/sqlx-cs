using Sqlx.Postgres.Query;

namespace Sqlx.Postgres.Connection;

public partial class PgConnectionTest
{
    [Test]
    public async Task ExecuteScalar_Should_EncodeAndDecode_When_TimestampTzAndDefaultEncoding(CancellationToken ct)
    {
        var value = new DateTimeOffset(2025, 1, 1, 1, 23, 45, TimeSpan.FromHours(2));
        await using IPgConnection connection = DatabaseFixture.BasicPool.CreateConnection();
        await using IPgExecutableQuery query = connection.CreateQuery("SELECT $1 timestamptz_col;");
        query.Bind(value);
        var result = await query.ExecuteScalar<DateTimeOffset>(ct);
        await Assert.That(result).Member(r => r.UtcDateTime, r => r.IsEqualTo(value.UtcDateTime));
    }

    [Test]
    public async Task ExecuteScalar_Should_Decode_When_TimestampTzAndTextEncoding(CancellationToken ct)
    {
        const string sql = "SELECT '2025-01-01 01:23:45+02'::timestamptz;";
        var value = new DateTimeOffset(2025, 1, 1, 1, 23, 45, TimeSpan.FromHours(2));
        await using IPgConnection
            connection = DatabaseFixture.SimpleQueryTextPool.CreateConnection();
        await using IPgExecutableQuery query = connection.CreateQuery(sql);
        var result = await query.ExecuteScalar<DateTimeOffset>(ct);
        await Assert.That(result).Member(r => r.UtcDateTime, r => r.IsEqualTo(value.UtcDateTime));
    }
}
