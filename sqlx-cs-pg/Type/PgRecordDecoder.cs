using System.Buffers;
using System.Text;
using Sqlx.Core;
using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Core.Result;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Exceptions;
using Sqlx.Postgres.Pool;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

/// <summary>
/// Decoder for Postgres composite types where values are decoded using its
/// <see cref="IFromRow{TDataRow,TResult}"/> implemention. Each decode method takes the value to
/// decode and converts that to a <see cref="IPgDataRow"/> to allow for decoding the value as if it
/// were a row.
/// <example>
/// For this type definition:
/// <code>
/// CREATE TYPE example AS (id integer, name text);
/// </code>
/// You would write this type:
/// <code>
/// public record Example(int Id, string Name) : IPgUdt&lt;Example&gt;, IFromRow&lt;IPgDataRow, Example&gt;
/// {
///     public static Example DecodeBytes(ref PgBinaryValue value)
///     {
///         return PgRecordDecoder.DecodeBinary&lt;Example&gt;(ref value);
///     }
/// 
///     public static Example DecodeText(in PgTextValue value)
///     {
///         return PgRecordDecoder.DecodeText&lt;Example&gt;(in value);
///     }
///
///     public static Example FromRow(IPgDataRow dataRow)
///     {
///         return new Example(dataRow.GetIntNotNull("id"), dataRow.GetStringNotNull("name"));
///     }
///
///     // Other IPgUdt methods and properties
/// }
/// </code>
/// </example>
/// </summary>
public static class PgRecordDecoder
{
    public static T DecodeBinary<T>(ref PgBinaryValue binaryValue)
        where T : IPgDbType<T>, IFromRow<IPgDataRow, T>
    {
        PgTypeInfo typeInfo = T.DbType;
        if (typeInfo.TypeKind is not CompositeType compositeType)
        {
            throw new PgException(
                $"Attempted to decode a type using a {nameof(PgRecordDecoder)} but that type if not a composite or the composite type was not mapped to the connection pool using {nameof(PgConnectionPool.MapCompositeAsync)}");
        }

        var attributeCount = binaryValue.Buffer.ReadInt();
        if (attributeCount != compositeType.Fields.Length)
        {
            throw new PgException(
                $"Mismatch in attribute counts. Expected {compositeType.Fields.Length}, Found {attributeCount}");
        }

        var columns = new PgColumnMetadata[attributeCount];
        using PooledArrayBufferWriter bufferWriter = new();
        bufferWriter.WriteShort((short)attributeCount);

        for (var i = 0; i < attributeCount; i++)
        {
            CompositeField attribute = compositeType.Fields[i];
            columns[i] = new PgColumnMetadata(
                attribute.Name,
                0,
                0,
                PgTypeInfo.FromOid(attribute.TypeOid),
                0,
                0,
                PgFormatCode.Binary);
            // Skip attribute Oid
            binaryValue.Buffer.ReadInt();

            var attributeLength = binaryValue.Buffer.ReadInt();
            bufferWriter.WriteInt(attributeLength);
            if (attributeLength == -1)
            {
                continue;
            }
            
            bufferWriter.Write(binaryValue.Buffer.ReadBytesAsSpan(attributeLength));
        }

        var span = bufferWriter.ReadableSpan;
        using var row = new PgDataRow(ref span, new PgStatementMetadata(columns));
        return T.FromRow(row);
    }

    public static T DecodeText<T>(in PgTextValue textValue)
        where T : IPgDbType<T>, IFromRow<IPgDataRow, T>
    {
        PgTypeInfo typeInfo = T.DbType;
        if (typeInfo.TypeKind is not CompositeType compositeType)
        {
            throw new PgException(
                $"Attempted to decode a type using a {nameof(PgRecordDecoder)} but that type if not a composite or the composite type was not mapped to the connection pool using {nameof(PgConnectionPool.MapCompositeAsync)}");
        }

        var attributeLiterals = ParseCompositeLiteralToValueRanges<T>(in textValue);

        if (attributeLiterals.Count != compositeType.Fields.Length)
        {
            throw new PgException(
                $"Mismatch in attribute counts. Expected {compositeType.Fields.Length}, Found {attributeLiterals.Count}");
        }
        
        var columns = new PgColumnMetadata[attributeLiterals.Count];
        using PooledArrayBufferWriter bufferWriter = new();
        bufferWriter.WriteShort((short)attributeLiterals.Count);

        for (var i = 0; i < attributeLiterals.Count; i++)
        {
            var attributeLiteral = attributeLiterals[i];
            CompositeField field = compositeType.Fields[i];
            columns[i] = new PgColumnMetadata(
                field.Name,
                0,
                0,
                PgTypeInfo.FromOid(field.TypeOid),
                0,
                0,
                PgFormatCode.Text);

            if (attributeLiteral is null)
            {
                bufferWriter.WriteInt(-1);
                continue;
            }

            var literalByteCount = Charsets.Default.GetByteCount(attributeLiteral);
            bufferWriter.WriteInt(literalByteCount);
            var span = bufferWriter.GetSpan(literalByteCount);
            Charsets.Default.GetBytes(attributeLiteral, span);
            bufferWriter.Advance(literalByteCount);
        }

        var dataRowSpan = bufferWriter.ReadableSpan;
        using var row = new PgDataRow(ref dataRowSpan, new PgStatementMetadata(columns));
        return T.FromRow(row);
    }

    private static List<string?> ParseCompositeLiteralToValueRanges<T>(in PgTextValue value)
        where T : notnull
    {
        if (!value.Chars.StartsWith("(") || !value.Chars.EndsWith(")"))
        {
            throw ColumnDecodeException.Create<T>(
                value.ColumnMetadata,
                $"Composite literal must be enclosed in parenthesis. Found '{value.Chars}'");
        }
        
        List<string?> result = [];
        using ReadOnlySpan<char>.Enumerator chars = value.Chars[1..^1].GetEnumerator();
        var builder = new StringBuilder();
        var isDone = false;

        while (!isDone)
        {
            var foundDelimiter = false;
            var inQuotes = false;
            var inEscape = false;
            var previousChar = '\u0000';
            while (true)
            {
                if (!chars.MoveNext())
                {
                    isDone = true;
                    break;
                }
                
                var currentChar = chars.Current;
                if (inEscape)
                {
                    builder.Append(currentChar);
                    inEscape = false;
                    continue;
                }

                switch (currentChar)
                {
                    case '"' when inQuotes:
                        inQuotes = false;
                        break;
                    case '"':
                    {
                        inQuotes = true;
                        if (previousChar == '"')
                        {
                            builder.Append(currentChar);
                        }
                        break;
                    }
                    case '\\' when !inEscape:
                        inEscape = true;
                        break;
                    case ',' when !inQuotes:
                        foundDelimiter = true;
                        break;
                    default:
                        builder.Append(currentChar);
                        break;
                }
                
                if (foundDelimiter) break;

                previousChar = currentChar;
            }

            var slice = builder.ToString();
            result.Add(slice is "NULL" ? null : slice);
            builder.Clear();
        }

        return result;
    }
}
