using Autoszerelo.models;
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
