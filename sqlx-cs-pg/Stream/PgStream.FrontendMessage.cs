using Sqlx.Core;
using Sqlx.Postgres.Buffer;
using Sqlx.Postgres.Connection;
using Sqlx.Postgres.Message.Frontend;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Stream;

internal sealed partial class PgStream
{
    private const int MajorVersionNo = 3;
    private const int MinorVersionNo = 0;
    private const string UserProperty = "user";
    private const string DatabaseProperty = "database";
    private const string SearchPathProperty = "search_path";
    private const string ApplicationNameProperty = "application_name";
    private const string ExtraFloatDigitsProperty = "extra_float_digits";
    private const string StatementTimeoutProperty = "statement_timeout";
    private const string ClientEncodingProperty = "client_encoding";
    private const string DefaultClientEncoding = "UTF-8";
    private const string DateStyleProperty = "DateStyle";
    private const string DefaultDateStyle = "ISO";
    private const string IntervalStyleProperty = "intervalstyle";
    private const string DefaultIntervalStyle = "iso_8601";
    private const string TimeZoneProperty = "TimeZone";
    private const string DefaultTimeZone = "UTC";
    private const string ByteaOutputProperty = "bytea_output";
    private const string DefaultByteaOutput = "hex";
    private const string DefaultPropertiesStr = $"{ClientEncodingProperty}\0{DefaultClientEncoding}\0" +
                                                $"{DateStyleProperty}\0{DefaultDateStyle}\0" +
                                                $"{IntervalStyleProperty}\0{DefaultIntervalStyle}\0" +
                                                $"{TimeZoneProperty}\0{DefaultTimeZone}\0" +
                                                $"{ByteaOutputProperty}\0{DefaultByteaOutput}\0";
    private static readonly byte[] DefaultProperties = Charsets.Default.GetBytes(DefaultPropertiesStr);
    
    private Task SendStartupMessage(PgConnectOptions options, CancellationToken cancellationToken)
    {
        var extractFloatPointsStr = options.ExtraFloatPoints.ToString();
        var queryTimeout = int.Max((int)options.QueryTimeout.TotalMilliseconds, 0);
        var queryTimeoutStr = queryTimeout.ToString();
        var length = sizeof(short)
            + sizeof(short)
            + UserProperty.Length + sizeof(byte)
            + Charsets.Default.GetByteCount(options.Username) + sizeof(byte)
            + (options.Database is not null ? DatabaseProperty.Length + sizeof(byte) + Charsets.Default.GetByteCount(options.Database) + sizeof(byte) : 0)
            + ExtraFloatDigitsProperty.Length + sizeof(byte)
            + extractFloatPointsStr.Length + sizeof(byte)
            + (options.CurrentSchema is not null ? SearchPathProperty.Length + sizeof(byte) + Charsets.Default.GetByteCount(options.CurrentSchema) + sizeof(byte) : 0)
            + ApplicationNameProperty.Length + sizeof(byte)
            + Charsets.Default.GetByteCount(options.ApplicationName) + sizeof(byte)
            + StatementTimeoutProperty.Length + sizeof(byte)
            + queryTimeoutStr.Length + sizeof(byte)
            + DefaultProperties.Length
            + sizeof(byte)
            + sizeof(int);
        _buffer.WriteInt(length);
        _buffer.WriteShort(MajorVersionNo);
        _buffer.WriteShort(MinorVersionNo);
        _buffer.WriteCString(UserProperty);
        _buffer.WriteCString(options.Username);
        if (options.Database is not null)
        {
            _buffer.WriteCString(DatabaseProperty);
            _buffer.WriteCString(options.Database);
        }
        _buffer.WriteCString(ExtraFloatDigitsProperty);
        _buffer.WriteCString(extractFloatPointsStr);
        if (options.CurrentSchema is not null)
        {
            _buffer.WriteCString(SearchPathProperty);
            _buffer.WriteCString(options.CurrentSchema);
        }
        _buffer.WriteCString(ApplicationNameProperty);
        _buffer.WriteCString(options.ApplicationName);
        _buffer.WriteCString(StatementTimeoutProperty);
        _buffer.WriteCString(queryTimeoutStr);
        _buffer.WriteBytes(DefaultProperties.AsSpan());
        _buffer.WriteByte(0);
        return Flush(cancellationToken);
    }
    
    public void WriteBindMessage(
        ReadOnlySpan<char> portal,
        ReadOnlySpan<char> statementName,
        short argumentsCount,
        ReadOnlySpan<byte> arguments)
    {
        _buffer.WriteCode(PgFrontendMessageType.Bind);
        var length = Charsets.Default.GetByteCount(portal) + sizeof(byte)
            + Charsets.Default.GetByteCount(statementName) + sizeof(byte)
            + sizeof(short)
            + sizeof(short)
            + sizeof(short)
            + arguments.Length
            + sizeof(short)
            + sizeof(short)
            + sizeof(int);
        _buffer.WriteInt(length);
        _buffer.WriteCString(portal);
        _buffer.WriteCString(statementName);
        _buffer.WriteShort(1);
        _buffer.WriteShort(1);
        _buffer.WriteShort(argumentsCount);
        _buffer.WriteBytes(arguments);
        _buffer.WriteShort(1);
        _buffer.WriteShort(1);
    }

