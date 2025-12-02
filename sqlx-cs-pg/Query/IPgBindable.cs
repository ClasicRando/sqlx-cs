using Sqlx.Core.Query;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Query;

public interface IPgBindable : IBindable
{
    IBindable BindPg<TValue, TType>(TValue value)
        where TType : IPgDbType<TValue>
        where TValue : notnull;
}

public static class PgBindable
{
    extension(IPgBindable bindable)
    {
        public IBindable BindPgNullableStruct<TValue, TType>(TValue? value)
            where TType : IPgDbType<TValue>
            where TValue : struct
        {
            return !value.HasValue
                ? bindable.BindNull<TType>()
                : bindable.BindPg<TValue, TType>(value.Value);
        }
        
        public IBindable BindPgNullableClass<TValue, TType>(TValue? value)
            where TType : IPgDbType<TValue>
            where TValue : class
        {
            return value is null
                ? bindable.BindNull<TType>()
                : bindable.BindPg<TValue, TType>(value);
        }
    }
}
