namespace Sqlx.Postgres.Copy;

/// <summary>
/// Marker interface for <see cref="ICopyStatement"/>s that send or receive binary data instead of
/// text based formats
/// </summary>
public interface ICopyBinary : ICopyStatement;
