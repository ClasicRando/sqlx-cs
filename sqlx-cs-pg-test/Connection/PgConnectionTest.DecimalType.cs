using Sqlx.Postgres.Query;

namespace Sqlx.Postgres.Connection;

public partial class PgConnectionTest
{
    [Test]
    public async Task ExecuteScalar_Should_EncodeAndDecode_When_DecimalAndDefaultEncoding(CancellationToken ct)
    {
        var value = decimal.Parse("12345.67890");
        await using IPgConnection connection = DatabaseFixture.BasicPool.CreateConnection();
        await using IPgExecutableQuery query = connection.CreateQuery("SELECT $1 decimal_col;");
        query.Bind(value);
        var result = await query.ExecuteScalar<decimal>(ct);
        await Assert.That(result).IsEqualTo(value);
    }

    [Test]
    public async Task ExecuteScalar_Should_Decode_When_DecimalAndTextEncoding(CancellationToken ct)
    {
        const string sql = "SELECT '12345.67890'::numeric;";
        var value = decimal.Parse("12345.67890");
        await using IPgConnection
            connection = DatabaseFixture.SimpleQueryTextPool.CreateConnection();
        await using IPgExecutableQuery query = connection.CreateQuery(sql);
        var result = await query.ExecuteScalar<decimal>(ct);
        await Assert.That(result).IsEqualTo(value);
    }
}
