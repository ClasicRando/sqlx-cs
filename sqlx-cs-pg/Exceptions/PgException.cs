using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Message.Backend;

namespace Sqlx.Postgres.Exceptions;

/// <summary>
/// Specialized <see cref="SqlxException"/> for Postgres related errors.
/// </summary>
public class PgException : SqlxException
{
    internal PgException(string message, Exception exception) : base(message, exception)
    {
    }

    internal PgException(string message) : base(message)
    {
    }

    internal PgException(ErrorResponseMessage errorResponse) : this(
        $"General Postgresql Error:\n{errorResponse.InformationResponse}")
    {
    }

    [StackTraceHidden]
    internal static void ThrowIfNull<T>(
        [NotNull] T? value,
        [CallerArgumentExpression(nameof(value))] string name = "") where T : notnull
    {
        if (value is not null) return;

        throw new PgException($"Expected value {name} to be non-null");
    }
}
