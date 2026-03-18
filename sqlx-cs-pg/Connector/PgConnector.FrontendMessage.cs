using System.Buffers;
using System.Globalization;
using Sqlx.Core;
using Sqlx.Core.Buffer;
using Sqlx.Postgres.Buffer;
using Sqlx.Postgres.Connection;
using Sqlx.Postgres.Message.Frontend;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Connector;

public sealed partial class PgConnector
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

    /// <summary>
    /// Write all buffered content to the <see cref="_asyncConnector"/> and resets it's write buffer
    /// for future data messages
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the async operation</param>
    private ValueTask FlushStream(CancellationToken cancellationToken)
    {
        return _asyncConnector.FlushWriteBuffer(cancellationToken);
    }

    private ValueTask SendStartupMessage(
        PgConnectOptions options,
        CancellationToken cancellationToken)
    {
        var writer = _asyncConnector.Writer;
        var extractFloatPointsStr = options.ExtraFloatPoints.ToString(CultureInfo.InvariantCulture);
        var queryTimeout = int.Max((int)options.QueryTimeout.TotalMilliseconds, 0);
        var queryTimeoutStr = queryTimeout.ToString(CultureInfo.InvariantCulture);
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

        writer.WriteInt(length);
        writer.WriteShort(MajorVersionNo);
        writer.WriteShort(MinorVersionNo);
        writer.WriteCString(UserProperty);
        writer.WriteCString(options.Username);
        if (options.Database is not null)
        {
            writer.WriteCString(DatabaseProperty);
            writer.WriteCString(options.Database);
        }

        writer.WriteCString(ExtraFloatDigitsProperty);
        writer.WriteCString(extractFloatPointsStr);
        if (options.CurrentSchema is not null)
        {
            writer.WriteCString(SearchPathProperty);
            writer.WriteCString(options.CurrentSchema);
        }

        writer.WriteCString(ApplicationNameProperty);
        writer.WriteCString(options.ApplicationName);
        writer.WriteCString(StatementTimeoutProperty);
        writer.WriteCString(queryTimeoutStr);
        writer.Write(DefaultProperties.AsSpan());
        writer.WriteByte(0);
        return FlushStream(cancellationToken);
    }

    private void WriteBindMessage(
        string portal,
        string statementName,
        short argumentsCount,
        in ReadOnlySpan<byte> arguments)
    {
        var writer = _asyncConnector.Writer;
        writer.WriteCode(PgFrontendMessageType.Bind);
        var length = portal.Length + sizeof(byte) + statementName.Length + sizeof(byte) +
                     sizeof(short) + sizeof(short) + sizeof(short) + arguments.Length +
                     sizeof(short) + sizeof(short) + sizeof(int);
        writer.WriteInt(length);
        writer.WriteCString(portal);
        writer.WriteCString(statementName);
        writer.WriteShort(1);
        writer.WriteShort(1);
        writer.WriteShort(argumentsCount);
        writer.Write(arguments);
        writer.WriteShort(1);
        writer.WriteShort(1);
    }

    private ValueTask SendSaslInitialMessage(
        in ReadOnlySpan<char> mechanism,
        in ReadOnlySpan<char> saslData,
        CancellationToken cancellationToken)
    {
        var writer = _asyncConnector.Writer;
        writer.WriteCode(PgFrontendMessageType.Password);
        var length = mechanism.Length + sizeof(byte) + sizeof(int) + saslData.Length + sizeof(int);
        writer.WriteInt(length);
        writer.WriteCString(mechanism);
        writer.WriteInt(saslData.Length);
        writer.WriteString(saslData);
        return FlushStream(cancellationToken);
    }

    private ValueTask SendSaslResponseMessage(
        in ReadOnlySpan<char> clientMessage,
        CancellationToken cancellationToken)
    {
        var writer = _asyncConnector.Writer;
        writer.WriteCode(PgFrontendMessageType.Password);
        var length = clientMessage.Length + sizeof(int);
        writer.WriteInt(length);
        writer.WriteString(clientMessage);
        return FlushStream(cancellationToken);
    }

    private ValueTask SendSimplePasswordMessage(
        in ReadOnlySpan<byte> passwordBytes,
        CancellationToken cancellationToken)
    {
        var writer = _asyncConnector.Writer;
        writer.WriteCode(PgFrontendMessageType.Password);
        var length = passwordBytes.Length + sizeof(byte) + sizeof(int);
        writer.WriteInt(length);
        writer.Write(passwordBytes);
        writer.WriteByte(0);
        return FlushStream(cancellationToken);
    }

    private void WriteExecuteMessage(in ReadOnlySpan<char> portalName, int maxRowCount)
    {
        var writer = _asyncConnector.Writer;
        writer.WriteCode(PgFrontendMessageType.Execute);
        var length = portalName.Length + sizeof(byte) + sizeof(int) + sizeof(int);
        writer.WriteInt(length);
        writer.WriteCString(portalName);
        writer.WriteInt(maxRowCount);
    }

    private void WriteCloseMessage(MessageTarget messageTarget, in ReadOnlySpan<char> targetName)
    {
        var writer = _asyncConnector.Writer;
        writer.WriteCode(PgFrontendMessageType.Close);
        var length = sizeof(byte) + targetName.Length + sizeof(byte) + sizeof(int);
        writer.WriteInt(length);
        writer.WriteByte((byte)messageTarget);
        writer.WriteCString(targetName);
    }

    private void WriteParseMessage(
        in ReadOnlySpan<char> preparedStatementName,
        in ReadOnlySpan<char> query,
        IReadOnlyList<PgTypeInfo> pgTypes)
    {
        var writer = _asyncConnector.Writer;
        writer.WriteCode(PgFrontendMessageType.Parse);
        var queryByteLength = Charsets.Default.GetByteCount(query);
        var length = preparedStatementName.Length + sizeof(byte) +
                     queryByteLength + sizeof(byte) +
                     sizeof(short) + (sizeof(uint) * pgTypes.Count) + sizeof(int);
        writer.WriteInt(length);
        writer.WriteCString(preparedStatementName);

        var span = writer.GetSpan(queryByteLength);
        Charsets.Default.GetBytes(query, span);
        writer.Advance(queryByteLength);
        writer.WriteByte(0);

        writer.WriteShort((short)pgTypes.Count);
        foreach (PgTypeInfo pgType in pgTypes)
        {
            writer.WriteUInt(pgType.TypeOid.Inner);
        }
    }

    private void WriteDescribeMessage(
        MessageTarget messageTarget,
        in ReadOnlySpan<char> preparedStatementName)
    {
        var writer = _asyncConnector.Writer;
        writer.WriteCode(PgFrontendMessageType.Describe);
        var length = sizeof(byte) + preparedStatementName.Length + sizeof(byte) + sizeof(int);
        writer.WriteInt(length);
        writer.WriteByte((byte)messageTarget);
        writer.WriteCString(preparedStatementName);
    }

    internal ValueTask SendQueryMessage(
        in ReadOnlySpan<char> query,
        CancellationToken cancellationToken)
    {
        var writer = _asyncConnector.Writer;
        writer.WriteCode(PgFrontendMessageType.Query);
        var queryByteLength = Charsets.Default.GetByteCount(query);
        var length = queryByteLength + sizeof(byte) + sizeof(int);
        writer.WriteInt(length);

        var span = writer.GetSpan(queryByteLength);
        Charsets.Default.GetBytes(query, span);
        writer.Advance(queryByteLength);
        writer.WriteByte(0);

        return FlushStream(cancellationToken);
    }

    private ValueTask SendSyncMessage(CancellationToken cancellationToken)
    {
        var writer = _asyncConnector.Writer;
        writer.WriteCode(PgFrontendMessageType.Sync);
        writer.WriteInt(4);
        return FlushStream(cancellationToken);
    }

    private void WriteCopyDataMessage(in ReadOnlySpan<byte> data)
    {
        var writer = _asyncConnector.Writer;
        writer.WriteCode(PgFrontendMessageType.CopyData);
        writer.WriteInt(data.Length + sizeof(int));
        writer.Write(data);
    }

    private ValueTask SendCopyDoneMessage(CancellationToken cancellationToken)
    {
        var writer = _asyncConnector.Writer;
        writer.WriteCode(PgFrontendMessageType.CopyDone);
        writer.WriteInt(sizeof(int));
        return FlushStream(cancellationToken);
    }

    private ValueTask SendCopyFailMessage(
        in ReadOnlySpan<char> message,
        CancellationToken cancellationToken)
    {
        var writer = _asyncConnector.Writer;
        writer.WriteCode(PgFrontendMessageType.CopyFail);
        var length = Charsets.Default.GetByteCount(message) + sizeof(int);
        writer.WriteInt(length);
        writer.WriteCString(message);
        return FlushStream(cancellationToken);
    }

    private ValueTask SendTerminate()
    {
        var writer = _asyncConnector.Writer;
        writer.WriteCode(PgFrontendMessageType.Terminate);
        writer.WriteInt(sizeof(int));
        return FlushStream(CancellationToken.None);
    }
}
