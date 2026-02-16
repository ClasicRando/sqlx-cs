using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Sqlx.Core.Exceptions;

internal static class ThrowHelper
{
    [StackTraceHidden]
    internal static void ThrowInvalidOperationExceptionIfNull<T>(
        [NotNull] T? value,
        [CallerArgumentExpression(nameof(value))] string name = "") where T : notnull
    {
        if (value is not null) return;
        
        throw new InvalidOperationException($"Expected value {name} to be non-null");
    }
}
