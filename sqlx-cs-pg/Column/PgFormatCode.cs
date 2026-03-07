namespace Sqlx.Postgres.Column;

/// <summary>
/// Column format codes sent by Postgres. <see cref="PgFormatCode.Text"/> is the default value as
/// per the Postgres docs where data is sent as a string representation of the value.
/// <see cref="PgFormatCode.Binary"/> is the default format used by this driver in most cases since
/// it generally leads to better performance and less memory allocated.
/// </summary>
public enum PgFormatCode
{
    Text = 0,
    Binary = 1,
}
