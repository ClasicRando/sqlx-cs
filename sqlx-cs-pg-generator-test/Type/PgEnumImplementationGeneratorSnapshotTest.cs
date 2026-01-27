namespace Sqlx.Postgres.Generator.Tests.Type;

public class PgEnumImplementationGeneratorSnapshotTest
{
    [Test]
    public async Task When_SimpleEnum()
    {
        const string source = 
            """
            using Sqlx.Postgres.Generator;
            using Sqlx.Postgres.Generator.Type;

            [PgEnum(Name = "test_enum")]
            public enum TestEnum
            {
                None,
                Single,
                MultipleWords,
                Value_With4Words
            }
            """;

        await TestHelper.VerifyPgEnumImplementationGenerator(source);
    }
    
    [Test]
    public async Task When_EnumWithRenameAndSimpleTextWrapper()
    {
        const string source = 
            """
            using Sqlx.Postgres.Generator;
            using Sqlx.Postgres.Generator.Type;

            [PgEnum(
                Name = "test_enum",
                RenameAll = Rename.SnakeCase,
                Representation = EnumRepresentation.Text)]
            public enum TestEnum
            {
                None,
                Single,
                MultipleWords,
                Value_With4Words
            }
            """;

        await TestHelper.VerifyPgEnumImplementationGenerator(source);
    }
    
    [Test]
    public async Task When_EnumWithRenameAndSimpleIntWrapper()
    {
        const string source = 
            """
            using Sqlx.Postgres.Generator;
            using Sqlx.Postgres.Generator.Type;

            [PgEnum(
                Name = "test_enum",
                RenameAll = Rename.CamelCase,
                Representation = EnumRepresentation.Int)]
            public enum TestEnum
            {
                None,
                Single,
                MultipleWords,
                ValueWith4Words
            }
            """;

        await TestHelper.VerifyPgEnumImplementationGenerator(source);
    }
    
    
    [Test]
    public async Task When_EnumWithRenamePgNameAndSimpleIntWrapper()
    {
        const string source = 
            """
            using Sqlx.Postgres.Generator;
            using Sqlx.Postgres.Generator.Type;

            [PgEnum(Name = "test_enum", RenameAll = Rename.SnakeCase)]
            public enum TestEnum
            {
                None,
                Single,
                [PgName("multi_words")]
                MultipleWords,
            }
            """;

        await TestHelper.VerifyPgEnumImplementationGenerator(source);
    }
}
