using Sqlx.Postgres.Query;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Connection;

public partial class PgConnectionTest
{
    [Fact]
    public async Task ExecuteScalar_Should_EncodeAndDecode_When_TimestampTzAndDefaultEncoding()
    {
        var value = new DateTimeOffset(2025, 1, 1, 1, 23, 45, TimeSpan.FromHours(2));
        await using IPgConnection connection = _databaseFixture.BasicPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery("SELECT $1;");
        query.Bind(value);
        DateTimeOffset result = await query.ExecuteScalar<PgDateTimeOffset, DateTimeOffset>();
        Assert.Equal(value.UtcDateTime, result);
    }

    [Fact]
    public async Task ExecuteScalar_Should_Decode_When_TimestampTzAndTextEncoding()
    {
        const string sql = "SELECT '2025-01-01 01:23:45+02'::timestamptz;";
        var value = new DateTimeOffset(2025, 1, 1, 1, 23, 45, TimeSpan.FromHours(2));
        await using IPgConnection
            connection = _databaseFixture.SimpleQueryTextPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery(sql);
        DateTimeOffset result = await query.ExecuteScalar<PgDateTimeOffset, DateTimeOffset>();
        Assert.Equal(value.UtcDateTime, result);
    }
}
