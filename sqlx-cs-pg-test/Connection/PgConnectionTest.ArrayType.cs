using Sqlx.Core.Connection;
using Sqlx.Core.Query;
using Sqlx.Postgres.Query;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Connection;

public partial class PgConnectionTest
{
    [Fact]
    public async Task ExecuteScalar_Should_EncodeAndDecode_When_RefTypeArrayAndDefaultEncoding()
    {
        string?[] input = ["this", "is", null, "a", "test"];
        await using IConnection connection = _databaseFixture.BasicPool.CreateConnection();
        using IExecutableQuery query = connection.CreateQuery("SELECT $1;");
        query.Bind(input);
        var result = await query.ExecuteScalar<PgArrayTypeClass<string, PgString>, string?[]>();
        Assert.Equal(input, result);
    }

    [Fact]
    public async Task ExecuteScalar_Should_Decode_When_RefTypeArrayAndTextEncoding()
    {
        string?[] input = ["this", "is", null, "a", "test"];
        await using IConnection
            connection = _databaseFixture.SimpleQueryTextPool.CreateConnection();
        using IExecutableQuery query =
            connection.CreateQuery("SELECT '{this,is,NULL,a,test}'::text[];");
        query.Bind(input);
        var result = await query.ExecuteScalar<PgArrayTypeClass<string, PgString>, string?[]>();
        Assert.Equal(input, result);
    }

    [Fact]
    public async Task ExecuteScalar_Should_EncodeAndDecode_When_ValueTypeArrayAndDefaultEncoding()
    {
        int?[] input = [-493, 0, null, 34, 68095];
        await using IConnection connection = _databaseFixture.BasicPool.CreateConnection();
        using IExecutableQuery query = connection.CreateQuery("SELECT $1;");
        query.Bind(input);
        var result = await query.ExecuteScalar<PgArrayTypeStruct<int, PgInt>, int?[]>();
        Assert.Equal(input, result);
    }

    [Fact]
    public async Task ExecuteScalar_Should_Decode_When_ValueTypeArrayAndTextEncoding()
    {
        int?[] input = [-493, 0, null, 34, 68095];
        await using IConnection
            connection = _databaseFixture.SimpleQueryTextPool.CreateConnection();
        using IExecutableQuery query =
            connection.CreateQuery("SELECT '{-493,0,NULL,34,68095}'::int[];");
        query.Bind(input);
        var result = await query.ExecuteScalar<PgArrayTypeStruct<int, PgInt>, int?[]>();
        Assert.Equal(input, result);
    }
}
