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

    private ValueTask SendStartupMessage(PgConnectOptions options, CancellationToken cancellationToken)
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

        WriteBuffer.WriteInt(length);
        WriteBuffer.WriteShort(MajorVersionNo);
        WriteBuffer.WriteShort(MinorVersionNo);
        WriteBuffer.WriteCString(UserProperty);
        WriteBuffer.WriteCString(options.Username);
        if (options.Database is not null)
        {
            WriteBuffer.WriteCString(DatabaseProperty);
            WriteBuffer.WriteCString(options.Database);
        }

        WriteBuffer.WriteCString(ExtraFloatDigitsProperty);
        WriteBuffer.WriteCString(extractFloatPointsStr);
        if (options.CurrentSchema is not null)
        {
            WriteBuffer.WriteCString(SearchPathProperty);
            WriteBuffer.WriteCString(options.CurrentSchema);
        }

        WriteBuffer.WriteCString(ApplicationNameProperty);
        WriteBuffer.WriteCString(options.ApplicationName);
        WriteBuffer.WriteCString(StatementTimeoutProperty);
        WriteBuffer.WriteCString(queryTimeoutStr);
        WriteBuffer.WriteBytes(DefaultProperties.AsSpan());
        WriteBuffer.WriteByte(0);
        return FlushStream(cancellationToken);
    }

    private void WriteBindMessage(
        string portal,
        string statementName,
        short argumentsCount,
        in ReadOnlySpan<byte> arguments)
    {
        WriteBuffer.WriteCode(PgFrontendMessageType.Bind);
        var length = portal.Length + sizeof(byte) + statementName.Length + sizeof(byte) +
                     sizeof(short) + sizeof(short) + sizeof(short) + arguments.Length +
                     sizeof(short) + sizeof(short) + sizeof(int);
        WriteBuffer.WriteInt(length);
        WriteBuffer.WriteCString(portal);
        WriteBuffer.WriteCString(statementName);
        WriteBuffer.WriteShort(1);
        WriteBuffer.WriteShort(1);
        WriteBuffer.WriteShort(argumentsCount);
        WriteBuffer.WriteBytes(arguments);
        WriteBuffer.WriteShort(1);
        WriteBuffer.WriteShort(1);
    }

    private ValueTask SendSaslInitialMessage(
        in ReadOnlySpan<char> mechanism,
        in ReadOnlySpan<char> saslData,
        CancellationToken cancellationToken)
    {
        WriteBuffer.WriteCode(PgFrontendMessageType.Password);
        var length = mechanism.Length + sizeof(byte) + sizeof(int) + saslData.Length + sizeof(int);
        WriteBuffer.WriteInt(length);
        WriteBuffer.WriteCString(mechanism);
        WriteBuffer.WriteInt(saslData.Length);
        WriteBuffer.WriteString(saslData);
        return FlushStream(cancellationToken);
    }

    private ValueTask SendSaslResponseMessage(
        in ReadOnlySpan<char> clientMessage,
        CancellationToken cancellationToken)
    {
        WriteBuffer.WriteCode(PgFrontendMessageType.Password);
        var length = clientMessage.Length + sizeof(int);
        WriteBuffer.WriteInt(length);
        WriteBuffer.WriteString(clientMessage);
        return FlushStream(cancellationToken);
    }

    private ValueTask SendSimplePasswordMessage(
        in ReadOnlySpan<byte> passwordBytes,
        CancellationToken cancellationToken)
    {
        WriteBuffer.WriteCode(PgFrontendMessageType.Password);
        var length = passwordBytes.Length + sizeof(byte) + sizeof(int);
        WriteBuffer.WriteInt(length);
        WriteBuffer.WriteBytes(passwordBytes);
        WriteBuffer.WriteByte(0);
        return FlushStream(cancellationToken);
    }

    private void WriteExecuteMessage(in ReadOnlySpan<char> portalName, int maxRowCount)
    {
        WriteBuffer.WriteCode(PgFrontendMessageType.Execute);
        var length = portalName.Length + sizeof(byte) + sizeof(int) + sizeof(int);
        WriteBuffer.WriteInt(length);
        WriteBuffer.WriteCString(portalName);
        WriteBuffer.WriteInt(maxRowCount);
    }

    private void WriteCloseMessage(MessageTarget messageTarget, in ReadOnlySpan<char> targetName)
    {
        WriteBuffer.WriteCode(PgFrontendMessageType.Close);
        var length = sizeof(byte) + targetName.Length + sizeof(byte) + sizeof(int);
        WriteBuffer.WriteInt(length);
        WriteBuffer.WriteByte((byte)messageTarget);
        WriteBuffer.WriteCString(targetName);
    }

    private void WriteParseMessage(
        in ReadOnlySpan<char> preparedStatementName,
        in ReadOnlySpan<char> query,
        IReadOnlyList<PgTypeInfo> pgTypes)
    {
        WriteBuffer.WriteCode(PgFrontendMessageType.Parse);
        var queryByteLength = Charsets.Default.GetByteCount(query);
        var length = preparedStatementName.Length + sizeof(byte) +
                     queryByteLength + sizeof(byte) +
                     sizeof(short) + (sizeof(uint) * pgTypes.Count) + sizeof(int);
        WriteBuffer.WriteInt(length);
        WriteBuffer.WriteCString(preparedStatementName);
        
        var span = WriteBuffer.GetSpan(queryByteLength);
        Charsets.Default.GetBytes(query, span);
        WriteBuffer.Advance(queryByteLength);
        WriteBuffer.WriteByte(0);
        
        WriteBuffer.WriteShort((short)pgTypes.Count);
        foreach (PgTypeInfo pgType in pgTypes)
        {
            WriteBuffer.WriteUInt(pgType.TypeOid.Inner);
        }
    }

    private void WriteDescribeMessage(
        MessageTarget messageTarget,
        in ReadOnlySpan<char> preparedStatementName)
    {
        WriteBuffer.WriteCode(PgFrontendMessageType.Describe);
        var length = sizeof(byte) + preparedStatementName.Length + sizeof(byte) + sizeof(int);
        WriteBuffer.WriteInt(length);
        WriteBuffer.WriteByte((byte)messageTarget);
        WriteBuffer.WriteCString(preparedStatementName);
    }

    private ValueTask SendQueryMessage(in ReadOnlySpan<char> query, CancellationToken cancellationToken)
    {
        WriteBuffer.WriteCode(PgFrontendMessageType.Query);
        var queryByteLength = Charsets.Default.GetByteCount(query);
        var length = queryByteLength + sizeof(byte) + sizeof(int);
        WriteBuffer.WriteInt(length);
        
        var span = WriteBuffer.GetSpan(queryByteLength);
        Charsets.Default.GetBytes(query, span);
        WriteBuffer.Advance(queryByteLength);
        WriteBuffer.WriteByte(0);
        
        return FlushStream(cancellationToken);
    }

    private ValueTask SendSyncMessage(CancellationToken cancellationToken)
    {
        WriteBuffer.WriteCode(PgFrontendMessageType.Sync);
        WriteBuffer.WriteInt(4);
        return FlushStream(cancellationToken);
    }

    private void WriteCopyDataMessage(in ReadOnlySpan<byte> data)
    {
        WriteBuffer.WriteCode(PgFrontendMessageType.CopyData);
        WriteBuffer.WriteInt(data.Length + sizeof(int));
        WriteBuffer.WriteBytes(data);
    }

    private ValueTask SendCopyDoneMessage(CancellationToken cancellationToken)
    {
        WriteBuffer.WriteCode(PgFrontendMessageType.CopyDone);
        WriteBuffer.WriteInt(sizeof(int));
        return FlushStream(cancellationToken);
    }

    private ValueTask SendCopyFailMessage(
        in ReadOnlySpan<char> message,
        CancellationToken cancellationToken)
    {
        WriteBuffer.WriteCode(PgFrontendMessageType.CopyFail);
        var length = Charsets.Default.GetByteCount(message) + sizeof(int);
        WriteBuffer.WriteInt(length);
        WriteBuffer.WriteCString(message);
        return FlushStream(cancellationToken);
    }

    private ValueTask SendTerminate()
    {
        WriteBuffer.WriteCode(PgFrontendMessageType.Terminate);
        WriteBuffer.WriteInt(sizeof(int));
        return FlushStream(CancellationToken.None);
    }
}
