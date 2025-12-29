using Sqlx.Postgres.Query;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Connection;

public partial class PgConnectionTest
{
    [Test]
    public async Task ExecuteScalar_Should_EncodeAndDecode_When_CircleAndDefaultEncoding(CancellationToken ct)
    {
        var value = new PgCircle(new PgPoint(5.63, 8.59), 4);
        using IPgConnection connection = databaseFixture.BasicPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery("SELECT $1 circle_col;");
        query.Bind(value);
        var result = await query.ExecuteScalar<PgCircle>(ct);
        await Assert.That(result).IsEqualTo(value);
    }

    [Test]
    public async Task ExecuteScalar_Should_Decode_When_CircleAndTextEncoding(CancellationToken ct)
    {
        const string sql = "SELECT '<(5.63,8.59),4>'::circle;";
        var value = new PgCircle(new PgPoint(5.63, 8.59), 4);
        using IPgConnection
            connection = databaseFixture.SimpleQueryTextPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery(sql);
        var result = await query.ExecuteScalar<PgCircle>(ct);
        await Assert.That(result).IsEqualTo(value);
    }
}
