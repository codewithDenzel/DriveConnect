using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using DriveConnect.domain.DTO;
using DriveConnect.domain.Entities;
using DriveConnect.infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DriveConnect.api.Controllers;

[ApiController]
[Authorize(Roles = "Super Admin,Admin")]
[Route("companies/{companyId:int}/users")]
public sealed class UsersController : ControllerBase
{
    private static readonly string[] AllowedRoles = { "Admin", "Staff" };

    private readonly MasterDriveConnectDbContext _masterDb;
    private readonly PasswordHasher<AppUser> _passwordHasher = new();

    public UsersController(MasterDriveConnectDbContext masterDb)
    {
        _masterDb = masterDb;
    }

    [HttpGet]
    public async Task<ActionResult<List<UserListItem>>> GetAll(int companyId)
    {
        if (!await CompanyExistsAsync(companyId))
            return NotFound("Company not found.");

        if (!CanAccessCompany(companyId))
            return Forbid();

        var users = await _masterDb.AppUsers
            .AsNoTracking()
            .Include(x => x.Branch)
            .Where(x => x.CompanyId == companyId)
            .OrderBy(x => x.Username)
            .Select(x => new UserListItem(
                x.UserId,
                x.CompanyId,
                x.Username,
                x.Email,
                x.Role,
                x.BranchId,
                x.Branch != null ? x.Branch.BranchName : null,
                x.IsActive,
                x.CreatedAt))
            .ToListAsync();

        return Ok(users);
    }

