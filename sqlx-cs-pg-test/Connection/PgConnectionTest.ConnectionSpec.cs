using Sqlx.Core;
using Sqlx.Core.Connection;
using Sqlx.Core.Pool;
using Sqlx.Core.Query;
using Sqlx.Core.Result;

namespace Sqlx.Postgres.Connection;

public partial class PgConnectionTest
{
    [Fact]
    public async Task OpenAsync_Should_SucceedWithSaslAuth_When_DefaultAuth()
    {
        await using IConnection connection = _databaseFixture.BasicPool.CreateConnection();
        await connection.OpenAsync(TestContext.Current.CancellationToken);
        Assert.Equal(ConnectionStatus.Idle, connection.Status);
        using IExecutableQuery query = connection.CreateQuery("SELECT 1;");
        var rowsAffected = await query.ExecuteNonQuery(TestContext.Current.CancellationToken);
        Assert.Equal(1, rowsAffected);
    }

    [Fact]
    public async Task CloseAsync_Should_Succeed_When_OpenConnection()
    {
        await using IConnection connection = _databaseFixture.BasicPool.CreateConnection();
        await connection.OpenAsync(TestContext.Current.CancellationToken);
        Assert.Equal(ConnectionStatus.Idle, connection.Status);
        await connection.CloseAsync(TestContext.Current.CancellationToken);
        Assert.Equal(ConnectionStatus.Closed, connection.Status);
    }

    [Fact]
    public async Task CloseAsync_Should_Succeed_When_ClosedConnection()
    {
        await using IConnection connection = _databaseFixture.BasicPool.CreateConnection();
        Assert.Equal(ConnectionStatus.Closed, connection.Status);
        await connection.CloseAsync(TestContext.Current.CancellationToken);
        Assert.Equal(ConnectionStatus.Closed, connection.Status);
    }

    [Fact]
    public async Task CloseAsync_Should_Fail_When_DisposedConnection()
    {
        IConnection connection = _databaseFixture.BasicPool.CreateConnection();
        Assert.Equal(ConnectionStatus.Closed, connection.Status);
        await connection.DisposeAsync();
        await Assert.ThrowsAsync<ObjectDisposedException>(async () =>
            await connection.CloseAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task CommitAsync_Should_SucceedAndIncrementTransactionId()
    {
        await using IConnection connection = _databaseFixture.BasicPool.CreateConnection();
        await connection.OpenAsync(TestContext.Current.CancellationToken);
        Assert.Equal(ConnectionStatus.Idle, connection.Status);
        var transactionIdStart = await GetConnectionTransactionId(connection);
        await connection.BeginAsync(TestContext.Current.CancellationToken);
        var transactionIdBegin = await GetConnectionTransactionId(connection);
        Assert.NotEqual(transactionIdStart, transactionIdBegin);
        Assert.True(transactionIdStart < transactionIdBegin);
        await connection.CommitAsync(TestContext.Current.CancellationToken);
        var transactionIdCommit = await GetConnectionTransactionId(connection);
        Assert.NotEqual(transactionIdBegin, transactionIdCommit);
        Assert.True(transactionIdBegin < transactionIdCommit);
    }

    [Fact]
    public async Task RollbackAsync_Should_SucceedAndIncrementTransactionId()
    {
        await using IConnection connection = _databaseFixture.BasicPool.CreateConnection();
        await connection.OpenAsync(TestContext.Current.CancellationToken);
        Assert.Equal(ConnectionStatus.Idle, connection.Status);
        var transactionIdStart = await GetConnectionTransactionId(connection);
        await connection.BeginAsync(TestContext.Current.CancellationToken);
        var transactionIdBegin = await GetConnectionTransactionId(connection);
        Assert.NotEqual(transactionIdStart, transactionIdBegin);
        Assert.True(transactionIdStart < transactionIdBegin);
        await connection.RollbackAsync(TestContext.Current.CancellationToken);
        var transactionIdRollback = await GetConnectionTransactionId(connection);
        Assert.NotEqual(transactionIdBegin, transactionIdRollback);
        Assert.True(transactionIdBegin < transactionIdRollback);
    }

    private static async Task<long> GetConnectionTransactionId(IConnection connection)
    {
        using IExecutableQuery query = connection.CreateQuery("SELECT txid_current();");
        var rows = await query.Execute(TestContext.Current.CancellationToken);
        return await rows.Where(result => result is Either<IDataRow, QueryResult>.Left)
            .Select(result => (Either<IDataRow, QueryResult>.Left)result)
            .Select(row => row.Value.GetLongNotNull(0))
            .FirstAsync();
    }
}
