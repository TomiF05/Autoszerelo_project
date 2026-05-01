using Autoszerelo.models;
using Shared;

namespace Autoszerelo.Services
{
    public interface ICustomerService
    {
        // Összes ügyfél lekérése szűréssel
        Task<IEnumerable<CustomerDto>> GetCustomersAsync(FilterCustomerDto? customer);

        // Egy ügyfél lekérése ID alapján (autókkal együtt)
        Task<CustomerDto?> GetCustomerByIdAsync(int id);

        // Új ügyfél létrehozása
        Task<CustomerDto> CreateCustomerAsync(CustomerDto customer);

        // Ügyfél adatainak frissítése
        Task<CustomerDto?> UpdateCustomerAsync( UpdateCustomerRequest customer);
        //Customer MapToEntity(AddCustomerRequest dto);
        CustomerDto MapToDto(Customer customer);

        // Ügyfél törlése
        Task<bool> DeleteCustomerAsync(int id);
    }
}