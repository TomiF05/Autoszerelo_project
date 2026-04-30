using Autoszerelo.models;
using Microsoft.AspNetCore.Mvc;

namespace Autoszerelo.Services
{
    public interface IWorkService
    {
        Task<ActionResult<IEnumerable<Works>>> GetWorksAsync(string? licensePlate = null,
        int? productionYear = null,
        int? serousness = null,
        string? faultDescription = null,
        string? category = null,
        string? status = null,
        int? customerId = null);

        Task<ActionResult<Works>> PostWork(string licenseplate, int productionyear, string description, string category, int seriousness110, int customerId);
        Task<ActionResult<Customer>> GetCustomerByWorkId(int id);
        Task<ActionResult<Works>> GetWorkById(int id);
        Task<IActionResult> DeleteWork(int id);

        Task<IActionResult> PutWork(
        int id,
        string? licensePlate = null,
        int? productionYear = null,
        int? seriousness = null,
        string? faultDescription = null,
        string? category = null,
        int? customerId = null);
        Task<ActionResult<Works>> ChangeStatus(int id);


    }
}
