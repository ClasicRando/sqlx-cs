using Sqlx.Core.Buffer;
using Sqlx.Core.Result;
using Sqlx.Postgres.Query;
using Sqlx.Postgres.Result;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Connection;

public partial class PgConnectionTest
{
    [Fact]
    public async Task ExecuteScalar_Should_EncodeAndDecode_When_CompositeAndDefaultEncoding()
    {
        var value = new TestCompositeType
        {
            Id = 1,
            Name = "name",
            Title = null,
        };
        using IPgConnection connection = _databaseFixture.BasicPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery("SELECT $1 comp_col;");
        query.Bind(value);
        var result = await query.ExecuteScalarPg<TestCompositeType>();
        Assert.Equal(value, result);
    }

    [Fact]
    public async Task ExecuteScalar_Should_Decode_When_CompositeAndTextEncoding()
    {
        const string sql = "SELECT '(1,name,NULL)'::composite_type;";
        var value = new TestCompositeType
        {
            Id = 1,
            Name = "name",
            Title = null,
        };
        using IPgConnection
            connection = _databaseFixture.SimpleQueryTextPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery(sql);
        var result = await query.ExecuteScalarPg<TestCompositeType>();
        Assert.Equal(value, result);
    }

    private async Task CreateCompositeType()
    {
        const string createTypeQuery =
            """
            DROP TYPE IF EXISTS public.composite_type;
            CREATE TYPE public.composite_type AS
            (
                id int,
                name text,
                title text
            );
            """;
        using IPgConnection connection = _databaseFixture.BasicPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery(createTypeQuery);
        await query.ExecuteNonQueryAsync();
        await _databaseFixture.BasicPool.MapCompositeAsync<TestCompositeType>(TestContext.Current.CancellationToken);
    }
}

public readonly struct TestCompositeType : IPgUdt<TestCompositeType>, IFromRow<IPgDataRow, TestCompositeType>
{
    public int Id { get; init; }
    
    public string Name { get; init; }
    
    public string? Title { get; init; }

    public static PgTypeInfo DbType { get; set; } = PgTypeInfo.Unknown;
    
    public static string TypeName => "composite_type";
    
    public static void Encode(TestCompositeType value, WriteBuffer buffer)
    {
        using PgRecordEncoder recordEncoder = new(DbType);
        recordEncoder.Bind(value.Id);
        recordEncoder.Bind(value.Name);
        recordEncoder.Bind(value.Title);
        buffer.WriteBytes(recordEncoder.Data);
    }

    public static TestCompositeType DecodeBytes(ref PgBinaryValue value)
    {
        return PgRecordDecoder.DecodeBinary<TestCompositeType>(ref value);
    }

    public static TestCompositeType DecodeText(PgTextValue value)
    {
        return PgRecordDecoder.DecodeText<TestCompositeType>(in value);
    }

    public static bool IsCompatible(PgTypeInfo typeInfo)
    {
        return typeInfo == DbType;
    }

    public static TestCompositeType FromRow(IPgDataRow dataRow)
    {
        return new TestCompositeType
        {
            Id = dataRow.GetIntNotNull("id"),
            Name = dataRow.GetStringNotNull("name"),
            Title = dataRow.GetString("title"),
        };
    }
}
