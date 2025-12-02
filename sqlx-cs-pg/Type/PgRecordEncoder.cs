using System.Text.Json.Serialization.Metadata;
using Sqlx.Core.Query;
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
                $"Attempted to encode a type using a {nameof(PgRecordEncoder)} but that type if not a composite or the composite type was not mapped to the connection pool using {nameof(PgConnectionPool.MapComposite)}");
        }
        _attributes = compositeType.Attributes;
        _parameterBuffer.WriteInt(_attributes.Length);
    }
    
    public IBindable Bind(bool value)
    {
        return BindPg<bool, PgBool>(value);
    }

    public IBindable Bind(sbyte value)
    {
        return BindPg<sbyte, PgChar>(value);
    }

    public IBindable Bind(short value)
    {
        return BindPg<short, PgShort>(value);
    }

    public IBindable Bind(int value)
    {
        return BindPg<int, PgInt>(value);
    }

    public IBindable Bind(long value)
    {
        return BindPg<long, PgLong>(value);
    }

    public IBindable Bind(float value)
    {
        return BindPg<float, PgFloat>(value);
    }

    public IBindable Bind(double value)
    {
        return BindPg<double, PgDouble>(value);
    }

    public IBindable Bind(TimeOnly value)
    {
        return BindPg<TimeOnly, PgTime>(value);
    }

    public IBindable Bind(DateOnly value)
    {
        return BindPg<DateOnly, PgDate>(value);
    }

    public IBindable Bind(DateTime value)
    {
        return BindPg<DateTime, PgDateTime>(value);
    }

    public IBindable Bind(DateTimeOffset value)
    {
        return BindPg<DateTimeOffset, PgDateTimeOffset>(value);
    }

    public IBindable Bind(decimal value)
    {
        return BindPg<decimal, PgDecimal>(value);
    }

    public IBindable Bind(byte[]? value)
    {
        return this.BindPgNullableClass<byte[], PgBytea>(value);
    }

    public IBindable Bind(ReadOnlySpan<byte> value)
    {
        _parameterBuffer.EncodeBytes(value);
        return this;
    }

    public IBindable Bind(string? value)
    {
        return this.BindPgNullableClass<string, PgString>(value);
    }

    public IBindable Bind(ReadOnlySpan<char> value)
    {
        _parameterBuffer.EncodeChars(value);
        return this;
    }

    public IBindable Bind(Guid value)
    {
        return BindPg<Guid, PgUuid>(value);
    }

    public IBindable BindJson<T>(T? value, JsonTypeInfo<T>? typeInfo = null) where T : notnull
    {
        if (value is null)
        {
            _parameterBuffer.EncodeNull();
            return this;
        }
        _parameterBuffer.EncodeJsonValue(value, typeInfo);
        return this;
    }

    public IBindable BindNull<T>() where T : notnull
    {
        _parameterBuffer.WriteOid(_attributes[_parameterBuffer.ParameterCount].TypeOid);
        _parameterBuffer.EncodeNull();
        return this;
    }

    public void Dispose()
    {
        _parameterBuffer.Dispose();
    }
    
    public IBindable BindPg<TValue, TType>(TValue value)
        where TType : IPgDbType<TValue>
        where TValue : notnull
    {
        _parameterBuffer.WriteOid(TType.DbType.TypeOid);
        _parameterBuffer.EncodeValue<TValue, TType>(value);
        return this;
    }
}
