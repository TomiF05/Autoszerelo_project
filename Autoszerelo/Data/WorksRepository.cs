using Autoszerelo.models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Autoszerelo.Data
{
    public class WorksRepository : IWorksRepository
    {
        private readonly AppDbContext _context;

        public WorksRepository(AppDbContext context)
        {
            _context = context;
        }

        private double CalculateMultiplier(string category, int seriousness, int productionYear)
        {
            category = category.ToLower();

            // 1. Alap szorzó kategória alapján
            double multiplier = category switch
            {
                "karosszéria" => 3,
                "motor" => 8,
                "futómű" => 6,
                "fékberendezés" => 4,
                _ => 1.0 // Biztonsági default, ha ismeretlen kategória jönne
            };

            // 2. Súlyosság szorzó
            multiplier *= seriousness switch
            {
                10 => 1.0,
                > 7 => 0.8,
                > 4 => 0.6,
                > 2 => 0.4,
                _ => 0.2
            };

            // 3. Kor szorzó
            int age = DateTime.Now.Year - productionYear;
            multiplier *= age switch
            {
                < 5 => 0.5,
                < 10 => 1.0,
                < 20 => 1.5,
                _ => 2.0
            };

            return multiplier;
        }

        public async Task<ActionResult<IEnumerable<Works>>> GetWorksAsync(
       string? licensePlate = null,
       int? productionYear = null,
       int? serousness = null,
       string? faultDescription = null,
       string? category = null,
       string? status = null,
       int? customerId = null
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
                query = query.Where(w => w.ProductionYear == productionYear);
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

            return results;
        }
        public async Task AddAsync(Works work)
        {
            await _context.AddAsync(work);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Works work)
        {
            _context.Works.Remove(work);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Works>> GetAllAsync()
        {
            return await _context.Works.ToListAsync();
        }

        public async Task<Works?> GetByIdAsync(int id)
        {
            return await _context.Works.FirstOrDefaultAsync(w => w.Id == id);
        }

        public async Task<IEnumerable<Works>> GetWorksByCustomerIdAsync(int customerId)
        {
            return await _context.Works.Where(w => w.CustomerId == customerId).ToListAsync();
        }

        public async Task<bool> LicensePlateExistAsync(string licensePlate)
        {
            return await _context.Works.AnyAsync(x => x.LicensePlate.ToLower() == licensePlate.ToLower());
        }

        public async Task<bool> LicensePlateExistForOtherWorkAsync(int workId, string licensePlate)
        {
            return await _context.Works.AnyAsync(x => x.Id != workId && x.LicensePlate.ToLower() == licensePlate.ToLower());
        }

        public async Task UpdateAsync(Works work)
        {
            var existingWork = await _context.Works.FirstOrDefaultAsync(w => w.Id == work.Id);
            if (existingWork != null)
            {
                existingWork.CustomerId = work.CustomerId;
                existingWork.seriousness = work.seriousness;
                existingWork.EstimatedHours = work.EstimatedHours;
                existingWork.FaultDescription = work.FaultDescription;
                existingWork.LicensePlate = work.LicensePlate;
                existingWork.ProductionYear = work.ProductionYear;
                existingWork.Status = work.Status;
                existingWork.Category = work.Category;
                existingWork.Id = work.Id;
            }
            await _context.SaveChangesAsync();
        }
    }
}
