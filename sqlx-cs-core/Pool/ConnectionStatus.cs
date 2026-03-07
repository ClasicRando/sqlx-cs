namespace Sqlx.Core.Pool;

/// <summary>
/// Various states of a connection
/// </summary>
public enum ConnectionStatus
{
    Connecting,
    Idle,
    Executing,
    Fetching,
    Broken,
    Closed,
}
