using JetBrains.Annotations;
using Sqlx.Core.Connection;
using Sqlx.Core.Query;
using Sqlx.Postgres.Fixtures;

namespace Sqlx.Postgres.Connection;

[Collection("Postgres Database")]
[TestSubject(typeof(PgConnection))]
public partial class PgConnectionTest
{
    private readonly DatabaseFixture _databaseFixture;

    public PgConnectionTest(DatabaseFixture databaseFixture)
    {
        _databaseFixture = databaseFixture;
    }
    
    private const string OutProcedureName = "test_proc_out";
    private const string InOutProcedureName = "test_proc_in_out";

    private const string SetUpQuery = 
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

    private static async Task InitializeStoredProcedures(IConnection connection)
    {
        using IExecutableQuery setUp = connection.CreateQuery(SetUpQuery);
        await setUp.ExecuteNonQuery();
    }
}
