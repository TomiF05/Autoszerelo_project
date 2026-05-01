using Shared;
using System.Net.Http.Json;

namespace Frontend.Services
{
    public class WorkRepostory : IWorkRepostory
    {
        private readonly HttpClient _httpClient;

        public WorkRepostory(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<WorkDto>> GetAllWorksAsync()
        {
            var result = await _httpClient.GetFromJsonAsync<GetAllWorkResponse>("/api/works");

            return result.Works;
        }

        public async Task<WorkDto> GetWorkByIdAsync(int id)
        {
            var result = await _httpClient.GetFromJsonAsync<GetWorkByIdResponse>($"/api/works/{id}");

            return result.Work;
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
    }
}
