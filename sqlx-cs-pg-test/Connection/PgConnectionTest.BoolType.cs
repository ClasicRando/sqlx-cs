using Sqlx.Core.Connection;
using Sqlx.Core.Query;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Connection;

public partial class PgConnectionTest
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ExecuteScalar_Should_EncodeAndDecode_When_BoolAndDefaultEncoding(bool value)
    {
        await using IConnection connection = _databaseFixture.BasicPool.CreateConnection();
        using IExecutableQuery query = connection.CreateQuery("SELECT $1;");
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
        await using IConnection
            connection = _databaseFixture.SimpleQueryTextPool.CreateConnection();
        using IExecutableQuery query = connection.CreateQuery(sql);
        var result = await query.ExecuteScalar<PgBool, bool>();
        Assert.Equal(value, result);
    }
}
