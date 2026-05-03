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

        // WorkRepository.cs
        public async Task<List<WorkDto>> GetAllWorksAsync()
        {
            try
            {
                // Kiolvassuk a tokent
                var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");

                if (!string.IsNullOrEmpty(token))
                {
                    // Beállítjuk a fejlécet
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                // Itt hívod meg a sima listát
                var response = await _httpClient.GetFromJsonAsync<List<WorkDto>>("api/works");
                return response ?? new List<WorkDto>();
            }
            catch (Exception ex)
            {
                // Ha 401-et kapsz, ne dőljön össze az oldal, csak adjon vissza üres listát
                return new List<WorkDto>();
            }
        }
        public async Task<WorkDto?> GetWorkByIdAsync(int id)
        {
            // 1. Token kiolvasása a böngészőből
            var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");

            if (!string.IsNullOrEmpty(token))
            {
                // 2. Token berakása a fejlécbe
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            try
            {
                // 3. Mehet a kérés
                return await _httpClient.GetFromJsonAsync<WorkDto>($"api/works/{id}");
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                // Ha mégis 401, akkor valószínűleg lejárt a munkameneted
                return null;
            }
        }

        public async Task AddWorkAsync(AddWorkRequest request)
        {
            await _httpClient.PostAsJsonAsync("/api/works", request);
        }

        public async Task UpdateWorkAsync(int id, UpdateWorkRequest request)
        {
            // Ne felejtsd el a tokent itt is beállítani!
            var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            // 2. Küldés a Backend-nek (az ID-t az URL-be tesszük)
            var response = await _httpClient.PutAsJsonAsync($"api/works/{id}", request);

            // 3. Ellenőrizzük, hogy sikerült-e
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteWorkAsync(int id)
        {
            var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Csak az ID megy az URL végén, nincs Body!
            await _httpClient.DeleteAsync($"api/works/{id}");
        }
        public async Task ChangeStatusAsync(int id)
        {
            // A null azt jelenti, hogy nem küldünk extra JSON dobozt, csak magát az URL-t hívjuk meg
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
                    // Ha még mindig 401, akkor a token nem érvényes vagy lejárt
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
