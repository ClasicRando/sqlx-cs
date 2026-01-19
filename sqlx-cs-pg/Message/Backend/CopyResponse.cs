using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using Sqlx.Core.Buffer;
using Sqlx.Postgres.Copy;

namespace Sqlx.Postgres.Message.Backend;

/// <summary>
/// <c>COPY</c> command response specifying the overall format of the copied data and the number of
/// columns. Technically the contents of the message also contain the format of each column but that
/// will not deviate from the overall format so keeping that data in the message is not important.
/// </summary>
[SuppressMessage("ReSharper", "NotAccessedPositionalProperty.Global")]
internal record CopyResponse(CopyFormat CopyFormat, short ColumnCount)
{
    internal static CopyResponse Decode(ReadOnlySequence<byte> buffer)
    {
        var copyFormatCode = buffer.ReadByte();
        var columnCount = buffer.ReadShort();
        return new CopyResponse(CopyFormat.FromByte(copyFormatCode), columnCount);
    }
}
