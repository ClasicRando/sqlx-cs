using Sqlx.Postgres.Query;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Connection;

public partial class PgConnectionTest
{
    [Test]
    public async Task ExecuteScalar_Should_EncodeAndDecode_When_PolygonAndDefaultEncoding(CancellationToken ct)
    {
        var value = new PgPolygon([new PgPoint(5.63, 8.59), new PgPoint(4.87, 2.8)]);
        using IPgConnection connection = DatabaseFixture.BasicPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery("SELECT $1 polygon_col;");
        query.BindPg(value);
        var result = await query.ExecuteScalar<PgPolygon>(ct);
        await Assert.That(result).IsEqualTo(value);
    }

    [Test]
    public async Task ExecuteScalar_Should_Decode_When_PolygonAndTextEncoding(CancellationToken ct)
    {
        const string sql = "SELECT '((5.63,8.59),(4.87,2.8))'::polygon;";
        var value = new PgPolygon([new PgPoint(5.63, 8.59), new PgPoint(4.87, 2.8)]);
        using IPgConnection
            connection = DatabaseFixture.SimpleQueryTextPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery(sql);
        var result = await query.ExecuteScalar<PgPolygon>(ct);
        await Assert.That(result).IsEqualTo(value);
    }
}
