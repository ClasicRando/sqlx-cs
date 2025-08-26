using Sqlx.Postgres.Exceptions;

namespace Sqlx.Postgres.Message.Backend.Information;

public sealed class InvalidInformationResponse : PgException
{
    internal InvalidInformationResponse(byte missingField)
        : base($"InformationResponse message missing expected field '{missingField}") {}
}
