using System.Text.Json.Serialization.Metadata;

namespace Sqlx.Core.Query;

public interface IQuery : IDisposable
{
    public string Query { get; }
    
    public void Bind<T>(T? value) where T : notnull;

    public void BindJson<T>(T? value, JsonTypeInfo<T>? typeInfo = null) where T : notnull;

    public void BindOutParameter<T>() where T : notnull;
}
