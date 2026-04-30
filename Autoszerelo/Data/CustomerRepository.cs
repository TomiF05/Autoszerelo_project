using Autoszerelo.models;
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
        public async Task AddAsync(Customer customer)
        {
            await _context.Customers.AddAsync(customer);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> AddressExistsAsync(string address)
        {
            return await _context.Customers.AnyAsync(x => x.Address.Equals(address, StringComparison.OrdinalIgnoreCase));
        }

        public async Task DeleteAsync(Customer customer)
        {
            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Customers.AnyAsync(x => x.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<bool> EmailExistsForOtherCustomerAsync(int customerId, string email)
        {
            return await _context.Customers.AnyAsync(x => x.Id != customerId && x.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<IEnumerable<Customer>> GetAllAsync()
        {
            return await _context.Customers.ToListAsync();
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
            var existingCustomer = _context.Customers.FirstOrDefault(c => c.Id == customer.Id);
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
