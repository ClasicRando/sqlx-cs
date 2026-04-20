using Sqlx.Postgres.Generator.Type;
using Sqlx.Postgres.Query;

namespace Sqlx.Postgres.Connection;

public partial class PgConnectionTest
{
    [Test]
    [Arguments(TestPgEnum.None)]
    [Arguments(TestPgEnum.Something)]
    public async Task ExecuteScalar_Should_EncodeAndDecode_When_PgEnumAndDefaultEncoding(TestPgEnum value, CancellationToken ct)
    {
        await using IPgConnection connection = DatabaseFixture.BasicPool.CreateConnection();
        await using IPgExecutableQuery query = connection.CreateQuery("SELECT $1 pg_enum_col;");
        query.Bind(value);
        var result = await query.ExecuteScalar<TestPgEnum>(ct);
        await Assert.That(result).IsEqualTo(value);
    }

    [Test]
    [Arguments("None", TestPgEnum.None)]
    [Arguments("Something", TestPgEnum.Something)]
    public async Task ExecuteScalar_Should_Decode_When_PgEnumAndTextEncoding(string literal, TestPgEnum value, CancellationToken ct)
    {
        var sql = $"SELECT '{literal}'::enum_type;";
        await using IPgConnection
            connection = DatabaseFixture.SimpleQueryTextPool.CreateConnection();
        await using IPgExecutableQuery query = connection.CreateQuery(sql);
        var result = await query.ExecuteScalar<TestPgEnum>(ct);
        await Assert.That(result).IsEqualTo(value);
    }
    
    [Test]
    [Arguments(TestIntEnum.None)]
    [Arguments(TestIntEnum.Something)]
    public async Task ExecuteScalar_Should_EncodeAndDecode_When_IntEnumAndDefaultEncoding(TestIntEnum value, CancellationToken ct)
    {
        await using IPgConnection connection = DatabaseFixture.BasicPool.CreateConnection();
        await using IPgExecutableQuery query = connection.CreateQuery("SELECT $1 int_enum_col;");
        query.Bind(value);
        var result = await query.ExecuteScalar<TestIntEnum>(ct);
        await Assert.That(result).IsEqualTo(value);
    }
    
    [Test]
    [Arguments(0, TestIntEnum.None)]
    [Arguments(1, TestIntEnum.Something)]
    public async Task ExecuteScalar_Should_Decode_When_IntEnumAndTextEncoding(int intValue, TestIntEnum value, CancellationToken ct)
    {
        var sql = $"SELECT {intValue};";
        await using IPgConnection connection = DatabaseFixture.BasicPool.CreateConnection();
        await using IPgExecutableQuery query = connection.CreateQuery(sql);
        var result = await query.ExecuteScalar<TestIntEnum>(ct);
        await Assert.That(result).IsEqualTo(value);
    }
    
    [Test]
    [Arguments(TestTextEnum.None)]
    [Arguments(TestTextEnum.Something)]
    public async Task ExecuteScalar_Should_EncodeAndDecode_When_TextEnumAndDefaultEncoding(TestTextEnum value, CancellationToken ct)
    {
        await using IPgConnection connection = DatabaseFixture.BasicPool.CreateConnection();
        await using IPgExecutableQuery query = connection.CreateQuery("SELECT $1 text_enum_col;");
        query.Bind(value);
        var result = await query.ExecuteScalar<TestTextEnum>(ct);
        await Assert.That(result).IsEqualTo(value);
    }
    
    [Test]
    [Arguments("None", TestTextEnum.None)]
    [Arguments("Something", TestTextEnum.Something)]
    public async Task ExecuteScalar_Should_Decode_When_TextEnumAndTextEncoding(string literal, TestTextEnum value, CancellationToken ct)
    {
        var sql = $"SELECT '{literal}';";
        await using IPgConnection connection = DatabaseFixture.BasicPool.CreateConnection();
        await using IPgExecutableQuery query = connection.CreateQuery(sql);
        var result = await query.ExecuteScalar<TestTextEnum>(ct);
        await Assert.That(result).IsEqualTo(value);
    }
}

[PgEnum(Name = "enum_type")]
public enum TestPgEnum
{
    None,
    Something,
}

[WrapperEnum(Representation = EnumRepresentation.Int)]
public enum TestIntEnum
{
    None = 0,
    Something = 1,
}

[WrapperEnum(Representation = EnumRepresentation.Text)]
public enum TestTextEnum
{
    None,
    Something,
}
