using Sqlx.Postgres.Query;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Connection;

public partial class PgConnectionTest
{
    [Test]
    public async Task ExecuteScalar_Should_EncodeAndDecode_When_LineSegmentAndDefaultEncoding(CancellationToken ct)
    {
        var value = new PgLineSegment(new PgPoint(5.63, 8.59), new PgPoint(4.87, 2.8));
        await using IPgConnection connection = DatabaseFixture.BasicPool.CreateConnection();
        await using IPgExecutableQuery query = connection.CreateQuery("SELECT $1 lseg_col;");
        query.Bind(value);
        var result = await query.ExecuteScalar<PgLineSegment>(ct);
        await Assert.That(result).IsEqualTo(value);
    }

    [Test]
    public async Task ExecuteScalar_Should_Decode_When_LineSegmentAndTextEncoding(CancellationToken ct)
    {
        const string sql = "SELECT '((5.63,8.59),(4.87,2.8))'::lseg;";
        var value = new PgLineSegment(new PgPoint(5.63, 8.59), new PgPoint(4.87, 2.8));
        await using IPgConnection
            connection = DatabaseFixture.SimpleQueryTextPool.CreateConnection();
        await using IPgExecutableQuery query = connection.CreateQuery(sql);
        var result = await query.ExecuteScalar<PgLineSegment>(ct);
        await Assert.That(result).IsEqualTo(value);
    }
}
