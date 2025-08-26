using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Message.Backend;

namespace Sqlx.Postgres.Exceptions;

public class PgException : SqlxException
{
    internal PgException(string message, Exception exception) : base(message, exception) {}
    
    internal PgException(string message) : base(message) {}

    internal PgException(Exception exception) : base(exception) {}
    
    internal PgException(ErrorResponseMessage errorResponse) : this($"General Postgresql Error:\n{errorResponse.InformationResponse}") {}

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowIfNull<T>([NotNull] T? value, [CallerArgumentExpression(nameof(value))] string name = "") where T : notnull
    {
        if (value is null)
        {
            throw new PgException($"Expected value {name} to be non-null");
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static TSub CheckIfIs<TSuper, TSub>(TSuper value)
        where TSuper : notnull
        where TSub : TSuper
    {
        if (value is not TSub sub)
        {
            throw new PgException($"Expected value of type {typeof(TSuper)} to be {typeof(TSub)} but it was actually {value.GetType()}");
        }

        return sub;
    }
}
