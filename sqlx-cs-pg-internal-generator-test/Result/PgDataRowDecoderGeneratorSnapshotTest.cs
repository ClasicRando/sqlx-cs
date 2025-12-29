namespace Sqlx.Postgres.Generator.Tests.Result;

public class PgDataRowDecoderGeneratorSnapshotTest
{
    [Test]
    public async Task When_NotNullRefTypeReturnStringIndex()
    {
        const string source = 
            """
            using Sqlx.Core.Result;
            using Sqlx.Postgres.Generator.Result;
            using Sqlx.Postgres.Types;

            public static class TestExtensions
            {
                [GeneratePgDecodeMethod(Decoder = typeof(PgString))]
                public static partial string GetStringNotNull(this IDataRow dataRow, string name);
            }
            """;

        await TestHelper.VerifyPgDataRowDecoderGenerator(source);
    }
    
    [Test]
    public async Task When_NullableRefTypeReturnIntIndex()
    {
        const string source = 
            """
            using Sqlx.Core.Result;
            using Sqlx.Postgres.Generator.Result;
            using Sqlx.Postgres.Types;

            public static class TestExtensions
            {
                [GeneratePgDecodeMethod(Decoder = typeof(PgString))]
                public static partial string? GetString(this IDataRow dataRow, int index);
            }
            """;

        await TestHelper.VerifyPgDataRowDecoderGenerator(source);
    }
    
    [Test]
    public async Task When_NullableValueTypeReturnIntIndex()
    {
        const string source = 
            """
            using Sqlx.Core.Result;
            using Sqlx.Postgres.Generator.Result;
            using Sqlx.Postgres.Types;

            public static class TestExtensions
            {
                [GeneratePgDecodeMethod(Decoder = typeof(PgInt))]
                public static partial int? GetInt(this IDataRow dataRow, int index);
            }
            """;

        await TestHelper.VerifyPgDataRowDecoderGenerator(source);
    }
    
    [Test]
    public async Task When_ValueTypeArray()
    {
        const string source = 
            """
            using Sqlx.Core.Result;
            using Sqlx.Postgres.Generator.Result;
            using Sqlx.Postgres.Types;

            public static class TestExtensions
            {
                [GeneratePgDecodeMethod(Decoder = typeof(PgBool))]
                public static partial bool?[]? GetPgBooleanArray(this IDataRow dataRow, int index);
            }
            """;

        await TestHelper.VerifyPgDataRowDecoderGenerator(source);
    }
    
    [Test]
    public async Task When_RefTypeArray()
    {
        const string source = 
            """
            using Sqlx.Core.Result;
            using Sqlx.Postgres.Generator.Result;
            using Sqlx.Postgres.Types;

            public static class TestExtensions
            {
                [GeneratePgDecodeMethod(Decoder = typeof(PgString))]
                public static partial string?[]? GetPgStringArray(this IDataRow dataRow, int index);
            }
            """;

        await TestHelper.VerifyPgDataRowDecoderGenerator(source);
    }
}