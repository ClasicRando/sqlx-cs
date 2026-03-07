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
    /// Execute a <c>COPY TO</c> query against the database and forward the fetched rows to the
    /// supplied <see cref="Stream"/>.
    /// </summary>
    /// <param name="copyOutStatement">COPY statement to execute for data extraction</param>
    /// <param name="stream">
    /// Stream to forward data returned from the <c>COPY TO</c> command
    /// </param>
    /// <param name="cancellationToken">Token to cancel the async operation</param>
    Task CopyOutAsync(
        ICopyTo copyOutStatement,
        Stream stream,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Execute a <c>COPY {Table} TO BINARY</c> statement and collect the results into the
    /// desired row type. This is possible because the copy binary format is the same as rows
    /// sent during regular query execution and is easily mapped to a row type. 
    /// </summary>
    /// <param name="copyOutStatement">Binary copy out statement to execute</param>
    /// <param name="cancellationToken">Token to cancel async operation</param>
    /// <typeparam name="TRow">Row type to decode to</typeparam>
    /// <returns>Stream of rows from the copy statement</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// If the copy statement is not <see cref="ICopyQuery"/> or <see cref="ICopyTable"/>
    /// </exception>
    IAsyncEnumerable<TRow> CopyOutRowsAsync<TRow>(
        TableToBinary copyOutStatement,
        CancellationToken cancellationToken = default)
        where TRow : IFromRow<IPgDataRow, TRow>;

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
