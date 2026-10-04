using DriveConnect.api.Services;
using DriveConnect.infrastructure.Data;
using Microsoft.AspNetCore.Mvc;

namespace DriveConnect.api.Controllers;

[ApiController]
public abstract class TenantControllerBase : ControllerBase
{
    private readonly TenantDbContextFactory _tenantDbFactory;

    protected TenantControllerBase(TenantDbContextFactory tenantDbFactory)
    {
        _tenantDbFactory = tenantDbFactory;
    }

    protected Task<TenantDriveConnectDbContext> GetTenantDbAsync(int companyId)
    {
        return _tenantDbFactory.CreateAsync(companyId);
    }
}
