using System.Buffers;
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

        Writer.WriteInt(length);
        Writer.WriteShort(MajorVersionNo);
        Writer.WriteShort(MinorVersionNo);
        Writer.WriteCString(UserProperty);
        Writer.WriteCString(options.Username);
        if (options.Database is not null)
        {
            Writer.WriteCString(DatabaseProperty);
            Writer.WriteCString(options.Database);
        }

        Writer.WriteCString(ExtraFloatDigitsProperty);
        Writer.WriteCString(extractFloatPointsStr);
        if (options.CurrentSchema is not null)
        {
            Writer.WriteCString(SearchPathProperty);
            Writer.WriteCString(options.CurrentSchema);
        }

        Writer.WriteCString(ApplicationNameProperty);
        Writer.WriteCString(options.ApplicationName);
        Writer.WriteCString(StatementTimeoutProperty);
        Writer.WriteCString(queryTimeoutStr);
        Writer.Write(DefaultProperties.AsSpan());
        Writer.WriteByte(0);
        return FlushStream(cancellationToken);
    }

    private void WriteBindMessage(
        string portal,
        string statementName,
        short argumentsCount,
        in ReadOnlySpan<byte> arguments)
    {
        Writer.WriteCode(PgFrontendMessageType.Bind);
        var length = portal.Length + sizeof(byte) + statementName.Length + sizeof(byte) +
                     sizeof(short) + sizeof(short) + sizeof(short) + arguments.Length +
                     sizeof(short) + sizeof(short) + sizeof(int);
        Writer.WriteInt(length);
        Writer.WriteCString(portal);
        Writer.WriteCString(statementName);
        Writer.WriteShort(1);
        Writer.WriteShort(1);
        Writer.WriteShort(argumentsCount);
        Writer.Write(arguments);
        Writer.WriteShort(1);
        Writer.WriteShort(1);
    }

    private ValueTask SendSaslInitialMessage(
        in ReadOnlySpan<char> mechanism,
        in ReadOnlySpan<char> saslData,
        CancellationToken cancellationToken)
    {
        Writer.WriteCode(PgFrontendMessageType.Password);
        var length = mechanism.Length + sizeof(byte) + sizeof(int) + saslData.Length + sizeof(int);
        Writer.WriteInt(length);
        Writer.WriteCString(mechanism);
        Writer.WriteInt(saslData.Length);
        Writer.WriteString(saslData);
        return FlushStream(cancellationToken);
    }

    private ValueTask SendSaslResponseMessage(
        in ReadOnlySpan<char> clientMessage,
        CancellationToken cancellationToken)
    {
        Writer.WriteCode(PgFrontendMessageType.Password);
        var length = clientMessage.Length + sizeof(int);
        Writer.WriteInt(length);
        Writer.WriteString(clientMessage);
        return FlushStream(cancellationToken);
    }

    private ValueTask SendSimplePasswordMessage(
        in ReadOnlySpan<byte> passwordBytes,
        CancellationToken cancellationToken)
    {
        Writer.WriteCode(PgFrontendMessageType.Password);
        var length = passwordBytes.Length + sizeof(byte) + sizeof(int);
        Writer.WriteInt(length);
        Writer.Write(passwordBytes);
        Writer.WriteByte(0);
        return FlushStream(cancellationToken);
    }

    private void WriteExecuteMessage(in ReadOnlySpan<char> portalName, int maxRowCount)
    {
        Writer.WriteCode(PgFrontendMessageType.Execute);
        var length = portalName.Length + sizeof(byte) + sizeof(int) + sizeof(int);
        Writer.WriteInt(length);
        Writer.WriteCString(portalName);
        Writer.WriteInt(maxRowCount);
    }

    private void WriteCloseMessage(MessageTarget messageTarget, in ReadOnlySpan<char> targetName)
    {
        Writer.WriteCode(PgFrontendMessageType.Close);
        var length = sizeof(byte) + targetName.Length + sizeof(byte) + sizeof(int);
        Writer.WriteInt(length);
        Writer.WriteByte((byte)messageTarget);
        Writer.WriteCString(targetName);
    }

    private void WriteParseMessage(
        in ReadOnlySpan<char> preparedStatementName,
        in ReadOnlySpan<char> query,
        IReadOnlyList<PgTypeInfo> pgTypes)
    {
        Writer.WriteCode(PgFrontendMessageType.Parse);
        var queryByteLength = Charsets.Default.GetByteCount(query);
        var length = preparedStatementName.Length + sizeof(byte) +
                     queryByteLength + sizeof(byte) +
                     sizeof(short) + (sizeof(uint) * pgTypes.Count) + sizeof(int);
        Writer.WriteInt(length);
        Writer.WriteCString(preparedStatementName);
        
        var span = Writer.GetSpan(queryByteLength);
        Charsets.Default.GetBytes(query, span);
        Writer.Advance(queryByteLength);
        Writer.WriteByte(0);
        
        Writer.WriteShort((short)pgTypes.Count);
        foreach (PgTypeInfo pgType in pgTypes)
        {
            Writer.WriteUInt(pgType.TypeOid.Inner);
        }
    }

    private void WriteDescribeMessage(
        MessageTarget messageTarget,
        in ReadOnlySpan<char> preparedStatementName)
    {
        Writer.WriteCode(PgFrontendMessageType.Describe);
        var length = sizeof(byte) + preparedStatementName.Length + sizeof(byte) + sizeof(int);
        Writer.WriteInt(length);
        Writer.WriteByte((byte)messageTarget);
        Writer.WriteCString(preparedStatementName);
    }

    private ValueTask SendQueryMessage(in ReadOnlySpan<char> query, CancellationToken cancellationToken)
    {
        Writer.WriteCode(PgFrontendMessageType.Query);
        var queryByteLength = Charsets.Default.GetByteCount(query);
        var length = queryByteLength + sizeof(byte) + sizeof(int);
        Writer.WriteInt(length);
        
        var span = Writer.GetSpan(queryByteLength);
        Charsets.Default.GetBytes(query, span);
        Writer.Advance(queryByteLength);
        Writer.WriteByte(0);
        
        return FlushStream(cancellationToken);
    }

    private ValueTask SendSyncMessage(CancellationToken cancellationToken)
    {
        Writer.WriteCode(PgFrontendMessageType.Sync);
        Writer.WriteInt(4);
        return FlushStream(cancellationToken);
    }

    private void WriteCopyDataMessage(in ReadOnlySpan<byte> data)
    {
        Writer.WriteCode(PgFrontendMessageType.CopyData);
        Writer.WriteInt(data.Length + sizeof(int));
        Writer.Write(data);
    }

    private ValueTask SendCopyDoneMessage(CancellationToken cancellationToken)
    {
        Writer.WriteCode(PgFrontendMessageType.CopyDone);
        Writer.WriteInt(sizeof(int));
        return FlushStream(cancellationToken);
    }

    private ValueTask SendCopyFailMessage(
        in ReadOnlySpan<char> message,
        CancellationToken cancellationToken)
    {
        Writer.WriteCode(PgFrontendMessageType.CopyFail);
        var length = Charsets.Default.GetByteCount(message) + sizeof(int);
        Writer.WriteInt(length);
        Writer.WriteCString(message);
        return FlushStream(cancellationToken);
    }

    private ValueTask SendTerminate()
    {
        Writer.WriteCode(PgFrontendMessageType.Terminate);
        Writer.WriteInt(sizeof(int));
        return FlushStream(CancellationToken.None);
    }
}
