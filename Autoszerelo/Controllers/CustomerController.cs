using Autoszerelo.models;
using Autoszerelo.Services;
using Microsoft.AspNetCore.Mvc;

namespace Autoszerelo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        // Csak a Service-t kapja meg, az adatbázist már nem is látja
        public CustomerController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        // --- GET: Összes ügyfél ---
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers(string? name, string? address, string? email)
        {
            var customers = await _customerService.GetCustomersAsync(name, address, email);
            return Ok(customers);
        }

        // --- GET: Egy ügyfél + autók ---
        [HttpGet("{id}")]
        public async Task<ActionResult<Customer>> GetCustomerWithCars(int id)
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);

            if (customer == null)
                return NotFound($"Nincs ügyfél ezzel az ID-val: {id}");

            return Ok(customer);
        }

        // --- POST: Új ügyfél ---
        [HttpPost]
        public async Task<ActionResult<Customer>> PostCustomer(string name, string address, string email)
        {
            // 1. Alapszintű validáció (amihez nem kell adatbázis)
            if (string.IsNullOrWhiteSpace(name)) return BadRequest("A név kötelező!");
            if (string.IsNullOrWhiteSpace(email) || !email.Contains("@")) return BadRequest("Hibás email!");

            try
            {
                // 2. A munka nehezét a Service végzi
                var newCustomer = await _customerService.CreateCustomerAsync(name, address, email);
                return Ok(newCustomer);
            }
            catch (InvalidOperationException ex)
            {
                // 3. Ha a Service kidobta, hogy duplikáció van, itt csinálunk belőle 400-as hibát
                return BadRequest(ex.Message);
            }
        }

        // --- PUT: Frissítés ---
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCustomer(int id, string? name, string? address, string? email)
        {
            try
            {
                var updated = await _customerService.UpdateCustomerAsync(id, name, address, email);
                if (updated == null) return NotFound("Nincs ilyen ügyfél.");

                return Ok(updated);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // --- DELETE: Törlés ---
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            var success = await _customerService.DeleteCustomerAsync(id);

            if (!success) return NotFound();

            return Ok("Ügyfél és autói törölve.");
        }
    }
}