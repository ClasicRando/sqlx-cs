using Sqlx.Postgres.Query;

namespace Sqlx.Postgres.Connection;

public partial class PgConnectionTest
{
    [Test]
    public async Task ExecuteScalar_Should_EncodeAndDecode_When_TimeAndDefaultEncoding(CancellationToken ct)
    {
        var value = new TimeOnly(4, 5, 6, 789, 123);
        await using IPgConnection connection = DatabaseFixture.BasicPool.CreateConnection();
        await using IPgExecutableQuery query = connection.CreateQuery("SELECT $1 time_col;");
        query.Bind(value);
        var result = await query.ExecuteScalar<TimeOnly>(ct);
        await Assert.That(result).IsEqualTo(value);
    }

    [Test]
    public async Task ExecuteScalar_Should_Decode_When_TimeAndTextEncoding(CancellationToken ct)
    {
        const string sql = "SELECT '04:05:06.789123'::time;";
        var value = new TimeOnly(4, 5, 6, 789, 123);
        await using IPgConnection
            connection = DatabaseFixture.SimpleQueryTextPool.CreateConnection();
        await using IPgExecutableQuery query = connection.CreateQuery(sql);
        var result = await query.ExecuteScalar<TimeOnly>(ct);
        await Assert.That(result).IsEqualTo(value);
    }
}
