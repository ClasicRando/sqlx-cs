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
        /// Bind a boolean value. The <c>BOOLEAN</c> type is not consistent across all databases so the
        /// driver specific implementation might vary.
        /// </summary>
        /// <param name="value">Boolean value</param>
        public void Bind(bool? value)
        {
            if (value.HasValue)
            {
                bindable.Bind(value.Value);
            }
            else
            {
                bindable.BindNull<bool>();
            }
        }

        /// <summary>
        /// Bind a sbyte value. This maps to the <c>TINYINT</c> type but can vary between database
        /// implementations since not all use <c>TINYINT</c>
        /// </summary>
        /// <param name="value">Sbyte value</param>
        public void Bind(sbyte? value)
        {
            if (value.HasValue)
            {
                bindable.Bind(value.Value);
            }
            else
            {
                bindable.BindNull<sbyte>();
            }
        }

        /// <summary>
        /// Bind a short value. This maps to the <c>SMALLINT</c> type.
        /// </summary>
        /// <param name="value">Short value</param>
        public void Bind(short? value)
        {
            if (value.HasValue)
            {
                bindable.Bind(value.Value);
            }
            else
            {
                bindable.BindNull<short>();
            }
        }

        /// <summary>
        /// Bind an int value. This maps to the <c>INTEGER</c> type.
        /// </summary>
        /// <param name="value">Int value</param>
        public void Bind(int? value)
        {
            if (value.HasValue)
            {
                bindable.Bind(value.Value);
            }
            else
            {
                bindable.BindNull<int>();
            }
        }

        /// <summary>
        /// Bind a long value. This maps to the <c>BIGINT</c> type.
        /// </summary>
        /// <param name="value">Long value</param>
        public void Bind(long? value)
        {
            if (value.HasValue)
            {
                bindable.Bind(value.Value);
            }
            else
            {
                bindable.BindNull<long>();
            }
        }

        /// <summary>
        /// Bind a float value. This maps to the <c>REAL</c> type.
        /// </summary>
        /// <param name="value">Float value</param>
        public void Bind(float? value)
        {
            if (value.HasValue)
            {
                bindable.Bind(value.Value);
            }
            else
            {
                bindable.BindNull<float>();
            }
        }

        /// <summary>
        /// Bind a double value. This maps to the <c>DOUBLE PRECISION</c> type.
        /// </summary>
        /// <param name="value">Double value</param>
        public void Bind(double? value)
        {
            if (value.HasValue)
            {
                bindable.Bind(value.Value);
            }
            else
            {
                bindable.BindNull<double>();
            }
        }

        /// <summary>
        /// Bind a TimeOnly value. This maps to the <c>TIME</c> type.
        /// </summary>
        /// <param name="value">TimeOnly value</param>
        public void Bind(TimeOnly? value)
        {
            if (value.HasValue)
            {
                bindable.Bind(value.Value);
            }
            else
            {
                bindable.BindNull<TimeOnly>();
            }
        }

        /// <summary>
        /// Bind a DateOnly value. This maps to the <c>DATE</c> type.
        /// </summary>
        /// <param name="value">DateOnly value</param>
        public void Bind(DateOnly? value)
        {
            if (value.HasValue)
            {
                bindable.Bind(value.Value);
            }
            else
            {
                bindable.BindNull<DateOnly>();
            }
        }

        /// <summary>
        /// Bind a DateTime value. This maps to the <c>TIMESTAMP</c> type.
        /// </summary>
        /// <param name="value">DateTime value</param>
        public void Bind(DateTime? value)
        {
            if (value.HasValue)
            {
                bindable.Bind(value.Value);
            }
            else
            {
                bindable.BindNull<DateTime>();
            }
        }

        /// <summary>
        /// Bind a DateTimeOffset value. This maps to the <c>TIMESTAMP WITH TIME ZONE</c> type.
        /// </summary>
        /// <param name="value">DateTimeOffset value</param>
        public void Bind(DateTimeOffset? value)
        {
            if (value.HasValue)
            {
                bindable.Bind(value.Value);
            }
            else
            {
                bindable.BindNull<DateTimeOffset>();
            }
        }

        /// <summary>
        /// Bind a decimal value. This maps to the <c>DECIMAL</c>/<c>NUMERIC</c> type.
        /// </summary>
        /// <param name="value">Decimal value</param>
        public void Bind(decimal? value)
        {
            if (value.HasValue)
            {
                bindable.Bind(value.Value);
            }
            else
            {
                bindable.BindNull<decimal>();
            }
        }

        /// <summary>
        /// Bind a Guid value. The <c>UUID</c>/<c>UNIQUEIDENTIFIER</c> type is not consistent across all
        /// databases so the driver specific implementation might vary. Generally it's either a built-in
        /// type or this method tries to interpret a <see cref="Guid"/> as bytes or a string.
        /// </summary>
        /// <param name="value">Guid value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Bind(Guid? value)
        {
            if (value.HasValue)
            {
                bindable.Bind(value.Value);
            }
            else
            {
                bindable.BindNull<Guid>();
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
