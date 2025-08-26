using Sqlx.Core.Buffer;
using Sqlx.Postgres.Copy;

namespace Sqlx.Postgres.Message.Backend;

internal sealed class CopyResponse(CopyFormat copyFormat, short columnCount, List<CopyFormat> copyFormats)
{
    internal CopyFormat CopyFormat { get; } = copyFormat;
    internal short ColumnCount { get; } = columnCount;
    internal IReadOnlyList<CopyFormat> CopyFormats { get; } = copyFormats;
    
    internal static CopyResponse Decode(ReadBuffer buffer)
    {
        CopyFormat copyFormat = CopyFormatExtensions.FromByte(buffer.ReadByte());
        var columnCount = buffer.ReadShort();
        List<CopyFormat> copyFormats = [];
        for (var i = 1; i < columnCount; i++)
        {
            copyFormats.Add(CopyFormatExtensions.FromByte(buffer.ReadByte()));
        }

        return new CopyResponse(copyFormat, columnCount, copyFormats);
    }
}
