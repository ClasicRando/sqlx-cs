using Sqlx.Postgres.Query;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Connection;

public partial class PgConnectionTest
{
    [Test]
    public async Task ExecuteScalar_Should_EncodeAndDecode_When_PathAndDefaultEncoding(CancellationToken ct)
    {
        var value = new PgPath(false, [new PgPoint(5.63, 8.59), new PgPoint(4.87, 2.8)]);
        using IPgConnection connection = DatabaseFixture.BasicPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery("SELECT $1 path_col;");
        query.Bind(value);
        var result = await query.ExecuteScalar<PgPath>(ct);
        await Assert.That(result).IsEqualTo(value);
    }

    [Test]
    public async Task ExecuteScalar_Should_Decode_When_PathAndTextEncoding(CancellationToken ct)
    {
        const string sql = "SELECT '[(5.63,8.59),(4.87,2.8)]'::path;";
        var value = new PgPath(false, [new PgPoint(5.63, 8.59), new PgPoint(4.87, 2.8)]);
        using IPgConnection
            connection = DatabaseFixture.SimpleQueryTextPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery(sql);
        var result = await query.ExecuteScalar<PgPath>(ct);
        await Assert.That(result).IsEqualTo(value);
    }
}
