using System.Security.Claims;
using DriveConnect.domain.Entities;
using DriveConnect.infrastructure.Data;
using DriveConnect.infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DriveConnect.api.Controllers;

[ApiController]
[Authorize(Roles = "Admin,Super Admin")]
[Route("companies/{companyId:int}/branches")]
public sealed class BranchesController : ControllerBase
{
    private readonly MasterDriveConnectDbContext _masterDb;
    private readonly ITenantPlanService _tenantPlanService;

    public BranchesController(
        MasterDriveConnectDbContext masterDb,
        ITenantPlanService tenantPlanService)
    {
        _masterDb = masterDb;
        _tenantPlanService = tenantPlanService;
    }

    private bool CanAccessCompany(int companyId)
    {
        var role = User.FindFirstValue(ClaimTypes.Role);

        if (string.Equals(role, "Super Admin", StringComparison.OrdinalIgnoreCase))
            return true;

        return int.TryParse(User.FindFirstValue("companyId"), out var userCompanyId)
            && userCompanyId == companyId;
    }

    [HttpGet]
    public async Task<ActionResult<List<Branch>>> GetAll(int companyId)
    {
        if (!CanAccessCompany(companyId))
            return Forbid();

        var companyExists = await _masterDb.Companies
            .AsNoTracking()
            .AnyAsync(x => x.CompanyId == companyId && x.IsActive);

        if (!companyExists)
            return NotFound("Company not found.");

        var branches = await _masterDb.Branches
            .AsNoTracking()
            .Where(x => x.CompanyId == companyId)
            .OrderBy(x => x.BranchName)
            .ToListAsync();

        return Ok(branches);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<Branch>> Create(int companyId, Branch branch)
    {
        if (!CanAccessCompany(companyId))
            return Forbid();

        if (!(await _tenantPlanService.GetFeaturesAsync(companyId)).CanUseBranching)
            return StatusCode(StatusCodes.Status403Forbidden, "Branch management requires the Pro Max plan.");


        if (string.IsNullOrWhiteSpace(branch.BranchCode) ||
            string.IsNullOrWhiteSpace(branch.BranchName))
        {
            return BadRequest("Branch code and branch name are required.");
        }

        var companyExists = await _masterDb.Companies
            .AsNoTracking()
            .AnyAsync(x => x.CompanyId == companyId && x.IsActive);

        if (!companyExists)
            return NotFound("Company not found.");

        var code = branch.BranchCode.Trim().ToUpperInvariant();
        var name = branch.BranchName.Trim();

        var duplicate = await _masterDb.Branches
            .AsNoTracking()
            .AnyAsync(x =>
                x.CompanyId == companyId &&
                (x.BranchCode == code || x.BranchName == name));

        if (duplicate)
            return Conflict("A branch with the same code or name already exists.");

        var newBranch = new Branch
        {
            CompanyId = companyId,
            BranchCode = code,
            BranchName = name,
            Address = string.IsNullOrWhiteSpace(branch.Address) ? null : branch.Address.Trim(),
            PhoneNumber = string.IsNullOrWhiteSpace(branch.PhoneNumber) ? null : branch.PhoneNumber.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _masterDb.Branches.Add(newBranch);
        await _masterDb.SaveChangesAsync();

        return Created($"/companies/{companyId}/branches/{newBranch.BranchId}", newBranch);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<Branch>> Update(int companyId, int id, Branch updated)
    {
        if (!CanAccessCompany(companyId))
            return Forbid();

        if (!(await _tenantPlanService.GetFeaturesAsync(companyId)).CanUseBranching)
            return StatusCode(StatusCodes.Status403Forbidden, "Branch management requires the Pro Max plan.");


        var branch = await _masterDb.Branches
            .FirstOrDefaultAsync(x => x.BranchId == id && x.CompanyId == companyId);

        if (branch == null)
            return NotFound("Branch not found.");

        if (string.IsNullOrWhiteSpace(updated.BranchCode) ||
            string.IsNullOrWhiteSpace(updated.BranchName))
        {
            return BadRequest("Branch code and branch name are required.");
        }

        var code = updated.BranchCode.Trim().ToUpperInvariant();
        var name = updated.BranchName.Trim();

        var duplicate = await _masterDb.Branches
            .AsNoTracking()
            .AnyAsync(x =>
                x.CompanyId == companyId &&
                x.BranchId != id &&
                (x.BranchCode == code || x.BranchName == name));

        if (duplicate)
            return Conflict("A branch with the same code or name already exists.");

        branch.BranchCode = code;
        branch.BranchName = name;
        branch.Address = string.IsNullOrWhiteSpace(updated.Address) ? null : updated.Address.Trim();
        branch.PhoneNumber = string.IsNullOrWhiteSpace(updated.PhoneNumber) ? null : updated.PhoneNumber.Trim();
        branch.IsActive = updated.IsActive;

        await _masterDb.SaveChangesAsync();

        return Ok(branch);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int companyId, int id)
    {
        if (!CanAccessCompany(companyId))
            return Forbid();

        if (!(await _tenantPlanService.GetFeaturesAsync(companyId)).CanUseBranching)
            return StatusCode(StatusCodes.Status403Forbidden, "Branch management requires the Pro Max plan.");


        var branch = await _masterDb.Branches
            .FirstOrDefaultAsync(x => x.BranchId == id && x.CompanyId == companyId);

        if (branch == null)
            return NotFound();

        _masterDb.Branches.Remove(branch);
        await _masterDb.SaveChangesAsync();

        return NoContent();
    }
}
