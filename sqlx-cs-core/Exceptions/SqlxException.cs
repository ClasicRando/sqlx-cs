using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Sqlx.Core.Exceptions;

/// <summary>
/// Base exception used in the SQLx library. 
/// </summary>
#pragma warning disable CA1032
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

    /// <summary>
    /// Throw a new <see cref="SqlxException"/> if the supplied value is null. If the method
    /// returns, then the value must be non-null.
    /// </summary>
    /// <param name="value">value to check</param>
    /// <param name="message">optional message to be used as the thrown exception's message</param>
    /// <param name="name">name of the variable specified to check by the caller</param>
    /// <typeparam name="T">type of the value to check</typeparam>
    /// <exception cref="SqlxException">if the value is null</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowIfNull<T>(
        [NotNull] T? value,
        string message = "",
        [CallerArgumentExpression(nameof(value))]
        string name = "")
        where T : notnull
    {
        if (value is null)
        {
            throw new SqlxException(
                string.IsNullOrWhiteSpace(message)
                    ? $"Expected value {name} to be non-null"
                    : message);
        }
    }
}
