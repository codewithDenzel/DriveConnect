using System.Net.Http.Headers;
using System.Net.Http.Json;
using DriveConnect.domain.DTO;
using DriveConnect.domain.Entities;

namespace DriveConnect.winforms.Services;

public sealed class UserManagementApiService
{
    private readonly HttpClient _http;

    public UserManagementApiService()
    {
        _http = new HttpClient
        {
            BaseAddress = new Uri("https://localhost:7162")
        };

        if (!string.IsNullOrWhiteSpace(UserSession.Token))
        {
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    UserSession.Token);
        }
    }

    public async Task<List<UserListItem>> GetUsersAsync(int companyId)
    {
        return await _http.GetFromJsonAsync<List<UserListItem>>(
            $"/companies/{companyId}/users") ?? new();
    }

    public Task<HttpResponseMessage> CreateUserAsync(
        int companyId,
        CreateUserRequest request)
    {
        return _http.PostAsJsonAsync(
            $"/companies/{companyId}/users",
            request);
    }

    public Task<HttpResponseMessage> UpdateUserAsync(
        int companyId,
        int userId,
        UpdateUserRequest request)
    {
        return _http.PutAsJsonAsync(
            $"/companies/{companyId}/users/{userId}",
            request);
    }

    public async Task<List<Branch>> GetBranchesAsync(int companyId)
    {
        return await _http.GetFromJsonAsync<List<Branch>>(
            $"/companies/{companyId}/branches") ?? new();
    }
}
