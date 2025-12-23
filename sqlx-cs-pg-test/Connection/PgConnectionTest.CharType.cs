using Sqlx.Postgres.Query;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Connection;

public partial class PgConnectionTest
{
    [Fact]
    public async Task ExecuteScalar_Should_EncodeAndDecode_When_CharAndDefaultEncoding()
    {
        const sbyte value = (sbyte)'e';
        using IPgConnection connection = _databaseFixture.BasicPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery("SELECT $1 char_col;");
        query.Bind(value);
        var result = await query.ExecuteScalar<PgChar, sbyte>();
        Assert.Equal(value, result);
    }

    [Fact]
    public async Task ExecuteScalar_Should_Decode_When_CharAndTextEncoding()
    {
        const string sql = "SELECT 'e'::\"char\";";
        const sbyte value = (sbyte)'e';
        using IPgConnection
            connection = _databaseFixture.SimpleQueryTextPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery(sql);
        var result = await query.ExecuteScalar<PgChar, sbyte>();
        Assert.Equal(value, result);
    }
}
