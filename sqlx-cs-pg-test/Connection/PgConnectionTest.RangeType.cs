using Sqlx.Core.Connection;
using Sqlx.Core.Query;
using Sqlx.Postgres.Query;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Connection;

public partial class PgConnectionTest
{
    [Fact]
    public async Task ExecuteScalar_Should_EncodeAndDecode_When_IntRangeAndDefaultEncoding()
    {
        var value = new PgRange<int>(Bound<int>.Included(-1), Bound<int>.Excluded(11));
        await using IConnection connection = _databaseFixture.BasicPool.CreateConnection();
        using IExecutableQuery query = connection.CreateQuery("SELECT $1;");
        query.Bind(value);
        var result = await query.ExecuteScalar<PgRangeType<int, PgInt>, PgRange<int>>();
        Assert.Equal(value, result);
    }

    [Fact]
    public async Task ExecuteScalar_Should_Decode_When_IntRangeAndTextEncoding()
    {
        const string sql = "SELECT '[-1,11)'::int4range;";
        var value = new PgRange<int>(Bound<int>.Included(-1), Bound<int>.Excluded(11));
        await using IConnection
            connection = _databaseFixture.SimpleQueryTextPool.CreateConnection();
        using IExecutableQuery query = connection.CreateQuery(sql);
        var result = await query.ExecuteScalar<PgRangeType<int, PgInt>, PgRange<int>>();
        Assert.Equal(value, result);
    }
}
