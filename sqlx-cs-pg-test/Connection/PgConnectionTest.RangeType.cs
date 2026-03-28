using Sqlx.Postgres.Query;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Connection;

public partial class PgConnectionTest
{
    [Test]
    public async Task ExecuteScalar_Should_EncodeAndDecode_When_IntRangeAndDefaultEncoding(CancellationToken ct)
    {
        var value = new PgRange<int>(Bound.Included(-1), Bound.Excluded(11));
        using IPgConnection connection = DatabaseFixture.BasicPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery("SELECT $1 range_col;");
        query.BindPg<PgRange<int>, PgRangeType<int, PgInt>>(value);
        var result = await query.ExecuteScalar<PgRange<int>, PgRangeType<int, PgInt>>(ct);
        await Assert.That(result).IsEqualTo(value);
    }

    [Test]
    public async Task ExecuteScalar_Should_Decode_When_IntRangeAndTextEncoding(CancellationToken ct)
    {
        const string sql = "SELECT '[-1,11)'::int4range;";
        var value = new PgRange<int>(Bound.Included(-1), Bound.Excluded(11));
        using IPgConnection
            connection = DatabaseFixture.SimpleQueryTextPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery(sql);
        var result = await query.ExecuteScalar<PgRange<int>, PgRangeType<int, PgInt>>(ct);
        await Assert.That(result).IsEqualTo(value);
    }
}
