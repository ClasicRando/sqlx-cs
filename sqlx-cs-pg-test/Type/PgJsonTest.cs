using System.Text.Json;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using Sqlx.Core.Buffer;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

[TestSubject(typeof(PgJson<>))]
public class PgJsonTest
{
    [Test]
    [Arguments(
        1,
        "Test1",
        true,
        new byte[]
        {
            1, 123, 34, 73, 100, 34, 58, 49, 44, 34, 78, 97, 109, 101, 34, 58, 34, 84, 101, 115,
            116, 49, 34, 125,
        })]
    [Arguments(
        2,
        "Test2",
        false,
        new byte[]
        {
            1, 123, 34, 73, 100, 34, 58, 50, 44, 34, 78, 97, 109, 101, 34, 58, 34, 84, 101, 115,
            116, 50, 34, 125,
        })]
    public async Task Encode_Should_WriteJson(
        int id,
        string name,
        bool useSourceGeneration,
        byte[] expectedBytes)
    {
        var value = new Inner(id, name);
        using var buffer = new PooledArrayBufferWriter();

        PgJson<Inner>.Encode(
            value,
            buffer,
            useSourceGeneration ? SourceGenerationContext.Default.Inner : null);

        var actualBytes = buffer.ReadableSpan.ToArray();

        await Assert.That(actualBytes).IsEquivalentTo(expectedBytes);
    }

    [Test]
    [Arguments(
        new byte[]
        {
            1, 123, 34, 73, 100, 34, 58, 49, 44, 34, 78, 97, 109, 101, 34, 58, 34, 84, 101, 115,
            116, 49, 34, 125,
        },
        true,
        1,
        "Test1",
        true)]
    [Arguments(
        new byte[]
        {
            123, 34, 73, 100, 34, 58, 50, 44, 34, 78, 97, 109, 101, 34, 58, 34, 84, 101, 115, 116,
            50, 34, 125,
        },
        false,
        2,
        "Test2",
        false)]
    public async Task DecodeBytes_Should_DecodeBinaryEncodedValueAsJson(
        byte[] binaryData,
        bool isJsonB,
        int id,
        string name,
        bool useSourceGeneration)
    {
        var expectedValue = new Inner(id, name);
        var columnMetadata = new PgColumnMetadata(
            string.Empty,
            0,
            0,
            isJsonB ? PgTypeInfo.Jsonb : PgTypeInfo.Json,
            0,
            0,
            PgFormatCode.Binary);
        var binaryValue = new PgBinaryValue(new ReadBuffer(binaryData), ref columnMetadata);

        Inner actualValue = PgJson<Inner>.DecodeBytes(
            ref binaryValue,
            useSourceGeneration ? SourceGenerationContext.Default.Inner : null);

        await Assert.That(actualValue).IsEqualTo(expectedValue);
    }

    [Test]
    [Arguments("{\"Id\":1,\"Name\":\"Test1\"}", true, 1, "Test1", true)]
    [Arguments("{\"Id\":2,\"Name\":\"Test2\"}", false, 2, "Test2", false)]
    public async Task DecodeText_Should_DecodeTextEncodedValueAsJson(
        string textData,
        bool isJsonB,
        int id,
        string name,
        bool useSourceGeneration)
    {
        var expectedValue = new Inner(id, name);
        var columnMetadata = new PgColumnMetadata(
            string.Empty,
            0,
            0,
            isJsonB ? PgTypeInfo.Jsonb : PgTypeInfo.Json,
            0,
            0,
            PgFormatCode.Binary);
        var textValue = new PgTextValue(textData, ref columnMetadata);

        Inner actualValue = PgJson<Inner>.DecodeText(
            textValue,
            useSourceGeneration ? SourceGenerationContext.Default.Inner : null);

        await Assert.That(actualValue).IsEqualTo(expectedValue);
    }

    [Test]
    public async Task DecodeText_Should_Fail_When_JsonValueIsNull()
    {
        const string textData = "null";
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, ref columnMetadata);

        try
        {
            PgJson<Inner>.DecodeText(textValue);
            Assert.Fail("Decoding should have failed");
        }
        catch (ArgumentException e)
        {
            await Assert.That(e.Message)
                .Contains("JSON deserialization returned null. Cannot create Json from null");
        }
        catch (Exception e)
        {
            Assert.Fail($"Decoding should have failed due to column decode error. Instead: {e}");
        }
    }

    [Test]
    [Arguments(
        "{\"ID\":2}",
        "The JSON property 'ID' could not be mapped to any .NET member contained in type 'Sqlx.Postgres.Type.Inner'.")]
    [Arguments(
        "{\"Id\":2,\"Name\":null}",
        "The constructor parameter 'name' on type 'Sqlx.Postgres.Type.Inner' doesn't allow null values.")]
    [Arguments(
        "{\"Id\":2,\"Name\":\"\",\"Other\":null}",
        "The JSON property 'Other' could not be mapped to any .NET member contained in type 'Sqlx.Postgres.Type.Inner'.")]
    public async Task DecodeText_Should_Fail_When_InvalidJson(string textData, string message)
    {
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, ref columnMetadata);

        try
        {
            Inner value = PgJson<Inner>.DecodeText(
                textValue,
                SourceGenerationContext.Default.Inner);
            Assert.Fail($"Decoding should have failed. Found '{value}'");
        }
        catch (JsonException e)
        {
            await Assert.That(e.Message).Contains(message);
        }
        catch (Exception e)
        {
            Assert.Fail($"Decoding should have failed due to column decode error. Instead: {e}");
        }
    }

    [Test]
    public async Task DbType_Should_ReturnJsonType() => await Assert.That(PgTypeInfo.Jsonb).IsEqualTo(PgJson<Inner>.DbType);

    [Test]
    public async Task ArrayDbType_Should_ReturnJsonType() =>
        await Assert.That(PgTypeInfo.JsonbArray).IsEqualTo(PgJson<Inner>.ArrayDbType);

    [Test]
    [MethodDataSource(nameof(IsCompatibleCases))]
    public async Task IsCompatible(PgTypeInfo pgType, bool expectedResult) =>
        await Assert.That(PgJson<Inner>.IsCompatible(pgType)).IsEqualTo(expectedResult);
    
    public static IEnumerable<Func<(PgTypeInfo, bool)>> IsCompatibleCases()
    {
        yield return () => (PgTypeInfo.Json, true);
        yield return () => (PgTypeInfo.Jsonb, true);
        yield return () => (PgTypeInfo.JsonbArray, false);
        yield return () => (PgTypeInfo.Int4, false);
    }
}

public class Inner(int id, string name) : IEquatable<Inner>
{
    [JsonRequired] public int Id { get; init; } = id;
    [JsonRequired] public string Name { get; init; } = name;

    public bool Equals(Inner? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id == other.Id && Name == other.Name;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj is Inner inner && Equals(inner);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, Name);
    }

    public static bool operator ==(Inner? left, Inner? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Inner? left, Inner? right)
    {
        return !Equals(left, right);
    }

    public override string ToString()
    {
        return $"{nameof(Inner)} {{ {nameof(Id)}: {Id}, {nameof(Name)}: {Name} }}";
    }
}

[JsonSerializable(typeof(Inner))]
[JsonSourceGenerationOptions(
    UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
    RespectNullableAnnotations = true)]
public partial class SourceGenerationContext : JsonSerializerContext;
