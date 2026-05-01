using Shared;

namespace Frontend.Services
{
    public interface ICustomerRepository
    {
        Task<IEnumerable<CustomerDto>> GetAllAsync();

        Task AddAsync(AddCustomerRequest request);
    }
}
