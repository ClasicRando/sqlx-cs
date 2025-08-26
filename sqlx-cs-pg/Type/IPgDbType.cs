namespace Sqlx.Postgres.Type;

public interface IPgDbType<T> : IPgEncoder<T>, IPgDecoder<T> where T : notnull
{
    public static abstract PgType DbType { get; }

    public static abstract bool IsCompatible(PgType dbType);

    public static abstract PgType GetActualType(T value);
}
