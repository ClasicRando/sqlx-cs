namespace Sqlx.Postgres.Copy;

/// <summary>
/// <see cref="ICopyTo"/> where a query is specified as the source of the data to copy to STDOUT
/// </summary>
public interface ICopyQuery : ICopyTo
{
    /// <summary>
    /// SQL query to be executed as the supplier of the records in the <c>COPY TO</c> operation
    /// </summary>
    string Query { get; }
}
