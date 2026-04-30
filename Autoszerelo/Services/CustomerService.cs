using Autoszerelo.Data;
using Autoszerelo.models;
using Microsoft.EntityFrameworkCore;

namespace Autoszerelo.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly AppDbContext _context;

        public CustomerService(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
           // _context = context;
        }

        // --- Összes ügyfél lekérése szűréssel ---
        public async Task<IEnumerable<Customer>> GetCustomersAsync(string? name, string? address, string? email)
        {
            return await _customerRepository.GetCustomersAsync(name, address, email);
        }

        // --- Egy ügyfél keresése ID alapján (autókkal együtt) ---
        public async Task<Customer?> GetCustomerByIdAsync(int id)
        {
            return await _customerRepository.GetByIdWithWorksAsync(id);
        }

        // --- Új ügyfél létrehozása validációval ---
        public async Task<Customer> CreateCustomerAsync(string name, string address, string email)
        {
            // Duplikáció ellenőrzése
            bool isDuplicate = await _customerRepository.EmailExistsAsync(email) && await _customerRepository.AddressExistsAsync(address);

            if (isDuplicate)
            {
                // Kivételt dobunk, amit a Controller elkap és BadRequest-et csinál belőle
                throw new InvalidOperationException("Hiba: Ez az ügyfél már szerepel a rendszerben!");
            }
            if (string.IsNullOrWhiteSpace(name) && name != "string")
            {
                throw new ArgumentException("Nem adtál meg nevet!");
            }
            if (string.IsNullOrWhiteSpace(address) && address != "string")
            {
                throw new ArgumentException("Nem adtál meg címet!");
            }
            if (string.IsNullOrWhiteSpace(email) && email != "string")
            {
                
                    throw new ArgumentException("Nem adtál meg email címet!");
            }
            if (!email.Contains("@"))
                throw new ArgumentException("Érvénytelen email formátum!");

            var customer = new Customer
            {
                Name = name,
                Address = address,
                Email = email
            };

            await _customerRepository.AddAsync(customer);
            return customer;
        }

        // --- Ügyfél adatainak frissítése ---
        public async Task<Customer?> UpdateCustomerAsync(int id, string? name, string? address, string? email)
        {
            var customer = await _customerRepository.GetByIdAsync(id);
            if (customer == null) return null;

            if (!string.IsNullOrWhiteSpace(name) && name != "string")
            {
                customer.Name = name;
            }

            if (!string.IsNullOrWhiteSpace(address) && address != "string")
            {
                customer.Address = address;
            }

            if (!string.IsNullOrWhiteSpace(email) && email != "string")
            {
                if (!email.Contains("@"))
                    throw new ArgumentException("Érvénytelen email formátum!");

                customer.Email = email;
            }
            if (await _customerRepository.EmailExistsForOtherCustomerAsync(id, customer.Email))
            {
                throw new InvalidOperationException("Hiba: Ez az email cím már egy másik ügyfélhez van rendelve!");
            }

            await _customerRepository.UpdateAsync(customer);
            return customer;
        }

        // --- Ügyfél törlése ---
        public async Task<bool> DeleteCustomerAsync(int id)
        {
            var customer = await _customerRepository.GetByIdAsync(id);
            if (customer == null) return false;

            await _customerRepository.DeleteAsync(customer);
            return true;
        }
    }
}