using Sqlx.Postgres.Query;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Connection;

public partial class PgConnectionTest
{
    [Test]
    public async Task ExecuteScalar_Should_EncodeAndDecode_When_ShortAndDefaultEncoding(CancellationToken ct)
    {
        const short value = 5234;
        using IPgConnection connection = DatabaseFixture.BasicPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery("SELECT $1 short_col;");
        query.Bind(value);
        var result = await query.ExecuteScalar<short, PgShort>(ct);
        await Assert.That(result).IsEqualTo(value);
    }

    [Test]
    public async Task ExecuteScalar_Should_Decode_When_ShortAndTextEncoding(CancellationToken ct)
    {
        const string sql = "SELECT CAST(5234 AS SMALLINT);";
        const short value = 5234;
        using IPgConnection
            connection = DatabaseFixture.SimpleQueryTextPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery(sql);
        var result = await query.ExecuteScalar<short, PgShort>(ct);
        await Assert.That(result).IsEqualTo(value);
    }

    [Test]
    public async Task ExecuteScalar_Should_EncodeAndDecode_When_IntAndDefaultEncoding(CancellationToken ct)
    {
        const int value = 523566486;
        using IPgConnection connection = DatabaseFixture.BasicPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery("SELECT $1 int_col;");
        query.Bind(value);
        var result = await query.ExecuteScalar<int, PgInt>(ct);
        await Assert.That(result).IsEqualTo(value);
    }

    [Test]
    public async Task ExecuteScalar_Should_Decode_When_IntAndTextEncoding(CancellationToken ct)
    {
        const string sql = "SELECT CAST(523566486 AS INTEGER);";
        const int value = 523566486;
        using IPgConnection
            connection = DatabaseFixture.SimpleQueryTextPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery(sql);
        var result = await query.ExecuteScalar<int, PgInt>(ct);
        await Assert.That(result).IsEqualTo(value);
    }

    [Test]
    public async Task ExecuteScalar_Should_EncodeAndDecode_When_OidAndDefaultEncoding(CancellationToken ct)
    {
        const uint value = 523566486u;
        using IPgConnection connection = DatabaseFixture.BasicPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery("SELECT $1 oid_col;");
        query.Bind(new PgOid(value));
        var result = await query.ExecuteScalar<PgOid>(ct);
        await Assert.That(result).Member(r => r.Inner, r => r.IsEqualTo(value));
    }

    [Test]
    public async Task ExecuteScalar_Should_Decode_When_OidAndTextEncoding(CancellationToken ct)
    {
        const string sql = "SELECT CAST(523566486 AS OID);";
        const uint value = 523566486u;
        using IPgConnection
            connection = DatabaseFixture.SimpleQueryTextPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery(sql);
        var result = await query.ExecuteScalar<PgOid>(ct);
        await Assert.That(result).Member(r => r.Inner, r => r.IsEqualTo(value));
    }

    [Test]
    public async Task ExecuteScalar_Should_EncodeAndDecode_When_LongAndDefaultEncoding(CancellationToken ct)
    {
        const long value = 2523557916465468;
        using IPgConnection connection = DatabaseFixture.BasicPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery("SELECT $1 long_col;");
        query.Bind(value);
        var result = await query.ExecuteScalar<long, PgLong>(ct);
        await Assert.That(result).IsEqualTo(value);
    }

    [Test]
    public async Task ExecuteScalar_Should_Decode_When_LongAndTextEncoding(CancellationToken ct)
    {
        const string sql = "SELECT CAST(2523557916465468 AS BIGINT);";
        const long value = 2523557916465468;
        using IPgConnection
            connection = DatabaseFixture.SimpleQueryTextPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery(sql);
        var result = await query.ExecuteScalar<long, PgLong>(ct);
        await Assert.That(result).IsEqualTo(value);
    }
}
