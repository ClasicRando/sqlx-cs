using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

public interface IPgDecoder<out T> where T : notnull
{
    static abstract T DecodeBytes(PgBinaryValue value);

    static abstract T DecodeText(PgTextValue value);
}
