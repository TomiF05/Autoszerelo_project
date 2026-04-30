using Autoszerelo.Data;
using Autoszerelo.models;
using Microsoft.EntityFrameworkCore;

namespace Autoszerelo.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly AppDbContext _context;

        public CustomerService(AppDbContext context)
        {
            _context = context;
        }

        // --- Összes ügyfél lekérése szűréssel ---
        public async Task<IEnumerable<Customer>> GetCustomersAsync(string? name, string? address, string? email)
        {
            var query = _context.Customers.AsQueryable();

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(c => c.Name.ToLower().Contains(name.ToLower()));
            }

            if (!string.IsNullOrEmpty(address))
            {
                query = query.Where(c => c.Address.ToLower().Contains(address.ToLower()));
            }

            if (!string.IsNullOrEmpty(email))
            {
                query = query.Where(c => c.Email.ToLower().Contains(email.ToLower()));
            }

            return await query.ToListAsync();
        }

        // --- Egy ügyfél keresése ID alapján (autókkal együtt) ---
        public async Task<Customer?> GetCustomerByIdAsync(int id)
        {
            return await _context.Customers
                .Include(c => c.Works)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        // --- Új ügyfél létrehozása validációval ---
        public async Task<Customer> CreateCustomerAsync(string name, string address, string email)
        {
            // Duplikáció ellenőrzése
            bool isDuplicate = await _context.Customers.AnyAsync(c =>
                c.Name.ToLower() == name.ToLower() &&
                c.Email.ToLower() == email.ToLower() &&
                c.Address.ToLower() == address.ToLower());

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

            await _context.Customers.AddAsync(customer);
            await _context.SaveChangesAsync();
            return customer;
        }

        // --- Ügyfél adatainak frissítése ---
        public async Task<Customer?> UpdateCustomerAsync(int id, string? name, string? address, string? email)
        {
            var customer = await _context.Customers.FindAsync(id);
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

            await _context.SaveChangesAsync();
            return customer;
        }

        // --- Ügyfél törlése ---
        public async Task<bool> DeleteCustomerAsync(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return false;

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}