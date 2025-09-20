using System.Diagnostics.CodeAnalysis;
using Sqlx.Core.Buffer;
using Sqlx.Postgres.Exceptions;

namespace Sqlx.Postgres.Message.Backend.Information;

/// <summary>
/// Contents of an information based database server response message. Specify a number of
/// properties about the error or notice message.
/// </summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
internal class InformationResponse
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

    /// <summary>
    /// Severity of the message
    /// </summary>
    public Severity Severity { get; }
    /// <summary>
    /// SQLSTATE code of the message
    /// </summary>
    public SqlState Code { get; }
    /// <summary>
    /// Human-readable version of the message
    /// </summary>
    public string Message { get; }
    /// <summary>
    /// Optional extra details along with the message
    /// </summary>
    public string? Detail { get; }
    /// <summary>
    /// Optional suggestion about the problem
    /// </summary>
    public string? Hint { get; }
    /// <summary>
    /// Error cursor position within the original query string. Index is character not bytes
    /// </summary>
    public int? Position { get; }
    /// <summary>
    /// Pair where the first value is the error cursor position within the internal command and the
    /// second value is the internal command's query (e.g. the SQL query within a PL/pgsql
    /// function).
    /// </summary>
    public KeyValuePair<int, string>? InternalQueryData { get; }
    /// <summary>
    /// Call stack traceback of the active procedural language function or internal-generated query
    /// </summary>
    public string? Where { get; }
    /// <summary>
    /// If the message is associated with a specific database object, this is the name of the schema
    /// containing the object
    /// </summary>
    public string? SchemaName { get; }
    /// <summary>
    /// If the message is associated with a specific database table, this is the name of the table
    /// </summary>
    public string? TableName { get; }
    /// <summary>
    /// If the message is associated with a specific table column, this is the name of the column
    /// </summary>
    public string? ColumnName { get; }
    /// <summary>
    /// If the message is associated with a specific data type, this is the name of the data type
    /// </summary>
    public string? DataTypeName { get; }
    /// <summary>
    /// If the message is associated with a specific constraint, this is the name of the constraint
    /// </summary>
    public string? ConstraintName { get; }
    /// <summary>
    /// The file name of the source code where the error was reported
    /// </summary>
    public string? File { get; }
    /// <summary>
    /// The line number of the source code where the error was reported
    /// </summary>
    public int? Line { get; }
    /// <summary>
    /// The name of the source code routine reporting the error
    /// </summary>
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

    /// <summary>
    /// <para>
    /// Generic decoder for messages that contains similarly structured information messages. The
    /// contents are 1 or more error code and String value pairs where the key is a
    /// <see cref="byte"/> and the value is a CString.
    /// </para>
    /// <a href="https://www.postgresql.org/docs/current/protocol-error-fields.html">docs</a>
    /// </summary>
    /// <param name="buffer">Buffer of message contents to parse</param>
    /// <returns>Deserialized information response object</returns>
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
               ?? throw new PgException(
                   $"InformationResponse message missing expected field '{desiredField}");
    }
}
