using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using DriveConnect.domain.Entities;

namespace DriveConnect.winforms.Services
{
    public class CrmApiService
    {
        // Make sure this port matches your running API port
        private readonly HttpClient _http = new HttpClient { BaseAddress = new Uri("https://localhost:7162") };

        public async Task<List<SalesLead>> GetSalesAsync(int companyId) => await _http.GetFromJsonAsync<List<SalesLead>>($"/tenant/{companyId}/sales") ?? new();
        public async Task<HttpResponseMessage> CreateSalesAsync(int companyId, SalesLead lead) => await _http.PostAsJsonAsync($"/tenant/{companyId}/sales", lead);
        public async Task<HttpResponseMessage> UpdateSalesAsync(int companyId, int id, SalesLead lead) => await _http.PutAsJsonAsync($"/tenant/{companyId}/sales/{id}", lead);

        public async Task<List<RepairTicket>> GetRepairsAsync(int companyId) => await _http.GetFromJsonAsync<List<RepairTicket>>($"/tenant/{companyId}/repairs") ?? new();
        public async Task<HttpResponseMessage> CreateRepairAsync(int companyId, RepairTicket ticket) => await _http.PostAsJsonAsync($"/tenant/{companyId}/repairs", ticket);
        public async Task<HttpResponseMessage> UpdateRepairAsync(int companyId, int id, RepairTicket ticket) => await _http.PutAsJsonAsync($"/tenant/{companyId}/repairs/{id}", ticket);

        public async Task<List<Promotion>> GetPromotionsAsync(int companyId) => await _http.GetFromJsonAsync<List<Promotion>>($"/tenant/{companyId}/promotions") ?? new();
        public async Task<HttpResponseMessage> CreatePromotionAsync(int companyId, Promotion item) => await _http.PostAsJsonAsync($"/tenant/{companyId}/promotions", item);
        public async Task<HttpResponseMessage> UpdatePromotionAsync(int companyId, int id, Promotion item) => await _http.PutAsJsonAsync($"/tenant/{companyId}/promotions/{id}", item);

        public async Task<List<Feedback>> GetFeedbackAsync(int companyId) => await _http.GetFromJsonAsync<List<Feedback>>($"/tenant/{companyId}/feedback") ?? new();
        public async Task<HttpResponseMessage> CreateFeedbackAsync(int companyId, Feedback item) => await _http.PostAsJsonAsync($"/tenant/{companyId}/feedback", item);
        public async Task<HttpResponseMessage> UpdateFeedbackAsync(int companyId, int id, Feedback item) => await _http.PutAsJsonAsync($"/tenant/{companyId}/feedback/{id}", item);

        public async Task<List<Complaint>> GetComplaintsAsync(int companyId) => await _http.GetFromJsonAsync<List<Complaint>>($"/tenant/{companyId}/complaints") ?? new();
        public async Task<HttpResponseMessage> CreateComplaintAsync(int companyId, Complaint item) => await _http.PostAsJsonAsync($"/tenant/{companyId}/complaints", item);
        public async Task<HttpResponseMessage> UpdateComplaintAsync(int companyId, int id, Complaint item) => await _http.PutAsJsonAsync($"/tenant/{companyId}/complaints/{id}", item);

        public async Task<List<InteractionLog>> GetInteractionsAsync(int companyId) => await _http.GetFromJsonAsync<List<InteractionLog>>($"/tenant/{companyId}/interactions") ?? new();
        public async Task<HttpResponseMessage> CreateInteractionAsync(int companyId, InteractionLog item) => await _http.PostAsJsonAsync($"/tenant/{companyId}/interactions", item);
        public async Task<HttpResponseMessage> UpdateInteractionAsync(int companyId, int id, InteractionLog item) => await _http.PutAsJsonAsync($"/tenant/{companyId}/interactions/{id}", item);

        public async Task<List<VehicleWarranty>> GetWarrantiesAsync(int companyId) => await _http.GetFromJsonAsync<List<VehicleWarranty>>($"/tenant/{companyId}/warranties") ?? new();
        public async Task<HttpResponseMessage> CreateWarrantyAsync(int companyId, VehicleWarranty item) => await _http.PostAsJsonAsync($"/tenant/{companyId}/warranties", item);
        public async Task<HttpResponseMessage> UpdateWarrantyAsync(int companyId, int id, VehicleWarranty item) => await _http.PutAsJsonAsync($"/tenant/{companyId}/warranties/{id}", item);

        public async Task<List<WarrantyClaim>> GetWarrantyClaimsAsync(int companyId) => await _http.GetFromJsonAsync<List<WarrantyClaim>>($"/tenant/{companyId}/warranty-claims") ?? new();
        public async Task<HttpResponseMessage> CreateWarrantyClaimAsync(int companyId, WarrantyClaim item) => await _http.PostAsJsonAsync($"/tenant/{companyId}/warranty-claims", item);
        public async Task<HttpResponseMessage> UpdateWarrantyClaimAsync(int companyId, int id, WarrantyClaim item) => await _http.PutAsJsonAsync($"/tenant/{companyId}/warranty-claims/{id}", item);

        public async Task<List<MaintenanceRecord>> GetMaintenanceAsync(int companyId) => await _http.GetFromJsonAsync<List<MaintenanceRecord>>($"/tenant/{companyId}/maintenance") ?? new();
        public async Task<HttpResponseMessage> CreateMaintenanceAsync(int companyId, MaintenanceRecord item) => await _http.PostAsJsonAsync($"/tenant/{companyId}/maintenance", item);
        public async Task<HttpResponseMessage> UpdateMaintenanceAsync(int companyId, int id, MaintenanceRecord item) => await _http.PutAsJsonAsync($"/tenant/{companyId}/maintenance/{id}", item);
    }
}