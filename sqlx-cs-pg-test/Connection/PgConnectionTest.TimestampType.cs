using Sqlx.Core.Connection;
using Sqlx.Core.Query;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Connection;

public partial class PgConnectionTest
{
    [Fact]
    public async Task ExecuteScalar_Should_EncodeAndDecode_When_TimestampAndDefaultEncoding()
    {
        var value = new DateTime(2025, 1, 1, 1, 23, 45);
        await using IConnection connection = _databaseFixture.BasicPool.CreateConnection();
        using IExecutableQuery query = connection.CreateQuery("SELECT $1;");
        query.Bind(value);
        DateTime result = await query.ExecuteScalar<PgDateTime, DateTime>();
        Assert.Equal(value, result);
    }

    [Fact]
    public async Task ExecuteScalar_Should_Decode_When_TimestampAndTextEncoding()
    {
        const string sql = "SELECT '2025-01-01 01:23:45'::timestamp;";
        var value = new DateTime(2025, 1, 1, 1, 23, 45);
        await using IConnection
            connection = _databaseFixture.SimpleQueryTextPool.CreateConnection();
        using IExecutableQuery query = connection.CreateQuery(sql);
        DateTime result = await query.ExecuteScalar<PgDateTime, DateTime>();
        Assert.Equal(value, result);
    }
}
