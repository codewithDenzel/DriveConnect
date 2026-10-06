using DriveConnect.domain.Entities;
using DriveConnect.infrastructure.Data;
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

    public BranchesController(MasterDriveConnectDbContext masterDb)
    {
        _masterDb = masterDb;
    }

    [HttpGet]
    public async Task<ActionResult<List<Branch>>> GetAll(int companyId)
    {
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

    [HttpPost]
    public async Task<ActionResult<Branch>> Create(int companyId, Branch branch)
    {
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

    [HttpPut("{id:int}")]
    public async Task<ActionResult<Branch>> Update(int companyId, int id, Branch updated)
    {
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

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int companyId, int id)
    {
        var branch = await _masterDb.Branches
            .FirstOrDefaultAsync(x => x.BranchId == id && x.CompanyId == companyId);

        if (branch == null)
            return NotFound();

        _masterDb.Branches.Remove(branch);
        await _masterDb.SaveChangesAsync();

        return NoContent();
    }
}
