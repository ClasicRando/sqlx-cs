using Sqlx.Postgres.Query;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Connection;

public partial class PgConnectionTest
{
    [Test]
    [Arguments(true)]
    [Arguments(false)]
    public async Task ExecuteScalar_Should_EncodeAndDecode_When_BoolAndDefaultEncoding(bool value, CancellationToken ct)
    {
        using IPgConnection connection = DatabaseFixture.BasicPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery("SELECT $1 bool_col;");
        query.Bind(value);
        var result = await query.ExecuteScalar<bool, PgBool>(ct);
        await Assert.That(result).IsEqualTo(value);
    }

    [Test]
    [Arguments(true, "SELECT true;")]
    [Arguments(false, "SELECT false;")]
    public async Task ExecuteScalar_Should_Decode_When_BoolAndTextEncoding(
        bool value,
        string sql,
        CancellationToken ct)
    {
        using IPgConnection
            connection = DatabaseFixture.SimpleQueryTextPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery(sql);
        var result = await query.ExecuteScalar<bool, PgBool>(ct);
        await Assert.That(result).IsEqualTo(value);
    }
}
