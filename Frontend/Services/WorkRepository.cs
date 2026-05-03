using Shared;
using System.Net.Http.Json;
using static System.Net.WebRequestMethods;

namespace Frontend.Services
{
    public class WorkRepository : IWorkRepository
    {
        private readonly HttpClient _httpClient;

        public WorkRepository(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<WorkDto>> GetAllWorksAsync()
        {
            // Közvetlenül List<WorkDto>-ként olvassuk be a választ
            var works = await _httpClient.GetFromJsonAsync<List<WorkDto>>("api/works");
            return works ?? new List<WorkDto>();
        }

        public async Task<WorkDto> GetWorkByIdAsync(int id)
        {
            // Közvetlenül WorkDto-ként kérjük le, ahogy a lista esetében is!
            var result = await _httpClient.GetFromJsonAsync<WorkDto>($"/api/works/{id}");

            // Ha nem találja, adunk egy üres objektumot, hogy ne dőljön össze
            return result ?? new WorkDto();
        }

        public async Task AddWorkAsync(AddWorkRequest request)
        {
            await _httpClient.PostAsJsonAsync("/api/works", request);
        }

        public async Task UpdateWorkAsync(int id, UpdateWorkRequest request)
        {
            await _httpClient.PutAsJsonAsync($"/api/works/{id}", request);
        }

        public async Task DeleteWorkAsync(int id)
        {
            await _httpClient.DeleteAsync($"/api/works/{id}");
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
                // A Backend címe: api/Works/my-works
                var response = await _httpClient.GetFromJsonAsync<List<WorkDto>>("api/Works/my-works");
                return response ?? new List<WorkDto>();
            }
            catch (Exception)
            {
                return new List<WorkDto>();
            }
        }
    }
}
