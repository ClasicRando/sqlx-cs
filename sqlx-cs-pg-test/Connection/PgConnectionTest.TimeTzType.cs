using Sqlx.Postgres.Query;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Connection;

public partial class PgConnectionTest
{
    [Test]
    public async Task ExecuteScalar_Should_EncodeAndDecode_When_TimeTzAndDefaultEncoding(CancellationToken ct)
    {
        var value = new PgTimeTz(new TimeOnly(4, 5, 6, 789, 0), 3600);
        using IPgConnection connection = databaseFixture.BasicPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery("SELECT $1 timetz_col;");
        query.Bind(value);
        var result = await query.ExecuteScalar<PgTimeTz>(ct);
        await Assert.That(result).IsEqualTo(value);
    }

    [Test]
    public async Task ExecuteScalar_Should_Decode_When_TimeTzAndTextEncoding(CancellationToken ct)
    {
        const string sql = "SELECT '04:05:06.789+01'::timetz;";
        var value = new PgTimeTz(new TimeOnly(4, 5, 6, 789, 0), 3600);
        using IPgConnection
            connection = databaseFixture.SimpleQueryTextPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery(sql);
        var result = await query.ExecuteScalar<PgTimeTz>(ct);
        await Assert.That(result).IsEqualTo(value);
    }
}
