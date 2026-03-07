using Sqlx.Postgres.Query;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Connection;

public partial class PgConnectionTest
{
    [Test]
    public async Task ExecuteScalar_Should_EncodeAndDecode_When_CharAndDefaultEncoding(CancellationToken ct)
    {
        const sbyte value = (sbyte)'e';
        using IPgConnection connection = DatabaseFixture.BasicPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery("SELECT $1 char_col;");
        query.Bind(value);
        var result = await query.ExecuteScalar<sbyte, PgChar>(ct);
        await Assert.That(result).IsEqualTo(value);
    }

    [Test]
    public async Task ExecuteScalar_Should_Decode_When_CharAndTextEncoding(CancellationToken ct)
    {
        const string sql = "SELECT 'e'::\"char\";";
        const sbyte value = (sbyte)'e';
        using IPgConnection
            connection = DatabaseFixture.SimpleQueryTextPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery(sql);
        var result = await query.ExecuteScalar<sbyte, PgChar>(ct);
        await Assert.That(result).IsEqualTo(value);
    }
}