    private Task SendSaslInitialMessage(
        ReadOnlySpan<char> mechanism,
        ReadOnlySpan<char> saslData,
        CancellationToken cancellationToken)
    {
        _buffer.WriteCode(PgFrontendMessageType.Password);
        var length = mechanism.Length + sizeof(byte)
            + sizeof(int)
            + saslData.Length
            + sizeof(int);
        _buffer.WriteInt(length);
        _buffer.WriteCString(mechanism);
        _buffer.WriteInt(saslData.Length);
        _buffer.WriteString(saslData);
        return Flush(cancellationToken);
    }

    private Task SendSaslResponseMessage(
        ReadOnlySpan<char> clientMessage,
        CancellationToken cancellationToken)
    {
        _buffer.WriteCode(PgFrontendMessageType.Password);
        var length = clientMessage.Length + sizeof(int);
        _buffer.WriteInt(length);
        _buffer.WriteString(clientMessage);
        return Flush(cancellationToken);
    }

    private Task SendSimplePasswordMessage(
        ReadOnlySpan<byte> passwordBytes,
        CancellationToken cancellationToken)
    {
        _buffer.WriteCode(PgFrontendMessageType.Password);
        var length = passwordBytes.Length + sizeof(byte) + sizeof(int);
        _buffer.WriteInt(length);
        _buffer.WriteBytes(passwordBytes);
        _buffer.WriteByte(0);
        return Flush(cancellationToken);
    }

    public void WriteExecuteMessage(ReadOnlySpan<char> portalName, int maxRowCount)
    {
        _buffer.WriteCode(PgFrontendMessageType.Execute);
        var length = Charsets.Default.GetByteCount(portalName) + sizeof(byte)
            + sizeof(int)
            + sizeof(int);
        _buffer.WriteInt(length);
        _buffer.WriteCString(portalName);
        _buffer.WriteInt(maxRowCount);
    }

    public void WriteCloseMessage(MessageTarget messageTarget, ReadOnlySpan<char> targetName)
    {
        _buffer.WriteCode(PgFrontendMessageType.Close);
        var length = sizeof(byte)
            + Charsets.Default.GetByteCount(targetName) + sizeof(byte)
            + sizeof(int);
        _buffer.WriteInt(length);
        _buffer.WriteByte((byte)messageTarget);
        _buffer.WriteCString(targetName);
    }

    public void WriteParseMessage(
        ReadOnlySpan<char> preparedStatementName,
        ReadOnlySpan<char> query,
        IReadOnlyList<PgType> pgTypes)
    {
        _buffer.WriteCode(PgFrontendMessageType.Parse);
        var length = Charsets.Default.GetByteCount(preparedStatementName) + sizeof(byte)
            + Charsets.Default.GetByteCount(query) + sizeof(byte)
            + sizeof(short)
            + (sizeof(int) * pgTypes.Count)
            + sizeof(int);
        _buffer.WriteInt(length);
        _buffer.WriteCString(preparedStatementName);
        _buffer.WriteCString(query);
        _buffer.WriteShort((short)pgTypes.Count);
        foreach (PgType pgType in pgTypes)
        {
            _buffer.WriteInt(pgType.TypeOid);
        }
    }

    public void WriteDescribeMessage(
        MessageTarget messageTarget,
        ReadOnlySpan<char> preparedStatementName)
    {
        _buffer.WriteCode(PgFrontendMessageType.Describe);
        var length = sizeof(byte)
            + Charsets.Default.GetByteCount(preparedStatementName) + sizeof(byte)
            + sizeof(int);
        _buffer.WriteInt(length);
        _buffer.WriteByte((byte)messageTarget);
        _buffer.WriteCString(preparedStatementName);
    }

    public Task SendQueryMessage(ReadOnlySpan<char> query, CancellationToken cancellationToken)
    {
        _buffer.WriteCode(PgFrontendMessageType.Query);
        var length = Charsets.Default.GetByteCount(query) + sizeof(byte) + sizeof(int);
        _buffer.WriteInt(length);
        _buffer.WriteCString(query);
        return Flush(cancellationToken);
    }

    public Task SendSyncMessage(CancellationToken cancellationToken)
    {
        _buffer.WriteCode(PgFrontendMessageType.Sync);
        _buffer.WriteInt(4);
        return Flush(cancellationToken);
    }

    public void WriteCopyDataMessage(ReadOnlySpan<byte> data)
    {
        _buffer.WriteCode(PgFrontendMessageType.CopyData);
        _buffer.WriteInt(data.Length + sizeof(int));
        _buffer.WriteBytes(data);
    }
}
