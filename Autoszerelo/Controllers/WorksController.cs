using Autoszerelo.Data;
using Autoszerelo.models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

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
        public async Task<ActionResult<Works>> PostWork(string licenseplate, int productionyear,string description,string category,int seriousness110,int customerId)
        {
            // 1. Alap validációk
            if (productionyear < 1900 || productionyear > DateTime.Now.Year)
            {
                return BadRequest("Érvénytelen gyártási év!");
            }

            if (seriousness110 < 1 || seriousness110 > 10)
            {
                return BadRequest("Érvénytelen súlyosság!(ez egy 1 ls 10 közötti egész szám)");
            }

            // 2. Létezik-e az ügyfél, akihez a munkát kötjük? (FONTOS!)
            var customerExists = await _context.Customers.AnyAsync(c => c.Id == customerId);
            if (!customerExists)
            {
                return BadRequest("A megadott ügyfél (CustomerId) nem létezik az adatbázisban!");
            }
            var existingCustomer = await _context.Customers.FindAsync(customerId);

            if (existingCustomer == null)
            {
                return BadRequest("Hiba: Ilyen ID-jű ügyfél nem létezik! Előbb hozd létre az ügyfelet a Customer oldalon.");
            }
            

            double ehours = category switch
            {
                "Karosszéria" => 3,
                "Motor" => 8,
                "Futómű" => 6,
                "Fékberendezés" => 4,
                _ => 5  // Ha olyat írnak be, ami nincs a listában
            };

            double serious = seriousness110 switch
            {
                10 => 1.0,   // Fixen 10-es értékre
                > 7 => 0.8,   // 7-nél nagyobb (8, 9)
                > 4 => 0.6,   // 4-nél nagyobb (5, 6, 7)
                > 2 => 0.4,   // 2-nél nagyobb (3, 4)
                _ => 0.2    // Minden másra (2 vagy az alatti)
            };

            int age = DateTime.Now.Year - productionyear;

            double ageMultiplier = age switch
            {
                < 5 => 0.5,
                < 10 => 1.0,
                < 20 => 1.5,
                _ => 2.0  // Ez a "default", mindenre, ami 20 vagy több
            };
            //work.Customer = existingCustomer;
            var work = new Works
            {
                LicensePlate = licenseplate,
                ProductionYear = productionyear,
                FaultDescription = description,
                Category = category,
                seriousness = seriousness110,
                EstimatedHours = ehours*ageMultiplier*serious,
                CustomerId = customerId
            };
            
            // 3. Mentés
            _context.Works.Add(work);
            await _context.SaveChangesAsync();

            return Ok(work);
        }
    }
}
