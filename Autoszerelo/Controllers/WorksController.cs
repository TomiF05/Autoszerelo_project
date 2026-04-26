using Autoszerelo.Data;
using Autoszerelo.models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.Net;
using System.Text.RegularExpressions;

namespace Autoszerelo.Controllers
{
    [Route("api/[controller]")] 
    [ApiController]
    public class WorksController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly string[] allowedCategories = new[] { "motor", "karosszéria", "futómű", "fékberendezés" };
        private const string pattern = @"^([A-Z]{3}-\d{3}|[A-Z]{2}-[A-Z]{2}-\d{3})$";
        public WorksController(AppDbContext context)
        {
            _context = context;
        }

        //lekéri az összes munkát , vagy a szűrési feltételeknek megfelelő munkákat, ahova nem írsz ,arra nem szűr.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Works>>> GetWorks(
        string? licensePlate = null,
        int? productionYear=null,
        int? serousness=null,
        string? faultDescription = null,
        string? category = null,
        string? status = null,
        int? customerId=null
            )
        {
            // 1. Alap lekérdezés létrehozása (még nem fut le az adatbázison)
            var query = _context.Works.AsQueryable();

            // 2. Szűrés rendszámra (töredék keresés)
            if (!string.IsNullOrWhiteSpace(licensePlate))
            {
                query = query.Where(w => w.LicensePlate.ToUpper().Contains(licensePlate.ToUpper()));
            }

            if ((productionYear).HasValue)
            {
                query = query.Where(w => w.ProductionYear==productionYear);
            }

            if ((serousness).HasValue)
            {
                query = query.Where(w => w.seriousness == serousness);
            }

            if ((customerId).HasValue)
            {
                query = query.Where(w => w.CustomerId == customerId);
            }


            // 3. Szűrés hiba leírására (töredék keresés)


            // 4. Szűrés kategóriára (pontos egyezés vagy töredék)
            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(w => w.Category.ToUpper().Contains(category.ToUpper()));
            }

            // 5. Szűrés státuszra
            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(w => w.Status.ToUpper().Contains(status.ToUpper()));
            }



            // 6. A kész lekérdezés lefuttatása és az eredmény visszaadása
            var results = await query.ToListAsync();

            return Ok(results);
        }
        // --- Új munka felvétele (Validációval) ---
        [HttpPost]
        public async Task<ActionResult<Works>> PostWork(string licenseplate, int productionyear,string description,string category,int seriousness110,int customerId)
        {

            // Regex magyarázat:
            // ^[A-Z]{3}-\d{3}$          -> Régi formátum (pl. ABC-123)
            // ^[A-Z]{2}-[A-Z]{2}-\d{3}$ -> Új formátum (pl. AA-BB-123)
           

            // Alakítsuk nagybetűssé, mielőtt ellenőrizzük (így a kisbetűs beírás is jó lesz)
            licenseplate = licenseplate.ToUpper();

            if (!Regex.IsMatch(licenseplate, pattern))
            {
                return BadRequest("Érvénytelen rendszám formátum! Használj ABC-123 vagy AA-BB-123 formátumot kötőjelekkel.");
            }

            
            // 1. Alap validációk
            if (productionyear < 1900 || productionyear > DateTime.Now.Year)
            {
                return BadRequest("Érvénytelen gyártási év!");
            }

            if (seriousness110 < 1 || seriousness110 > 10)
            {
                return BadRequest("Érvénytelen súlyosság!(ez egy 1 ls 10 közötti egész szám)");
            }

            
            var existingCustomer = await _context.Customers.FindAsync(customerId);

            if (existingCustomer == null)
            {
                return BadRequest("Hiba: Ilyen ID-jű ügyfél nem létezik! Előbb hozd létre az ügyfelet a Customer oldalon.");
            }

          
            if (!allowedCategories.Contains(category))
            {
                return BadRequest("Érvénytelen kategória! Válassz a listából: Motor, Karosszéria, Futómű, Fékberendezés.");
            }
            category = category.ToLower();
            double ehours = category switch
            {
                "karosszéria" => 3,
                "motor" => 8,
                "futómű" => 6,
                "fékberendezés" => 4,
                
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

        // GET: api/Works/customer/5     megadja egy munka alapján a hozzá tartozó ügyfelet
        [HttpGet("customer/{id}")]
        public async Task<ActionResult<Customer>> GetCustomerByWorkId(int id)
        {
            // Beemeljük (Include) a kapcsolódó Customer objektumot is a munkához
            var work = await _context.Works
                .Include(w => w.Customer)
                .FirstOrDefaultAsync(w => w.Id == id);

            if (work == null)
            {
                return NotFound("Nincs ilyen munka!");
            }

            if (work.Customer == null)
            {
                return NotFound("Ehhez a munkához nem tartozik ügyfél!");
            }

            // Mivel az Include-dal betöltöttük, egyszerűen visszaadjuk a Customer-t
            return Ok(work.Customer);
        }
        //megadja id alapján a munkát
        [HttpGet("{id}")]
        public async Task<ActionResult<Works>> GetWorkById(int id)
        {
            var work = await _context.Works.FindAsync(id);

            if (work == null)
            {
                return NotFound("Nincs ilyen munka!");
            }

            

            return Ok(work);
        }
        //id alpján töröl
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWork(int id)
        {
            // 1. Megkeressük a munkát
            var work = await _context.Works.FindAsync(id);

            if (work == null)
            {
                return NotFound("A törölni kívánt munka nem található!");
            }

            // 2. Eltávolítjuk
            _context.Works.Remove(work);
            await _context.SaveChangesAsync();

            return Ok($"A(z) {id} azonosítójú munka sikeresen törölve.");
        }
        // id alapján a kitöltött mezőket módosítja, az üresen hagyottakat változatlanul hagyja, munkaórát újraszámolja.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutWork(
        int id,
        string? licensePlate = null,
        int? productionYear = null,
        int? seriousness = null,
        string? faultDescription = null,
        string? category = null,

        int? customerId = null)
        {
            // 1. Megkeressük az eredeti munkát az adatbázisban
            var work = await _context.Works.FindAsync(id);

            if (work == null)
            {
                return NotFound($"Nincs munka ezzel az ID-val: {id}");
            }

            // 2. Csak azt írjuk át, amit ténylegesen megadtak (nem null)
            if (!string.IsNullOrEmpty(licensePlate))
            {
                // Regex magyarázat:
                // ^[A-Z]{3}-\d{3}$          -> Régi formátum (pl. ABC-123)
                // ^[A-Z]{2}-[A-Z]{2}-\d{3}$ -> Új formátum (pl. AA-BB-123)
               

                // Alakítsuk nagybetűssé, mielőtt ellenőrizzük (így a kisbetűs beírás is jó lesz)
                licensePlate = licensePlate.ToUpper();

                if (!Regex.IsMatch(licensePlate, pattern))
                {
                    return BadRequest("Érvénytelen rendszám formátum! Használj ABC-123 vagy AA-BB-123 formátumot kötőjelekkel.");
                }

                work.LicensePlate = licensePlate;
            }

            if (productionYear.HasValue)
            {
                int currentYear = DateTime.Now.Year;
                if (productionYear < 1900 || productionYear > currentYear)
                {
                    return BadRequest($"A gyártási évnek 1900 és {currentYear + 1} között kell lennie!");
                }
                work.ProductionYear = productionYear.Value;
            }

            if (seriousness.HasValue)
            {
                if (seriousness < 1 || seriousness > 100)
                {
                    return BadRequest("A súlyosság mérőszáma em megfelelő! Ez legye egy 1 és 10 közötti egyész szám");
                }
                work.seriousness = seriousness.Value;
            }

            if (!string.IsNullOrEmpty(faultDescription))
            {
                work.FaultDescription = faultDescription;
            }

            if (!string.IsNullOrEmpty(category))
            {
                category = category.ToLower();
                
                if (!allowedCategories.Contains(category))
                {
                    return BadRequest("Érvénytelen kategória! Válassz a listából: Motor, Karosszéria, Futómű, Fékberendezés.");
                }
                work.Category = category;
            }
            double multiplier = 1;
            multiplier*= work.Category switch
            {
                "karosszéria" => 3,
                "motor" => 8,
                "futómű" => 6,
                "fékberendezés" => 4,
                
            };

            multiplier*= work.seriousness switch
            {
                10 => 1.0,   // Fixen 10-es értékre
                > 7 => 0.8,   // 7-nél nagyobb (8, 9)
                > 4 => 0.6,   // 4-nél nagyobb (5, 6, 7)
                > 2 => 0.4,   // 2-nél nagyobb (3, 4)
                _ => 0.2    // Minden másra (2 vagy az alatti)
            };

            int age = (int)(DateTime.Now.Year - work.ProductionYear);

            multiplier *= age switch
            {
                < 5 => 0.5,
                < 10 => 1.0,
                < 20 => 1.5,
                _ => 2.0  // Ez a "default", mindenre, ami 20 vagy több
            };

            
                work.EstimatedHours = multiplier;
            

            if (customerId.HasValue)
            {
                var customerExists = await _context.Customers.AnyAsync(c => c.Id == customerId);
                if (!customerExists)
                {
                    return BadRequest("A megadott ügyfél (CustomerId) nem létezik!");
                }
                work.CustomerId = customerId.Value;
            }

            // 3. Mentés
            try
            {
                await _context.SaveChangesAsync();
                return Ok(work); // Visszaadjuk a frissített objektumot, hogy lássuk az eredményt
            }
            catch (Exception ex)
            {
                return BadRequest($"Hiba a mentés során: {ex.Message}");
            }
        }
        //státusz léptető
        [HttpPut("change-status/{id}")]
        public async Task<ActionResult<Works>> ChangeStatus(int id)
        {
            // 1. Kikérjük az autót
            var work = await _context.Works
                .FirstOrDefaultAsync(w => w.Id == id);

            if (work == null) return NotFound("Nincs ilyen munka!");

            // 2. Logika a léptetéshez
            // Felvett Munka -> Elvégzés alatt -> Befejezett

            if (work.Status == "Felvett")
            {
                work.Status = "Elvégzés alatt";
            }
            else if (work.Status == "Elvégzés alatt")
            {
                work.Status = "Befejezett";
            }
            else if (work.Status == "Befejezett")
            {
                return BadRequest("A munka már befejezett állapotban van, nem léptethető tovább!");
            }
           

            // 3. Mentés és kiadás
            await _context.SaveChangesAsync();
            return Ok(work);
        }


    }
}
