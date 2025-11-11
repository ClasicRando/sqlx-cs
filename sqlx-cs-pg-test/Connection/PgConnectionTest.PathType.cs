using Sqlx.Core.Connection;
using Sqlx.Core.Query;
using Sqlx.Postgres.Query;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Connection;

public partial class PgConnectionTest
{
    [Fact]
    public async Task ExecuteScalar_Should_EncodeAndDecode_When_PathAndDefaultEncoding()
    {
        var value = new PgPath(false, [new PgPoint(5.63, 8.59), new PgPoint(4.87, 2.8)]);
        await using IConnection connection = _databaseFixture.BasicPool.CreateConnection();
        using IExecutableQuery query = connection.CreateQuery("SELECT $1;");
        query.Bind(value);
        var result = await query.ExecuteScalarPg<PgPath>();
        Assert.Equal(value, result);
    }

    [Fact]
    public async Task ExecuteScalar_Should_Decode_When_PathAndTextEncoding()
    {
        const string sql = "SELECT '[(5.63,8.59),(4.87,2.8)]'::path;";
        var value = new PgPath(false, [new PgPoint(5.63, 8.59), new PgPoint(4.87, 2.8)]);
        await using IConnection
            connection = _databaseFixture.SimpleQueryTextPool.CreateConnection();
        using IExecutableQuery query = connection.CreateQuery(sql);
        var result = await query.ExecuteScalarPg<PgPath>();
        Assert.Equal(value, result);
    }
}
