using Autoszerelo.models;
using Microsoft.AspNetCore.Mvc;
using Shared;

namespace Autoszerelo.Services
{
    public interface IWorkService
    {
        Task<ActionResult<IEnumerable<WorkDto>>> GetWorksAsync(GetWorkDto work);

        Task<ActionResult<Works>> PostWork(string licenseplate, int productionyear, string description, string category, int seriousness110, int customerId);
        Task<ActionResult<CustomerDto>> GetCustomerByWorkId(int id);
        Task<ActionResult<WorkDto>> GetWorkById(int id);
        Task<IActionResult> DeleteWork(int id);

        Task<IActionResult> PutWork(UpdateWorkRequest request);
        Task<ActionResult<WorkDto>> ChangeStatus(int id);


    }
}
