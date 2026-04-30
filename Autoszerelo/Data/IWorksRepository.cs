using Autoszerelo.models;

namespace Autoszerelo.Data
{
    public interface IWorksRepository
    {
        Task<IEnumerable<Works>> GetAllAsync();
        Task<Works?> GetByIdAsync(int id);
        Task<IEnumerable<Works>> GetWorksByCustomerIdAsync(int customerId);
        Task<bool> LicensePlateExistAsync(string licensePlate);
        Task<bool> LicensePlateExistForOtherWorkAsync(int workId,string licensePlate);
        Task<bool> IdExists(int id);
        Task AddAsync(Works work);
        Task UpdateAsync(Works work);
        Task DeleteAsync(Works work);
    }
}
