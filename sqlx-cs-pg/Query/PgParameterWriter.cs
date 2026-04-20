using System.Buffers;
using System.Text.Json.Serialization.Metadata;
using Sqlx.Core;
using Sqlx.Core.Buffer;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Query;

/// <summary>
/// Buffer writer for binding binary encoded parameters to an underlining
/// <see cref="IBufferWriter{byte}"/>. All bind operations are written to the buffer with an
/// <see cref="int"/> length prefix and the type's <see cref="PgTypeInfo"/> added to the list of
/// types.
/// </summary>
internal sealed class PgParameterWriter : IPgBindable
{
    private readonly ArrayBufferWriter _buffer;
    private readonly List<PgTypeInfo> _pgTypes = [];

    public PgParameterWriter(ArrayBufferWriter buffer)
    {
        _buffer = buffer;
    }

    /// <summary>
    /// Number of parameters encoded
    /// </summary>
    public short ParameterCount => (short)_pgTypes.Count;

    /// <summary>
    /// <see cref="PgTypeInfo"/>s encoded into this buffer (in order of encoding)
    /// </summary>
    public IReadOnlyList<PgTypeInfo> PgTypes => _pgTypes;

    public void Bind(in ReadOnlySpan<byte> value)
    {
        _buffer.WriteInt(value.Length);
        _buffer.Write(value);
        _pgTypes.Add(PgBytea.DbType);
    }

    public void Bind(in ReadOnlySpan<char> value)
    {
        var byteLength = Charsets.Default.GetByteCount(value);
        _buffer.WriteInt(byteLength);
        var span = _buffer.GetSpan(byteLength);
        Charsets.Default.GetBytes(value, span);
        _buffer.Advance(byteLength);
        _pgTypes.Add(PgString.DbType);
    }

    public void BindJson<T>(T value, JsonTypeInfo<T>? typeInfo = null) where T : notnull
    {
        var startLocation = _buffer.StartWritingLengthPrefixed();
        PgJson<T>.Encode(value, _buffer);
        _buffer.FinishWritingLengthPrefixed(startLocation, includeLength: false);
        _pgTypes.Add(PgJson<T>.DbType);
    }

    public void BindNull<T>() where T : notnull
    {
        _buffer.WriteInt(-1);
        _pgTypes.Add(PgTypeInfo.Unspecified);
    }

    public void BindPg<TValue, TType>(TValue value)
        where TValue : notnull where TType : IPgDbType<TValue>
    {
        var startLocation = _buffer.StartWritingLengthPrefixed();
        TType.Encode(value, _buffer);
        _buffer.FinishWritingLengthPrefixed(startLocation, includeLength: false);
        _pgTypes.Add(TType.DbType);
    }

    public void Reset()
    {
        _pgTypes.Clear();
        _buffer.Clear();
    }

    public void Dispose()
    {
    }
}
