using System.Net;
using Sqlx.Core.Connection;
using Sqlx.Core.Query;
using Sqlx.Postgres.Query;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Connection;

public partial class PgConnectionTest
{
    [Fact]
    public async Task ExecuteScalar_Should_EncodeAndDecode_When_InetPgInetAndDefaultEncoding()
    {
        var value = new PgInet(new IPAddress([192, 168, 0, 1]), 24);
        await using IConnection connection = _databaseFixture.BasicPool.CreateConnection();
        using IExecutableQuery query = connection.CreateQuery("SELECT $1;");
        query.Bind(value);
        var result = await query.ExecuteScalarPg<PgInet>();
        Assert.Equal(value, result);
    }

    [Fact]
    public async Task ExecuteScalar_Should_Decode_When_InetPgInetAndTextEncoding()
    {
        const string sql = "SELECT '192.168.0.1/24'::inet;";
        var value = new PgInet(new IPAddress([192, 168, 0, 1]), 24);
        await using IConnection
            connection = _databaseFixture.SimpleQueryTextPool.CreateConnection();
        using IExecutableQuery query = connection.CreateQuery(sql);
        var result = await query.ExecuteScalarPg<PgInet>();
        Assert.Equal(value, result);
    }

    [Fact]
    public async Task ExecuteScalar_Should_Decode_When_CidrPgInetAndTextEncoding()
    {
        const string sql = "SELECT '192.168.0.0/24'::cidr;";
        var value = new PgInet(new IPAddress([192, 168, 0, 0]), 24);
        await using IConnection
            connection = _databaseFixture.SimpleQueryTextPool.CreateConnection();
        using IExecutableQuery query = connection.CreateQuery(sql);
        var result = await query.ExecuteScalarPg<PgInet>();
        Assert.Equal(value, result);
    }
    
    [Fact]
    public async Task ExecuteScalar_Should_EncodeAndDecode_When_InetIpNetworkAndDefaultEncoding()
    {
        var value = new IPNetwork(new IPAddress([192, 168, 0, 0]), 24);
        await using IConnection connection = _databaseFixture.BasicPool.CreateConnection();
        using IExecutableQuery query = connection.CreateQuery("SELECT $1;");
        query.Bind(value);
        IPNetwork result = await query.ExecuteScalar<PgIpNetwork, IPNetwork>();
        Assert.Equal(value, result);
    }

    [Fact]
    public async Task ExecuteScalar_Should_Decode_When_InetIpNetworkAndTextEncoding()
    {
        const string sql = "SELECT '192.168.0.0/24'::inet;";
        var value = new IPNetwork(new IPAddress([192, 168, 0, 0]), 24);
        await using IConnection
            connection = _databaseFixture.SimpleQueryTextPool.CreateConnection();
        using IExecutableQuery query = connection.CreateQuery(sql);
        IPNetwork result = await query.ExecuteScalar<PgIpNetwork, IPNetwork>();
        Assert.Equal(value, result);
    }

    [Fact]
    public async Task ExecuteScalar_Should_Decode_When_CidrIpNetworkAndTextEncoding()
    {
        const string sql = "SELECT '192.168.0.0/24'::cidr;";
        var value = new IPNetwork(new IPAddress([192, 168, 0, 0]), 24);
        await using IConnection
            connection = _databaseFixture.SimpleQueryTextPool.CreateConnection();
        using IExecutableQuery query = connection.CreateQuery(sql);
        IPNetwork result = await query.ExecuteScalar<PgIpNetwork, IPNetwork>();
        Assert.Equal(value, result);
    }
}
