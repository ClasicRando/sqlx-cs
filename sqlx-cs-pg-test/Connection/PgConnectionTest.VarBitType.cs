using System.Collections;
using Sqlx.Postgres.Query;

namespace Sqlx.Postgres.Connection;

public partial class PgConnectionTest
{
    [Test]
    [Arguments(new[] { true, false, true, false })]
    [Arguments(new[] { true, false, true, false, true, false, true, false })]
    public async Task ExecuteScalar_Should_EncodeAndDecode_When_VarBitAndDefaultEncoding(
        bool[] bits,
        CancellationToken ct)
    {
        var value = new BitArray(bits);
        await using IPgConnection connection = DatabaseFixture.BasicPool.CreateConnection();
        await using IPgExecutableQuery query = connection.CreateQuery("SELECT $1 varbit_col;");
        query.Bind(value);
        var result = await query.ExecuteScalar<BitArray>(ct);
        await Assert.That(result).IsEquivalentTo(value);
    }

    [Test]
    [Arguments(new[] { true, false, true, false }, "SELECT '1010'::varbit;")]
    [Arguments(new[] { true, false, true, false, true, false, true, false }, "SELECT '10101010'::varbit;")]
    public async Task ExecuteScalar_Should_Decode_When_VarBitAndTextEncoding(
        bool[] bits,
        string sql,
        CancellationToken ct)
    {
        var value = new BitArray(bits);
        await using IPgConnection
            connection = DatabaseFixture.SimpleQueryTextPool.CreateConnection();
        await using IPgExecutableQuery query = connection.CreateQuery(sql);
        var result = await query.ExecuteScalar<BitArray>(ct);
        await Assert.That(result).IsEquivalentTo(value);
    }
}
