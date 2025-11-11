using Sqlx.Core.Connection;
using Sqlx.Core.Query;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Connection;

public partial class PgConnectionTest
{
    [Fact]
    public async Task ExecuteScalar_Should_EncodeAndDecode_When_DateAndDefaultEncoding()
    {
        var value = new DateOnly(2025, 1, 1);
        await using IConnection connection = _databaseFixture.BasicPool.CreateConnection();
        using IExecutableQuery query = connection.CreateQuery("SELECT $1;");
        query.Bind(value);
        DateOnly result = await query.ExecuteScalar<PgDate, DateOnly>();
        Assert.Equal(value, result);
    }

    [Fact]
    public async Task ExecuteScalar_Should_Decode_When_DateAndTextEncoding()
    {
        const string sql = "SELECT '2025-01-01'::date;";
        var value = new DateOnly(2025, 1, 1);
        await using IConnection
            connection = _databaseFixture.SimpleQueryTextPool.CreateConnection();
        using IExecutableQuery query = connection.CreateQuery(sql);
        DateOnly result = await query.ExecuteScalar<PgDate, DateOnly>();
        Assert.Equal(value, result);
    }
}
