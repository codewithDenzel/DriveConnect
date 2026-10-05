using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DriveConnect.domain.DTO;
using DriveConnect.domain.Entities;
using DriveConnect.infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace DriveConnect.api.Controllers;

[ApiController]
[Route("auth")]
public sealed class AuthController : ControllerBase
{
    private readonly MasterDriveConnectDbContext _masterDb;
    private readonly IConfiguration _configuration;
    private readonly PasswordHasher<AppUser> _passwordHasher = new();

    [AllowAnonymous]
    [HttpPost("bootstrap")]
    public async Task<ActionResult> Bootstrap(BootstrapRequest request)
    {
        var setupKey = _configuration["Auth:BootstrapKey"];

        if (string.IsNullOrWhiteSpace(setupKey) ||
            !string.Equals(request.SetupKey, setupKey, StringComparison.Ordinal))
        {
            return Unauthorized("Invalid setup key.");
        }

        if (await _masterDb.AppUsers.AnyAsync())
            return Conflict("User setup has already been completed.");

        if (string.IsNullOrWhiteSpace(request.Username) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest("Username, email, and password are required.");
        }

        if (request.Password.Length < 8)
            return BadRequest("Password must be at least 8 characters.");

        var username = request.Username.Trim();
        var email = request.Email.Trim();

        if (!IsValidEmail(email))
            return BadRequest("Please enter a valid email address.");

        var company = await _masterDb.Companies
            .FirstOrDefaultAsync(x =>
                x.CompanyId == request.CompanyId &&
                x.IsActive);

        if (company == null)
            return NotFound("Company not found.");

        if (await _masterDb.AppUsers.AnyAsync(x =>
                x.Username == username || x.Email == email))
        {
            return Conflict("Username or email is already in use.");
        }

        var user = new AppUser
        {
            CompanyId = company.CompanyId,
            Username = username,
            Email = email,
            PasswordHash = string.Empty,
            Role = "Super Admin",
            BranchId = null,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        user.PasswordHash = _passwordHasher.HashPassword(
            user,
            request.Password);

        _masterDb.AppUsers.Add(user);
        await _masterDb.SaveChangesAsync();

        return Ok(new
        {
            userId = user.UserId,
            username = user.Username,
            role = user.Role
        });
    }


    public AuthController(
        MasterDriveConnectDbContext masterDb,
        IConfiguration configuration)
    {
        _masterDb = masterDb;
        _configuration = configuration;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest("Username and password are required.");
        }

        var username = request.Username.Trim();

        var user = await _masterDb.AppUsers
            .Include(x => x.Company)
            .Include(x => x.Branch)
            .FirstOrDefaultAsync(x => x.Username == username);

        if (user == null || !user.IsActive)
            return Unauthorized("Invalid username or password.");

        if (user.Company == null || !user.Company.IsActive)
            return Unauthorized("This company is inactive.");

        if (user.BranchId.HasValue &&
            (user.Branch == null || !user.Branch.IsActive))
        {
            return Unauthorized("This user's branch is inactive.");
        }

        var verification = _passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            request.Password);

        if (verification == PasswordVerificationResult.Failed)
            return Unauthorized("Invalid username or password.");

        var key = _configuration["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(key) || Encoding.UTF8.GetByteCount(key) < 32)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                "JWT signing key is not configured correctly.");
        }

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Role, user.Role),
            new("companyId", user.CompanyId.ToString())
        };

        if (user.BranchId.HasValue)
            claims.Add(new Claim("branchId", user.BranchId.Value.ToString()));

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(
            signingKey,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: credentials);

        var tokenText = new JwtSecurityTokenHandler().WriteToken(token);

        return Ok(new LoginResponse(
            tokenText,
            user.UserId,
            user.CompanyId,
            user.Username,
            user.Role,
            user.BranchId,
            user.Branch?.BranchName));
    }
}
