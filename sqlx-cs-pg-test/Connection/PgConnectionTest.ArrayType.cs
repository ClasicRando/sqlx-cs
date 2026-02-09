using Sqlx.Postgres.Query;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Connection;

public partial class PgConnectionTest
{
    [Test]
    public async Task ExecuteScalar_Should_EncodeAndDecode_When_RefTypeArrayAndDefaultEncoding(CancellationToken ct)
    {
        string?[] input = ["this", "is", null, "a", "test"];
        using IPgConnection connection = DatabaseFixture.BasicPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery("SELECT $1 text_array_col;");
        query.Bind(input);
        var result = await query.ExecuteScalar<string?[], PgArrayTypeClass<string, PgString>>(ct);
        await Assert.That(result).IsEquivalentTo(input);
    }

    [Test]
    public async Task ExecuteScalar_Should_Decode_When_RefTypeArrayAndTextEncoding(CancellationToken ct)
    {
        string?[] input = ["this", "is", null, "a", "test"];
        using IPgConnection
            connection = DatabaseFixture.SimpleQueryTextPool.CreateConnection();
        using IPgExecutableQuery query =
            connection.CreateQuery("SELECT '{this,is,NULL,a,test}'::text[];");
        query.Bind(input);
        var result = await query.ExecuteScalar<string?[], PgArrayTypeClass<string, PgString>>(ct);
        await Assert.That(result).IsEquivalentTo(input);
    }

    [Test]
    public async Task ExecuteScalar_Should_EncodeAndDecode_When_ValueTypeArrayAndDefaultEncoding(CancellationToken ct)
    {
        int?[] input = [-493, 0, null, 34, 68095];
        using IPgConnection connection = DatabaseFixture.BasicPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery("SELECT $1 int_array_col;");
        query.Bind(input);
        var result = await query.ExecuteScalar<int?[], PgArrayTypeStruct<int, PgInt>>(ct);
        await Assert.That(result).IsEquivalentTo(input);
    }

    [Test]
    public async Task ExecuteScalar_Should_Decode_When_ValueTypeArrayAndTextEncoding(CancellationToken ct)
    {
        int?[] input = [-493, 0, null, 34, 68095];
        using IPgConnection
            connection = DatabaseFixture.SimpleQueryTextPool.CreateConnection();
        using IPgExecutableQuery query =
            connection.CreateQuery("SELECT '{-493,0,NULL,34,68095}'::int[];");
        query.Bind(input);
        var result = await query.ExecuteScalar<int?[], PgArrayTypeStruct<int, PgInt>>(ct);
        await Assert.That(result).IsEquivalentTo(input);
    }
}
