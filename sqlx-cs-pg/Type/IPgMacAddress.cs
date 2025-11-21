using System.Net.NetworkInformation;

namespace Sqlx.Postgres.Type;

public interface IPgMacAddress
{
    PhysicalAddress ToPhysicalAddress();
}
