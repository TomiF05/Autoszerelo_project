using Shared;

namespace Frontend.Services
{
    public interface ICustomerRepository
    {
        Task<IEnumerable<CustomerDto>> GetAllAsync();

        Task AddAsync(AddCustomerRequest request);
        Task<bool> DeleteAsync(int id);

        Task<CustomerDto?> GetByIdAsync(int id);
        Task UpdateAsync(CustomerDto customer);
    }
}
