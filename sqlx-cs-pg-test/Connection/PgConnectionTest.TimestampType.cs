using Sqlx.Postgres.Query;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Connection;

public partial class PgConnectionTest
{
    [Fact]
    public async Task ExecuteScalar_Should_EncodeAndDecode_When_TimestampAndDefaultEncoding()
    {
        var value = new DateTime(2025, 1, 1, 1, 23, 45);
        using IPgConnection connection = _databaseFixture.BasicPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery("SELECT $1 timestamp_col;");
        query.Bind(value);
        DateTime result = await query.ExecuteScalar<PgDateTime, DateTime>();
        Assert.Equal(value, result);
    }

    [Fact]
    public async Task ExecuteScalar_Should_Decode_When_TimestampAndTextEncoding()
    {
        const string sql = "SELECT '2025-01-01 01:23:45'::timestamp;";
        var value = new DateTime(2025, 1, 1, 1, 23, 45);
        using IPgConnection
            connection = _databaseFixture.SimpleQueryTextPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery(sql);
        DateTime result = await query.ExecuteScalar<PgDateTime, DateTime>();
        Assert.Equal(value, result);
    }
}
