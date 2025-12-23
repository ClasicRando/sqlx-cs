using Sqlx.Postgres.Query;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Connection;

public partial class PgConnectionTest
{
    [Fact]
    public async Task ExecuteScalar_Should_EncodeAndDecode_When_PointAndDefaultEncoding()
    {
        var value = new PgPoint(5.63, 8.59);
        using IPgConnection connection = _databaseFixture.BasicPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery("SELECT $1 point_col;");
        query.Bind(value);
        var result = await query.ExecuteScalarPg<PgPoint>();
        Assert.Equal(value, result);
    }

    [Fact]
    public async Task ExecuteScalar_Should_Decode_When_PointAndTextEncoding()
    {
        const string sql = "SELECT '(5.63,8.59)'::point;";
        var value = new PgPoint(5.63, 8.59);
        using IPgConnection
            connection = _databaseFixture.SimpleQueryTextPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery(sql);
        var result = await query.ExecuteScalarPg<PgPoint>();
        Assert.Equal(value, result);
    }
}
