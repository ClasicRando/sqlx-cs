using Sqlx.Postgres.Query;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Connection;

public partial class PgConnectionTest
{
    [Fact]
    public async Task ExecuteScalar_Should_EncodeAndDecode_When_FloatAndDefaultEncoding()
    {
        const float value = 12345.67890F;
        await using IPgConnection connection = _databaseFixture.BasicPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery("SELECT $1;");
        query.Bind(value);
        var result = await query.ExecuteScalar<PgFloat, float>();
        Assert.Equal(value, result);
    }

    [Fact]
    public async Task ExecuteScalar_Should_Decode_When_FloatAndTextEncoding()
    {
        const string sql = "SELECT CAST(12345.67890 AS REAL);";
        const float value = 12345.67890F;
        await using IPgConnection
            connection = _databaseFixture.SimpleQueryTextPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery(sql);
        var result = await query.ExecuteScalar<PgFloat, float>();
        Assert.Equal(value, result);
    }

    [Fact]
    public async Task ExecuteScalar_Should_EncodeAndDecode_When_DoubleAndDefaultEncoding()
    {
        const double value = 12345.67890;
        await using IPgConnection connection = _databaseFixture.BasicPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery("SELECT $1;");
        query.Bind(value);
        var result = await query.ExecuteScalar<PgDouble, double>();
        Assert.Equal(value, result);
    }

    [Fact]
    public async Task ExecuteScalar_Should_Decode_When_DoubleAndTextEncoding()
    {
        const string sql = "SELECT CAST(12345.67890 AS DOUBLE PRECISION);";
        const double value = 12345.67890;
        await using IPgConnection
            connection = _databaseFixture.SimpleQueryTextPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery(sql);
        var result = await query.ExecuteScalar<PgDouble, double>();
        Assert.Equal(value, result);
    }
}
