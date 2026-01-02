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
    IConnection<IPgExecutableQuery, IPgBindable, IPgQueryBatch, IPgDataRow>, IPgQueryExecutor
{
    /// <summary>
    /// Executes the <c>COPY TO</c> command and returns a stream of row data as text, CSV or binary.
    /// Generally this is not the method you want to use since it requires knowing each row type and
    /// using raw bytes. Prefer using other copy out methods:
    /// <list type="bullet">
    ///     <item>
    ///     Copy results to stream ->
    ///     <see cref="PgConnectionExtensions.CopyOut(IPgConnection, ICopyTo, Stream, CancellationToken)"/>
    ///     </item>
    ///     <item>
    ///     Copy results to file path ->
    ///     <see cref="PgConnectionExtensions.CopyOut(IPgConnection, ICopyTo, string, FileMode, CancellationToken)"/>
    ///     </item>
    ///     <item>
    ///     Parse results as rows ->
    ///     <see cref="PgConnectionExtensions.CopyOutRows{TStatement, TRow}(IPgConnection, TStatement, CancellationToken)"/>
    ///     </item>
    /// </list>
    /// <a href="https://www.postgresql.org/docs/current/sql-copy.html"> postgres docs</a>
    /// </summary>
    /// <param name="copyOutStatement">COPY statement to execute for data extraction</param>
    /// <param name="cancellationToken">Token to cancel the async operation</param>
    /// <returns>A stream of rows returned as a result of the copy statement</returns>
    Task<IAsyncEnumerable<byte[]>> CopyOut(
        ICopyTo copyOutStatement,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes the <c>COPY FROM</c> command with the supplied <paramref name="data"/> and returns
    /// a <see cref="QueryResult"/> indicating the outcome. Generally this is not the method you
    /// want to use since it requires using raw bytes of the expected data type. Prefer using other
    /// copy in methods:
    /// <list type="bullet">
    ///     <item>
    ///     Copy results to stream ->
    ///     <see cref="PgConnectionExtensions.CopyOut(IPgConnection, ICopyTo, Stream, CancellationToken)"/>
    ///     </item>
    ///     <item>
    ///     Copy results to file path ->
    ///     <see cref="PgConnectionExtensions.CopyOut(IPgConnection, ICopyTo, string, FileMode, CancellationToken)"/>
    ///     </item>
    ///     <item>
    ///     Parse results as rows ->
    ///     <see cref="PgConnectionExtensions.CopyOutRows{TStatement, TRow}(IPgConnection, TStatement, CancellationToken)"/>
    ///     </item>
    /// </list>
    /// <a href="https://www.postgresql.org/docs/current/sql-copy.html"> postgres docs</a>
    /// </summary>
    /// <param name="copyInStatement">COPY statement to execute for data ingestion</param>
    /// <param name="data">
    /// Reader of a pipe that provides the source data to send to the database
    /// </param>
    /// <param name="cancellationToken">Token to cancel the async operation</param>
    /// <returns>Query result object with details on what happened during the execution</returns>
    Task<QueryResult> CopyIn(
        ICopyFrom copyInStatement,
        PipeReader data,
        CancellationToken cancellationToken = default);
}
