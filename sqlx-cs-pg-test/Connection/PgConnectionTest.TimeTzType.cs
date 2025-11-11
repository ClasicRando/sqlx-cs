using Sqlx.Core.Connection;
using Sqlx.Core.Query;
using Sqlx.Postgres.Query;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Connection;

public partial class PgConnectionTest
{
    [Fact]
    public async Task ExecuteScalar_Should_EncodeAndDecode_When_TimeTzAndDefaultEncoding()
    {
        var value = new PgTimeTz(new TimeOnly(4, 5, 6, 789, 0), 3600);
        await using IConnection connection = _databaseFixture.BasicPool.CreateConnection();
        using IExecutableQuery query = connection.CreateQuery("SELECT $1;");
        query.Bind(value);
        var result = await query.ExecuteScalarPg<PgTimeTz>();
        Assert.Equal(value, result);
    }

    [Fact]
    public async Task ExecuteScalar_Should_Decode_When_TimeTzAndTextEncoding()
    {
        const string sql = "SELECT '04:05:06.789+01'::timetz;";
        var value = new PgTimeTz(new TimeOnly(4, 5, 6, 789, 0), 3600);
        await using IConnection
            connection = _databaseFixture.SimpleQueryTextPool.CreateConnection();
        using IExecutableQuery query = connection.CreateQuery(sql);
        var result = await query.ExecuteScalarPg<PgTimeTz>();
        Assert.Equal(value, result);
    }
}
