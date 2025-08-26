using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

public interface IPgDecoder<out T> where T : notnull
{
    // public static abstract T Decode(PgValue value);

    public static abstract T DecodeBytes(PgBinaryValue value);

    public static abstract T DecodeText(PgTextValue value);
}
