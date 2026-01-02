namespace Sqlx.Postgres.Copy;

/// <summary>
/// <see cref="ICopyStatement"/> implementation for copying to STDOUT as binary data extracted from
/// the query specified
/// </summary>
public record QueryToBinary : ICopyQuery, ICopyBinary
{
    public required string Query { get; init; }
    
    public string ToCopyQuery()
    {
        return $"COPY ({Query}) TO STDOUT WITH (FORMAT binary)";
    }
}
