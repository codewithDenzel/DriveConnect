using System.Security.Claims;
using DriveConnect.domain.DTO;
using DriveConnect.infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DriveConnect.api.Controllers;

[ApiController]
[Authorize(Roles = "Admin,Staff")]
[Route("tenant/{companyId:int}/features")]
public sealed class TenantFeaturesController : ControllerBase
{
    private readonly ITenantPlanService _tenantPlanService;

    public TenantFeaturesController(ITenantPlanService tenantPlanService)
    {
        _tenantPlanService = tenantPlanService;
    }

    [HttpGet]
    public async Task<ActionResult<TenantPlanFeatures>> Get(int companyId)
    {
        var claim = User.FindFirstValue("companyId");

        if (!int.TryParse(claim, out var userCompanyId) || userCompanyId != companyId)
            return Forbid();

        return Ok(await _tenantPlanService.GetFeaturesAsync(companyId));
    }
}
