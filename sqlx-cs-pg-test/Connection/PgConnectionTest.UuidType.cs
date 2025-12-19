using Sqlx.Postgres.Query;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Connection;

public partial class PgConnectionTest
{
    [Fact]
    public async Task ExecuteScalar_Should_EncodeAndDecode_When_UuidAndDefaultEncoding()
    {
        Guid value = Guid.Parse("019a22a1-8d4c-7e71-8ac5-e31d330b866c");
        await using IPgConnection connection = _databaseFixture.BasicPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery("SELECT $1;");
        query.Bind(value);
        Guid result = await query.ExecuteScalar<PgUuid, Guid>();
        Assert.Equal(value, result);
    }

    [Fact]
    public async Task ExecuteScalar_Should_Decode_When_UuidAndTextEncoding()
    {
        const string sql = "SELECT '019a22a1-8d4c-7e71-8ac5-e31d330b866c'::uuid;";
        Guid value = Guid.Parse("019a22a1-8d4c-7e71-8ac5-e31d330b866c");
        await using IPgConnection
            connection = _databaseFixture.SimpleQueryTextPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery(sql);
        Guid result = await query.ExecuteScalar<PgUuid, Guid>();
        Assert.Equal(value, result);
    }
}
