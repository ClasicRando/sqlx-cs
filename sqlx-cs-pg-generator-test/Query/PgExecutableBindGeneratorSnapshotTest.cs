namespace Sqlx.Postgres.Generator.Tests.Query;

[UsesVerify]
public class PgExecutableBindGeneratorSnapshotTest
{
    [Fact]
    public Task When_NotNullStructValueTypeAndNoCustomDecoderSpecified()
    {
        const string source = 
            """
            using Sqlx.Core.Query;
            using Sqlx.Postgres.Generator.Query;
            using Sqlx.Postgres.Types;
            
            public readonly record struct PgTimeTz(TimeOnly Time, int OffsetSeconds);

            public static class TestExtensions
            {
                [GeneratePgBindMethod]
                public static partial IQuery Bind(this IQuery query, PgTimeTz value);
            }
            """;

        return TestHelper.VerifyPgExecutableBindGenerator(source);
    }
    
    [Fact]
    public Task When_NotNullStructValueTypeAndCustomDecoderSpecified()
    {
        const string source = 
            """
            using Sqlx.Core.Query;
            using Sqlx.Postgres.Generator.Query;
            using Sqlx.Postgres.Types;

            public static class TestExtensions
            {
                [GeneratePgBindMethod(Encoder = typeof(PgUuid))]
                public static partial IQuery Bind(this IQuery query, System.Guid value);
            }
            """;

        return TestHelper.VerifyPgExecutableBindGenerator(source);
    }
    
     [Fact]
     public Task When_NullableStructValueTypeAndNoCustomDecoderSpecified()
     {
         const string source = 
             """
             using Sqlx.Core.Query;
             using Sqlx.Postgres.Generator.Query;
             using Sqlx.Postgres.Types;
             
             public readonly record struct PgTimeTz(TimeOnly Time, int OffsetSeconds);

             public static class TestExtensions
             {
                 [GeneratePgBindMethod]
                 public static partial IQuery Bind(this IQuery query, PgTimeTz? value);
             }
             """;

         return TestHelper.VerifyPgExecutableBindGenerator(source);
     }
    
     [Fact]
     public Task When_NullableStructValueTypeAndCustomDecoderSpecified()
     {
         const string source = 
             """
             using Sqlx.Core.Query;
             using Sqlx.Postgres.Generator.Query;
             using Sqlx.Postgres.Types;

             public static class TestExtensions
             {
                 [GeneratePgBindMethod(Encoder = typeof(PgUuid))]
                 public static partial IQuery Bind(this IQuery query, System.Guid? value);
             }
             """;

         return TestHelper.VerifyPgExecutableBindGenerator(source);
     }
    
     [Fact]
     public Task When_NullableClassValueTypeAndNoCustomDecoderSpecified()
     {
         const string source = 
             """
             using Sqlx.Core.Query;
             using Sqlx.Postgres.Generator.Query;
             using Sqlx.Postgres.Types;

             public static class TestExtensions
             {
                 [GeneratePgBindMethod]
                 public static partial IQuery Bind(this IQuery query, string? value);
             }
             """;

         return TestHelper.VerifyPgExecutableBindGenerator(source);
     }
    
     [Fact]
     public Task When_NullableClassValueTypeAndCustomDecoderSpecified()
     {
         const string source = 
             """
             using Sqlx.Core.Query;
             using Sqlx.Postgres.Generator.Query;
             using Sqlx.Postgres.Types;

             public static class TestExtensions
             {
                 [GeneratePgBindMethod(Encoder = typeof(PgString))]
                 public static partial IQuery Bind(this IQuery query, string? value);
             }
             """;

         return TestHelper.VerifyPgExecutableBindGenerator(source);
     }
     
     [Fact]
     public Task When_ArrayStructValueTypeAndNoCustomDecoderSpecified()
     {
         const string source = 
             """
             using Sqlx.Core.Query;
             using Sqlx.Postgres.Generator.Query;
             using Sqlx.Postgres.Types;

             public readonly record struct PgTimeTz(TimeOnly Time, int OffsetSeconds);

             public static class TestExtensions
             {
                 [GeneratePgBindMethod]
                 public static partial IQuery Bind(this IQuery query, PgTimeTz?[]? value);
             }
             """;

         return TestHelper.VerifyPgExecutableBindGenerator(source);
     }
    
     [Fact]
     public Task When_ArrayStructValueTypeAndCustomDecoderSpecified()
     {
         const string source = 
             """
             using Sqlx.Core.Query;
             using Sqlx.Postgres.Generator.Query;
             using Sqlx.Postgres.Types;

             public static class TestExtensions
             {
                 [GeneratePgBindMethod(Encoder = typeof(PgUuid))]
                 public static partial IQuery Bind(this IQuery query, System.Guid?[]? value);
             }
             """;

         return TestHelper.VerifyPgExecutableBindGenerator(source);
     }
    
     [Fact]
     public Task When_ArrayClassValueTypeAndNoCustomDecoderSpecified()
     {
         const string source = 
             """
             using Sqlx.Core.Query;
             using Sqlx.Postgres.Generator.Query;
             using Sqlx.Postgres.Types;

             public static class TestExtensions
             {
                 [GeneratePgBindMethod]
                 public static partial IQuery Bind(this IQuery query, string?[]? value);
             }
             """;

         return TestHelper.VerifyPgExecutableBindGenerator(source);
     }
    
     [Fact]
     public Task When_ArrayClassValueTypeAndCustomDecoderSpecified()
     {
         const string source = 
             """
             using Sqlx.Core.Query;
             using Sqlx.Postgres.Generator.Query;
             using Sqlx.Postgres.Types;

             public static class TestExtensions
             {
                 [GeneratePgBindMethod(Encoder = typeof(PgString))]
                 public static partial IQuery Bind(this IQuery query, string?[]? value);
             }
             """;

         return TestHelper.VerifyPgExecutableBindGenerator(source);
     }
}
