using Sqlx.Postgres.Query;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Connection;

public partial class PgConnectionTest
{
    [Test]
    public async Task ExecuteScalar_Should_EncodeAndDecode_When_DateAndDefaultEncoding(CancellationToken ct)
    {
        var value = new DateOnly(2025, 1, 1);
        using IPgConnection connection = DatabaseFixture.BasicPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery("SELECT $1 date_col;");
        query.Bind(value);
        DateOnly result = await query.ExecuteScalar<DateOnly, PgDate>(ct);
        await Assert.That(result).IsEqualTo(value);
    }

    [Test]
    public async Task ExecuteScalar_Should_Decode_When_DateAndTextEncoding(CancellationToken ct)
    {
        const string sql = "SELECT '2025-01-01'::date;";
        var value = new DateOnly(2025, 1, 1);
        using IPgConnection
            connection = DatabaseFixture.SimpleQueryTextPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery(sql);
        DateOnly result = await query.ExecuteScalar<DateOnly, PgDate>(ct);
        await Assert.That(result).IsEqualTo(value);
    }
}
