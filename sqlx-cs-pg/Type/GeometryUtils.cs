namespace Sqlx.Postgres.Type;

public static class GeometryUtils
{
    public static List<(int StartIndex, int EndIndex)> ExtractPointIndexes(
        ReadOnlySpan<char> chars)
    {
        if (chars.Length == 0)
        {
            return [];
        }

        List<(int, int)> result = [];
        var previousChar = chars[0];
        var start = 0;

        for (var i = 1; i < chars.Length; i++)
        {
            if (previousChar == ')' && chars[i] == ',')
            {
                result.Add((start, i));
                start = i + 1;
                continue;
            }
            previousChar = chars[i];
        }

        if (result[^1].Item2 != chars.Length - 1)
        {
            result.Add((start, chars.Length));
        }

        return result;
    }
}
