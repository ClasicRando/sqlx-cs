using Sqlx.Core;
using Sqlx.Core.Buffer;
using Sqlx.Postgres.Buffer;
using Sqlx.Postgres.Connection;
using Sqlx.Postgres.Message.Frontend;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Stream;

public sealed partial class PgStream
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

    private const string DefaultPropertiesStr =
        $"{ClientEncodingProperty}\0{DefaultClientEncoding}\0" +
        $"{DateStyleProperty}\0{DefaultDateStyle}\0" +
        $"{IntervalStyleProperty}\0{DefaultIntervalStyle}\0" +
        $"{TimeZoneProperty}\0{DefaultTimeZone}\0" +
        $"{ByteaOutputProperty}\0{DefaultByteaOutput}\0";

    private static readonly byte[] DefaultProperties =
        Charsets.Default.GetBytes(DefaultPropertiesStr);

    private Task SendStartupMessage(PgConnectOptions options, CancellationToken cancellationToken)
    {
        var extractFloatPointsStr = options.ExtraFloatPoints.ToString();
        var queryTimeout = int.Max((int)options.QueryTimeout.TotalMilliseconds, 0);
        var queryTimeoutStr = queryTimeout.ToString();
        var length = sizeof(short) + sizeof(short) + UserProperty.Length + sizeof(byte) +
                     Charsets.Default.GetByteCount(options.Username) + sizeof(byte) +
                     ExtraFloatDigitsProperty.Length + sizeof(byte) +
                     extractFloatPointsStr.Length + sizeof(byte) +
                     ApplicationNameProperty.Length + sizeof(byte) +
                     Charsets.Default.GetByteCount(options.ApplicationName) + sizeof(byte) +
                     StatementTimeoutProperty.Length + sizeof(byte) +
                     queryTimeoutStr.Length + sizeof(byte) +
                     DefaultProperties.Length + sizeof(byte) + sizeof(int);
        if (options.Database is not null)
        {
            length += DatabaseProperty.Length + sizeof(byte) +
                      Charsets.Default.GetByteCount(options.Database) + sizeof(byte);
        }

        if (options.CurrentSchema is not null)
        {
            length += SearchPathProperty.Length + sizeof(byte) +
                      Charsets.Default.GetByteCount(options.CurrentSchema) + sizeof(byte);
        }

        _writeBuffer.WriteInt(length);
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
        return Flush(cancellationToken);
    }

    private void WriteBindMessage(
        string portal,
        string statementName,
        short argumentsCount,
        in ReadOnlySpan<byte> arguments)
    {
        _writeBuffer.WriteCode(PgFrontendMessageType.Bind);
        var length = portal.Length + sizeof(byte) + statementName.Length + sizeof(byte) +
                     sizeof(short) + sizeof(short) + sizeof(short) + arguments.Length +
                     sizeof(short) + sizeof(short) + sizeof(int);
        _writeBuffer.WriteInt(length);
        _writeBuffer.WriteCString(portal);
        _writeBuffer.WriteCString(statementName);
        _writeBuffer.WriteShort(1);
        _writeBuffer.WriteShort(1);
        _writeBuffer.WriteShort(argumentsCount);
        _writeBuffer.WriteBytes(arguments);
        _writeBuffer.WriteShort(1);
        _writeBuffer.WriteShort(1);
    }

    private Task SendSaslInitialMessage(
        in ReadOnlySpan<char> mechanism,
        in ReadOnlySpan<char> saslData,
        CancellationToken cancellationToken)
    {
        _writeBuffer.WriteCode(PgFrontendMessageType.Password);
        var length = mechanism.Length + sizeof(byte) + sizeof(int) + saslData.Length + sizeof(int);
        _writeBuffer.WriteInt(length);
        _writeBuffer.WriteCString(mechanism);
        _writeBuffer.WriteInt(saslData.Length);
        _writeBuffer.WriteString(saslData);
        return Flush(cancellationToken);
    }

    private Task SendSaslResponseMessage(
        in ReadOnlySpan<char> clientMessage,
        CancellationToken cancellationToken)
    {
        _writeBuffer.WriteCode(PgFrontendMessageType.Password);
        var length = clientMessage.Length + sizeof(int);
        _writeBuffer.WriteInt(length);
        _writeBuffer.WriteString(clientMessage);
        return Flush(cancellationToken);
    }

    private Task SendSimplePasswordMessage(
        in ReadOnlySpan<byte> passwordBytes,
        CancellationToken cancellationToken)
    {
        _writeBuffer.WriteCode(PgFrontendMessageType.Password);
        var length = passwordBytes.Length + sizeof(byte) + sizeof(int);
        _writeBuffer.WriteInt(length);
        _writeBuffer.WriteBytes(passwordBytes);
        _writeBuffer.WriteByte(0);
        return Flush(cancellationToken);
    }

    private void WriteExecuteMessage(in ReadOnlySpan<char> portalName, int maxRowCount)
    {
        _writeBuffer.WriteCode(PgFrontendMessageType.Execute);
        var length = portalName.Length + sizeof(byte) + sizeof(int) + sizeof(int);
        _writeBuffer.WriteInt(length);
        _writeBuffer.WriteCString(portalName);
        _writeBuffer.WriteInt(maxRowCount);
    }

    private void WriteCloseMessage(MessageTarget messageTarget, in ReadOnlySpan<char> targetName)
    {
        _writeBuffer.WriteCode(PgFrontendMessageType.Close);
        var length = sizeof(byte) + targetName.Length + sizeof(byte) + sizeof(int);
        _writeBuffer.WriteInt(length);
        _writeBuffer.WriteByte((byte)messageTarget);
        _writeBuffer.WriteCString(targetName);
    }

    private void WriteParseMessage(
        in ReadOnlySpan<char> preparedStatementName,
        in ReadOnlySpan<char> query,
        IReadOnlyList<PgTypeInfo> pgTypes)
    {
        _writeBuffer.WriteCode(PgFrontendMessageType.Parse);
        var queryByteLength = Charsets.Default.GetByteCount(query);
        var length = preparedStatementName.Length + sizeof(byte) +
                     queryByteLength + sizeof(byte) +
                     sizeof(short) + (sizeof(uint) * pgTypes.Count) + sizeof(int);
        _writeBuffer.WriteInt(length);
        _writeBuffer.WriteCString(preparedStatementName);
        
        var span = _writeBuffer.GetSpan(queryByteLength);
        Charsets.Default.GetBytes(query, span);
        _writeBuffer.Advance(queryByteLength);
        _writeBuffer.WriteByte(0);
        
        _writeBuffer.WriteShort((short)pgTypes.Count);
        foreach (PgTypeInfo pgType in pgTypes)
        {
            _writeBuffer.WriteUInt(pgType.TypeOid.Inner);
        }
    }

    private void WriteDescribeMessage(
        MessageTarget messageTarget,
        in ReadOnlySpan<char> preparedStatementName)
    {
        _writeBuffer.WriteCode(PgFrontendMessageType.Describe);
        var length = sizeof(byte) + preparedStatementName.Length + sizeof(byte) + sizeof(int);
        _writeBuffer.WriteInt(length);
        _writeBuffer.WriteByte((byte)messageTarget);
        _writeBuffer.WriteCString(preparedStatementName);
    }

    private Task SendQueryMessage(in ReadOnlySpan<char> query, CancellationToken cancellationToken)
    {
        _writeBuffer.WriteCode(PgFrontendMessageType.Query);
        var queryByteLength = Charsets.Default.GetByteCount(query);
        var length = queryByteLength + sizeof(byte) + sizeof(int);
        _writeBuffer.WriteInt(length);
        
        var span = _writeBuffer.GetSpan(queryByteLength);
        Charsets.Default.GetBytes(query, span);
        _writeBuffer.Advance(queryByteLength);
        _writeBuffer.WriteByte(0);
        
        return Flush(cancellationToken);
    }

    private Task SendSyncMessage(CancellationToken cancellationToken)
    {
        _writeBuffer.WriteCode(PgFrontendMessageType.Sync);
        _writeBuffer.WriteInt(4);
        return Flush(cancellationToken);
    }

    private void WriteCopyDataMessage(in ReadOnlySpan<byte> data)
    {
        _writeBuffer.WriteCode(PgFrontendMessageType.CopyData);
        _writeBuffer.WriteInt(data.Length + sizeof(int));
        _writeBuffer.WriteBytes(data);
    }

    private Task SendCopyDoneMessage(CancellationToken cancellationToken)
    {
        _writeBuffer.WriteCode(PgFrontendMessageType.CopyDone);
        _writeBuffer.WriteInt(sizeof(int));
        return Flush(cancellationToken);
    }

    private Task SendCopyFailMessage(
        in ReadOnlySpan<char> message,
        CancellationToken cancellationToken)
    {
        _writeBuffer.WriteCode(PgFrontendMessageType.CopyFail);
        var length = Charsets.Default.GetByteCount(message) + sizeof(int);
        _writeBuffer.WriteInt(length);
        _writeBuffer.WriteCString(message);
        return Flush(cancellationToken);
    }

    private Task SendTerminate()
    {
        _writeBuffer.WriteCode(PgFrontendMessageType.Terminate);
        _writeBuffer.WriteInt(sizeof(int));
        return Flush(CancellationToken.None);
    }
}
