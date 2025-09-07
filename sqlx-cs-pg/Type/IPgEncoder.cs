using Sqlx.Core.Buffer;

namespace Sqlx.Postgres.Type;

public interface IPgEncoder<in T> where T : notnull
{
    static abstract void Encode(T value, WriteBuffer buffer);
}
