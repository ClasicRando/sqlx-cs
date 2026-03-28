using System.Buffers;
using System.Collections.Immutable;
using System.Text.Json.Serialization.Metadata;
using Sqlx.Core.Buffer;
using Sqlx.Core.Query;
using Sqlx.Postgres.Exceptions;
using Sqlx.Postgres.Pool;
using Sqlx.Postgres.Query;

namespace Sqlx.Postgres.Type;

/// <summary>
/// Encoder for Postgres composite types where values are encoded like structs. Note that when
/// encoding attributes you must follow the exact ordering and types of the attributes found in the
/// Postgres type definition.
/// <example>
/// For this type definition:
/// <code>
/// CREATE TYPE example AS (id integer, name text);
/// </code>
/// You would write this type:
/// <code>
/// public record Example(int Id, string Name) : IPgUdt&lt;Example&gt;, IBindMany&lt;IPgBindable&gt;
/// {
///     public static void Encode(T value, IBufferWriter&lt;byte&gt; buffer)
///     {
///         PgRecordEncoder.EncodeRecord(value, buffer);
///     }
///
///     public void BindMany(IPgBindable bindable)
///     {
///         bindable.Bind(Id);
///         bindable.Bind(Name);
///     }
///
///     // Other IPgUdt methods and properties
/// }
/// </code>
/// </example>
/// </summary>
public sealed class PgRecordEncoder : IPgBindable
{
    private readonly ImmutableArray<CompositeField> _compositeFields;
    private readonly ArrayBufferWriter _buffer;
    private readonly PgParameterWriter _parameterWriter;

    private ReadOnlySpan<byte> Data => _buffer.ReadableSpan;

    private PgRecordEncoder(PgTypeInfo typeInfo)
    {
        ArgumentNullException.ThrowIfNull(typeInfo);
        _buffer = new ArrayBufferWriter();
        _parameterWriter = new PgParameterWriter(_buffer);
        if (typeInfo.TypeKind is not CompositeType compositeType)
        {
            throw new PgException(
                $"Attempted to encode a type using a {nameof(PgRecordEncoder)} but that type if not a composite or the composite type was not mapped to the connection pool using {nameof(PgConnectionPool.MapCompositeAsync)}");
        }

        _compositeFields = compositeType.Fields;
        _buffer.WriteInt(_compositeFields.Length);
    }

    public void Bind(bool value)
    {
        BindPg<bool, PgBool>(value);
    }

    public void Bind(sbyte value)
    {
        BindPg<sbyte, PgChar>(value);
    }

    public void Bind(short value)
    {
        BindPg<short, PgShort>(value);
    }

    public void Bind(int value)
    {
        BindPg<int, PgInt>(value);
    }

    public void Bind(long value)
    {
        BindPg<long, PgLong>(value);
    }

    public void Bind(float value)
    {
        BindPg<float, PgFloat>(value);
    }

    public void Bind(double value)
    {
        BindPg<double, PgDouble>(value);
    }

    public void Bind(TimeOnly value)
    {
        BindPg<TimeOnly, PgTime>(value);
    }

    public void Bind(DateOnly value)
    {
        BindPg<DateOnly, PgDate>(value);
    }

    public void Bind(DateTime value)
    {
        BindPg<DateTime, PgDateTime>(value);
    }

    public void Bind(in DateTimeOffset value)
    {
        BindPg<DateTimeOffset, PgDateTimeOffset>(value);
    }

    public void Bind(decimal value)
    {
        BindPg<decimal, PgDecimal>(value);
    }

    public void Bind(byte[]? value)
    {
        if (value is null)
        {
            BindNull<PgBytea>();
        }
        else
        {
            BindPg<byte[], PgBytea>(value);
        }
    }

    public void Bind(in ReadOnlySpan<byte> value)
    {
        _buffer.WriteUInt(PgBytea.DbType.TypeOid.Inner);
        _parameterWriter.Bind(value);
    }

    public void Bind(string? value)
    {
        if (value is null)
        {
            BindNull<PgString>();
        }
        else
        {
            BindPg<string, PgString>(value);
        }
    }

    public void Bind(in ReadOnlySpan<char> value)
    {
        _buffer.WriteUInt(PgString.DbType.TypeOid.Inner);
        _parameterWriter.Bind(value);
    }

    public void Bind(in Guid value)
    {
        BindPg<Guid, PgUuid>(value);
    }

    public void BindJson<T>(T value, JsonTypeInfo<T>? typeInfo = null) where T : notnull
    {
        _buffer.WriteUInt(PgJson<T>.DbType.TypeOid.Inner);
        _parameterWriter.BindJson(value, typeInfo);
    }

    public void BindNull<T>() where T : notnull
    {
        _buffer.WriteUInt(_compositeFields[_parameterWriter.ParameterCount].TypeOid.Inner);
        _parameterWriter.BindNull<T>();
    }

    public void Dispose()
    {
        _buffer.Dispose();
        _parameterWriter.Dispose();
    }

    public void BindPg<TValue, TType>(TValue value)
        where TType : IPgDbType<TValue>
        where TValue : notnull
    {
        _buffer.WriteUInt(TType.DbType.TypeOid.Inner);
        _parameterWriter.BindPg<TValue, TType>(value);
    }

    public static void EncodeRecord<T>(T value, IBufferWriter<byte> buffer)
        where T : IPgDbType<T>, IBindMany<IPgBindable>
    {
        using PgRecordEncoder recordEncoder = new(T.DbType);
        value.BindMany(recordEncoder);
        buffer.Write(recordEncoder.Data);
    }
}
