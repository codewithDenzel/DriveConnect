using DriveConnect.domain.DTO;

namespace DriveConnect.winforms.Services;

public static class UserSession
{
    public static string Token { get; private set; } = string.Empty;
    public static int UserId { get; private set; }
    public static int CompanyId { get; private set; }
    public static string Username { get; private set; } = string.Empty;
    public static string Role { get; private set; } = string.Empty;
    public static int? BranchId { get; private set; }
    public static string? BranchName { get; private set; }

    public static bool IsLoggedIn => !string.IsNullOrWhiteSpace(Token);

    public static void Set(LoginResponse response)
    {
        Token = response.Token;
        UserId = response.UserId;
        CompanyId = response.CompanyId;
        Username = response.Username;
        Role = response.Role;
        BranchId = response.BranchId;
        BranchName = response.BranchName;
    }

    public static void Clear()
    {
        Token = string.Empty;
        UserId = 0;
        CompanyId = 0;
        Username = string.Empty;
        Role = string.Empty;
        BranchId = null;
        BranchName = null;
    }
}
