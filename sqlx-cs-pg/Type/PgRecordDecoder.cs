using System.Buffers;
using System.Text;
using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Core.Result;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Exceptions;
using Sqlx.Postgres.Pool;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

public static class PgRecordDecoder
{
    public static T DecodeBinary<T>(ref PgBinaryValue binaryValue)
        where T : IPgDbType<T>, IFromRow<IPgDataRow, T>
    {
        PgTypeInfo typeInfo = T.DbType;
        if (typeInfo.TypeKind is not CompositeType compositeType)
        {
            throw new PgException(
                $"Attempted to decode a type using a {nameof(PgRecordDecoder)} but that type if not a composite or the composite type was not mapped to the connection pool using {nameof(PgConnectionPool.MapComposite)}");
        }

        var attributeCount = binaryValue.Buffer.ReadInt();
        if (attributeCount != compositeType.Attributes.Length)
        {
            throw new PgException(
                $"Mismatch in attribute counts. Expected {compositeType.Attributes.Length}, Found {attributeCount}");
        }

        var columns = new PgColumnMetadata[attributeCount];
        using WriteBuffer writeBuffer = new();
        writeBuffer.WriteShort((short)attributeCount);

        for (var i = 0; i < attributeCount; i++)
        {
            CompositeType.Attribute attribute = compositeType.Attributes[i];
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
            writeBuffer.WriteInt(attributeLength);
            if (attributeLength == -1)
            {
                continue;
            }
            
            writeBuffer.WriteBytes(binaryValue.Buffer.ReadBytesAsSpan(attributeLength));
        }

        var buffer = ArrayPool<byte>.Shared.Rent(writeBuffer.ReadableSpan.Length);
        try
        {
            writeBuffer.ReadableSpan.CopyTo(buffer);
            var row = new PgDataRow(buffer, new PgStatementMetadata(columns));
            return T.FromRow(row);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    public static T DecodeText<T>(in PgTextValue textValue)
        where T : IPgDbType<T>, IFromRow<IPgDataRow, T>
    {
        PgTypeInfo typeInfo = T.DbType;
        if (typeInfo.TypeKind is not CompositeType compositeType)
        {
            throw new PgException(
                $"Attempted to decode a type using a {nameof(PgRecordDecoder)} but that type if not a composite or the composite type was not mapped to the connection pool using {nameof(PgConnectionPool.MapComposite)}");
        }

        var attributeLiterals = ParseCompositeLiteralToValueRanges<T>(in textValue);

        if (attributeLiterals.Count != compositeType.Attributes.Length)
        {
            throw new PgException(
                $"Mismatch in attribute counts. Expected {compositeType.Attributes.Length}, Found {attributeLiterals.Count}");
        }
        
        var columns = new PgColumnMetadata[attributeLiterals.Count];
        using WriteBuffer writeBuffer = new();
        writeBuffer.WriteShort((short)attributeLiterals.Count);

        for (var i = 0; i < attributeLiterals.Count; i++)
        {
            var attributeLiteral = attributeLiterals[i];
            CompositeType.Attribute attribute = compositeType.Attributes[i];
            columns[i] = new PgColumnMetadata(
                attribute.Name,
                0,
                0,
                PgTypeInfo.FromOid(attribute.TypeOid),
                0,
                0,
                PgFormatCode.Text);

            if (attributeLiteral is null)
            {
                writeBuffer.WriteInt(-1);
                continue;
            }

            var startPosition = writeBuffer.StartWritingLengthPrefixed();
            writeBuffer.WriteString(attributeLiteral);
            writeBuffer.FinishWritingLengthPrefixed(startPosition, false);
        }

        var buffer = ArrayPool<byte>.Shared.Rent(writeBuffer.ReadableSpan.Length);
        try
        {
            writeBuffer.ReadableSpan.CopyTo(buffer);
            var row = new PgDataRow(buffer, new PgStatementMetadata(columns));
            return T.FromRow(row);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    private static List<string?> ParseCompositeLiteralToValueRanges<T>(in PgTextValue value)
        where T : notnull
    {
        if (!value.Chars.StartsWith("(") || !value.Chars.EndsWith(")"))
        {
            throw ColumnDecodeException.Create<T>(
                value.ColumnMetadata,
                $"Composite literal must be enclosed in parenthesis. Found '{value}'");
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
