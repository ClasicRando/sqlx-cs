using Sqlx.Postgres.Query;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Connection;

public partial class PgConnectionTest
{
    [Test]
    public async Task ExecuteScalar_Should_EncodeAndDecode_When_PointAndDefaultEncoding(CancellationToken ct)
    {
        var value = new PgPoint(5.63, 8.59);
        await using IPgConnection connection = DatabaseFixture.BasicPool.CreateConnection();
        await using IPgExecutableQuery query = connection.CreateQuery("SELECT $1 point_col;");
        query.Bind(value);
        var result = await query.ExecuteScalar<PgPoint>(ct);
        await Assert.That(result).IsEqualTo(value);
    }

    [Test]
    public async Task ExecuteScalar_Should_Decode_When_PointAndTextEncoding(CancellationToken ct)
    {
        const string sql = "SELECT '(5.63,8.59)'::point;";
        var value = new PgPoint(5.63, 8.59);
        await using IPgConnection
            connection = DatabaseFixture.SimpleQueryTextPool.CreateConnection();
        await using IPgExecutableQuery query = connection.CreateQuery(sql);
        var result = await query.ExecuteScalar<PgPoint>(ct);
        await Assert.That(result).IsEqualTo(value);
    }
}
