using Sqlx.Postgres.Query;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Connection;

public partial class PgConnectionTest
{
    [Fact]
    public async Task ExecuteScalar_Should_EncodeAndDecode_When_BoxAndDefaultEncoding()
    {
        var value = new PgBox(new PgPoint(1,2), new PgPoint(3,4));
        using IPgConnection connection = _databaseFixture.BasicPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery("SELECT $1 box_bol;");
        query.Bind(value);
        var result = await query.ExecuteScalarPg<PgBox>();
        Assert.Equal(value, result);
    }

    [Fact]
    public async Task ExecuteScalar_Should_Decode_When_BoxAndTextEncoding()
    {
        const string sql = "SELECT '(3,4),(1,2)'::box;";
        var value = new PgBox(new PgPoint(1,2), new PgPoint(3,4));
        using IPgConnection
            connection = _databaseFixture.SimpleQueryTextPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery(sql);
        var result = await query.ExecuteScalarPg<PgBox>();
        Assert.Equal(value, result);
    }
}
