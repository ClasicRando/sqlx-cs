using System.IO.Pipelines;
using Sqlx.Core.Connection;
using Sqlx.Core.Result;
using Sqlx.Postgres.Copy;
using Sqlx.Postgres.Query;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Connection;

/// <summary>
/// Postgres connection interface. Includes all base connection operations, is a query executor and
/// also includes other Postgres specific connection capabilities.
/// </summary>
public interface IPgConnection :
    IConnection<IPgExecutableQuery, IPgBindable, IPgQueryBatch, IPgDataRow>
{
    /// <summary>
    /// Executes the <c>COPY TO</c> command and returns a stream of row data as text, CSV or binary.
    /// Generally this is not the method you want to use since it requires knowing each row type and
    /// using raw bytes. Prefer using other copy out methods:
    /// <list type="bullet">
    ///     <item>
    ///     Copy results to stream ->
    ///     <see cref="PgConnectionExtensions.CopyOutAsync(IPgConnection,ICopyTo,Stream,CancellationToken)"/>
    ///     </item>
    ///     <item>
    ///     Copy results to file path ->
    ///     <see cref="PgConnectionExtensions.CopyOutAsync(IPgConnection,ICopyTo,string,FileMode,CancellationToken)"/>
    ///     </item>
    ///     <item>
    ///     Parse results as rows ->
    ///     <see cref="PgConnectionExtensions.CopyOutRowsAsync{TRow}"/>
    ///     </item>
    /// </list>
    /// <a href="https://www.postgresql.org/docs/current/sql-copy.html"> postgres docs</a>
    /// </summary>
    /// <param name="copyOutStatement">COPY statement to execute for data extraction</param>
    /// <param name="cancellationToken">Token to cancel the async operation</param>
    /// <returns>A stream of rows returned as a result of the copy statement</returns>
    IAsyncEnumerable<byte[]> CopyOutAsync(
        ICopyTo copyOutStatement,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes the <c>COPY FROM</c> command with the supplied <paramref name="data"/> and returns
    /// a <see cref="QueryResult"/> indicating the outcome.
    /// <a href="https://www.postgresql.org/docs/current/sql-copy.html"> postgres docs</a>
    /// </summary>
    /// <param name="copyInStatement">COPY statement to execute for data ingestion</param>
    /// <param name="data">
    /// Reader of a pipe that provides the source data to send to the database
    /// </param>
    /// <param name="cancellationToken">Token to cancel the async operation</param>
    /// <returns>Query result object with details on what happened during the execution</returns>
    Task<QueryResult> CopyInAsync(
        ICopyFrom copyInStatement,
        Stream data,
        CancellationToken cancellationToken = default);


    /// <summary>
    /// Execute a <c>COPY FROM</c> query against the database and copies all supplied
    /// <paramref name="rows"/> as the copy data.  
    /// </summary>
    /// <param name="copyInStatement">COPY statement to execute for data extraction</param>
    /// <param name="rows">
    /// Async stream of copy rows to encode and send to the server as the copy statement row
    /// data
    /// </param>
    /// <param name="cancellationToken">Token to cancel the async operation</param>
    Task<QueryResult> CopyInRowsAsync<TCopyStatement, TCopyRow>(
        TCopyStatement copyInStatement,
        IAsyncEnumerable<TCopyRow> rows,
        CancellationToken cancellationToken = default)
        where TCopyStatement : ICopyFrom, ICopyBinary
        where TCopyRow : IPgBinaryCopyRow;
}