    [HttpPost]
    public async Task<ActionResult<UserListItem>> Create(
        int companyId,
        CreateUserRequest request)
    {
        if (!await CompanyExistsAsync(companyId))
            return NotFound("Company not found.");

        if (!CanAccessCompany(companyId))
            return Forbid();

        if (!CanManageRole(request.Role))
            return Forbid();

        var validationError = await ValidateUserRequestAsync(
            companyId,
            request.Username,
            request.Email,
            request.Password,
            request.Role,
            request.BranchId);

        if (validationError != null)
            return BadRequest(validationError);

        var username = request.Username.Trim();
        var email = request.Email.Trim();

        var duplicate = await _masterDb.AppUsers
            .AsNoTracking()
            .AnyAsync(x =>
                x.Username == username ||
                x.Email == email);

        if (duplicate)
            return Conflict("Username or email is already in use.");

        var user = new AppUser
        {
            CompanyId = companyId,
            Username = username,
            Email = email,
            Role = request.Role.Trim(),
            BranchId = request.BranchId,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        _masterDb.AppUsers.Add(user);
        await _masterDb.SaveChangesAsync();

        return Ok(await ToUserListItemAsync(user.UserId));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<UserListItem>> Update(
        int companyId,
        int id,
        UpdateUserRequest request)
    {
        if (!await CompanyExistsAsync(companyId))
            return NotFound("Company not found.");

        if (!CanAccessCompany(companyId))
            return Forbid();

        var user = await _masterDb.AppUsers
            .FirstOrDefaultAsync(x =>
                x.UserId == id &&
                x.CompanyId == companyId);

        if (user == null)
            return NotFound("User not found.");

        if (!CanAccessUser(user))
            return Forbid();

        var editingOwnSuperAdmin =
            string.Equals(
                user.Role,
                "Super Admin",
                StringComparison.OrdinalIgnoreCase) &&
            string.Equals(
                request.Role,
                "Super Admin",
                StringComparison.OrdinalIgnoreCase);

        if (!editingOwnSuperAdmin && !CanManageRole(request.Role))
            return Forbid();

        if (string.Equals(user.Role, "Super Admin", StringComparison.OrdinalIgnoreCase))
        {
            if (!editingOwnSuperAdmin ||
                request.BranchId.HasValue ||
                !request.IsActive)
            {
                return BadRequest(
                    "The Super Admin account cannot be reassigned, moved to a branch, or deactivated.");
            }
        }

        var validationError = await ValidateUserRequestAsync(
            companyId,
            request.Username,
            request.Email,
            request.Password,
            request.Role,
            request.BranchId,
            isUpdate: true);

        if (validationError != null)
            return BadRequest(validationError);

        var username = request.Username.Trim();
        var email = request.Email.Trim();

        var duplicate = await _masterDb.AppUsers
            .AsNoTracking()
            .AnyAsync(x =>
                x.UserId != id &&
                (x.Username == username || x.Email == email));

        if (duplicate)
            return Conflict("Username or email is already in use.");

        user.Username = username;
        user.Email = email;
        user.Role = request.Role.Trim();
        user.BranchId = request.BranchId;
        user.IsActive = request.IsActive;

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            user.PasswordHash = _passwordHasher.HashPassword(
                user,
                request.Password);
        }

        await _masterDb.SaveChangesAsync();

        return Ok(await ToUserListItemAsync(user.UserId));
    }

    private async Task<string?> ValidateUserRequestAsync(
        int companyId,
        string username,
        string email,
        string? password,
        string role,
        int? branchId,
        bool isUpdate = false)
    {
        if (string.IsNullOrWhiteSpace(username))
            return "Username is required.";

        if (username.Trim().Length < 3)
            return "Username must be at least 3 characters.";

        if (string.IsNullOrWhiteSpace(email) ||
            !IsValidEmail(email.Trim()))
        {
            return "A valid email address is required.";
        }

        if (!isUpdate && string.IsNullOrWhiteSpace(password))
            return "Password is required.";

        if (!string.IsNullOrWhiteSpace(password) &&
            password.Length < 8)
        {
            return "Password must be at least 8 characters.";
        }

        var normalizedRole = role.Trim();

        if (!AllowedRoles.Contains(
                normalizedRole,
                StringComparer.OrdinalIgnoreCase) &&
            !normalizedRole.Equals("Super Admin", StringComparison.OrdinalIgnoreCase))
        {
            return "Invalid role. Use Admin or Staff.";
        }

        if (normalizedRole.Equals(
                "Super Admin",
                StringComparison.OrdinalIgnoreCase))
        {
            return isUpdate
                ? null
                : "A Super Admin account already exists and cannot be created here.";
        }

        if (normalizedRole.Equals(
                "Staff",
                StringComparison.OrdinalIgnoreCase) &&
            !branchId.HasValue)
        {
            return "Staff users must be assigned to a branch.";
        }

        if (branchId.HasValue)
        {
            var branchExists = await _masterDb.Branches
                .AsNoTracking()
                .AnyAsync(x =>
                    x.BranchId == branchId.Value &&
                    x.CompanyId == companyId &&
                    x.IsActive);

            if (!branchExists)
                return "Selected branch was not found or is inactive.";
        }

        return null;
    }

    private bool CanManageRole(string role)
    {
        var currentRole = User.FindFirstValue(ClaimTypes.Role);

        if (string.Equals(
                currentRole,
                "Super Admin",
                StringComparison.OrdinalIgnoreCase))
        {
            return string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(role, "Staff", StringComparison.OrdinalIgnoreCase);
        }

        return string.Equals(
                   currentRole,
                   "Admin",
                   StringComparison.OrdinalIgnoreCase) &&
               string.Equals(
                   role,
                   "Staff",
                   StringComparison.OrdinalIgnoreCase);
    }

    private bool CanAccessUser(AppUser targetUser)
    {
        var currentRole = User.FindFirstValue(ClaimTypes.Role);
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                            User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (string.Equals(
                currentRole,
                "Super Admin",
                StringComparison.OrdinalIgnoreCase))
        {
            if (!string.Equals(
                    targetUser.Role,
                    "Super Admin",
                    StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return int.TryParse(currentUserId, out var id) &&
                   id == targetUser.UserId;
        }

        return !string.Equals(
                   targetUser.Role,
                   "Super Admin",
                   StringComparison.OrdinalIgnoreCase);
    }

    private bool CanAccessCompany(int companyId)
    {
        var companyClaim = User.FindFirstValue("companyId");

        return int.TryParse(companyClaim, out var userCompanyId) &&
               userCompanyId == companyId;
    }

    private Task<bool> CompanyExistsAsync(int companyId)
    {
        return _masterDb.Companies
            .AsNoTracking()
            .AnyAsync(x =>
                x.CompanyId == companyId &&
                x.IsActive);
    }

    private Task<UserListItem> ToUserListItemAsync(int userId)
    {
        return _masterDb.AppUsers
            .AsNoTracking()
            .Include(x => x.Branch)
            .Where(x => x.UserId == userId)
            .Select(x => new UserListItem(
                x.UserId,
                x.CompanyId,
                x.Username,
                x.Email,
                x.Role,
                x.BranchId,
                x.Branch != null ? x.Branch.BranchName : null,
                x.IsActive,
                x.CreatedAt))
            .SingleAsync();
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var address = new System.Net.Mail.MailAddress(email);

            return string.Equals(
                address.Address,
                email,
                StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }
}
