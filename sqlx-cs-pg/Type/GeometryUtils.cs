using System.Buffers;
using System.Text;
using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
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
    public static Range[] ExtractPointRanges(ReadOnlySpan<char> chars)
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
    /// <para>
    /// Decode a <see cref="PgPoint"/> from the supplied <see cref="PgTextValue"/>. Extracts 2
    /// <see cref="double"/> values for the <see cref="PgPoint.X"/> and <see cref="PgPoint.Y"/>
    /// coordinates from the characters assuming the format is '({x},{y})'.
    /// </para>
    /// <a href="https://github.com/postgres/postgres/blob/1fe66680c09b6cc1ed20236c84f0913a7b786bbc/src/backend/utils/adt/geo_ops.c#L1842">pg source code</a>
    /// </summary>
    /// <param name="value">Text value to parse</param>
    /// <typeparam name="T">Final decoding type, used to report errors</typeparam>
    /// <returns>A new point from the supplied text</returns>
    /// <exception cref="ColumnDecodeException">
    /// If either coordinate cannot be parsed from the characters
    /// </exception>
    public static PgPoint DecodePoint<T>(in PgTextValue value)
        where T : notnull 
    {
        var commaIndex = value.Chars.IndexOf(',');
        if (commaIndex == -1)
        {
            throw ColumnDecodeException.Create<T>(
                value.ColumnMetadata,
                "Could not find point separator character");
        }

        if (!double.TryParse(value.Chars[1..commaIndex], out var x))
        {
            throw ColumnDecodeException.Create<T>(
                value.ColumnMetadata,
                "Could not parse X coordinate");
        }

        return !double.TryParse(
            value.Chars.Slice(commaIndex + 1, value.Chars.Length - commaIndex - 2), out var y)
            ? throw ColumnDecodeException.Create<T>(
                value.ColumnMetadata,
                "Could not parse Y coordinate")
            : new PgPoint(x, y);
    }

    /// <summary>
    /// Create a string literal for the collection of points. Points are always enclosed in a set of
    /// complimentary characters depending on if the collection is closed or open. Close collections
    /// are wrapped in <c>(...)</c> and open collection are wrapped in <c>[...]</c>.
    /// </summary>
    /// <param name="points">Points to encode into a literal value</param>
    /// <param name="isClosed">True if the collection points close into a complete shape</param>
    /// <returns>The string literal representation of this collection of points</returns>
    public static string GeneratePointCollectionLiteral(ReadOnlySpan<PgPoint> points, bool isClosed)
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
    public static void EncodePoints(ReadOnlySpan<PgPoint> points, IBufferWriter<byte> buffer)
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
    public static PgPoint[] DecodePoints(ref PgBinaryValue value)
    {
        var size = value.Buffer.ReadInt();
        var points = new PgPoint[size];
        for (var i = 0; i < size; i++)
        {
            points[i] = PgPoint.DecodeBytes(ref value);
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
    public static PgPoint[] DecodePoints<T>(in PgTextValue value) where T : notnull
    {
        PgTextValue pointChars = value.Slice(1..^1);
        var indexPairs = ExtractPointRanges(pointChars);
        var points = new PgPoint[indexPairs.Length];
        for (var i = 0; i < points.Length; i++)
        {
            PgTextValue slice = pointChars.Slice(indexPairs[i]);
            points[i] = DecodePoint<T>(in slice);
        }
        return points;
    }
}
