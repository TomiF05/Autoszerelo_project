using Autoszerelo.Data;
using Autoszerelo.models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Autoszerelo.Services
{
    public class WorkService : IWorkService
    {
        private readonly IWorksRepository _worksRepository;
        private readonly string[] allowedCategories = new[] { "motor", "karosszéria", "futómű", "fékberendezés" };
        private const string pattern = @"^([A-Z]{3}-\d{3}|[A-Z]{2}-[A-Z]{2}-\d{3})$";
        private readonly string[] stat = {"felvett munka", "elvégzés alatt", "befejezett" };
        public WorkService(IWorksRepository worksRepository)
        {
            _worksRepository = worksRepository;
        }

        // --- ÚJ METÓDUS: ÜGYFÉL SAJÁT MUNKÁINAK LEKÉRÉSE ---
        public async Task<List<WorkDto>> GetWorksByCustomerIdAsync(int customerId)
        {
            var actionResult = await _worksRepository.GetWorksAsync(customerId: customerId);
            var works = actionResult.Value;

            if (works == null)
            {
                return new List<WorkDto>();
            }

            // Átalakítjuk DTO listává, hogy a Frontend megkapja az adatokat
            return works.Where(w => w.CustomerId == customerId).Select(w => new WorkDto
            {
                Id = w.Id,
                CustomerId = w.CustomerId,
                LicensePlate = w.LicensePlate,
                ProductionYear = w.ProductionYear,
                Category = w.Category,
                FaultDescription = w.FaultDescription,
                Seriousness = w.seriousness,
                Status = w.Status,
                EstimatedHours = w.EstimatedHours
            }).ToList();
        }

        public async Task<ActionResult<Works>> PostWork(string licenseplate, int productionyear, string description, string category, int seriousness110, int customerId)
        {
            licenseplate = licenseplate.ToUpper();
            string normalizedCategory = category.ToLower();

            if (!Regex.IsMatch(licenseplate, pattern))
            {
                throw new ArgumentException("Érvénytelen rendszám formátum! Használj ABC-123 vagy AA-BB-123 formátumot kötőjelekkel.");
            }

            if (productionyear < 1900 || productionyear > DateTime.Now.Year)
            {
                throw new ArgumentException("Érvénytelen gyártási év!");
            }

            if (seriousness110 < 1 || seriousness110 > 10)
            {
                throw new ArgumentException("Érvénytelen súlyosság!(ez egy 1 ls 10 közötti egész szám)");
            }

            bool existingCustomer = await _worksRepository.IdExists(customerId);
            if (!existingCustomer)
            {
                throw new ArgumentException("Hiba: Ilyen ID-jű ügyfél nem létezik! Előbb hozd létre az ügyfelet a Customer oldalon.");
            }

            if (!allowedCategories.Contains(normalizedCategory))
            {
                throw new ArgumentException("Érvénytelen kategória! Válassz a listából: Motor, Karosszéria, Futómű, Fékberendezés.");
            }

            var work = new Works
            {
                LicensePlate = licenseplate,
                ProductionYear = productionyear,
                FaultDescription = description,
                Category = category,
                seriousness = seriousness110,
                EstimatedHours = _worksRepository.CalculateMultiplier(category, seriousness110, productionyear),
                CustomerId = customerId
            };

            await _worksRepository.AddAsync(work);
            return work;
        }

        public async Task<ActionResult<CustomerDto>> GetCustomerByWorkId(int id)
        {
            var result = await _worksRepository.GetCustomerByWorkId(id);
            var customer = result.Value;
            if (customer == null)
            {
                throw new ArgumentException("Hiba: Ilyen ID-jű ügyfél nem létezik! Előbb hozd létre az ügyfelet a Customer oldalon.");
            }
            return new CustomerDto
            {
                Id = customer.Id,
                Name = customer.Name,
                Email = customer.Email,
                Address = customer.Address,
                Works = customer.Works?.Select(w => new WorkDto
                {
                    Id = w.Id,
                    CustomerId = w.CustomerId,
                    LicensePlate = w.LicensePlate,
                    ProductionYear = w.ProductionYear,
                    Category = w.Category,
                    FaultDescription = w.FaultDescription,
                    Seriousness = w.seriousness,
                    Status = w.Status,
                    EstimatedHours = w.EstimatedHours
                }).ToList() ?? new List<WorkDto>()
            };
        }

        public async Task<ActionResult<WorkDto>> GetWorkById(int id)
        {
            Works? works = await _worksRepository.GetByIdAsync(id);
            if (works == null)
            {
                throw new ArgumentException("Hiba: Ilyen ID-jű munka nem létezik!");
            }
            return new WorkDto
            {
                Category = works.Category,
                CustomerId = works.CustomerId,
                Seriousness = works.seriousness,
                EstimatedHours = works.EstimatedHours,
                Id = works.Id,
                LicensePlate = works.LicensePlate,
                FaultDescription = works.FaultDescription,
                Status = works.Status,
                ProductionYear = works.ProductionYear
            };
        }

        public async Task<IActionResult> DeleteWork(int id)
        {
            var work = await _worksRepository.GetByIdAsync(id);
            if (work == null) return new NotFoundResult();

            await _worksRepository.DeleteAsync(work);
            return new OkResult();
        }

        public async Task<ActionResult<WorkDto>> ChangeStatus(int id)
        {
            await _worksRepository.ChangeStatus(id);
            var result = await _worksRepository.GetByIdAsync(id);
            if (result == null)
            {
                throw new ArgumentException("Hiba: Ilyen ID-jű munka nem létezik!");
            }
            return new WorkDto
            {
                Category = result.Category,
                CustomerId = result.CustomerId,
                Seriousness = result.seriousness,
                EstimatedHours = result.EstimatedHours,
                Id = result.Id,
                LicensePlate = result.LicensePlate,
                FaultDescription = result.FaultDescription,
                Status = result.Status,
                ProductionYear = result.ProductionYear
            };
        }

        public async Task<ActionResult<IEnumerable<WorkDto>>> GetWorksAsync(GetWorkDto work)
        {
            var actionresult = await _worksRepository.GetWorksAsync(work.LicensePlate, work.ProductionYear, work.Seriousness, work.FaultDescription, work.Category, work.Status, work.CustomerId);
            var works = actionresult.Value;

            if (works == null)
            {
                return new NotFoundResult();
            }

            return works.Select(static w => new WorkDto
            {
                Id = w.Id,
                CustomerId = w.CustomerId,
                LicensePlate = w.LicensePlate,
                ProductionYear = w.ProductionYear,
                Category = w.Category,
                FaultDescription = w.FaultDescription,
                Seriousness = w.seriousness,
                Status = w.Status,
                EstimatedHours = w.EstimatedHours
            }).ToList();
        }

        public async Task<IActionResult> PutWork(UpdateWorkRequest request)
        {
            var work = await _worksRepository.GetByIdAsync(request.Id);

            if (work == null)
            {
                return new NotFoundResult();
            }

            // --- Rendszám frissítése (csak ha érvényes) ---
            if (!string.IsNullOrEmpty(request.LicensePlate))
            {
                string licensePlate = request.LicensePlate.ToUpper();
                if (Regex.IsMatch(licensePlate, pattern))
                {
                    work.LicensePlate = licensePlate;
                }
            }

            // --- Hiba leírása ---
            if (!string.IsNullOrEmpty(request.FaultDescription))
            {
                work.FaultDescription = request.FaultDescription;
            }

            // --- Kategória: Ha üres vagy érvénytelen, NEM nyúlunk hozzá, marad a régi ---
            if (!string.IsNullOrEmpty(request.Category))
            {
                // Tipp: Itt vedd ki a .ToLower()-t, ha a Regex/Lista nagybetűvel várja!
                if (allowedCategories.Contains(request.Category))
                {
                    work.Category = request.Category;
                }
                // else: Itt nem dobunk hibát, csak csendben ignoráljuk az érvénytelen kategóriát
            }

            // --- Állapot frissítése (Ez az előbb kimaradt, most pótoljuk) ---
            if (!string.IsNullOrEmpty(request.Status))
            {
                if(!stat.Contains(request.Status.ToLower()))
                {
                    return new BadRequestObjectResult(new { message = "Nem megfelelő státusz! Ilyen státusz nem létezik a rendszerben." });
                }
                if(stat.IndexOf(request.Status) < stat.IndexOf(work.Status.ToLower())) {
                    work.Status = request.Status;
                }
                else
                {
                    return new BadRequestObjectResult(new { message = "Nem megfelelő státusz! A státusz nem léptethető visszafelé." });
                }
            }

            // --- Újraszámolás ---
            // Ha a kategória nem változott, akkor a régi kategóriával számol újra.
            work.EstimatedHours = _worksRepository.CalculateMultiplier(work.Category, work.seriousness, work.ProductionYear);

            await _worksRepository.UpdateAsync(work);
            return new OkObjectResult(work);
        }
    }
}
