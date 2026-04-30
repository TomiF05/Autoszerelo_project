using Autoszerelo.Data;
using Autoszerelo.models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.Net;
using System.Text.RegularExpressions;
using Autoszerelo.Services;
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
            return await _workService.GetWorksAsync(licensePlate, productionYear, serousness, faultDescription, category, status, customerId);
        }
        
        // --- Új munka felvétele (Validációval) ---
        [HttpPost]
        public async Task<ActionResult<Works>> PostWork(string licenseplate, int productionyear,string description,string category,int seriousness110,int customerId)
        {

           _workService.PostWork(licenseplate, productionyear, description, category, seriousness110, customerId);
            return Ok("Sikeres munka felvétel!");
        }

        // GET: api/Works/customer/5     megadja egy munka alapján a hozzá tartozó ügyfelet
        [HttpGet("customer/{id}")]
        public async Task<ActionResult<Customer>> GetCustomerByWorkId(int id)
        {
            // Beemeljük (Include) a kapcsolódó Customer objektumot is a munkához

            return await _workService.GetCustomerByWorkId(id);
        }
        //megadja id alapján a munkát
        [HttpGet("{id}")]
        public async Task<ActionResult<Works>> GetWorkById(int id)
        {
            return await _workService.GetWorkById(id);
            
        }
        //id alpján töröl
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWork(int id)
        {
            // 1. Megkeressük a munkát
            return await _workService.DeleteWork(id);
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
           return await _workService.PutWork(id, licensePlate, productionYear, seriousness, faultDescription, category, customerId);
        }
        //státusz léptető
        [HttpPut("change-status/{id}")]
        public async Task<ActionResult<Works>> ChangeStatus(int id)
        {
            return await _workService.ChangeStatus(id);
        }


    }
}
