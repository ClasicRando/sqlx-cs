using System.Text;
using Sqlx.Core.Buffer;
using Sqlx.Postgres.Result;

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

    /// <summary>
    /// Create a string literal for the collection of points. Points are always enclosed in a set of
    /// complimentary characters depending on if the collection is closed or open. Close collections
    /// are wrapped in <c>(...)</c> and open collection are wrapped in <c>[...]</c>.
    /// </summary>
    /// <param name="points">Points to encode into a literal value</param>
    /// <param name="isClosed">True if the collection points close into a complete shape</param>
    /// <returns>The string literal representation of this collection of points</returns>
    public static string GeneratePointCollectionLiteral(PgPoint[] points, bool isClosed)
    {
        var builder = new StringBuilder();
        builder.Append(isClosed ? '(' : '[');
        for (var i = 0; i < points.Length; i++)
        {
            PgPoint point = points[i];
            if (i > 0)
            {
                builder.Append(',');
            }
            builder.Append(point.GeometryLiteral);
        }
        builder.Append(isClosed ? ')' : ']');
        return builder.ToString();
    }

    /// <summary>
    /// Encodes all provided <see cref="PgPoint"/>s to the buffer with a prefix <see cref="int"/> as
    /// the number of points encoded.
    /// </summary>
    /// <param name="points"><see cref="PgPoint"/>s to encode</param>
    /// <param name="buffer">Buffer to encode the points to</param>
    public static void EncodePoints(PgPoint[] points, WriteBuffer buffer)
    {
        buffer.WriteInt(points.Length);
        foreach (PgPoint point in points)
        {
            PgPoint.Encode(point, buffer);
        }
    }

    /// <summary>
    /// Reads the first integer as the number of points, then calls
    /// <see cref="PgPoint.DecodeBytes"/> for that number of points to extract all points.
    /// </summary>
    /// <param name="value">Binary encoded value that contains a collection of points</param>
    /// <returns>An array of points decoded from the binary value</returns>
    public static PgPoint[] DecodePoints(PgBinaryValue value)
    {
        var size = value.Buffer.ReadInt();
        var points = new PgPoint[size];
        for (var i = 0; i < size; i++)
        {
            points[i] = PgPoint.DecodeBytes(value);
        }

        return points;
    }

    /// <summary>
    /// Reads the characters as a collection of points surrounded by an enclosing char and separated
    /// by commas. For each range of the characters that represents a point,
    /// <see cref="PgPoint.DecodeText"/> is called to construct the point.
    /// </summary>
    /// <param name="value">Text encoded value that contains a collection of points</param>
    /// <returns>An array of points decoded from the text value</returns>
    public static PgPoint[] DecodePoints(PgTextValue value)
    {
        PgTextValue pointChars = value.Slice(1..^1);
        var indexPairs = ExtractPointRanges(pointChars);
        var points = new PgPoint[indexPairs.Length];
        for (var i = 0; i < points.Length; i++)
        {
            points[i] = PgPoint.DecodeText(pointChars.Slice(indexPairs[i]));
        }
        return points;
    }
}
