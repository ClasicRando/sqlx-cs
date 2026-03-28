using Sqlx.Postgres.Generator.Type;
using Sqlx.Postgres.Query;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Connection;

public partial class PgConnectionTest
{
    [Test]
    [Arguments(TestPgEnum.None)]
    [Arguments(TestPgEnum.Something)]
    public async Task ExecuteScalar_Should_EncodeAndDecode_When_PgEnumAndDefaultEncoding(TestPgEnum value, CancellationToken ct)
    {
        using IPgConnection connection = DatabaseFixture.BasicPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery("SELECT $1 pg_enum_col;");
        query.Bind(value);
        TestPgEnum result = await query.ExecuteScalar<TestPgEnum, PgTestPgEnum>(ct);
        await Assert.That(result).IsEqualTo(value);
    }

    [Test]
    [Arguments("None", TestPgEnum.None)]
    [Arguments("Something", TestPgEnum.Something)]
    public async Task ExecuteScalar_Should_Decode_When_PgEnumAndTextEncoding(string literal, TestPgEnum value, CancellationToken ct)
    {
        var sql = $"SELECT '{literal}'::enum_type;";
        using IPgConnection
            connection = DatabaseFixture.SimpleQueryTextPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery(sql);
        TestPgEnum result = await query.ExecuteScalar<TestPgEnum, PgTestPgEnum>(ct);
        await Assert.That(result).IsEqualTo(value);
    }
    
    [Test]
    [Arguments(TestIntEnum.None)]
    [Arguments(TestIntEnum.Something)]
    public async Task ExecuteScalar_Should_EncodeAndDecode_When_IntEnumAndDefaultEncoding(TestIntEnum value, CancellationToken ct)
    {
        using IPgConnection connection = DatabaseFixture.BasicPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery("SELECT $1 int_enum_col;");
        query.Bind(value);
        TestIntEnum result = await ExecuteScalarEnumWrapperExtract(query, (row, i) => row.GetField<TestIntEnum>(i));
        await Assert.That(result).IsEqualTo(value);
    }
    
    [Test]
    [Arguments(0, TestIntEnum.None)]
    [Arguments(1, TestIntEnum.Something)]
    public async Task ExecuteScalar_Should_Decode_When_IntEnumAndTextEncoding(int intValue, TestIntEnum value, CancellationToken ct)
    {
        var sql = $"SELECT {intValue};";
        using IPgConnection connection = DatabaseFixture.BasicPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery(sql);
        TestIntEnum result = await ExecuteScalarEnumWrapperExtract(query, (row, i) => row.GetField<TestIntEnum>(i));
        await Assert.That(result).IsEqualTo(value);
    }
    
    [Test]
    [Arguments(TestTextEnum.None)]
    [Arguments(TestTextEnum.Something)]
    public async Task ExecuteScalar_Should_EncodeAndDecode_When_TextEnumAndDefaultEncoding(TestTextEnum value, CancellationToken ct)
    {
        using IPgConnection connection = DatabaseFixture.BasicPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery("SELECT $1 text_enum_col;");
        query.Bind(value);
        TestTextEnum result = await ExecuteScalarEnumWrapperExtract(query, (row, i) => row.GetField<TestTextEnum>(i));
        await Assert.That(result).IsEqualTo(value);
    }
    
    [Test]
    [Arguments("None", TestTextEnum.None)]
    [Arguments("Something", TestTextEnum.Something)]
    public async Task ExecuteScalar_Should_Decode_When_TextEnumAndTextEncoding(string literal, TestTextEnum value, CancellationToken ct)
    {
        var sql = $"SELECT '{literal}';";
        using IPgConnection connection = DatabaseFixture.BasicPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery(sql);
        TestTextEnum result = await ExecuteScalarEnumWrapperExtract(query, (row, i) => row.GetField<TestTextEnum>(i));
        await Assert.That(result).IsEqualTo(value);
    }

    private static async Task<TEnum> ExecuteScalarEnumWrapperExtract<TEnum>(IPgExecutableQuery query, Func<IPgDataRow, int, TEnum> extractor)
        where TEnum : Enum
    {
        TEnum? result = default;
        var results = await query.ExecuteAsync();
        while (await results.MoveNextAsync())
        {
            var current = results.Current;
            if (current.IsLeft)
            {
                result = extractor(current.Left, 0);
            }
            break;
        }

        if (result is null)
        {
            Assert.Fail("Did not find a row");
        }

        return result!;
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
