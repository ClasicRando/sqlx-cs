using Sqlx.Postgres.Query;

namespace Sqlx.Postgres.Connection;

public partial class PgConnectionTest
{
    [Test]
    public async Task ExecuteScalar_Should_EncodeAndDecode_When_UuidAndDefaultEncoding(CancellationToken ct)
    {
        Guid value = Guid.Parse("019a22a1-8d4c-7e71-8ac5-e31d330b866c");
        await using IPgConnection connection = DatabaseFixture.BasicPool.CreateConnection();
        await using IPgExecutableQuery query = connection.CreateQuery("SELECT $1 uuid_col;");
        query.Bind(value);
        var result = await query.ExecuteScalar<Guid>(ct);
        await Assert.That(result).IsEqualTo(value);
    }

    [Test]
    public async Task ExecuteScalar_Should_Decode_When_UuidAndTextEncoding(CancellationToken ct)
    {
        const string sql = "SELECT '019a22a1-8d4c-7e71-8ac5-e31d330b866c'::uuid;";
        Guid value = Guid.Parse("019a22a1-8d4c-7e71-8ac5-e31d330b866c");
        await using IPgConnection
            connection = DatabaseFixture.SimpleQueryTextPool.CreateConnection();
        await using IPgExecutableQuery query = connection.CreateQuery(sql);
        var result = await query.ExecuteScalar<Guid>(ct);
        await Assert.That(result).IsEqualTo(value);
    }
}
