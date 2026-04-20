using Sqlx.Postgres.Query;

namespace Sqlx.Postgres.Connection;

public partial class PgConnectionTest
{
    [Test]
    public async Task ExecuteScalar_Should_EncodeAndDecode_When_DateAndDefaultEncoding(CancellationToken ct)
    {
        var value = new DateOnly(2025, 1, 1);
        await using IPgConnection connection = DatabaseFixture.BasicPool.CreateConnection();
        await using IPgExecutableQuery query = connection.CreateQuery("SELECT $1 date_col;");
        query.Bind(value);
        var result = await query.ExecuteScalar<DateOnly>(ct);
        await Assert.That(result).IsEqualTo(value);
    }

    [Test]
    public async Task ExecuteScalar_Should_Decode_When_DateAndTextEncoding(CancellationToken ct)
    {
        const string sql = "SELECT '2025-01-01'::date;";
        var value = new DateOnly(2025, 1, 1);
        await using IPgConnection
            connection = DatabaseFixture.SimpleQueryTextPool.CreateConnection();
        await using IPgExecutableQuery query = connection.CreateQuery(sql);
        var result = await query.ExecuteScalar<DateOnly>(ct);
        await Assert.That(result).IsEqualTo(value);
    }
}
