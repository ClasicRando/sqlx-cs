namespace Sqlx.Postgres.Type;

public interface IPgDbType<T> : IPgEncoder<T>, IPgDecoder<T> where T : notnull
{
    static abstract PgType DbType { get; }
    
    static abstract PgType ArrayDbType { get; }

    static abstract bool IsCompatible(PgType dbType);

    static abstract PgType GetActualType(T value);
}
