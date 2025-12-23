using JetBrains.Annotations;
using Sqlx.Postgres.Fixtures;
using Sqlx.Postgres.Query;

namespace Sqlx.Postgres.Connection;

[Collection("Postgres Database")]
[TestSubject(typeof(PgConnection))]
public partial class PgConnectionTest
{
    private readonly DatabaseFixture _databaseFixture;

    public PgConnectionTest(DatabaseFixture databaseFixture)
    {
        _databaseFixture = databaseFixture;
        InitializeStoredProcedures().GetAwaiter().GetResult();
        CreateCompositeType().GetAwaiter().GetResult();
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

    private async Task InitializeStoredProcedures()
    {
        using IPgConnection connection = _databaseFixture.BasicPool.CreateConnection();
        using IPgExecutableQuery setUp = connection.CreateQuery(SetUpQuery);
        await setUp.ExecuteNonQuery();
    }
}
