using Autoszerelo.models;
using Autoszerelo.Services;
using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Shared;

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
        public async Task<ActionResult<IEnumerable<CustomerDto>>> GetCustomers([FromQuery] FilterCustomerDto? customer)
        {
            var customersDto = await _customerService.GetCustomersAsync(customer);
            return Ok(customersDto);
        }

        // --- GET: Egy ügyfél + autók ---
        [HttpGet("{id}")]
        public async Task<ActionResult<CustomerDto>> GetCustomerWithCars(int id)
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);

            if (customer == null)
                return NotFound($"Nincs ügyfél ezzel az ID-val: {id}");

            return Ok(customer);
        }
        // --- POST: Új ügyfél ---
        [HttpPost]
        public async Task<ActionResult<CustomerDto>> PostCustomer([FromBody] AddCustomerRequest request)
        {
            // 1. Alapszintű validáció (amihez nem kell adatbázis)
            if (string.IsNullOrWhiteSpace(request.Name)) return BadRequest("A név kötelező!");
            if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains("@")) return BadRequest("Hibás email!");
            if (string.IsNullOrWhiteSpace(request.Address)) return BadRequest("A lakcím kötelező!");
                var customer=new CustomerDto
                {
                    Name = request.Name,
                    Email = request.Email,
                    Address = request.Address
                };
            try
            {
                // 2. A munka nehezét a Service végzi
                var newCustomer = await _customerService.CreateCustomerAsync(customer);
                return Ok(newCustomer);
            }
            catch (InvalidOperationException ex)
            {
                // 3. Ha a Service kidobta, hogy duplikáció van, itt csinálunk belőle 400-as hibát
                return BadRequest(ex.Message);
            }
        }

        // --- PUT: Frissítés ---
        [HttpPut]
        public async Task<IActionResult> PutCustomer([FromQuery] UpdateCustomerRequest customer)
        {
            try
            {
                var updated = await _customerService.UpdateCustomerAsync(customer);
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