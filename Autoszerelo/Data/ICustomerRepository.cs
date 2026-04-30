using Autoszerelo.models;

namespace Autoszerelo.Data
{
    public interface ICustomerRepository
    {
        Task<IEnumerable<Customer>> GetAllAsync();
        Task<Customer?> GetByIdAsync(int id);
        Task<Customer?> GetByIdWithWorksAsync(int id);
        Task<bool> EmailExistsAsync(string email);
        Task<bool> EmailExistsForOtherCustomerAsync(int customerId, string email);
        Task<bool> AddressExistsAsync(string address);
        Task AddAsync(Customer customer);
        Task UpdateAsync(Customer customer);
        Task DeleteAsync(Customer customer);
    }
}
