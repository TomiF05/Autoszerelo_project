using Autoszerelo.models;
using Microsoft.AspNetCore.Mvc;

namespace Autoszerelo.Data
{
    public interface IWorksRepository
    {
        Task<IEnumerable<Works>> GetAllAsync();
        Task<Works?> GetByIdAsync(int id);
        Task<IEnumerable<Works>> GetWorksByCustomerIdAsync(int customerId);
        Task<bool> LicensePlateExistAsync(string licensePlate);
        Task<bool> LicensePlateExistForOtherWorkAsync(int workId,string licensePlate);
        Task AddAsync(Works work);
        Task UpdateAsync(Works work);
        Task DeleteAsync(Works work);
        double CalculateMultiplier(string category, int seriousness, int productionYear);
        Task<ActionResult<IEnumerable<Works>>> GetWorksAsync(
       string? licensePlate = null,
       int? productionYear = null,
       int? serousness = null,
       string? faultDescription = null,
       string? category = null,
       string? status = null,
       int? customerId = null
           );
    }
}
