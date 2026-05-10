using System.Runtime.CompilerServices;

namespace Sqlx.Core.Query;

public static class Bindable
{
    extension(IBindable bindable)
    {
        /// <summary>
        /// Wrapper method for specifying a parameter that is intended to be an <c>OUT</c> only
        /// parameter in a stored procedure call. This is equivalent to
        /// <see cref="IBindable.BindNull"/> since an <c>OUT</c> parameter always has an input value
        /// of <c>NULL</c>. Use this method to indicate that the parameter's output will be captured
        /// in the query result.
        /// </summary>
        /// <typeparam name="T">
        /// <c>OUT</c> parameter's CLR type to hint the driver as to the parameter's expected type.
        /// Drivers may or may not use this type to inform query preparing.
        /// </typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void BindOutParameter<T>() where T : notnull
        {
            bindable.BindNull<T>();
        }
    }
}
