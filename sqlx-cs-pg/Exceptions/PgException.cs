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
        return value is not TSub sub
            ? throw new PgException(
                $"Expected value of type {typeof(TSuper)} to be {typeof(TSub)} but it was actually {value.GetType()}")
            : sub;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static SqlxException EnumOutOfRange<TEnum>(TEnum enumValue) where TEnum : Enum
    {
        return new SqlxException(
            $"Expected enum value of {typeof(TEnum)} to be within range but found {enumValue}");
    }
}
