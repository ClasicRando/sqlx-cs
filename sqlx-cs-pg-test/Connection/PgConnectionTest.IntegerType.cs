using Sqlx.Postgres.Query;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Connection;

public partial class PgConnectionTest
{
    [Fact]
    public async Task ExecuteScalar_Should_EncodeAndDecode_When_ShortAndDefaultEncoding()
    {
        const short value = 5234;
        await using IPgConnection connection = _databaseFixture.BasicPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery("SELECT $1;");
        query.Bind(value);
        var result = await query.ExecuteScalar<PgShort, short>();
        Assert.Equal(value, result);
    }

    [Fact]
    public async Task ExecuteScalar_Should_Decode_When_ShortAndTextEncoding()
    {
        const string sql = "SELECT CAST(5234 AS SMALLINT);";
        const short value = 5234;
        await using IPgConnection
            connection = _databaseFixture.SimpleQueryTextPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery(sql);
        var result = await query.ExecuteScalar<PgShort, short>();
        Assert.Equal(value, result);
    }

    [Fact]
    public async Task ExecuteScalar_Should_EncodeAndDecode_When_IntAndDefaultEncoding()
    {
        const int value = 523566486;
        await using IPgConnection connection = _databaseFixture.BasicPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery("SELECT $1;");
        query.Bind(value);
        var result = await query.ExecuteScalar<PgInt, int>();
        Assert.Equal(value, result);
    }

    [Fact]
    public async Task ExecuteScalar_Should_Decode_When_IntAndTextEncoding()
    {
        const string sql = "SELECT CAST(523566486 AS INTEGER);";
        const int value = 523566486;
        await using IPgConnection
            connection = _databaseFixture.SimpleQueryTextPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery(sql);
        var result = await query.ExecuteScalar<PgInt, int>();
        Assert.Equal(value, result);
    }

    [Fact]
    public async Task ExecuteScalar_Should_EncodeAndDecode_When_OidAndDefaultEncoding()
    {
        const uint value = 523566486u;
        await using IPgConnection connection = _databaseFixture.BasicPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery("SELECT $1;");
        query.Bind(new PgOid(value));
        PgOid result = await query.ExecuteScalar<PgOid, PgOid>();
        Assert.Equal(value, result.Inner);
    }

    [Fact]
    public async Task ExecuteScalar_Should_Decode_When_OidAndTextEncoding()
    {
        const string sql = "SELECT CAST(523566486 AS OID);";
        const uint value = 523566486u;
        await using IPgConnection
            connection = _databaseFixture.SimpleQueryTextPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery(sql);
        PgOid result = await query.ExecuteScalar<PgOid, PgOid>();
        Assert.Equal(value, result.Inner);
    }

    [Fact]
    public async Task ExecuteScalar_Should_EncodeAndDecode_When_LongAndDefaultEncoding()
    {
        const long value = 2523557916465468;
        await using IPgConnection connection = _databaseFixture.BasicPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery("SELECT $1;");
        query.Bind(value);
        var result = await query.ExecuteScalar<PgLong, long>();
        Assert.Equal(value, result);
    }

    [Fact]
    public async Task ExecuteScalar_Should_Decode_When_LongAndTextEncoding()
    {
        const string sql = "SELECT CAST(2523557916465468 AS BIGINT);";
        const long value = 2523557916465468;
        await using IPgConnection
            connection = _databaseFixture.SimpleQueryTextPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery(sql);
        var result = await query.ExecuteScalar<PgLong, long>();
        Assert.Equal(value, result);
    }
}
