using Sqlx.Postgres.Query;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Connection;

public partial class PgConnectionTest
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ExecuteScalar_Should_EncodeAndDecode_When_JsonAndDefaultEncoding(
        bool useSourceGeneration)
    {
        var value = new Inner(1, "Test1");
        await using IPgConnection connection = _databaseFixture.BasicPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery("SELECT $1;");
        query.BindJson(value, useSourceGeneration ? SourceGenerationContext.Default.Inner : null);
        Inner result = await query.ExecuteScalarJson(
            useSourceGeneration ? SourceGenerationContext.Default.Inner : null);
        Assert.Equal(value, result);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ExecuteScalar_Should_Decode_When_JsonbAndTextEncoding(
        bool useSourceGeneration)
    {
        const string sql = "SELECT '{\"Id\":1,\"Name\":\"Test1\"}'::jsonb;";
        var value = new Inner(1, "Test1");
        await using IPgConnection
            connection = _databaseFixture.SimpleQueryTextPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery(sql);
        Inner result = await query.ExecuteScalarJson(
            useSourceGeneration ? SourceGenerationContext.Default.Inner : null);
        Assert.Equal(value, result);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ExecuteScalar_Should_Decode_When_JsonAndTextEncoding(bool useSourceGeneration)
    {
        const string sql = "SELECT '{\"Id\":1,\"Name\":\"Test1\"}'::json;";
        var value = new Inner(1, "Test1");
        await using IPgConnection
            connection = _databaseFixture.SimpleQueryTextPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery(sql);
        Inner result = await query.ExecuteScalarJson(
            useSourceGeneration ? SourceGenerationContext.Default.Inner : null);
        Assert.Equal(value, result);
    }
}
