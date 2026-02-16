namespace Sqlx.Core.Exceptions;

/// <summary>
/// Base exception used in the SQLx library. 
/// </summary>
public class SqlxException : Exception
{
    public SqlxException(string message) : base(message)
    {
    }
    
    public SqlxException(string message, Exception? exception) : base(message, exception)
    {
    }

    /// <summary>
    /// Wrap inner <see cref="Exception"/> as a new <see cref="SqlxException"/>
    /// </summary>
    /// <param name="exception">cause of the exception</param>
    public SqlxException(Exception exception) : base(null, exception)
    {
    }
}
