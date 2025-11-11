using Sqlx.Core.Connection;
using Sqlx.Core.Query;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Connection;

public partial class PgConnectionTest
{
    [Fact]
    public async Task ExecuteScalar_Should_EncodeAndDecode_When_TimeAndDefaultEncoding()
    {
        var value = new TimeOnly(4, 5, 6, 789, 123);
        await using IConnection connection = _databaseFixture.BasicPool.CreateConnection();
        using IExecutableQuery query = connection.CreateQuery("SELECT $1;");
        query.Bind(value);
        TimeOnly result = await query.ExecuteScalar<PgTime, TimeOnly>();
        Assert.Equal(value, result);
    }

    [Fact]
    public async Task ExecuteScalar_Should_Decode_When_TimeAndTextEncoding()
    {
        const string sql = "SELECT '04:05:06.789123'::time;";
        var value = new TimeOnly(4, 5, 6, 789, 123);
        await using IConnection
            connection = _databaseFixture.SimpleQueryTextPool.CreateConnection();
        using IExecutableQuery query = connection.CreateQuery(sql);
        TimeOnly result = await query.ExecuteScalar<PgTime, TimeOnly>();
        Assert.Equal(value, result);
    }
}
