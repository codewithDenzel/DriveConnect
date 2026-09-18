using System.Threading.Tasks;
using DriveConnect.infrastructure.Data;

namespace DriveConnect.infrastructure.Services
{
    public interface ITenantDbContextFactory
    {
        Task<TenantDriveConnectDbContext> CreateAsync(int companyId);
    }
}