using System;

namespace DriveConnect.domain.DTO;

public sealed record LoginRequest(string Username, string Password);

public sealed record LoginResponse(
    string Token,
    int UserId,
    int CompanyId,
    string Username,
    string FullName,
    string Role,
    int? BranchId,
    string? BranchName);

public sealed record UserListItem(
    int UserId,
    int CompanyId,
    string Username,
    string FirstName,
    string? MiddleName,
    string LastName,
    string Email,
    string Role,
    int? BranchId,
    string? BranchName,
    bool IsActive,
    DateTime CreatedAt);

public sealed record CreateUserRequest(
    string Username,
    string FirstName,
    string? MiddleName,
    string LastName,
    string Email,
    string Password,
    string Role,
    int? BranchId,
    bool IsActive = true);

public sealed record UpdateUserRequest(
    string Username,
    string FirstName,
    string? MiddleName,
    string LastName,
    string Email,
    string? Password,
    string Role,
    int? BranchId,
    bool IsActive);
