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
        _writeBuffer.StartWritingLengthPrefixed();
        _writeBuffer.WriteShort(MajorVersionNo);
        _writeBuffer.WriteShort(MinorVersionNo);
        _writeBuffer.WriteCString(UserProperty);
        _writeBuffer.WriteCString(options.Username);
        if (options.Database is not null)
        {
            _writeBuffer.WriteCString(DatabaseProperty);
            _writeBuffer.WriteCString(options.Database);
        }
        _writeBuffer.WriteCString(ExtraFloatDigitsProperty);
        _writeBuffer.WriteCString(extractFloatPointsStr);
        if (options.CurrentSchema is not null)
        {
            _writeBuffer.WriteCString(SearchPathProperty);
            _writeBuffer.WriteCString(options.CurrentSchema);
        }
        _writeBuffer.WriteCString(ApplicationNameProperty);
        _writeBuffer.WriteCString(options.ApplicationName);
        _writeBuffer.WriteCString(StatementTimeoutProperty);
        _writeBuffer.WriteCString(queryTimeoutStr);
        _writeBuffer.WriteBytes(DefaultProperties.AsSpan());
        _writeBuffer.WriteByte(0);
        _writeBuffer.FinishWritingLengthPrefixed(includeLength: true);
        return Flush(cancellationToken);
    }
    
    public void WriteBindMessage(
        ReadOnlySpan<char> portal,
        ReadOnlySpan<char> statementName,
        short argumentsCount,
        ReadOnlySpan<byte> arguments)
    {
        _writeBuffer.WriteCode(PgFrontendMessageType.Bind);
        _writeBuffer.StartWritingLengthPrefixed();
        _writeBuffer.WriteCString(portal);
        _writeBuffer.WriteCString(statementName);
        _writeBuffer.WriteShort(1);
        _writeBuffer.WriteShort(1);
        _writeBuffer.WriteShort(argumentsCount);
        _writeBuffer.WriteBytes(arguments);
        _writeBuffer.WriteShort(1);
        _writeBuffer.WriteShort(1);
        _writeBuffer.FinishWritingLengthPrefixed(includeLength: true);
    }

    private Task SendSaslInitialMessage(
        ReadOnlySpan<char> mechanism,
        ReadOnlySpan<char> saslData,
        CancellationToken cancellationToken)
    {
        _writeBuffer.WriteCode(PgFrontendMessageType.Password);
        var length = mechanism.Length + sizeof(byte)
            + sizeof(int)
            + saslData.Length
            + sizeof(int);
        _writeBuffer.WriteInt(length);
        _writeBuffer.WriteCString(mechanism);
        _writeBuffer.WriteInt(saslData.Length);
        _writeBuffer.WriteString(saslData);
        return Flush(cancellationToken);
    }

    private Task SendSaslResponseMessage(
        ReadOnlySpan<char> clientMessage,
        CancellationToken cancellationToken)
    {
        _writeBuffer.WriteCode(PgFrontendMessageType.Password);
        var length = clientMessage.Length + sizeof(int);
        _writeBuffer.WriteInt(length);
        _writeBuffer.WriteString(clientMessage);
        return Flush(cancellationToken);
    }

    private Task SendSimplePasswordMessage(
        ReadOnlySpan<byte> passwordBytes,
        CancellationToken cancellationToken)
    {
        _writeBuffer.WriteCode(PgFrontendMessageType.Password);
        var length = passwordBytes.Length + sizeof(byte) + sizeof(int);
        _writeBuffer.WriteInt(length);
        _writeBuffer.WriteBytes(passwordBytes);
        _writeBuffer.WriteByte(0);
        return Flush(cancellationToken);
    }

    public void WriteExecuteMessage(ReadOnlySpan<char> portalName, int maxRowCount)
    {
        _writeBuffer.WriteCode(PgFrontendMessageType.Execute);
        _writeBuffer.StartWritingLengthPrefixed();
        _writeBuffer.WriteCString(portalName);
        _writeBuffer.WriteInt(maxRowCount);
        _writeBuffer.FinishWritingLengthPrefixed(includeLength: true);
    }

    public void WriteCloseMessage(MessageTarget messageTarget, ReadOnlySpan<char> targetName)
    {
        _writeBuffer.WriteCode(PgFrontendMessageType.Close);
        _writeBuffer.StartWritingLengthPrefixed();
        _writeBuffer.WriteByte((byte)messageTarget);
        _writeBuffer.WriteCString(targetName);
        _writeBuffer.FinishWritingLengthPrefixed(includeLength: true);
    }

    public void WriteParseMessage(
        ReadOnlySpan<char> preparedStatementName,
        ReadOnlySpan<char> query,
        IReadOnlyList<PgType> pgTypes)
    {
        _writeBuffer.WriteCode(PgFrontendMessageType.Parse);
        _writeBuffer.StartWritingLengthPrefixed();
        _writeBuffer.WriteCString(preparedStatementName);
        _writeBuffer.WriteCString(query);
        _writeBuffer.WriteShort((short)pgTypes.Count);
        foreach (PgType pgType in pgTypes)
        {
            _writeBuffer.WriteInt(pgType.TypeOid);
        }
        _writeBuffer.FinishWritingLengthPrefixed(includeLength: true);
    }

    public void WriteDescribeMessage(
        MessageTarget messageTarget,
        ReadOnlySpan<char> preparedStatementName)
    {
        _writeBuffer.WriteCode(PgFrontendMessageType.Describe);
        _writeBuffer.StartWritingLengthPrefixed();
        _writeBuffer.WriteByte((byte)messageTarget);
        _writeBuffer.WriteCString(preparedStatementName);
        _writeBuffer.FinishWritingLengthPrefixed(includeLength: true);
    }

    public Task SendQueryMessage(ReadOnlySpan<char> query, CancellationToken cancellationToken)
    {
        _writeBuffer.WriteCode(PgFrontendMessageType.Query);
        _writeBuffer.StartWritingLengthPrefixed();
        _writeBuffer.WriteCString(query);
        _writeBuffer.FinishWritingLengthPrefixed(includeLength: true);
        return Flush(cancellationToken);
    }

    public Task SendSyncMessage(CancellationToken cancellationToken)
    {
        _writeBuffer.WriteCode(PgFrontendMessageType.Sync);
        _writeBuffer.WriteInt(4);
        return Flush(cancellationToken);
    }

    public void WriteCopyDataMessage(ReadOnlySpan<byte> data)
    {
        _writeBuffer.WriteCode(PgFrontendMessageType.CopyData);
        _writeBuffer.WriteInt(data.Length + sizeof(int));
        _writeBuffer.WriteBytes(data);
    }

    public Task SendTerminate()
    {
        _writeBuffer.WriteCode(PgFrontendMessageType.Terminate);
        _writeBuffer.WriteInt(sizeof(int));
        return Flush(CancellationToken.None);
    }
}
