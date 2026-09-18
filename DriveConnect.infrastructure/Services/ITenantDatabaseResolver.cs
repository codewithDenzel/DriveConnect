using System.Threading.Tasks;

namespace DriveConnect.infrastructure.Services
{
    public interface ITenantDatabaseResolver
    {
        Task<TenantDatabaseInfo> GetDatabaseInfoAsync(int companyId);
    }

    public class TenantDatabaseInfo
    {
        public string ServerName { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;
        public string CredentialKey { get; set; } = string.Empty;
    }
}