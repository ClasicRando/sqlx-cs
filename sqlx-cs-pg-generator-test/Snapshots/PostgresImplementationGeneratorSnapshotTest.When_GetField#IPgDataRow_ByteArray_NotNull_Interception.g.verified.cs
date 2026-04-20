//HintName: IPgDataRow_ByteArray_NotNull_Interception.g.cs
#nullable enable
namespace System.Runtime.CompilerServices
{
    [global::System.Diagnostics.Conditional("DEBUG")]
    [global::System.AttributeUsage(global::System.AttributeTargets.Method, AllowMultiple = true)]
    sealed file class InterceptsLocationAttribute : global::System.Attribute
    {
        public InterceptsLocationAttribute(int version, string data)
        {
            _ = version;
            _ = data;
        }
    }
}

namespace Sqlx.Postgres.Interceptors
{
    static file class GetInterceptors
    {
        [global::System.Runtime.CompilerServices.InterceptsLocation(1, "h0UqGfPwoHGV4YdJ3UU/KowAAAA=")] // (7,13)
        public static global::System.Byte[] GetNotNull(this global::Sqlx.Postgres.Result.IPgDataRow pgDataRow, int index)
        {
            return pgDataRow.GetPgNotNull<global::System.Byte[],global::Sqlx.Postgres.Type.PgBytea>(index);
        }
    }
}
