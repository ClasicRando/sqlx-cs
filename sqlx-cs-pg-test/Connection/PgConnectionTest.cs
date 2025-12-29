using JetBrains.Annotations;
using Sqlx.Postgres.Fixtures;

namespace Sqlx.Postgres.Connection;

[ClassDataSource<DatabaseFixture>(Shared = SharedType.PerClass)]
[TestSubject(typeof(PgConnection))]
public partial class PgConnectionTest(DatabaseFixture databaseFixture)
{
    private const string OutProcedureName = "test_proc_out";
    private const string InOutProcedureName = "test_proc_in_out";

    public const string SetUpQuery = 
        $"""
         DROP PROCEDURE IF EXISTS public.{OutProcedureName};
         CREATE PROCEDURE public.{OutProcedureName}(out int, out text)
         LANGUAGE plpgsql
         AS $$
         BEGIN
             $1 := 4;
             $2 := 'This is a test';
         END;
         $$;
         
         DROP PROCEDURE IF EXISTS public.{InOutProcedureName};
         CREATE PROCEDURE public.{InOutProcedureName}(in out int, in out text)
         LANGUAGE plpgsql
         AS $$
         BEGIN
             $1 := COALESCE($1,0) + 1;
             $2 := LTRIM($2 || ',' || $1, ',');
         END;
         $$;
         """;
    
    public const string CreateTypeQuery =
        """
        DROP TYPE IF EXISTS public.composite_type;
        CREATE TYPE public.composite_type AS
        (
            id int,
            name text,
            title text
        );
        """;
}