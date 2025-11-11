using Sqlx.Core.Connection;
using Sqlx.Core.Query;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Connection;

public partial class PgConnectionTest
{
    [Fact]
    public async Task ExecuteScalar_Should_EncodeAndDecode_When_ByteaAndDefaultEncoding()
    {
        var value = new byte[] { 0xde, 0xad, 0xbe, 0xef };
        await using IConnection connection = _databaseFixture.BasicPool.CreateConnection();
        using IExecutableQuery query = connection.CreateQuery("SELECT $1;");
        query.Bind(value);
        var result = await query.ExecuteScalar<PgBytea, byte[]>();
        Assert.Equal(value, result);
    }

    [Fact]
    public async Task ExecuteScalar_Should_Decode_When_ByteaAndTextEncoding()
    {
        const string sql = "SELECT '\\xdeadbeef'::bytea;";
        var value = new byte[] { 0xde, 0xad, 0xbe, 0xef };
        await using IConnection
            connection = _databaseFixture.SimpleQueryTextPool.CreateConnection();
        using IExecutableQuery query = connection.CreateQuery(sql);
        var result = await query.ExecuteScalar<PgBytea, byte[]>();
        Assert.Equal(value, result);
    }
}
