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
            BaseAddress = DriveConnectApiConfiguration.BaseUri
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
        using var response = await _http.GetAsync(
            $"/super-admin/companies/{companyId}/subscription");

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<SubscriptionOverview>();
    }

    public async Task<HttpResponseMessage> SaveSubscriptionAsync(
        int companyId,
        SubscriptionRequest request)
    {
        return await _http.PutAsJsonAsync(
            $"/super-admin/companies/{companyId}/subscription",
            request);
    }

    public async Task<HttpResponseMessage> CreateTenantCompanyAsync(
        TenantCompanyRegistrationRequest request)
    {
        return await _http.PostAsJsonAsync("/super-admin/companies", request);
    }

    public async Task<HttpResponseMessage> DeactivateSubscriptionAsync(int companyId)
    {
        return await _http.PostAsync(
            $"/super-admin/companies/{companyId}/subscription/deactivate",
            content: null);
    }

    public async Task<SystemStatusOverview?> GetSystemStatusAsync()
    {
        return await _http.GetFromJsonAsync<SystemStatusOverview>(
            "/super-admin/system-status");
    }
}

public sealed record TenantCompanyRegistrationRequest(
    string CompanyCode,
    string CompanyName,
    string ServerName,
    string DatabaseName,
    string PlanName,
    string BillingCycle,
    decimal BillingAmount,
    DateTime StartDate,
    string AdminUsername,
    string AdminFirstName,
    string? AdminMiddleName,
    string AdminLastName,
    string AdminEmail,
    string AdminPassword);

public sealed record CompanyOverview(
    int CompanyId,
    string CompanyCode,
    string CompanyName,
    bool IsActive,
    DateTime CreatedAt,
    string? ActivePlanName,
    decimal? ActiveBillingAmount,
    string? ActiveBillingCycle,
    DateTime? SubscriptionEndDate);

public sealed record SubscriptionOverview(
    int SubscriptionId,
    int CompanyId,
    string PlanName,
    string BillingCycle,
    decimal BillingAmount,
    decimal MonthlyFee,
    DateTime StartDate,
    DateTime EndDate,
    bool IsActive);

public sealed record SubscriptionRequest(
    string PlanName,
    string BillingCycle,
    decimal BillingAmount,
    DateTime StartDate,
    bool IsActive);

public sealed record SystemStatusOverview(
    string Status,
    string Service,
    string Role,
    string UserId);
