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
        [HttpGet]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers(string? name, string? address, string? email)
        {
            // 1. Elindítjuk a lekérdezést
            var query = _context.Customers.AsQueryable();

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

        // --- LÉTREHOZÁS (Új ügyfél mentése validációval) ---
        [HttpPost]
        public async Task<ActionResult<Customer>> PostCustomer(string name, string address, string email)
        {
            // 1. Kézi validáció (Bekérésnél ellenőrizzük)
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest("A név megadása kötelező!");
            }

            if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
            {
                return BadRequest("Érvénytelen email cím formátum!");
            }

            // Megnézzük, van-e már OLYAN sor, ahol a név ÉS az e-mail is ugyanaz
            bool isDuplicate = await _context.Customers.AnyAsync(c =>
                c.Name.ToLower() == name.ToLower() &&
                c.Email.ToLower() == email.ToLower()&& 
                c.Address.ToLower()==address.ToLower());

            if (isDuplicate)
            {
                return BadRequest("Hiba: Ez az ügyfél (ezzel a névvel és e-maillel és lakcímmel) már szerepel a rendszerben!");
            }

            var customer = new Customer
            {
                Name = name,
                Address = address,
                Email = email
            };
            // 2. Mentés az SQLite adatbázisba
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            
            return Ok(customer);
        }

        // GET: api/Customer/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Customer>> GetCustomer(int id)
        {
            // Megkeressük az ügyfelet az ID alapján, és "hozzácsapjuk" a munkáit is
            var customer = await _context.Customers
                .Include(c => c.Works)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (customer == null)
            {
                return NotFound($"Nincs ügyfél ezzel az ID-val: {id}");
            }

            return Ok(customer);
        }
    }
}
