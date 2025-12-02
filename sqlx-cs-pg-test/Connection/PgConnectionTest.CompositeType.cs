using Sqlx.Core.Buffer;
using Sqlx.Core.Connection;
using Sqlx.Core.Query;
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
        await _databaseFixture.BasicPool.MapComposite<TestCompositeType>(TestContext.Current.CancellationToken);
        await using IConnection connection = _databaseFixture.BasicPool.CreateConnection();
        using IExecutableQuery query = connection.CreateQuery("SELECT $1;");
        query.BindPg(value);
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
        await using IConnection
            connection = _databaseFixture.SimpleQueryTextPool.CreateConnection();
        using IExecutableQuery query = connection.CreateQuery(sql);
        var result = await query.ExecuteScalarPg<TestCompositeType>();
        Assert.Equal(value, result);
    }
}

public readonly struct TestCompositeType : IPgUdt<TestCompositeType>, IFromRow<TestCompositeType>
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

    public static TestCompositeType FromRow(IDataRow dataRow)
    {
        return new TestCompositeType
        {
            Id = dataRow.GetIntNotNull("id"),
            Name = dataRow.GetStringNotNull("name"),
            Title = dataRow.GetString("title"),
        };
    }
}
