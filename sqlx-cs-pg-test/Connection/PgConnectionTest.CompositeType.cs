using Sqlx.Postgres.Generator;
using Sqlx.Postgres.Generator.Type;
using Sqlx.Postgres.Query;

namespace Sqlx.Postgres.Connection;

public partial class PgConnectionTest
{
    [Test]
    public async Task ExecuteScalar_Should_EncodeAndDecode_When_CompositeAndDefaultEncoding(CancellationToken ct)
    {
        var value = new TestCompositeType
        {
            Id = 1,
            Name = "name",
            Title = null,
        };
        using IPgConnection connection = DatabaseFixture.BasicPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery("SELECT $1 comp_col;");
        query.BindPg(value);
        var result = await query.ExecuteScalar<TestCompositeType>(ct);
        await Assert.That(result).IsEqualTo(value);
    }

    [Test]
    public async Task ExecuteScalar_Should_Decode_When_CompositeAndTextEncoding(CancellationToken ct)
    {
        const string sql = "SELECT '(1,name,NULL)'::composite_type;";
        var value = new TestCompositeType
        {
            Id = 1,
            Name = "name",
            Title = null,
        };
        using IPgConnection
            connection = DatabaseFixture.SimpleQueryTextPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery(sql);
        var result = await query.ExecuteScalar<TestCompositeType>(ct);
        await Assert.That(result).IsEqualTo(value);
    }
}

[PgComposite(Name = "composite_type", RenameAll = Rename.SnakeCase)]
public readonly partial struct TestCompositeType
{
    public int Id { get; init; }
    
    public string Name { get; init; }
    
    public string? Title { get; init; }
}
