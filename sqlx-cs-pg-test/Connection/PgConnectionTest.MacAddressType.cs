using Sqlx.Core.Connection;
using Sqlx.Core.Query;
using Sqlx.Postgres.Query;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Connection;

public partial class PgConnectionTest
{
    [Fact]
    public async Task ExecuteScalar_Should_EncodeAndDecode_When_MacAddressAndDefaultEncoding()
    {
        PgMacAddress value = PgMacAddress.FromBytes([0x08, 0x00, 0x2b, 0x01, 0x02, 0x03]);
        await using IConnection connection = _databaseFixture.BasicPool.CreateConnection();
        using IExecutableQuery query = connection.CreateQuery("SELECT $1;");
        query.Bind(value);
        var result = await query.ExecuteScalarPg<PgMacAddress>();
        Assert.Equal(value, result);
    }

    [Fact]
    public async Task ExecuteScalar_Should_Decode_When_MacAddressAndTextEncoding()
    {
        const string sql = "SELECT '08:00:2b:01:02:03'::macaddr;";
        PgMacAddress value = PgMacAddress.FromBytes([0x08, 0x00, 0x2b, 0x01, 0x02, 0x03]);
        await using IConnection
            connection = _databaseFixture.SimpleQueryTextPool.CreateConnection();
        using IExecutableQuery query = connection.CreateQuery(sql);
        var result = await query.ExecuteScalarPg<PgMacAddress>();
        Assert.Equal(value, result);
    }
    
    [Fact]
    public async Task ExecuteScalar_Should_EncodeAndDecode_When_MacAddress8AndDefaultEncoding()
    {
        PgMacAddress8 value = PgMacAddress8.FromBytes([0x08, 0x00, 0x2b, 0x01, 0x02, 0x03, 0x04, 0x05]);
        await using IConnection connection = _databaseFixture.BasicPool.CreateConnection();
        using IExecutableQuery query = connection.CreateQuery("SELECT $1;");
        query.Bind(value);
        var result = await query.ExecuteScalarPg<PgMacAddress8>();
        Assert.Equal(value, result);
    }

    [Fact]
    public async Task ExecuteScalar_Should_Decode_When_MacAddress8AndTextEncoding()
    {
        const string sql = "SELECT '08:00:2b:01:02:03:04:05'::macaddr8;";
        PgMacAddress8 value = PgMacAddress8.FromBytes([0x08, 0x00, 0x2b, 0x01, 0x02, 0x03, 0x04, 0x05]);
        await using IConnection
            connection = _databaseFixture.SimpleQueryTextPool.CreateConnection();
        using IExecutableQuery query = connection.CreateQuery(sql);
        var result = await query.ExecuteScalarPg<PgMacAddress8>();
        Assert.Equal(value, result);
    }
}
