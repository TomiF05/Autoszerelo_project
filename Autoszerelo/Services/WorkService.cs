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
        public WorkService(IWorksRepository worksRepository)
        {
            _worksRepository = worksRepository;
        }





        public async Task<ActionResult<Works>> PostWork(string licenseplate, int productionyear, string description, string category, int seriousness110, int customerId)
        {

            // Regex magyarázat: ez statikus változó lesz
            // ^[A-Z]{3}-\d{3}$          -> Régi formátum (pl. ABC-123)
            // ^[A-Z]{2}-[A-Z]{2}-\d{3}$ -> Új formátum (pl. AA-BB-123)


            // Alakítsuk nagybetűssé, mielőtt ellenőrizzük (így a kisbetűs beírás is jó lesz)
            licenseplate = licenseplate.ToUpper();
            string normalizedCategory = category.ToLower();

            if (!Regex.IsMatch(licenseplate, pattern))
            {
                throw new ArgumentException("Érvénytelen rendszám formátum! Használj ABC-123 vagy AA-BB-123 formátumot kötőjelekkel.");
            }


            // 1. Alap validációk
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

            //work.Customer = existingCustomer;
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

            // 3. Mentés
            await _worksRepository.AddAsync(work);


            return work;
        }

        // GET: api/Works/customer/5     megadja egy munka alapján a hozzá tartozó ügyfelet

        public async Task<ActionResult<CustomerDto>> GetCustomerByWorkId(int id)
        {
            // Beemeljük (Include) a kapcsolódó Customer objektumot is a munkához
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
                // EZT A RÉSZT ADTUK HOZZÁ:
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
        //megadja id alapján a munkát

        public async Task<ActionResult<WorkDto>> GetWorkById(int id)
        {
            Works? works = await _worksRepository.GetByIdAsync(id);
            if(works == null)
            {
                throw new ArgumentException("Hiba: Ilyen ID-jű munka nem létezik!");
            }
            var result = new WorkDto
            {
                Category = works.Category,
                CustomerId = works.CustomerId,
                Seriousness = works.seriousness,
                EstimatedHours = works.EstimatedHours,
                Id = works.Id,
                LicensePlate =  works.LicensePlate,
                FaultDescription = works.FaultDescription,
                Status = works.Status,
                ProductionYear = works.ProductionYear
            };
            return result;
        }
        //id alpján töröl

        public async Task<IActionResult> DeleteWork(int id)
        {
            var work = await _worksRepository.GetByIdAsync(id); // 1. Megvárjuk a konkrét objektumot[cite: 5, 10]
            if (work == null) return new NotFoundResult();

            await _worksRepository.DeleteAsync(work); // 2. A konkrét objektumot töröljük[cite: 9, 10]
            return new OkResult();
        }
        // id alapján a kitöltött mezőket módosítja, az üresen hagyottakat változatlanul hagyja, munkaórát újraszámolja.


        //státusz léptető

        public async Task<ActionResult<WorkDto>> ChangeStatus(int id)
        {
            // 1. Kikérjük az autót
            await _worksRepository.ChangeStatus(id);
            var result=await _worksRepository.GetByIdAsync(id);
            if(result == null)
            {
                throw new ArgumentException("Hiba: Ilyen ID-jű munka nem létezik!");
            }
            var work = new WorkDto
            {
                Category=result.Category,
                CustomerId=result.CustomerId,
                Seriousness=result.seriousness,
                EstimatedHours=result.EstimatedHours,
                Id=result.Id,
                LicensePlate=result.LicensePlate,
                FaultDescription=result.FaultDescription,
                Status=result.Status,
                ProductionYear=result.ProductionYear
            };
            return work;
            
        }

        public async Task<ActionResult<IEnumerable<WorkDto>>> GetWorksAsync(GetWorkDto work)
        {
            var actionresult = await _worksRepository.GetWorksAsync(work.LicensePlate, work.ProductionYear, work.Seriousness, work.FaultDescription, work.Category, work.Status, work.CustomerId);
            
            var works = actionresult.Value;

            if(works == null)
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
            // 1. Megkeressük az eredeti munkát az adatbázisban
            var work = await _worksRepository.GetByIdAsync(request.Id);

            if (work == null)
            {
                return new NotFoundResult(); // Helyes HTTP válasz, ha nem találja
            }

            // 2. Csak azt írjuk át, amit ténylegesen megadtak a Frontendről (request)

            if (!string.IsNullOrEmpty(request.LicensePlate))
            {
                string licensePlate = request.LicensePlate.ToUpper();
                if (!Regex.IsMatch(licensePlate, pattern)) // Gondolom a pattern fentebb definiálva van // igen
                {
                    throw new ArgumentException("Érvénytelen rendszám formátum! Használj ABC-123 vagy AA-BB-123 formátumot kötőjelekkel.");
                }
                work.LicensePlate = licensePlate;
            }

            if (!string.IsNullOrEmpty(request.FaultDescription))
            {
                work.FaultDescription = request.FaultDescription;
            }

            if (!string.IsNullOrEmpty(request.Category))
            {
                string category = request.Category.ToLower();
                if (!allowedCategories.Contains(category)) // Gondolom ez is definiálva van fent // igen
                {
                    throw new ArgumentException("Érvénytelen kategória! Válassz a listából: Motor, Karosszéria, Futómű, Fékberendezés.");
                }
                work.Category = category;
            }

            // --- EZ A RÉSZ HIÁNYZOTT EDDIG: A STÁTUSZ FRISSÍTÉSE ---
            //if (!string.IsNullOrEmpty(request.Status))
            //{
            //    work.Status = request.Status;
            //}


            // Mivel változhatott a kategória, újraszámoljuk az időt az EREDETI (adatbázisos) évjárat és súlyosság alapján
            work.EstimatedHours = _worksRepository.CalculateMultiplier(work.Category, work.seriousness, work.ProductionYear);

            // 3. Mentés
            await _worksRepository.UpdateAsync(work);

            // 4. Visszaadjuk a sikeres eredményt az újrafelhasználáshoz
            return new OkObjectResult(work);
        }
    }
}
