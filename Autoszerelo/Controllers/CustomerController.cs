using Autoszerelo.Data;
using Autoszerelo.models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Autoszerelo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly AppDbContext _context;

        // A konstruktorban kérjük el az adatbázis elérést
        public CustomerController(AppDbContext context)
        {
            _context = context;
        }

        // --- LEKÉRÉS (Összes ügyfél) ---
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers()
        {
            return await _context.Customers.ToListAsync();
        }

        // --- LÉTREHOZÁS (Új ügyfél mentése validációval) ---
        [HttpPost]
        public async Task<ActionResult<Customer>> PostCustomer(Customer customer)
        {
            // 1. Kézi validáció (Bekérésnél ellenőrizzük)
            if (string.IsNullOrWhiteSpace(customer.Name))
            {
                return BadRequest("A név megadása kötelező!");
            }

            if (string.IsNullOrWhiteSpace(customer.Email) || !customer.Email.Contains("@"))
            {
                return BadRequest("Érvénytelen email cím formátum!");
            }

            // 2. Mentés az SQLite adatbázisba
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            
            return Ok(customer);
        }
    }
}
