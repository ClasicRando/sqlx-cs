using Sqlx.Postgres.Query;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Connection;

public partial class PgConnectionTest
{
    [Test]
    [Arguments(true)]
    [Arguments(false)]
    public async Task ExecuteScalar_Should_EncodeAndDecode_When_JsonAndDefaultEncoding(
        bool useSourceGeneration,
        CancellationToken ct)
    {
        var value = new Inner(1, "Test1");
        using IPgConnection connection = DatabaseFixture.BasicPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery("SELECT $1 json_col;");
        query.BindJson(value, useSourceGeneration ? SourceGenerationContext.Default.Inner : null);
        Inner result = await query.ExecuteScalarJson(
            useSourceGeneration ? SourceGenerationContext.Default.Inner : null,
            ct);
        await Assert.That(result).IsEqualTo(value);
    }

    [Test]
    [Arguments(true)]
    [Arguments(false)]
    public async Task ExecuteScalar_Should_Decode_When_JsonbAndTextEncoding(
        bool useSourceGeneration,
        CancellationToken ct)
    {
        const string sql = "SELECT '{\"Id\":1,\"Name\":\"Test1\"}'::jsonb;";
        var value = new Inner(1, "Test1");
        using IPgConnection
            connection = DatabaseFixture.SimpleQueryTextPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery(sql);
        Inner result = await query.ExecuteScalarJson(
            useSourceGeneration ? SourceGenerationContext.Default.Inner : null,
            ct);
        await Assert.That(result).IsEqualTo(value);
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
        using IPgConnection
            connection = DatabaseFixture.SimpleQueryTextPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery(sql);
        Inner result = await query.ExecuteScalarJson(
            useSourceGeneration ? SourceGenerationContext.Default.Inner : null,
            ct);
        await Assert.That(result).IsEqualTo(value);
    }
}
