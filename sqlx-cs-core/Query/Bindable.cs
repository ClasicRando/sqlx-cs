using System.Runtime.CompilerServices;
using System.Text.Json.Serialization.Metadata;

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

        /// <summary>
        /// Bind a <typeparamref name="T"/> value as a JSON. Some databases have a JSON specific
        /// field type but other database drivers will treat the JSON encoding as string or bytes.
        /// When using this method, it's recommended to supply the <see cref="JsonTypeInfo"/>
        /// parameter to aid serialization.
        /// </summary>
        /// <param name="value">Value to encode as JSON</param>
        /// <param name="typeInfo">Optional type metadata for JSON serialization</param>
        /// <typeparam name="T">CLR type to encode as JSON</typeparam>
        public void BindJsonRef<T>(T? value, JsonTypeInfo<T>? typeInfo = null) where T : class
        {
            if (value is null)
            {
                bindable.BindNull<T>();
            }
            else
            {
                bindable.BindJson(value, typeInfo);
            }
        }

        /// <summary>
        /// Bind a <typeparamref name="T"/> value as a JSON. Some databases have a JSON specific
        /// field type but other database drivers will treat the JSON encoding as string or bytes.
        /// When using this method, it's recommended to supply the <see cref="JsonTypeInfo"/>
        /// parameter to aid serialization.
        /// </summary>
        /// <param name="value">Value to encode as JSON</param>
        /// <param name="typeInfo">Optional type metadata for JSON serialization</param>
        /// <typeparam name="T">CLR type to encode as JSON</typeparam>
        public void BindJsonVal<T>(T? value, JsonTypeInfo<T>? typeInfo = null) where T : struct
        {
            if (!value.HasValue)
            {
                bindable.BindNull<T>();
            }
            else
            {
                bindable.BindJson(value.Value, typeInfo);
            }
        }
    }
}
