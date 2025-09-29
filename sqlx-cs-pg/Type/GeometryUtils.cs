namespace Sqlx.Postgres.Type;

/// <summary>
/// Helper class for geometry related methods
/// </summary>
public static class GeometryUtils
{
    /// <summary>
    /// Extract the ranges within the specified chars that contain a point. This assumes that points
    /// are written as <c>(x,y)</c> so points are separated by '),'.
    /// </summary>
    /// <param name="chars">Characters that contain zero or more points</param>
    /// <returns>Array of ranges spanning the points within the characters</returns>
    public static Range[] ExtractPointRanges(
        ReadOnlySpan<char> chars)
    {
        if (chars.Length == 0)
        {
            return [];
        }

        var capacity = ((chars.Count(',') - 1) / 2) + 1;
        var result = new Range[capacity];
        var index = 0;

        foreach (Range rng in chars.Split("),"))
        {
            Range finalRange = rng.End.Value == chars.Length
                ? rng
                : new Range(rng.Start, rng.End.Value + 1);
            result[index++] = finalRange;
        }

        return result;
    }
}
