using Autoszerelo.models;

namespace Autoszerelo.Services
{
    public interface ICustomerService
    {
        // Összes ügyfél lekérése szűréssel
        Task<IEnumerable<Customer>> GetCustomersAsync(string? name, string? address, string? email);

        // Egy ügyfél lekérése ID alapján (autókkal együtt)
        Task<Customer?> GetCustomerByIdAsync(int id);

        // Új ügyfél létrehozása
        Task<Customer> CreateCustomerAsync(string name, string address, string email);

        // Ügyfél adatainak frissítése
        Task<Customer?> UpdateCustomerAsync(int id, string? name, string? address, string? email);

        // Ügyfél törlése
        Task<bool> DeleteCustomerAsync(int id);
    }
}