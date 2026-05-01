using Shared;
using System.Net.Http.Json;

namespace Frontend.Services
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly HttpClient _httpClient;

        public CustomerRepository(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<CustomerDto>> GetAllAsync()
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<CustomerDto>>("api/Customer") ?? new List<CustomerDto>();
        }

        public async Task AddAsync(AddCustomerRequest request)
        {
            await _httpClient.PostAsJsonAsync("api/Customer", request);
        }
    }
}
