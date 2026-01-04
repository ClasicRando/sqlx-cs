using JetBrains.Annotations;
using Sqlx.Postgres.Fixtures;

namespace Sqlx.Postgres.Connection;

[ClassDataSource<DatabaseFixture>(Shared = SharedType.PerClass)]
[TestSubject(typeof(PgConnection))]
public partial class PgConnectionTest(DatabaseFixture databaseFixture)
{
    private const string OutProcedureName = "test_proc_out";
    private const string InOutProcedureName = "test_proc_in_out";

    public const string CreateProceduresQuery = 
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

    public static readonly string CreateCopyTables =
        $"""
        DROP TABLE IF EXISTS public.copy_in_test;
        CREATE TABLE public.copy_in_test(id int not null, text_field text not null);
        
        DROP TABLE IF EXISTS public.copy_out_test;
        CREATE TABLE public.copy_out_test(id int not null, text_field text not null);
        INSERT INTO public.copy_out_test(id, text_field)
        SELECT t.t, t.t || ' Value'
        FROM generate_series(1, {CopyRowCount}) t;
        """;
}