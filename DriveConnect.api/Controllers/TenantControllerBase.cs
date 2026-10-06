using DriveConnect.infrastructure.Data;
using DriveConnect.infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DriveConnect.api.Controllers;

[ApiController]
[Authorize(Roles = "Admin,Staff")]
public abstract class TenantControllerBase : ControllerBase
{
    private readonly ITenantDbContextFactory _tenantDbFactory;

    protected TenantControllerBase(ITenantDbContextFactory tenantDbFactory)
    {
        _tenantDbFactory = tenantDbFactory;
    }

    protected Task<TenantDriveConnectDbContext> GetTenantDbAsync(int companyId)
    {
        return _tenantDbFactory.CreateAsync(companyId);
    }
}
