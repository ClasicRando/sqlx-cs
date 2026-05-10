using Sqlx.Core.Types;
using Sqlx.Postgres.Query;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Connection;

public partial class PgConnectionTest
{
    [Test]
    public async Task ExecuteScalar_Should_EncodeAndDecode_When_JsonAndDefaultEncoding(CancellationToken ct)
    {
        var value = new Inner(1, "Test1");
        await using IPgConnection connection = DatabaseFixture.BasicPool.CreateConnection();
        await using IPgExecutableQuery query = connection.CreateQuery("SELECT $1 json_col;");
        query.Bind(new JsonValue<Inner> { Inner = value });
        var result = await query.ExecuteScalar<JsonValue<Inner>>(ct);
        await Assert.That(result.Inner).IsEqualTo(value);
    }

    [Test]
    public async Task ExecuteScalar_Should_Decode_When_JsonbAndTextEncoding(CancellationToken ct)
    {
        const string sql = "SELECT '{\"Id\":1,\"Name\":\"Test1\"}'::jsonb;";
        var value = new Inner(1, "Test1");
        await using IPgConnection
            connection = DatabaseFixture.SimpleQueryTextPool.CreateConnection();
        await using IPgExecutableQuery query = connection.CreateQuery(sql);
        var result = await query.ExecuteScalar<JsonValue<Inner>>(ct);
        await Assert.That(result.Inner).IsEqualTo(value);
    }

    [Test]
    [Arguments(true)]
    [Arguments(false)]
    public async Task ExecuteScalar_Should_Decode_When_JsonAndTextEncoding(
        bool useSourceGeneration,
        CancellationToken ct)
    {
        const string sql = "SELECT '{\"Id\":1,\"Name\":\"Test1\"}'::json;";
        var value = new Inner(1, "Test1");
        await using IPgConnection
            connection = DatabaseFixture.SimpleQueryTextPool.CreateConnection();
        await using IPgExecutableQuery query = connection.CreateQuery(sql);
        var result = await query.ExecuteScalar<JsonValue<Inner>>(ct);
        await Assert.That(result.Inner).IsEqualTo(value);
    }
}
