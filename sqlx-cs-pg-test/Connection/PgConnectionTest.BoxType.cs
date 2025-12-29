using Sqlx.Postgres.Query;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Connection;

public partial class PgConnectionTest
{
    [Test]
    public async Task ExecuteScalar_Should_EncodeAndDecode_When_BoxAndDefaultEncoding(CancellationToken ct)
    {
        var value = new PgBox(new PgPoint(1,2), new PgPoint(3,4));
        using IPgConnection connection = databaseFixture.BasicPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery("SELECT $1 box_bol;");
        query.Bind(value);
        var result = await query.ExecuteScalar<PgBox>(ct);
        await Assert.That(result).IsEqualTo(value);
    }

    [Test]
    public async Task ExecuteScalar_Should_Decode_When_BoxAndTextEncoding(CancellationToken ct)
    {
        const string sql = "SELECT '(3,4),(1,2)'::box;";
        var value = new PgBox(new PgPoint(1,2), new PgPoint(3,4));
        using IPgConnection
            connection = databaseFixture.SimpleQueryTextPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery(sql);
        var result = await query.ExecuteScalar<PgBox>(ct);
        await Assert.That(result).IsEqualTo(value);
    }
}
