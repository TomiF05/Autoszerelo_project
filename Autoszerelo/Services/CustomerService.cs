using Autoszerelo.Data;
using Autoszerelo.models;
using Microsoft.EntityFrameworkCore;
using Shared;

namespace Autoszerelo.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;
        

        public CustomerService(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
           // _context = context;
        }

        public CustomerDto MapToDto(Customer customer)
        {
            return new CustomerDto
            {
                Id = customer.Id,
                Name = customer.Name,
                Email = customer.Email,
                Address = customer.Address,
                // EZT A RÉSZT ADTUK HOZZÁ:
                Works = customer.Works?.Select(w => new WorkDto
                {
                    Id = w.Id,
                    CustomerId = w.CustomerId,
                    LicensePlate = w.LicensePlate,
                    ManufactureYear = w.ProductionYear,
                    Category = w.Category,
                    FaultDescription = w.FaultDescription,
                    Seriousness = w.seriousness,
                    Status = w.Status,
                    EstimatedHours = w.EstimatedHours
                }).ToList() ?? new List<WorkDto>()
            };
        }

        //private Customer MapToEntity(AddCustomerRequest dto)
        //{
        //    return new Customer
        //    {
        //        Name = dto.Name,
        //        Email = dto.Email
        //    };
        //}

        //--- Összes ügyfél lekérése szűréssel ---
        public async Task<IEnumerable<CustomerDto>> GetCustomersAsync(FilterCustomerDto? filter)
        {
            // Itt küldjük le a szűrési feltételeket a Repository-nak
            var entities = await _customerRepository.GetCustomersAsync(
                filter?.Id,
                filter?.Name,
                filter?.Address,
                filter?.Email);

            // Használd a MapToDto-t, így ha ott javítod az Address-t, itt is jó lesz!
            return entities.Select(entity => MapToDto(entity));
        }
        //public async Task<IEnumerable<CustomerDto>> GetCustomersAsync(FilterCustomerDto? filter)
        //{
        //    // 1. Megvárjuk a Repository válaszát (Entity lista)
        //    var entities = await _customerRepository.GetCustomersAsync(
        //        filter?.Id,
        //        filter?.Name,
        //        filter?.Address,
        //        filter?.Email);

        //    // 2. Ha a repo null-t adna vissza, üres listával térünk vissza
        //    if (entities == null)
        //    {
        //        return Enumerable.Empty<CustomerDto>();
        //    }

        //    // 3. Konvertálás és LISTÁVÁ alakítás (.ToList() vagy .ToArray())
        //    // Ez biztosítja, hogy a visszatérési típus pontosan IEnumerable<CustomerDto> legyen
        //    return entities.Select(entity => MapToDto(entity)).ToList();
        //}

        // --- Egy ügyfél keresése ID alapján (autókkal együtt) ---
        public async Task<CustomerDto?> GetCustomerByIdAsync(int id)
        {
            var customer = await _customerRepository.GetByIdWithWorksAsync(id);
             
            if (customer != null) {
                var newCustomer = MapToDto(customer);
                newCustomer.Works = customer.Works?.Select(w => new WorkDto
                {
                    Id = w.Id,
                    CustomerId = w.CustomerId,
                    LicensePlate = w.LicensePlate,
                    ManufactureYear = w.ProductionYear,
                    Category = w.Category,
                    FaultDescription = w.FaultDescription,
                    Seriousness = w.seriousness,
                    Status = w.Status,
                    // Átadjuk a backend által kiszámolt értéket
                    EstimatedHours = w.EstimatedHours
                }).ToList() ?? new List<WorkDto>();

                return newCustomer;
   
        }
            return null;
        }

        // --- Új ügyfél létrehozása validációval ---
        public async Task<CustomerDto> CreateCustomerAsync(CustomerDto customer)
        {
            // Duplikáció ellenőrzése
            bool isDuplicate = await _customerRepository.EmailExistsAsync(customer.Email);

            if (isDuplicate)
            {
                // Kivételt dobunk, amit a Controller elkap és BadRequest-et csinál belőle
                throw new InvalidOperationException("Hiba: Ez az ügyfél már szerepel a rendszerben!");
            }
            if (string.IsNullOrWhiteSpace(customer.Name) || customer.Name == "string")
            {
                throw new ArgumentException("Nem adtál meg nevet!");
            }
            if (string.IsNullOrWhiteSpace(customer.Address) || customer.Address == "string")
            {
                throw new ArgumentException("Nem adtál meg címet!");
            }
            if (string.IsNullOrWhiteSpace(customer.Email) || customer.Email == "string")
            {
                
                throw new ArgumentException("Nem adtál meg email címet!");
            }
            if (!customer.Email.Contains("@"))
                throw new ArgumentException("Érvénytelen email formátum!");

            // Use a different variable name to avoid shadowing the parameter
            var newCustomer = new Customer
            {
                Name = customer.Name,
                Address = customer.Address,
                Email = customer.Email
            };

            await _customerRepository.AddAsync(newCustomer);
            return MapToDto(newCustomer);
        }

        // --- Ügyfél adatainak frissítése ---
        public async Task<CustomerDto?> UpdateCustomerAsync(UpdateCustomerRequest customerDto)
        {
            var customer = await _customerRepository.GetByIdAsync(customerDto.Id);
            if (customer == null) return null;

            if (!string.IsNullOrWhiteSpace(customerDto.Name) && customerDto.Name != "string")
            {
                customer.Name = customerDto.Name;
            }

            if (!string.IsNullOrWhiteSpace(customerDto.Address) && customerDto.Address != "string")
            {
                customer.Address = customerDto.Address;
            }

            if (!string.IsNullOrWhiteSpace(customerDto.Email) && customerDto.Email != "string")
            {
                if (!customerDto.Email.Contains("@"))
                    throw new ArgumentException("Érvénytelen email formátum!");

                customer.Email = customerDto.Email;
            }
            if (await _customerRepository.EmailExistsForOtherCustomerAsync(customerDto.Id, customer.Email))
            {
                throw new InvalidOperationException("Hiba: Ez az email cím már egy másik ügyfélhez van rendelve!");
            }

            await _customerRepository.UpdateAsync(customer);
            return MapToDto(customer);
        }

        // --- Ügyfél törlése ---
        public async Task<bool> DeleteCustomerAsync(int id)
        {
            var customer = await _customerRepository.GetByIdAsync(id);
            if (customer == null) return false;

            await _customerRepository.DeleteAsync(customer);
            return true;
        }
    }
}