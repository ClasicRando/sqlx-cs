namespace Sqlx.Postgres.Generator.Tests.Result;

[UsesVerify]
public class PgDataRowDecoderGeneratorSnapshotTest
{
    [Fact]
    public Task When_NotNullReturnIntIndexAndNoCustomDecoderSpecified()
    {
        const string source = 
            """
            using Sqlx.Core.Result;
            using Sqlx.Postgres.Generator.Result;
            using Sqlx.Postgres.Types;

            public static class TestExtensions
            {
                [GeneratePgDecodeMethod]
                public static partial PgTimeTz GetPgTimeTz(this IDataRow dataRow, int index);
            }
            """;

        return TestHelper.VerifyPgDataRowDecoderGenerator(source);
    }
    
    [Fact]
    public Task When_NotNullReturnStringIndexAndNoCustomDecoderSpecified()
    {
        const string source = 
            """
            using Sqlx.Core.Result;
            using Sqlx.Postgres.Generator.Result;
            using Sqlx.Postgres.Types;

            public static class TestExtensions
            {
                [GeneratePgDecodeMethod]
                public static partial PgTimeTz GetPgTimeTz(this IDataRow dataRow, string name);
            }
            """;

        return TestHelper.VerifyPgDataRowDecoderGenerator(source);
    }
    
    [Fact]
    public Task When_NullableValueTypeReturnIntIndexAndNoCustomDecoderSpecified()
    {
        const string source = 
            """
            using Sqlx.Core.Result;
            using Sqlx.Postgres.Generator.Result;
            using Sqlx.Postgres.Types;

            public static class TestExtensions
            {
                [GeneratePgDecodeMethod]
                public static partial PgTimeTz? GetPgTimeTz(this IDataRow dataRow, int index);
            }
            """;

        return TestHelper.VerifyPgDataRowDecoderGenerator(source);
    }
    
    [Fact]
    public Task When_NullableRefTypeReturnIntIndexAndCustomDecoderSpecified()
    {
        const string source = 
            """
            using Sqlx.Core.Result;
            using Sqlx.Postgres.Generator.Result;
            using Sqlx.Postgres.Types;

            public static class TestExtensions
            {
                [GeneratePgDecodeMethod(Decoder = typeof(PgString))]
                public static partial string? GetPgString(this IDataRow dataRow, int index);
            }
            """;

        return TestHelper.VerifyPgDataRowDecoderGenerator(source);
    }
    
    [Fact]
    public Task When_CustomDecoderSpecified()
    {
        const string source = 
            """
            using Sqlx.Core.Result;
            using Sqlx.Postgres.Generator.Result;
            using Sqlx.Postgres.Types;

            public static class TestExtensions
            {
                [GeneratePgDecodeMethod(Decoder = typeof(PgBool))]
                public static partial bool GetPgBoolean(this IDataRow dataRow, int index);
            }
            """;

        return TestHelper.VerifyPgDataRowDecoderGenerator(source);
    }
    
    [Fact]
    public Task When_CustomGenericDecoderSpecified()
    {
        const string source = 
            """
            using Sqlx.Core.Result;
            using Sqlx.Postgres.Generator.Result;
            using Sqlx.Postgres.Types;

            public static class TestExtensions
            {
                [GeneratePgDecodeMethod(Decoder = typeof(PgArrayTypeStruct<bool, PgBool>))]
                public static partial bool?[]? GetPgBooleanArray(this IDataRow dataRow, int index);
            }
            """;

        return TestHelper.VerifyPgDataRowDecoderGenerator(source);
    }
}
