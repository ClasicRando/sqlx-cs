using System.Collections;
using Sqlx.Core.Connection;
using Sqlx.Core.Query;
using Sqlx.Postgres.Query;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Connection;

public partial class PgConnectionTest
{
    [Theory]
    [InlineData(new[] { true, false, true, false })]
    [InlineData(new[] { true, false, true, false, true, false, true, false })]
    public async Task ExecuteScalar_Should_EncodeAndDecode_When_VarBitAndDefaultEncoding(bool[] bits)
    {
        var value = new BitArray(bits);
        await using IConnection connection = _databaseFixture.BasicPool.CreateConnection();
        using IExecutableQuery query = connection.CreateQuery("SELECT $1;");
        query.Bind(value);
        BitArray result = await query.ExecuteScalar<PgBitString, BitArray>();
        Assert.Equal(value, result);
    }

    [Theory]
    [InlineData(new[] { true, false, true, false }, "SELECT '1010'::varbit;")]
    [InlineData(new[] { true, false, true, false, true, false, true, false }, "SELECT '10101010'::varbit;")]
    public async Task ExecuteScalar_Should_Decode_When_VarBitAndTextEncoding(
        bool[] bits,
        string sql)
    {
        var value = new BitArray(bits);
        await using IConnection
            connection = _databaseFixture.SimpleQueryTextPool.CreateConnection();
        using IExecutableQuery query = connection.CreateQuery(sql);
        BitArray result = await query.ExecuteScalar<PgBitString, BitArray>();
        Assert.Equal(value, result);
    }
}
