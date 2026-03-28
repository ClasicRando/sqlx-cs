using Sqlx.Postgres.Query;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Connection;

public partial class PgConnectionTest
{
    [Test]
    public async Task ExecuteScalar_Should_EncodeAndDecode_When_LineAndDefaultEncoding(CancellationToken ct)
    {
        var value = new PgLine(5.63, 8.59, 4);
        using IPgConnection connection = DatabaseFixture.BasicPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery("SELECT $1 line_col;");
        query.BindPg(value);
        var result = await query.ExecuteScalar<PgLine>(ct);
        await Assert.That(result).IsEqualTo(value);
    }

    [Test]
    public async Task ExecuteScalar_Should_Decode_When_LineAndTextEncoding(CancellationToken ct)
    {
        const string sql = "SELECT '{5.63,8.59,4}'::line;";
        var value = new PgLine(5.63, 8.59, 4);
        using IPgConnection
            connection = DatabaseFixture.SimpleQueryTextPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery(sql);
        var result = await query.ExecuteScalar<PgLine>(ct);
        await Assert.That(result).IsEqualTo(value);
    }
}
