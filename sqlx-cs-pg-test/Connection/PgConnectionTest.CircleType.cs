using Sqlx.Postgres.Query;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Connection;

public partial class PgConnectionTest
{
    [Fact]
    public async Task ExecuteScalar_Should_EncodeAndDecode_When_CircleAndDefaultEncoding()
    {
        var value = new PgCircle(new PgPoint(5.63, 8.59), 4);
        await using IPgConnection connection = _databaseFixture.BasicPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery("SELECT $1;");
        query.Bind(value);
        var result = await query.ExecuteScalarPg<PgCircle>();
        Assert.Equal(value, result);
    }

    [Fact]
    public async Task ExecuteScalar_Should_Decode_When_CircleAndTextEncoding()
    {
        const string sql = "SELECT '<(5.63,8.59),4>'::circle;";
        var value = new PgCircle(new PgPoint(5.63, 8.59), 4);
        await using IPgConnection
            connection = _databaseFixture.SimpleQueryTextPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery(sql);
        var result = await query.ExecuteScalarPg<PgCircle>();
        Assert.Equal(value, result);
    }
}
