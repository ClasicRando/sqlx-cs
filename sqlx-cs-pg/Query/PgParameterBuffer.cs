using System.Text.Json.Serialization.Metadata;
using Sqlx.Core;
using Sqlx.Core.Buffer;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Query;

internal sealed class PgParameterBuffer : IDisposable
{
    private readonly WriteBuffer _buffer = new();
    private readonly List<PgType> _pgTypes = [];

    public short ParameterCount => (short)_pgTypes.Count;
    public ReadOnlyMemory<byte> Memory => _buffer.ReadableMemory;
    public IReadOnlyList<PgType> PgTypes => _pgTypes;

    public void EncodeNull()
    {
        _buffer.WriteInt(-1);
        _pgTypes.Add(PgType.Unspecified);
    }

    public void EncodeValue<TValue, TPgType>(TValue value)
        where TValue : notnull
        where TPgType : IPgDbType<TValue>
    {
        _buffer.WriteLengthPrefixed(false, buf => TPgType.Encode(value, buf));
        _pgTypes.Add(TPgType.DbType);
    }

    public void EncodeBytes(ReadOnlySpan<byte> bytes)
    {
        _buffer.WriteInt(bytes.Length);
        _buffer.WriteBytes(bytes);
        _pgTypes.Add(PgBytea.DbType);
    }

    public void EncodeChars(ReadOnlySpan<char> chars)
    {
        _buffer.WriteInt(Charsets.Default.GetByteCount(chars));
        _buffer.WriteString(chars);
        _pgTypes.Add(PgString.DbType);
    }

    public void EncodeJsonValue<T>(T value, JsonTypeInfo<T>? typeInfo) where T : notnull
    {
        _buffer.WriteLengthPrefixed(false, buf => PgJson<T>.Encode(value, buf, typeInfo));
        _pgTypes.Add(PgJson<T>.DbType);
    }

    public void Dispose()
    {
        _buffer.Dispose();
    }
}
