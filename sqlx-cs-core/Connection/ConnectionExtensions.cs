using Sqlx.Core.Exceptions;

namespace Sqlx.Core.Connection;

public static class ConnectionExtensions
{
    /// <summary>
    /// Cast this opaque connection as the desired type (if possible)
    /// </summary>
    /// <typeparam name="TConnection">desired connection output type</typeparam>
    /// <returns>this connection cast to the required output type</returns>
    /// <exception cref="SqlxException">
    /// if the underlining connection is not of the desired output type
    /// </exception>
    public static TConnection Unwrap<TConnection>(this IConnection connection)
        where TConnection : IConnection
    {
        if (connection is TConnection result)
        {
            return result;
        }

        throw new SqlxException(
            $"Could not unwrap a {connection.GetType()} as {typeof(TConnection)}");
    }
}
