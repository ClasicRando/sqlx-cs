using Sqlx.Postgres.Query;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Connection;

public partial class PgConnectionTest
{
    [Fact]
    public async Task ExecuteScalar_Should_EncodeAndDecode_When_PolygonAndDefaultEncoding()
    {
        var value = new PgPolygon([new PgPoint(5.63, 8.59), new PgPoint(4.87, 2.8)]);
        await using IPgConnection connection = _databaseFixture.BasicPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery("SELECT $1;");
        query.Bind(value);
        var result = await query.ExecuteScalarPg<PgPolygon>();
        Assert.Equal(value, result);
    }

    [Fact]
    public async Task ExecuteScalar_Should_Decode_When_PolygonAndTextEncoding()
    {
        const string sql = "SELECT '((5.63,8.59),(4.87,2.8))'::polygon;";
        var value = new PgPolygon([new PgPoint(5.63, 8.59), new PgPoint(4.87, 2.8)]);
        await using IPgConnection
            connection = _databaseFixture.SimpleQueryTextPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery(sql);
        var result = await query.ExecuteScalarPg<PgPolygon>();
        Assert.Equal(value, result);
    }
}
