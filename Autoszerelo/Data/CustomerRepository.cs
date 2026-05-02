using Autoszerelo.models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace Autoszerelo.Data
{

    public class CustomerRepository : ICustomerRepository
    {
        private readonly AppDbContext _context;
        public CustomerRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Customer>> GetCustomersAsync(int? id, string? name, string? address, string? email)
        {
            // 1. Elindítjuk a lekérdezést, BELEÉRTVE a munkákat is!
            var query = _context.Customers.AsQueryable();

            if (id.HasValue)
            {
                query = query.Where(c => c.Id == id.Value);
            }
            // 2. Szűrés névre (ha megadták)
            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(c => c.Name.ToLower().Contains(name.ToLower()));
            }

            // 3. Szűrés városra/címre (ha megadták)
            if (!string.IsNullOrEmpty(address))
            {
                query = query.Where(c => c.Address.ToLower().Contains(address.ToLower()));
            }

            // 4. Szűrés emailre (ha megadták)
            if (!string.IsNullOrEmpty(email))
            {
                query = query.Where(c => c.Email.ToLower().Contains(email.ToLower()));
            }

            // 5. Eredmény lekérése az adatbázisból
            return await query.ToListAsync();
        }

        public async Task<IEnumerable<Customer>> GetAllAsync()
        {
            // Ide is betesszük az Include-ot, ha valahol ezt használnád a kódodban
            return await _context.Customers.Include(c => c.Works).ToListAsync();
        }

        public async Task AddAsync(Customer customer)
        {
            await _context.Customers.AddAsync(customer);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> AddressExistsAsync(string address)
        {
            return await _context.Customers.AnyAsync(x => x.Address.ToLower() == address.ToLower());
        }
        public async Task<bool> CustomerExistsAsync(string address, string email)
        {
            return await _context.Customers.AnyAsync(x => x.Address.ToLower() == address.ToLower() && x.Email.ToLower() == email.ToLower());
        }

        public async Task DeleteAsync(Customer customer)
        {
            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Customers.AnyAsync(x => x.Email.ToLower() == email.ToLower());
        }

        public async Task<bool> EmailExistsForOtherCustomerAsync(int customerId, string email)
        {
            return await _context.Customers.AnyAsync(x => x.Id != customerId && x.Email.ToLower() == email.ToLower());
        }

        public async Task<Customer?> GetByIdAsync(int id)
        {
            return await _context.Customers.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Customer?> GetByIdWithWorksAsync(int id)
        {
            return await _context.Customers.Include(c => c.Works).FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task UpdateAsync(Customer customer)
        {
            var existingCustomer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == customer.Id);
            if (existingCustomer != null)
            { 
                existingCustomer.Id = customer.Id;
                existingCustomer.Name = customer.Name;
                existingCustomer.Address = customer.Address;
                existingCustomer.Email = customer.Email;
                existingCustomer.Works = customer.Works;
            }
            await _context.SaveChangesAsync();
        }
    }
}
