using Sqlx.Postgres.Query;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Connection;

public partial class PgConnectionTest
{
    [Test]
    public async Task ExecuteScalar_Should_EncodeAndDecode_When_FloatAndDefaultEncoding(CancellationToken ct)
    {
        const float value = 12345.67890F;
        using IPgConnection connection = DatabaseFixture.BasicPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery("SELECT $1 float_col;");
        query.Bind(value);
        var result = await query.ExecuteScalar<float, PgFloat>(ct);
        await Assert.That(result).IsEqualTo(value);
    }

    [Test]
    public async Task ExecuteScalar_Should_Decode_When_FloatAndTextEncoding(CancellationToken ct)
    {
        const string sql = "SELECT CAST(12345.67890 AS REAL);";
        const float value = 12345.67890F;
        using IPgConnection
            connection = DatabaseFixture.SimpleQueryTextPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery(sql);
        var result = await query.ExecuteScalar<float, PgFloat>(ct);
        await Assert.That(result).IsEqualTo(value);
    }

    [Test]
    public async Task ExecuteScalar_Should_EncodeAndDecode_When_DoubleAndDefaultEncoding(CancellationToken ct)
    {
        const double value = 12345.67890;
        using IPgConnection connection = DatabaseFixture.BasicPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery("SELECT $1 double_col;");
        query.Bind(value);
        var result = await query.ExecuteScalar<double, PgDouble>(ct);
        await Assert.That(result).IsEqualTo(value);
    }

    [Test]
    public async Task ExecuteScalar_Should_Decode_When_DoubleAndTextEncoding(CancellationToken ct)
    {
        const string sql = "SELECT CAST(12345.67890 AS DOUBLE PRECISION);";
        const double value = 12345.67890;
        using IPgConnection
            connection = DatabaseFixture.SimpleQueryTextPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery(sql);
        var result = await query.ExecuteScalar<double, PgDouble>(ct);
        await Assert.That(result).IsEqualTo(value);
    }
}
