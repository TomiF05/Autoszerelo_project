using Microsoft.JSInterop;
using Shared;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using static System.Net.WebRequestMethods;

namespace Frontend.Services
{
    public class WorkRepository : IWorkRepository
    {
        private readonly HttpClient _httpClient;
        private readonly IJSRuntime _jsRuntime;

        public WorkRepository(HttpClient httpClient, IJSRuntime jsRuntime)
        {
            _httpClient = httpClient;
            _jsRuntime = jsRuntime;
        }

        public async Task<List<WorkDto>> GetAllWorksAsync()
        {
            try
            {
                var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");

                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                var response = await _httpClient.GetFromJsonAsync<List<WorkDto>>("api/works");
                return response ?? new List<WorkDto>();
            }
            catch (Exception ex)
            {
                return new List<WorkDto>();
            }
        }
        public async Task<WorkDto?> GetWorkByIdAsync(int id)
        {
            var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");

            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            try
            {
                return await _httpClient.GetFromJsonAsync<WorkDto>($"api/works/{id}");
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return null;
            }
        }

        public async Task AddWorkAsync(AddWorkRequest request)
        {
            await _httpClient.PostAsJsonAsync("/api/works", request);
        }

        public async Task UpdateWorkAsync(int id, UpdateWorkRequest request)
        {
            var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await _httpClient.PutAsJsonAsync($"api/works/{id}", request);

            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteWorkAsync(int id)
        {
            var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            await _httpClient.DeleteAsync($"api/works/{id}");
        }
        public async Task ChangeStatusAsync(int id)
        {
            await _httpClient.PutAsync($"/api/works/change-status/{id}", null);
        }
        public async Task<List<WorkDto>> GetMyWorksAsync()
        {
            try
            {
                var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");
                if (string.IsNullOrEmpty(token)) return new List<WorkDto>();

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync("api/Works/my-works");

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return new List<WorkDto>();
                }

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<WorkDto>>() ?? new List<WorkDto>();
                }

                return new List<WorkDto>();
            }
            catch { return new List<WorkDto>(); }
        }
    }

}
