using Autoszerelo.Data;
using Autoszerelo.models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Autoszerelo.Controllers
{
    [Route("api/[controller]")] 
    [ApiController]
    public class WorksController : ControllerBase
    {
        private readonly AppDbContext _context;

        public WorksController(AppDbContext context)
        {
            _context = context;
        }

        // --- Összes munka lekérése az ügyfél adataival együtt ---
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Works>>> GetWorks()
        {
            return await _context.Works
               // .Include(w => w.Customer) // <--- Ez a kulcsszó! Ezzel húzza be az ügyfél nevét, emailjét is.
                .ToListAsync();
        }

        // --- Új munka felvétele (Validációval) ---
        [HttpPost]
        public async Task<ActionResult<Works>> PostWork(Works work)
        {
            // 1. Alap validációk
            if (work.ProductionYear < 1900 || work.ProductionYear > DateTime.Now.Year + 1)
            {
                return BadRequest("Érvénytelen gyártási év!");
            }

            if (work.EstimatedHours <= 0)
            {
                return BadRequest("A becsült munkaidőnek többnek kell lennie 0-nál!");
            }

            // 2. Létezik-e az ügyfél, akihez a munkát kötjük? (FONTOS!)
            var customerExists = await _context.Customers.AnyAsync(c => c.Id == work.CustomerId);
            if (!customerExists)
            {
                return BadRequest("A megadott ügyfél (CustomerId) nem létezik az adatbázisban!");
            }
            var existingCustomer = await _context.Customers.FindAsync(work.CustomerId);

            if (existingCustomer == null)
            {
                return BadRequest("Hiba: Ilyen ID-jű ügyfél nem létezik! Előbb hozd létre az ügyfelet a Customer oldalon.");
            }

            //work.Customer = existingCustomer;

            // 3. Mentés
            _context.Works.Add(work);
            await _context.SaveChangesAsync();

            return Ok(work);
        }
    }
}
