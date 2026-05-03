using Microsoft.JSInterop;
using Shared;
using System.Net.Http.Json;

namespace Frontend.Services
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly HttpClient _httpClient;
        private readonly IJSRuntime _jsRuntime;

        public CustomerRepository(HttpClient httpClient, IJSRuntime jsRuntime)
        {
            _httpClient = httpClient;
            _jsRuntime = jsRuntime;
        }

        public async Task<IEnumerable<CustomerDto>> GetAllAsync()
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<CustomerDto>>("api/customers");
        }

        public async Task AddAsync(AddCustomerRequest request)
        {
            await _httpClient.PostAsJsonAsync("api/customers", request);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/customers/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<CustomerDto?> GetByIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<CustomerDto>($"api/customers/{id}");
        }

        public async Task UpdateAsync(CustomerDto customer)
        {
            await _httpClient.PutAsJsonAsync($"api/customers/{customer.Id}", customer);
        }
    }
}
