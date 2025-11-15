using Sqlx.Core.Result;

namespace Sqlx.Core.Query;

/// <summary>
/// <para>
/// Implementors represent 1 or more queries to be executed against the database within a single
/// execution (i.e. pipelined when sent to the database). This can be much more performant when
/// compared to executing each query 1 at a time since the client does not need to wait for a
/// response indicating the completion of the query before sending another. The execution flow would
/// then look like:
/// </para>
/// <para>
/// Sequential Execution
/// <list type="table">
///     <listheader>
///         <term>Client</term>
///         <description>Server</description>
///     </listheader>
///     <item>
///         <term>Send Query 1</term>
///         <description></description>
///     </item>
///     <item>
///         <term></term>
///         <description>Process Query 1</description>
///     </item>
///     <item>
///         <term>Receive Query 1 Result</term>
///         <description></description>
///     </item>
///     <item>
///         <term>Send Query 2</term>
///         <description></description>
///     </item>
///     <item>
///         <term></term>
///         <description>Process Query 2</description>
///     </item>
///     <item>
///         <term>Receive Query 2 Result</term>
///         <description></description>
///     </item>
///     <item>
///         <term>Send Query 3</term>
///         <description></description>
///     </item>
///     <item>
///         <term></term>
///         <description>Process Query 3</description>
///     </item>
///     <item>
///         <term>Receive Query 3 Result</term>
///         <description></description>
///     </item>
/// </list>
/// </para>
/// <para>
/// Batch Execution
/// <list type="table">
///     <listheader>
///         <term>Client</term>
///         <description>Server</description>
///     </listheader>
///     <item>
///         <term>Send Query 1</term>
///         <description></description>
///     </item>
///     <item>
///         <term>Send Query 2</term>
///         <description>Process Query 1</description>
///     </item>
///     <item>
///         <term>Send Query 3</term>
///         <description>Process Query 2</description>
///     </item>
///     <item>
///         <term>Receive Query 1 Result</term>
///         <description>Process Query 3</description>
///     </item>
///     <item>
///         <term>Receive Query 2 Result</term>
///         <description></description>
///     </item>
///     <item>
///         <term>Receive Query 3 Result</term>
///         <description></description>
///     </item>
/// </list>
/// </para>
/// <para>
/// To Use this API, create the batch using
/// <see cref="Sqlx.Core.Query.IQueryExecutor.CreateQueryBatch"/> and create each query in the
/// batch using <see cref="CreateQuery"/>. Each query will be saved within this batch before
/// executing the entire batch.
/// <code>
/// const string InsertStatement = "INSERT INTO table(column_1, column_2) VALUES (?,?);";
/// IConnection connection = // Create connection instance
/// IQueryBatch batch = connection.CreateQueryBatch();
/// batch.CreateQuery(InsertStatement)
///     .Bind(1)
///     .Bind("Name 1");
/// batch.CreateQuery(InsertStatement)
///     .Bind(2)
///     .Bind("Name 2");
/// long affectedRows = await batch.ExecuteNonQuery();
/// Console.WriteLine($"Inserted {affectedRows} rows");
/// </code>
/// </para>
/// <para>
/// Another way to improve performance of batching queries is to wrap the execution of the batch
/// in a transaction. This can be done by setting <see cref="WrapBatchInTransaction"/> to true. When
/// this option is set, before executing the batch the connection will enter a transaction and close
/// that transaction when completing the query (committing if successful, rolling back if an error
/// occurs). However, this option will do nothing if the underlining connection is already in a
/// transaction and the existing transaction will not be closed when completing the batch.
/// </para>
/// </summary>
public interface IQueryBatch : IDisposable
{
    bool WrapBatchInTransaction { get; set; }

    /// <summary>
    /// Add a new <see cref="IQuery"/> to this batch and allow for the caller to bind parameters as
    /// needed to the query.
    /// </summary>
    /// <param name="sql">Query to execute</param>
    /// <returns>Query to bind parameters to (if needed)</returns>
    IQuery CreateQuery(string sql);

    /// <summary>
    /// Execute the query batch and yield a stream of <see cref="IDataRow"/>s and
    /// <see cref="QueryResult"/>s.
    /// </summary>
    /// <returns>Stream of <see cref="IDataRow"/>s and <see cref="QueryResult"/>s</returns>
    Task<IAsyncEnumerable<Either<IDataRow, QueryResult>>> ExecuteBatch(
        CancellationToken cancellationToken);
}
