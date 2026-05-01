using Autoszerelo.Data;
using Autoszerelo.models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

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
        
        public async Task<ActionResult<Customer>> GetCustomerByWorkId(int id)
        {
            // Beemeljük (Include) a kapcsolódó Customer objektumot is a munkához
            return await _worksRepository.GetCustomerByWorkId(id);
        }
        //megadja id alapján a munkát
        
        public async Task<ActionResult<Works>> GetWorkById(int id)
        {
            Works? works = await _worksRepository.GetByIdAsync(id);
            return works;
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

        public async Task<ActionResult<Works>> ChangeStatus(int id)
        {
            // 1. Kikérjük az autót
            await _worksRepository.ChangeStatus(id);
            return await _worksRepository.GetByIdAsync(id);
           
        }

        public async Task<ActionResult<IEnumerable<Works>>> GetWorksAsync(string? licensePlate = null,
        int? productionYear = null,
        int? serousness = null,
        string? faultDescription = null,
        string? category = null,
        string? status = null,
        int? customerId = null)
        {
                return await _worksRepository.GetWorksAsync(licensePlate, productionYear, serousness, faultDescription, category, status, customerId);
            
        }

        public async Task<IActionResult> PutWork(
        int id,
        string? licensePlate = null,
        int? productionYear = null,
        int? seriousness = null,
        string? faultDescription = null,
        string? category = null,

        int? customerId = null)
        {
            // 1. Megkeressük az eredeti munkát az adatbázisban
            var work = await _worksRepository.GetByIdAsync(id);

            if (work == null)
            {
                throw new ArgumentException($"Nincs munka ezzel az ID-val: {id}");
            }

            // 2. Csak azt írjuk át, amit ténylegesen megadtak (nem null)
            if (!string.IsNullOrEmpty(licensePlate))
            {
                // Regex magyarázat:
                // ^[A-Z]{3}-\d{3}$          -> Régi formátum (pl. ABC-123)
                // ^[A-Z]{2}-[A-Z]{2}-\d{3}$ -> Új formátum (pl. AA-BB-123)


                // Alakítsuk nagybetűssé, mielőtt ellenőrizzük (így a kisbetűs beírás is jó lesz)
                licensePlate = licensePlate.ToUpper();

                if (!Regex.IsMatch(licensePlate, pattern))
                {
                    throw new ArgumentException("Érvénytelen rendszám formátum! Használj ABC-123 vagy AA-BB-123 formátumot kötőjelekkel.");
                }

                work.LicensePlate = licensePlate;
            }

            if (productionYear.HasValue)
            {
                int currentYear = DateTime.Now.Year;
                if (productionYear < 1900 || productionYear > currentYear)
                {
                    throw new ArgumentException($"A gyártási évnek 1900 és {currentYear + 1} között kell lennie!");
                }
                work.ProductionYear = productionYear.Value;
            }

            if (seriousness.HasValue)
            {
                if (seriousness < 1 || seriousness > 100)
                {
                    throw new ArgumentException("A súlyosság mérőszáma em megfelelő! Ez legye egy 1 és 10 közötti egyész szám");
                }
                work.seriousness = seriousness.Value;
            }

            if (!string.IsNullOrEmpty(faultDescription))
            {
                work.FaultDescription = faultDescription;
            }

            if (!string.IsNullOrEmpty(category))
            {
                category = category.ToLower();

                if (!allowedCategories.Contains(category))
                {
                    throw new ArgumentException("Érvénytelen kategória! Válassz a listából: Motor, Karosszéria, Futómű, Fékberendezés.");
                }
                work.Category = category;
            }


            work.EstimatedHours = _worksRepository.CalculateMultiplier(work.Category, work.seriousness, work.ProductionYear);


            if (customerId.HasValue)
            {
                var customerExists = await _worksRepository.IdExists(customerId.Value);
                if (!customerExists)
                {
                    throw new ArgumentException("A megadott ügyfél (CustomerId) nem létezik!");
                }
                work.CustomerId = customerId.Value;
            }

            // 3. Mentés
            await _worksRepository.UpdateAsync(work);
            return (IActionResult)work;
        }
    }
}

