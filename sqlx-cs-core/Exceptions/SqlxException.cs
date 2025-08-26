using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Sqlx.Core.Exceptions;

public class SqlxException : Exception
{
    public SqlxException(string message, Exception? exception = null) : base(message, exception)
    {
    }

    public SqlxException(Exception exception) : base(null, exception)
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowIfNull<T>(
        [NotNull] T? value,
        string message = "",
        [CallerArgumentExpression(nameof(value))]
        string name = "") where T : notnull
    {
        if (value is null)
        {
            throw new SqlxException(
                string.IsNullOrWhiteSpace(message)
                    ? $"Expected value {name} to be non-null"
                    : message);
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static SqlxException EnumOutOfRange<TEnum>(TEnum enumValue) where TEnum : Enum
    {
        return new SqlxException(
            $"Expected enum value of {typeof(TEnum)} to be within range but found {enumValue}");
    }
}
