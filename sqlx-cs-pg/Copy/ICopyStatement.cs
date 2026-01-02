namespace Sqlx.Postgres.Copy;

/// <summary>
/// Constructor of valid <c>COPY</c> statements for the Postgresql extended query feature. See the
/// postgresql <a href="https://www.postgresql.org/docs/current/sql-copy.html">docs</a> for more
/// information. Most of the parameter descriptions were taken from the Postgresql docs if they
/// represent the exact option or query component.
/// </summary>
public interface ICopyStatement
{
    /// <returns>
    /// <c>COPY</c> query that will be sent to the database for the specified options
    /// </returns>
    string ToCopyQuery();
}

/// <summary>
/// <c>COPY TO</c> statement that copies data from the database to the standard out
/// </summary>
public interface ICopyTo : ICopyStatement;

/// <summary>
/// <c>COPY FROM</c> statement that copies user supplied data to the database from the standard
/// input
/// </summary>
public interface ICopyFrom : ICopyStatement;
