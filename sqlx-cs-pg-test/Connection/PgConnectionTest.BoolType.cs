using Sqlx.Postgres.Query;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Connection;

public partial class PgConnectionTest
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ExecuteScalar_Should_EncodeAndDecode_When_BoolAndDefaultEncoding(bool value)
    {
        using IPgConnection connection = _databaseFixture.BasicPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery("SELECT $1 bool_col;");
        query.Bind(value);
        var result = await query.ExecuteScalar<PgBool, bool>();
        Assert.Equal(value, result);
    }

    [Theory]
    [InlineData(true, "SELECT true;")]
    [InlineData(false, "SELECT false;")]
    public async Task ExecuteScalar_Should_Decode_When_BoolAndTextEncoding(
        bool value,
        string sql)
    {
        using IPgConnection
            connection = _databaseFixture.SimpleQueryTextPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery(sql);
        var result = await query.ExecuteScalar<PgBool, bool>();
        Assert.Equal(value, result);
    }
}
