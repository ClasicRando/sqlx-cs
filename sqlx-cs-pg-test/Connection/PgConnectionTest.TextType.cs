using Sqlx.Postgres.Query;

namespace Sqlx.Postgres.Connection;

public partial class PgConnectionTest
{
    [Test]
    public async Task ExecuteScalar_Should_EncodeAndDecode_When_TextAndDefaultEncoding(CancellationToken ct)
    {
        const string value = "This is a test";
        await using IPgConnection connection = DatabaseFixture.BasicPool.CreateConnection();
        await using IPgExecutableQuery query = connection.CreateQuery("SELECT $1 text_col;");
        query.Bind(value);
        var result = await query.ExecuteScalar<string>(ct);
        await Assert.That(result).IsEqualTo(value);
    }

    [Test]
    public async Task ExecuteScalar_Should_Decode_When_TextAndTextEncoding(CancellationToken ct)
    {
        const string sql = "SELECT 'This is a test';";
        const string value = "This is a test";
        await using IPgConnection
            connection = DatabaseFixture.SimpleQueryTextPool.CreateConnection();
        await using IPgExecutableQuery query = connection.CreateQuery(sql);
        var result = await query.ExecuteScalar<string>(ct);
        await Assert.That(result).IsEqualTo(value);
    }
}
