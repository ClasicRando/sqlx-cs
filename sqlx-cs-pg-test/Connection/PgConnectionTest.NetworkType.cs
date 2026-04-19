using System.Net;
using Sqlx.Postgres.Query;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Connection;

public partial class PgConnectionTest
{
    [Test]
    public async Task ExecuteScalar_Should_EncodeAndDecode_When_InetPgInetAndDefaultEncoding(CancellationToken ct)
    {
        var value = new PgInet(new IPAddress([192, 168, 0, 1]), 24);
        await using IPgConnection connection = DatabaseFixture.BasicPool.CreateConnection();
        await using IPgExecutableQuery query = connection.CreateQuery("SELECT $1 inet_col;");
        query.Bind(value);
        var result = await query.ExecuteScalar<PgInet>(ct);
        await Assert.That(result).IsEqualTo(value);
    }

    [Test]
    public async Task ExecuteScalar_Should_Decode_When_InetPgInetAndTextEncoding(CancellationToken ct)
    {
        const string sql = "SELECT '192.168.0.1/24'::inet;";
        var value = new PgInet(new IPAddress([192, 168, 0, 1]), 24);
        await using IPgConnection
            connection = DatabaseFixture.SimpleQueryTextPool.CreateConnection();
        await using IPgExecutableQuery query = connection.CreateQuery(sql);
        var result = await query.ExecuteScalar<PgInet>(ct);
        await Assert.That(result).IsEqualTo(value);
    }

    [Test]
    public async Task ExecuteScalar_Should_Decode_When_CidrPgInetAndTextEncoding(CancellationToken ct)
    {
        const string sql = "SELECT '192.168.0.0/24'::cidr;";
        var value = new PgInet(new IPAddress([192, 168, 0, 0]), 24);
        await using IPgConnection
            connection = DatabaseFixture.SimpleQueryTextPool.CreateConnection();
        await using IPgExecutableQuery query = connection.CreateQuery(sql);
        var result = await query.ExecuteScalar<PgInet>(ct);
        await Assert.That(result).IsEqualTo(value);
    }
    
    [Test]
    public async Task ExecuteScalar_Should_EncodeAndDecode_When_InetIpNetworkAndDefaultEncoding(CancellationToken ct)
    {
        var value = new IPNetwork(new IPAddress([192, 168, 0, 0]), 24);
        await using IPgConnection connection = DatabaseFixture.BasicPool.CreateConnection();
        await using IPgExecutableQuery query = connection.CreateQuery("SELECT $1 ipnetwork_col;");
        query.Bind(value);
        var result = await query.ExecuteScalar<IPNetwork>(ct);
        await Assert.That(result).IsEqualTo(value);
    }

    [Test]
    public async Task ExecuteScalar_Should_Decode_When_InetIpNetworkAndTextEncoding(CancellationToken ct)
    {
        const string sql = "SELECT '192.168.0.0/24'::inet;";
        var value = new IPNetwork(new IPAddress([192, 168, 0, 0]), 24);
        await using IPgConnection
            connection = DatabaseFixture.SimpleQueryTextPool.CreateConnection();
        await using IPgExecutableQuery query = connection.CreateQuery(sql);
        var result = await query.ExecuteScalar<IPNetwork>(ct);
        await Assert.That(result).IsEqualTo(value);
    }

    [Test]
    public async Task ExecuteScalar_Should_Decode_When_CidrIpNetworkAndTextEncoding(CancellationToken ct)
    {
        const string sql = "SELECT '192.168.0.0/24'::cidr;";
        var value = new IPNetwork(new IPAddress([192, 168, 0, 0]), 24);
        await using IPgConnection
            connection = DatabaseFixture.SimpleQueryTextPool.CreateConnection();
        await using IPgExecutableQuery query = connection.CreateQuery(sql);
        var result = await query.ExecuteScalar<IPNetwork>(ct);
        await Assert.That(result).IsEqualTo(value);
    }
}
