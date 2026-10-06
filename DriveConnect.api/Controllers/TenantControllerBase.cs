using System.Security.Claims;
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

    protected bool CanAccessCompany(int companyId)
    {
        var companyClaim = User.FindFirstValue("companyId");

        return int.TryParse(companyClaim, out var userCompanyId) &&
               userCompanyId == companyId;
    }

    protected bool IsAdminUser =>
        string.Equals(
            User.FindFirstValue(ClaimTypes.Role),
            "Admin",
            StringComparison.OrdinalIgnoreCase);

    protected bool IsStaffUser =>
        string.Equals(
            User.FindFirstValue(ClaimTypes.Role),
            "Staff",
            StringComparison.OrdinalIgnoreCase);

    protected bool TryGetCurrentBranchId(out int branchId)
    {
        var branchClaim = User.FindFirstValue("branchId");

        return int.TryParse(branchClaim, out branchId);
    }

    protected bool CanAccessBranch(int? recordBranchId)
    {
        if (IsAdminUser)
            return true;

        return IsStaffUser &&
               TryGetCurrentBranchId(out var branchId) &&
               recordBranchId == branchId;
    }
}
