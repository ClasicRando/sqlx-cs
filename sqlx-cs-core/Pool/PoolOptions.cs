namespace Sqlx.Core.Pool;

public record PoolOptions
{
    public int MaxConnections { get; init; } = 10;

    public int MinIdleConnections
    {
        get;
        init => field = value switch
        {
            0 => MaxConnections,
            < 0 => throw new ArgumentException("Cannot be negative", nameof(MinIdleConnections)),
            _ => value,
        };
    }

    public TimeSpan AcquireTimeout
    {
        get;
        init => field = CheckValidTimeSpan(value, nameof(AcquireTimeout));
    } = Timeout.InfiniteTimeSpan;

    public TimeSpan IdleTimeout
    {
        get;
        init => field = CheckValidTimeSpan(value, nameof(IdleTimeout));
    } = new(0, 5, 0);

    public TimeSpan IdleCleanupInterval
    {
        get;
        init => field = CheckValidTimeSpan(value, nameof(IdleTimeout));
    } = new(0, 0, 10);

    public TimeSpan IdleKeepAliveInterval
    {
        get;
        init => field = CheckValidTimeSpan(value, nameof(IdleKeepAliveInterval));
    } = Timeout.InfiniteTimeSpan;

    public bool ValidateOnReturn { get; init; } = true;

    public TimeSpan MaxLifetime
    {
        get;
        init => field = CheckValidTimeSpan(value, nameof(MaxLifetime));
    } = new(0, 30, 0);

    private static TimeSpan CheckValidTimeSpan(TimeSpan value, string propertyName)
    {
        return value > TimeSpan.Zero || value == Timeout.InfiniteTimeSpan
            ? value
            : throw new ArgumentException("Cannot be negative or zero", propertyName);
    }
}
