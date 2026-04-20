using Sqlx.Core.Pool;
using Sqlx.Postgres.Connection;
using Sqlx.Postgres.Notify;
using Sqlx.Postgres.Query;
using Sqlx.Postgres.Result;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Pool;

/// <summary>
/// Postgres connection pool. Exposes the postgres specific connection options for each physical
/// database connection as well as other postgres specific features.
/// </summary>
public interface IPgConnectionPool : IConnectionPool<IPgConnection, IPgBindable, IPgExecutableQuery,
    IPgQueryBatch, IPgDataRow>
{
    /// <summary>
    /// Connection options used to create each connection in the pool
    /// </summary>
    PgConnectOptions ConnectOptions { get; }

    /// <summary>
    /// Create a listener over a postgres connection attached to this pool. The connection will be
    /// in use until the listener is disposed of. This means if you call
    /// <see cref="IPgListener.ReceiveNotificationsAsync"/>, the connection will be in use until the
    /// enumeration is cancelled which may impact your pool. Generally if your application is using
    /// that method you should create a separate pool for listeners.
    /// </summary>
    /// <returns>A listener using a connection from this pool to listen for notifications</returns>
    IPgListener CreateListener();

    /// <summary>
    /// <para>
    /// Map the specified <see cref="Enum"/> type as a Postgres enum type to enable encoding and
    /// decoding the type during database operations involving this pool.
    /// </para>
    /// <para>
    /// This CLR type must implement <see cref="IPgDbType{T}"/> to provide type specific information
    /// and allow for updating the type's definition with database specific details (i.e. the type's
    /// OID specific to the database).
    /// </para>
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the async operation</param>
    /// <typeparam name="TEnum">
    /// User defined enum that represents a Postgres enum UDT or simple integer wrapper
    /// </typeparam>
    /// <typeparam name="TType">Wrapper type definition to add behaviour to enums</typeparam>
    ValueTask MapEnumAsync<TEnum, TType>(CancellationToken cancellationToken = default)
        where TEnum : Enum
        where TType : IPgUdt<TEnum>;

    /// <summary>
    /// <para>
    /// Map the specified type as a Postgres composite type to enable encoding and decoding the type
    /// during database operations involving this pool.
    /// </para>
    /// <para>
    /// This CLR type must implement <see cref="IPgDbType{T}"/> to provide type specific information
    /// and allow for updating the type's definition with database specific details (i.e. the type's
    /// OID specific to the database).
    /// </para>
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the async operation</param>
    /// <typeparam name="TComposite">
    /// User defined type that represents a Postgres composite UDT
    /// </typeparam>
    Task MapCompositeAsync<TComposite>(CancellationToken cancellationToken = default)
        where TComposite : IPgUdt<TComposite>;
}
