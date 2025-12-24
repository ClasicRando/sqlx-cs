using System.Text.Json.Serialization.Metadata;
using Sqlx.Postgres.Exceptions;
using Sqlx.Postgres.Pool;
using Sqlx.Postgres.Query;

namespace Sqlx.Postgres.Type;

public sealed class PgRecordEncoder : IPgBindable
{
    private readonly CompositeType.Attribute[] _attributes;
    private readonly PgParameterBuffer _parameterBuffer = new();

    public ReadOnlySpan<byte> Data => _parameterBuffer.Span;

    public PgRecordEncoder(PgTypeInfo typeInfo)
    {
        if (typeInfo.TypeKind is not CompositeType compositeType)
        {
            throw new PgException(
                $"Attempted to encode a type using a {nameof(PgRecordEncoder)} but that type if not a composite or the composite type was not mapped to the connection pool using {nameof(PgConnectionPool.MapCompositeAsync)}");
        }
        _attributes = compositeType.Attributes;
        _parameterBuffer.WriteInt(_attributes.Length);
    }
    
    public void Bind(bool value)
    {
        Bind<bool, PgBool>(value);
    }

    public void Bind(sbyte value)
    {
        Bind<sbyte, PgChar>(value);
    }

    public void Bind(short value)
    {
        Bind<short, PgShort>(value);
    }

    public void Bind(int value)
    {
        Bind<int, PgInt>(value);
    }

    public void Bind(long value)
    {
        Bind<long, PgLong>(value);
    }

    public void Bind(float value)
    {
        Bind<float, PgFloat>(value);
    }

    public void Bind(double value)
    {
        Bind<double, PgDouble>(value);
    }

    public void Bind(TimeOnly value)
    {
        Bind<TimeOnly, PgTime>(value);
    }

    public void Bind(DateOnly value)
    {
        Bind<DateOnly, PgDate>(value);
    }

    public void Bind(DateTime value)
    {
        Bind<DateTime, PgDateTime>(value);
    }

    public void Bind(DateTimeOffset value)
    {
        Bind<DateTimeOffset, PgDateTimeOffset>(value);
    }

    public void Bind(decimal value)
    {
        Bind<decimal, PgDecimal>(value);
    }
    
    public void Bind(byte[]? value)
    {
        this.BindRef<byte[], PgBytea>(value);
    }

    public void Bind(ReadOnlySpan<byte> value)
    {
        _parameterBuffer.EncodeBytes(value);
    }

    public void Bind(string? value)
    {
        this.BindRef<string, PgString>(value);
    }

    public void Bind(ReadOnlySpan<char> value)
    {
        _parameterBuffer.EncodeChars(value);
    }

    public void Bind(Guid value)
    {
        Bind<Guid, PgUuid>(value);
    }

    public void BindJson<T>(T value, JsonTypeInfo<T>? typeInfo = null) where T : notnull
    {
        _parameterBuffer.EncodeJsonValue(value, typeInfo);
    }

    public void BindNull<T>() where T : notnull
    {
        _parameterBuffer.WriteOid(_attributes[_parameterBuffer.ParameterCount].TypeOid);
        _parameterBuffer.EncodeNull();
    }

    public void Dispose()
    {
        _parameterBuffer.Dispose();
    }
    
    public void Bind<TValue, TType>(TValue value)
        where TType : IPgDbType<TValue>
        where TValue : notnull
    {
        _parameterBuffer.WriteOid(TType.DbType.TypeOid);
        _parameterBuffer.EncodeValue<TValue, TType>(value);
    }
}
