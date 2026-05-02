using Autoszerelo.Data;
using Autoszerelo.models;
using Autoszerelo.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared;
using System.ComponentModel;
using System.Net;
using System.Text.RegularExpressions;
namespace Autoszerelo.Controllers
{
    [Route("api/[controller]")] 
    [ApiController]
    public class WorksController : ControllerBase
    {
        private readonly IWorkService _workService;
        public WorksController(IWorkService workService)
        {
            _workService = workService;
        }

        //lekéri az összes munkát , vagy a szűrési feltételeknek megfelelő munkákat, ahova nem írsz ,arra nem szűr.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<WorkDto>>> GetWorks([FromQuery]GetWorkDto work)
        {
            return await _workService.GetWorksAsync(work);
        }

        // --- Új munka felvétele (Validációval) ---
        [HttpPost]
        // 1. Változtatás: Egyetlen [FromBody] paramétert várunk, magát a DTO-t!
        public async Task<ActionResult<Works>> PostWork([FromBody] AddWorkRequest request)
        {
            // 2. Változtatás: A request objektumból szedjük ki az adatokat és adjuk át a Service-nek
            await _workService.PostWork(
                request.LicensePlate,
                request.ProductionYear,
                request.FaultDescription, 
                request.Category,
                request.Seriousness,
                request.CustomerId
            );

            return Ok("Sikeres munka felvétel!");
        }

        // GET: api/Works/customer/5     megadja egy munka alapján a hozzá tartozó ügyfelet
        [HttpGet("customer")]
        public async Task<ActionResult<CustomerDto>> GetCustomerByWorkId([FromQuery]IdDto id)
        {
            // Beemeljük (Include) a kapcsolódó Customer objektumot is a munkához

            return await _workService.GetCustomerByWorkId(id.Id);
        }
        //megadja id alapján a munkát
        [HttpGet("GetWorkById")]
        public async Task<ActionResult<WorkDto>> GetWorkById(IdDto id)
        {
            return await _workService.GetWorkById(id.Id);
            
        }
        //id alpján töröl
        [HttpDelete]
        public async Task<IActionResult> DeleteWork(IdDto id)
        {
            // 1. Megkeressük a munkát
            return await _workService.DeleteWork(id.Id);
        }
        // id alapján a kitöltött mezőket módosítja, az üresen hagyottakat változatlanul hagyja, munkaórát újraszámolja.
        [HttpPut]
        public async Task<IActionResult> PutWork([FromQuery] UpdateWorkRequest? request)
        {
            // Átadjuk a teljes, Frontendtől kapott dobozt a Service-nek
            return await _workService.PutWork(request);
        }
        //státusz léptető
        [HttpPut("change-status")]
        public async Task<ActionResult<WorkDto>> ChangeStatus(IdDto id)
        {
            return await _workService.ChangeStatus(id.Id);
        }


    }
}
