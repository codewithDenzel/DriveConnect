using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace DriveConnect.winforms.Services;

public sealed class SuperAdminApiService
{
    private readonly HttpClient _http;

    public SuperAdminApiService()
    {
        _http = new HttpClient
        {
            BaseAddress = new Uri("https://localhost:7162")
        };

        if (!string.IsNullOrWhiteSpace(UserSession.Token))
        {
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", UserSession.Token);
        }
    }

    public async Task<List<CompanyOverview>> GetCompaniesAsync()
    {
        return await _http.GetFromJsonAsync<List<CompanyOverview>>(
            "/super-admin/companies") ?? new();
    }

    public async Task<SubscriptionOverview?> GetSubscriptionAsync(int companyId)
    {
        return await _http.GetFromJsonAsync<SubscriptionOverview>(
            $"/super-admin/companies/{companyId}/subscription");
    }

    public async Task<SystemStatusOverview?> GetSystemStatusAsync()
    {
        return await _http.GetFromJsonAsync<SystemStatusOverview>(
            "/super-admin/system-status");
    }
}

public sealed record CompanyOverview(
    int CompanyId,
    string CompanyCode,
    string CompanyName,
    bool IsActive,
    DateTime CreatedAt,
    string? ActivePlanName,
    decimal? ActiveMonthlyFee,
    DateTime? SubscriptionEndDate);

public sealed record SubscriptionOverview(
    int SubscriptionId,
    int CompanyId,
    string PlanName,
    decimal MonthlyFee,
    DateTime StartDate,
    DateTime EndDate,
    bool IsActive);

public sealed record SystemStatusOverview(
    string Status,
    string Service,
    string Role,
    string UserId);
