using System.Text;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using Sqlx.Core.Buffer;

namespace Sqlx.Core.Types;

[TestSubject(typeof(Json))]
public partial class JsonTest
{
    [JsonSerializable(typeof(JsonType))]
    public partial class SourceGenerationContext : JsonSerializerContext;

    public record JsonType(int Id, string Name);

    private readonly JsonType _jsonType = new(1, "Test");
    private const string JsonTypeStr = "{\"Id\":1,\"Name\":\"Test\"}";

    [Fact]
    public void WriteToBuffer_Should_WriteJsonValueToBuffer_When_SourceGeneratedType()
    {
        using var buffer = new WriteBuffer();
        
        Json.WriteToBuffer(buffer, _jsonType, SourceGenerationContext.Default.JsonType);
        
        Assert.Equal(JsonTypeStr, Encoding.UTF8.GetString(buffer.ReadableSpan));
    }
    
    [Fact]
    public void WriteToBuffer_Should_WriteJsonValueToBuffer_When_ReflectionBasedSerialization()
    {
        using var buffer = new WriteBuffer();
        
        Json.WriteToBuffer(buffer, _jsonType, null);
        
        Assert.Equal(JsonTypeStr, Encoding.UTF8.GetString(buffer.ReadableSpan));
    }

    [Fact]
    public void FromBytes_Should_ReadJsonValueFromBytes_When_SourceGeneratedType()
    {
        var bytes = Encoding.UTF8.GetBytes(JsonTypeStr);

        var result = Json.FromBytes<JsonType>(bytes, SourceGenerationContext.Default.JsonType);
        
        Assert.Equal(_jsonType, result);
    }

    [Fact]
    public void FromBytes_Should_ReadJsonValueFromBytes_When_ReflectionBasedSerialization()
    {
        var bytes = Encoding.UTF8.GetBytes(JsonTypeStr);

        var result = Json.FromBytes<JsonType>(bytes);
        
        Assert.Equal(_jsonType, result);
    }

    [Fact]
    public void FromChars_Should_ReadJsonValueFromBytes_When_SourceGeneratedType()
    {
        var result = Json.FromChars<JsonType>(
            JsonTypeStr,
            SourceGenerationContext.Default.JsonType);
        
        Assert.Equal(_jsonType, result);
    }

    [Fact]
    public void FromChars_Should_ReadJsonValueFromBytes_When_ReflectionBasedSerialization()
    {
        var result = Json.FromChars<JsonType>(JsonTypeStr);
        
        Assert.Equal(_jsonType, result);
    }
}
