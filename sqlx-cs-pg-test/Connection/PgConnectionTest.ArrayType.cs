using Sqlx.Postgres.Query;

namespace Sqlx.Postgres.Connection;

public partial class PgConnectionTest
{
    [Test]
    public async Task ExecuteScalar_Should_EncodeAndDecode_When_RefTypeArrayAndDefaultEncoding(CancellationToken ct)
    {
        string?[] input = ["this", "is", null, "a", "test"];
        await using IPgConnection connection = DatabaseFixture.BasicPool.CreateConnection();
        await using IPgExecutableQuery query = connection.CreateQuery("SELECT $1 text_array_col;");
        query.Bind(input);
        var result = await query.ExecuteScalar<string?[]>(ct);
        await Assert.That(result).IsEquivalentTo(input);
    }

    [Test]
    public async Task ExecuteScalar_Should_Decode_When_RefTypeArrayAndTextEncoding(CancellationToken ct)
    {
        string?[] input = ["this", "is", null, "a", "test"];
        await using IPgConnection
            connection = DatabaseFixture.SimpleQueryTextPool.CreateConnection();
        await using IPgExecutableQuery query =
            connection.CreateQuery("SELECT '{this,is,NULL,a,test}'::text[];");
        query.Bind(input);
        var result = await query.ExecuteScalar<string?[]>(ct);
        await Assert.That(result).IsEquivalentTo(input);
    }

    [Test]
    public async Task ExecuteScalar_Should_EncodeAndDecode_When_ValueTypeArrayAndDefaultEncoding(CancellationToken ct)
    {
        int?[] input = [-493, 0, null, 34, 68095];
        await using IPgConnection connection = DatabaseFixture.BasicPool.CreateConnection();
        await using IPgExecutableQuery query = connection.CreateQuery("SELECT $1 int_array_col;");
        query.Bind(input);
        var result = await query.ExecuteScalar<int?[]>(ct);
        await Assert.That(result).IsEquivalentTo(input);
    }

    [Test]
    public async Task ExecuteScalar_Should_Decode_When_ValueTypeArrayAndTextEncoding(CancellationToken ct)
    {
        int?[] input = [-493, 0, null, 34, 68095];
        await using IPgConnection
            connection = DatabaseFixture.SimpleQueryTextPool.CreateConnection();
        await using IPgExecutableQuery query =
            connection.CreateQuery("SELECT '{-493,0,NULL,34,68095}'::int[];");
        query.Bind(input);
        var result = await query.ExecuteScalar<int?[]>(ct);
        await Assert.That(result).IsEquivalentTo(input);
    }
}
