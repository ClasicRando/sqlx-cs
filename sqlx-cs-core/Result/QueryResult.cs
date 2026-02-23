namespace Sqlx.Core.Result;

/// <summary>
/// Query result data sent by the database
/// </summary>
/// <param name="RowsAffected">number of rows impacted by the query</param>
/// <param name="Message">message sent by the database after completing the query execution</param>
public readonly record struct QueryResult(long RowsAffected, string Message);
