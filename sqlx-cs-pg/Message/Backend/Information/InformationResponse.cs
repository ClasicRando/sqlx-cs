using System.Diagnostics.CodeAnalysis;
using Sqlx.Core.Buffer;

namespace Sqlx.Postgres.Message.Backend.Information;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class InformationResponse
{
    private const byte SEVERITY = (byte)'S';
    private const byte SEVERITY2 = (byte)'V';
    private const byte CODE = (byte)'C';
    private const byte MESSAGE = (byte)'M';
    private const byte DETAIL = (byte)'D';
    private const byte HINT = (byte)'H';
    private const byte POSITION = (byte)'P';
    private const byte INTERNAL_POSITION = (byte)'p';
    private const byte INTERNAL_QUERY = (byte)'q';
    private const byte WHERE = (byte)'W';
    private const byte SCHEMA = (byte)'s';
    private const byte TABLE = (byte)'t';
    private const byte COLUMN = (byte)'c';
    private const byte DATA_TYPE = (byte)'d';
    private const byte CONSTRAINT_NAME = (byte)'n';
    private const byte FILE = (byte)'F';
    private const byte LINE = (byte)'L';
    private const byte ROUTINE = (byte)'R';

    private InformationResponse(Dictionary<byte, string> fields)
    {
        Severity = SeverityExtensions.FromString(
            fields.GetValueOrDefault(SEVERITY, ThrowIfMissing(fields, SEVERITY2)));
        Code = SqlStateExtensions.FromChars(ThrowIfMissing(fields, CODE));
        Message = ThrowIfMissing(fields, MESSAGE);
        Detail = fields.GetValueOrDefault(DETAIL);
        Hint = fields.GetValueOrDefault(HINT);
        Position = fields.TryGetValue(POSITION, out var positionStr)
                   && int.TryParse(positionStr, out var position)
            ? position
            : null;
        InternalQueryData = fields.TryGetValue(INTERNAL_POSITION, out var internalPositionStr)
                            && int.TryParse(internalPositionStr, out var internalPosition)
                            && fields.TryGetValue(INTERNAL_QUERY, out var internalQuery)
            ? new KeyValuePair<int, string>(internalPosition, internalQuery)
            : null;
        Where = fields.GetValueOrDefault(WHERE);
        SchemaName = fields.GetValueOrDefault(SCHEMA);
        TableName = fields.GetValueOrDefault(TABLE);
        ColumnName = fields.GetValueOrDefault(COLUMN);
        DataTypeName = fields.GetValueOrDefault(DATA_TYPE);
        ConstraintName = fields.GetValueOrDefault(CONSTRAINT_NAME);
        File = fields.GetValueOrDefault(FILE);
        Line = fields.TryGetValue(LINE, out var lineStr)
               && int.TryParse(lineStr, out var line)
            ? line
            : null;
        Routine = fields.GetValueOrDefault(ROUTINE);
    }

    public Severity Severity { get; }
    public SqlState Code { get; }
    public string Message { get; }
    public string? Detail { get; }
    public string? Hint { get; }
    public int? Position { get; }
    public KeyValuePair<int, string>? InternalQueryData { get; }
    public string? Where { get; }
    public string? SchemaName { get; }
    public string? TableName { get; }
    public string? ColumnName { get; }
    public string? DataTypeName { get; }
    public string? ConstraintName { get; }
    public string? File { get; }
    public int? Line { get; }
    public string? Routine { get; }

    public override string ToString()
    {
        var (errorCode, conditionName) = Code.GetDetails();
        var internalQueryString = InternalQueryData is not null
            ? $"Position={InternalQueryData.Value.Key}, Query={InternalQueryData.Value.Value}"
            : string.Empty;
        return $"""
               Severity: {Severity.AsReadOnlySpan()}
               SQL State: {errorCode} -> ${conditionName}
               Message: {Message}
               Detail: {Detail}
               Hint: {Hint}
               Position: {Position}
               Internal Query Data: {internalQueryString}
               Where: {Where}
               Schema: {SchemaName}
               Table: {TableName}
               Column: {ColumnName}
               Data Type: {DataTypeName}
               Constraint: {ConstraintName}
               File: {File}
               Line: {Line}
               Routine: {Routine}
               """;
    }

    public static InformationResponse Decode(ReadBuffer buffer)
    {
        Dictionary<byte, string> fields = [];
        while (!buffer.IsExhausted)
        {
            var kind = buffer.ReadByte();
            if (kind != 0)
            {
                fields[kind] = buffer.ReadCString();
            }
        }

        return new InformationResponse(fields);
    }
    
    private static string ThrowIfMissing(
        Dictionary<byte, string> fields,
        byte desiredField)
    {
        return fields.GetValueOrDefault(desiredField)
               ?? throw new InvalidInformationResponse(desiredField);
    }
}
