using Sqlx.Core;
using Sqlx.Core.Pool;
using Sqlx.Core.Query;
using Sqlx.Core.Result;
using Sqlx.Postgres.Query;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Connection;

public partial class PgConnectionTest
{
    [Test]
    public async Task OpenAsync_Should_SucceedWithSaslAuth_When_DefaultAuth(CancellationToken ct)
    {
        using IPgConnection connection = DatabaseFixture.BasicPool.CreateConnection();
        await connection.OpenAsync(ct);
        await Assert.That(connection.Status).IsEqualTo(ConnectionStatus.Idle);
        using IPgExecutableQuery query = connection.CreateQuery("SELECT 1;");
        var rowsAffected = await query.ExecuteNonQueryAsync(ct);
        await Assert.That(rowsAffected).IsEqualTo(1);
    }

    [Test]
    public async Task CloseAsync_Should_Succeed_When_OpenConnection(CancellationToken ct)
    {
        using IPgConnection connection = DatabaseFixture.BasicPool.CreateConnection();
        await connection.OpenAsync(ct);
        await Assert.That(connection.Status).IsEqualTo(ConnectionStatus.Idle);
        await connection.CloseAsync(ct);
        await Assert.That(connection.Status).IsEqualTo(ConnectionStatus.Closed);
    }

    [Test]
    public async Task CloseAsync_Should_Succeed_When_ClosedConnection(CancellationToken ct)
    {
        using IPgConnection connection = DatabaseFixture.BasicPool.CreateConnection();
        await Assert.That(connection.Status).IsEqualTo(ConnectionStatus.Closed);
        await connection.CloseAsync(ct);
        await Assert.That(connection.Status).IsEqualTo(ConnectionStatus.Closed);
    }

    [Test]
    public async Task CloseAsync_Should_Fail_When_DisposedConnection(CancellationToken ct)
    {
        IPgConnection connection = DatabaseFixture.BasicPool.CreateConnection();
        await Assert.That(connection.Status).IsEqualTo(ConnectionStatus.Closed);
        connection.Dispose();
        await Assert.ThrowsAsync<ObjectDisposedException>(async () =>
            await connection.CloseAsync(ct));
    }

    [Test]
    public async Task CommitAsync_Should_SucceedAndIncrementTransactionId(CancellationToken ct)
    {
        using IPgConnection connection = DatabaseFixture.BasicPool.CreateConnection();
        await connection.OpenAsync(ct);
        await Assert.That(connection.Status).IsEqualTo(ConnectionStatus.Idle);
        var transactionIdStart = await GetConnectionTransactionId(connection, ct);
        await connection.BeginAsync(ct);
        var transactionIdBegin = await GetConnectionTransactionId(connection, ct);
        await Assert.That(transactionIdBegin).IsNotEqualTo(transactionIdStart);
        await Assert.That(transactionIdStart < transactionIdBegin).IsTrue();
        await connection.CommitAsync(ct);
        var transactionIdCommit = await GetConnectionTransactionId(connection, ct);
        await Assert.That(transactionIdCommit).IsNotEqualTo(transactionIdBegin);
        await Assert.That(transactionIdBegin < transactionIdCommit).IsTrue();
    }

    [Test]
    public async Task RollbackAsync_Should_SucceedAndIncrementTransactionId(CancellationToken ct)
    {
        using IPgConnection connection = DatabaseFixture.BasicPool.CreateConnection();
        await connection.OpenAsync(ct);
        await Assert.That(connection.Status).IsEqualTo(ConnectionStatus.Idle);
        var transactionIdStart = await GetConnectionTransactionId(connection, ct);
        await connection.BeginAsync(ct);
        var transactionIdBegin = await GetConnectionTransactionId(connection, ct);
        await Assert.That(transactionIdBegin).IsNotEqualTo(transactionIdStart);
        await Assert.That(transactionIdStart < transactionIdBegin).IsTrue();
        await connection.RollbackAsync(ct);
        var transactionIdRollback = await GetConnectionTransactionId(connection, ct);
        await Assert.That(transactionIdRollback).IsNotEqualTo(transactionIdBegin);
        await Assert.That(transactionIdBegin < transactionIdRollback).IsTrue();
    }

    private static async Task<long> GetConnectionTransactionId(IPgConnection connection, CancellationToken ct)
    {
        using IPgExecutableQuery query = connection.CreateQuery("SELECT txid_current();");
        return await query.ExecuteAsync(ct)
            .OfType<Either<IPgDataRow, QueryResult>.Left>()
            .Select(row => row.Value.GetLongNotNull(0))
            .FirstAsync(ct);
    }
}
