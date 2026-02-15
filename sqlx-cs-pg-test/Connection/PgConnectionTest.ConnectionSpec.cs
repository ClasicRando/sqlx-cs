using Sqlx.Core.Pool;
using Sqlx.Core.Query;
using Sqlx.Postgres.Query;

namespace Sqlx.Postgres.Connection;

public partial class PgConnectionTest
{
    [Test]
    public async Task OpenAsync_Should_SucceedWithSaslAuth_When_DefaultAuth(CancellationToken ct)
    {
        await using IPgConnection connection = DatabaseFixture.BasicPool.CreateConnection();
        await connection.OpenAsync(ct);
        await Assert.That(connection.Status).IsEqualTo(ConnectionStatus.Idle);
        using IPgExecutableQuery query = connection.CreateQuery("SELECT 1;");
        var rowsAffected = await query.ExecuteNonQueryAsync(ct);
        await Assert.That(rowsAffected).IsEqualTo(1);
    }

    [Test]
    public async Task CommitAsync_Should_SucceedAndIncrementTransactionId(CancellationToken ct)
    {
        await using IPgConnection connection = DatabaseFixture.BasicPool.CreateConnection();
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
        await using IPgConnection connection = DatabaseFixture.BasicPool.CreateConnection();
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
        using var queryResult = await query.ExecuteAsync(ct);
        if (!await queryResult.MoveNextAsync(ct))
        {
            throw new InvalidOperationException("Found not transaction ID");
        }

        var current = queryResult.Current;
        return !current.IsLeft
            ? throw new InvalidOperationException("No rows found")
            : current.Left.GetLongNotNull(0);
    }
}
