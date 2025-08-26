namespace Sqlx.Core.Result;

public interface IFromRow<out TResult> where TResult : notnull
{
    public static abstract TResult Decode(IDataRow dataRow);
}
