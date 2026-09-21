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
    }
}