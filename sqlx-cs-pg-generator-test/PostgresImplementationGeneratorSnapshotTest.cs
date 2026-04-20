namespace Sqlx.Postgres.Generator.Tests;

public class PostgresImplementationGeneratorSnapshotTest
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

        await TestHelper.VerifyPostgresGenerator(source);
    }

    [Test]
    public async Task When_EnumWithRenamePgNameAndPgEnum()
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

        await TestHelper.VerifyPostgresGenerator(source);
    }

    [Test]
    public async Task When_SimplePgComposite()
    {
        const string source =
            """
            using Sqlx.Postgres.Type;
            using Sqlx.Postgres.Generator;
            using Sqlx.Postgres.Generator.Type;

            [PgComposite(Name = "test_composite")]
            public readonly partial struct TestComposite
            {
                public required int Id { get; init; }
                
                public string Name { get; init; }
                
                public byte[]? Bytes { get; init; }
                
                public PgRange<long> LongRange { get; init; }
            }
            """;

        await TestHelper.VerifyPostgresGenerator(source);
    }

    [Test]
    public async Task When_FromRow()
    {
        const string source =
            """
            using System;
            using System.Collections;
            using System.Net;
            using Sqlx.Postgres.Type;
            using Sqlx.Postgres.Generator;
            using Sqlx.Postgres.Generator.Result;
            using Sqlx.Postgres.Generator.Type;

            [WrapperEnum(Representation = EnumRepresentation.Int)]
            public enum WrapperEnumType
            {
                None,
                Some,
            }

            [PgEnum(Name = "postgres_enum_type")]
            public enum PostgresEnumType
            {
                None,
                Some
            }

            [FromRow]
            public readonly partial record struct TestRow(
                bool BoolField,
                sbyte ByteField,
                short ShortField,
                int IntField,
                long LongField,
                float FloatField,
                double DoubleField,
                TimeOnly TimeField,
                DateOnly DateField,
                DateTime DateTimeField,
                DateTimeOffset DateTimeOffsetField,
                decimal DecimalField,
                string StringField,
                Guid GuidField,
                IPNetwork IpNetworkField,
                BitArray BitArrayField,
                PgRange<long> LongRangeField,
                PgRange<int> IntRangeField,
                PgRange<DateOnly> DateRangeField,
                PgRange<DateTime> DateTimeRangeField,
                PgRange<DateTimeOffset> DateTimeOffsetRangeField,
                PgRange<decimal> DecimalRangeField,
                byte[] BytesField,
                PgPoint PointField,
                int[] IntArrayField,
                int? NullableIntField,
                WrapperEnumType WrapperEnumTypeField,
                PostgresEnumType PostgresEnumTypeField)
            {
            }
            """;

        await TestHelper.VerifyPostgresGenerator(source);
    }

    [Test]
    public async Task When_FromRow_ShouldHandleAttributes()
    {
        const string source =
            """
            using System.Collections;
            using System.Net;
            using Sqlx.Postgres.Type;
            using Sqlx.Postgres.Generator;
            using Sqlx.Postgres.Generator.Result;
            using Sqlx.Postgres.Generator.Type;

            public readonly struct InnerRow : IFromRow<IPgDataRow, InnerRow>
            {
                public string InnerField { get; init; }
                
                public static InnerRow FromRow(IPgDataRow dataRow)
                {
                    return new InnerRow { InnerField = dataRow.GetStringNotNull("inner_field") };
                }
            }

            public readonly struct JsonFieldType
            {
                public required int NumberField { get; init; }
                
                public required string StringField { get; init; }
            }

            [FromRow]
            public readonly partial struct TestRow
            {
                public required int Id { get; init; }
                
                [FlattenField]
                public InnerRow Inner { get; init; }
                
                [JsonField]
                public JsonFieldType JsonField { get; init; }
                
                [JsonField]
                public JsonFieldType? OptionalJsonField { get; init; }
                
                [PgPropertySkip]
                public string Name { get; init; } = "";
            }
            """;

        await TestHelper.VerifyPostgresGenerator(source);
    }

    [Test]
    public async Task When_FromRow_ShouldFailIfDuplicateAttributes()
    {
        const string source =
            """
            using System.Collections;
            using Sqlx.Postgres.Type;
            using Sqlx.Postgres.Generator;
            using Sqlx.Postgres.Generator.Result;
            using Sqlx.Postgres.Generator.Type;

            public readonly struct InnerRow : IFromRow<IPgDataRow, InnerRow>
            {
                public string InnerField { get; init; }
                
                public static InnerRow FromRow(IPgDataRow dataRow)
                {
                    return new InnerRow { InnerField = dataRow.GetStringNotNull("inner_field") };
                }
            }

            [FromRow]
            public readonly partial struct TestRowDuplicatePropertyAttribute
            {
                [FlattenField]
                [JsonField]
                public InnerRow Inner { get; init; }
            }

            [FromRow]
            public readonly partial struct TestRowDuplicateParameterAttribute
            {
                public InnerRow Inner { get; }
                
                public TestRowDuplicateParameter(
                    [FlattenField]
                    [JsonField]
                    InnerRow inner)
                {
                    Inner = inner;
                }
            }
            """;

        await TestHelper.VerifyPostgresGenerator(source);
    }

    [Test]
    public async Task When_FromRow_ShouldFailIfUnknownType()
    {
        const string source =
            """
            using Sqlx.Postgres.Generator.Result;

            [FromRow]
            public readonly partial struct TestRowUnknownType
            {
                public Dictionary<int, string> Inner { get; init; }
            }
            """;

        await TestHelper.VerifyPostgresGenerator(source);
    }

    [Test]
    public async Task When_ToParam_ShouldSucceed()
    {
        const string source =
            """
            using Sqlx.Postgres.Generator;
            using Sqlx.Postgres.Generator.Query;

            [ToParam]
            public readonly partial struct TestParamBinding
            {
                public required int Id { get; init; }
                
                public required string Id { get; init; }
                
                [PgPropertySkip]
                public string IdString => Id.ToString();
            }
            """;

        await TestHelper.VerifyPostgresGenerator(source);
    }

    [Test]
    public async Task When_ToPgBinaryCopyRow_ShouldSucceed()
    {
        const string source =
            """
            using Sqlx.Postgres.Generator;
            using Sqlx.Postgres.Generator.Copy;

            [ToPgBinaryCopyRow]
            public readonly partial struct TestParamBinding
            {
                public required int Id { get; init; }
                
                public required string Id { get; init; }
                
                [PgPropertySkip]
                public string IdString => Id.ToString();
            }
            """;

        await TestHelper.VerifyPostgresGenerator(source);
    }
    
    [Test]
    public async Task When_WrapperType_ShouldSucceed()
    {
        const string source =
            """
            using Sqlx.Postgres.Generator;
            using Sqlx.Postgres.Generator.Type;

            [WrapperType]
            public readonly partial record struct TestWrapperType
            {
                public required int Id { get; init; }
                
                [PgPropertySkip]
                public string IdString => Id.ToString();
            }
            """;

        await TestHelper.VerifyPostgresGenerator(source);
    }
    
    [Test]
    public async Task When_WrapperTypeOverComplexType_ShouldSucceed()
    {
        const string source =
            """
            using Sqlx.Postgres.Generator;
            using Sqlx.Postgres.Generator.Type;

            [WrapperType]
            public readonly partial record struct TestWrapperIds
            {
                public required int[] Inner { get; init; }
            }
            """;

        await TestHelper.VerifyPostgresGenerator(source);
    }

    [Test]
    public async Task When_WrapperTextEnum()
    {
        const string source =
            """
            using Sqlx.Postgres.Generator;
            using Sqlx.Postgres.Generator.Type;
            
            [WrapperEnum(Representation = EnumRepresentation.Text)]
            public enum TestEnum
            {
                None,
                Single,
                MultipleWords,
                Value_With4Words
            }
            """;

        await TestHelper.VerifyPostgresGenerator(source);
    }

    [Test]
    public async Task When_GetField()
    {
        const string source =
            """
            using Sqlx.Postgres.Result;
            using Sqlx.Postgres.Generator;
            using Sqlx.Postgres.Generator.Type;

            void Test(IPgDataRow dataRow)
            {
                dataRow.GetField<byte[]>(0);
            }
            """;

        await TestHelper.VerifyPostgresGenerator(source);
    }
}
