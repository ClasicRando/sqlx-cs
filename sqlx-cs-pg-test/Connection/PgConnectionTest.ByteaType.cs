using Sqlx.Postgres.Query;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Connection;

public partial class PgConnectionTest
{
    [Test]
    public async Task ExecuteScalar_Should_EncodeAndDecode_When_ByteaAndDefaultEncoding(CancellationToken ct)
    {
        var value = new byte[] { 0xde, 0xad, 0xbe, 0xef };
        using IPgConnection connection = DatabaseFixture.BasicPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery("SELECT $1 bytea_col;");
        query.Bind(value);
        var result = await query.ExecuteScalar<byte[], PgBytea>(ct);
        await Assert.That(result).IsEquivalentTo(value);
    }

    [Test]
    public async Task ExecuteScalar_Should_Decode_When_ByteaAndTextEncoding(CancellationToken ct)
    {
        const string sql = "SELECT '\\xdeadbeef'::bytea;";
        var value = new byte[] { 0xde, 0xad, 0xbe, 0xef };
        using IPgConnection
            connection = DatabaseFixture.SimpleQueryTextPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery(sql);
        var result = await query.ExecuteScalar<byte[], PgBytea>(ct);
        await Assert.That(result).IsEquivalentTo(value);
    }
}
