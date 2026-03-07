using Sqlx.Postgres.Query;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Connection;

public partial class PgConnectionTest
{
    [Test]
    public async Task ExecuteScalar_Should_EncodeAndDecode_When_MacAddressAndDefaultEncoding(CancellationToken ct)
    {
        PgMacAddress value = PgMacAddress.FromBytes([0x08, 0x00, 0x2b, 0x01, 0x02, 0x03]);
        using IPgConnection connection = DatabaseFixture.BasicPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery("SELECT $1 macaddr_col;");
        query.Bind(value);
        var result = await query.ExecuteScalar<PgMacAddress>(ct);
        await Assert.That(result).IsEqualTo(value);
    }

    [Test]
    public async Task ExecuteScalar_Should_Decode_When_MacAddressAndTextEncoding(CancellationToken ct)
    {
        const string sql = "SELECT '08:00:2b:01:02:03'::macaddr;";
        PgMacAddress value = PgMacAddress.FromBytes([0x08, 0x00, 0x2b, 0x01, 0x02, 0x03]);
        using IPgConnection
            connection = DatabaseFixture.SimpleQueryTextPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery(sql);
        var result = await query.ExecuteScalar<PgMacAddress>(ct);
        await Assert.That(result).IsEqualTo(value);
    }
    
    [Test]
    public async Task ExecuteScalar_Should_EncodeAndDecode_When_MacAddress8AndDefaultEncoding(CancellationToken ct)
    {
        PgMacAddress8 value = PgMacAddress8.FromBytes([0x08, 0x00, 0x2b, 0x01, 0x02, 0x03, 0x04, 0x05]);
        using IPgConnection connection = DatabaseFixture.BasicPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery("SELECT $1 macaddr8_col;");
        query.Bind(value);
        var result = await query.ExecuteScalar<PgMacAddress8>(ct);
        await Assert.That(result).IsEqualTo(value);
    }

    [Test]
    public async Task ExecuteScalar_Should_Decode_When_MacAddress8AndTextEncoding(CancellationToken ct)
    {
        const string sql = "SELECT '08:00:2b:01:02:03:04:05'::macaddr8;";
        PgMacAddress8 value = PgMacAddress8.FromBytes([0x08, 0x00, 0x2b, 0x01, 0x02, 0x03, 0x04, 0x05]);
        using IPgConnection
            connection = DatabaseFixture.SimpleQueryTextPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery(sql);
        var result = await query.ExecuteScalar<PgMacAddress8>(ct);
        await Assert.That(result).IsEqualTo(value);
    }
}
