using System;
using System.Collections.Generic;
using Shared;
using Autoszerelo.Services;
using System.Text;
using Xunit;
using NSubstitute;
using Autoszerelo.Data;
using Autoszerelo.models;

namespace AutoszereloServicesTest
{
    public class WorksServiceTest
    {
        [Fact] 
        public async Task PostWork_ThrowsException_WhenLicensePlateIsInvalid()
        {
            var repository = Substitute.For<IWorksRepository>();
            var workService = new WorkService(repository);

            var uzenet = await Assert.ThrowsAsync<ArgumentException>(() => workService.PostWork("HIBÁSRENDSZÁM", 2010, "Nem indul", "motor", 2, 1));

            Assert.Equal("Érvénytelen rendszám formátum! Használj ABC-123 vagy AA-BB-123 formátumot kötőjelekkel.", uzenet.Message);
        }

        [Fact]
        public async Task PostWork_ThrowsException_WhenDateIsInvalid()
        {
            var repository = Substitute.For<IWorksRepository>();
            var workService = new WorkService(repository);

            var uzenet = await Assert.ThrowsAsync<ArgumentException>(() => workService.PostWork("ABC-123", 1800, "Nem indul", "motor", 2, 1));

            Assert.Equal("Érvénytelen gyártási év!", uzenet.Message);
        }

        [Fact]

        public async Task PostWork_ThrowsException_WhenSeriousnessIsInvalid()
        {
            var repository = Substitute.For<IWorksRepository>();
            var workService = new WorkService(repository);

            var uzenet = await Assert.ThrowsAsync<ArgumentException>(() => workService.PostWork("ABC-123", 2010, "Nem indul", "motor", 100, 1));

            Assert.Equal("Érvénytelen súlyosság!(ez egy 1 ls 10 közötti egész szám)", uzenet.Message);
        }

        [Fact]
        public async Task PostWork_ThrowsException_WhenCategoryIsInvalid()
        {
            var repository = Substitute.For<IWorksRepository>();
            var workService = new WorkService(repository);
            repository.IdExists(1).Returns(true);
            var uzenet = await Assert.ThrowsAsync<ArgumentException>(() => workService.PostWork("ABC-123", 2010, "Nem indul", "Hiba", 2, 1));
            Assert.Equal("Érvénytelen kategória! Válassz a listából: Motor, Karosszéria, Futómű, Fékberendezés.", uzenet.Message);
        }
        [Fact]
        public async Task PostWork_ThrowsException_WhenCustomerIdDoesNotExist()
        {
            var repository = Substitute.For<IWorksRepository>();
            var workService = new WorkService(repository);
            repository.IdExists(999).Returns(false);
            var uzenet = await Assert.ThrowsAsync<ArgumentException>(() => workService.PostWork("ABC-123", 2010, "Nem indul", "motor", 2, 999));
            Assert.Equal("Hiba: Ilyen ID-jű ügyfél nem létezik! Előbb hozd létre az ügyfelet a Customer oldalon.", uzenet.Message);
        }
        [Fact]
        public async Task PostWork_ReturnsWork_WhenAllParameterMatches()
        {
            var repository = Substitute.For<IWorksRepository>();
            var workService = new WorkService(repository);
            repository.IdExists(1).Returns(true);
            var eredmeny = await workService.PostWork("abc-123", 2010, "Nem indul", "motor", 2, 1);
            Assert.NotNull(eredmeny);
            var work = eredmeny.Value;
            Assert.NotNull(work);
            Assert.Equal("ABC-123", work.LicensePlate);
            Assert.Equal(2010, work.ProductionYear);
            Assert.Equal("Nem indul", work.FaultDescription);
            Assert.Equal("motor", work.Category);
            Assert.Equal(2, work.seriousness);
            Assert.Equal(1, work.CustomerId);
        }

        [Fact]

        public async Task GetCustomerByWorkId_ReturnsCustomer()
        {
            var repository = Substitute.For<IWorksRepository>();
            var workService = new WorkService(repository);
            var customer = new Customer
            {
                Id = 1,
                Name = "Kállai Péter",
                Address = "Debrecen",
                Email = "KallaiPeter1@gmail.com"
            };
            repository.GetCustomerByWorkId(1).Returns(customer);
            var eredmeny = await workService.GetCustomerByWorkId(1);
            var kibontott = eredmeny.Value;
            Assert.NotNull(kibontott);
            Assert.Equal(customer.Id, kibontott.Id);
            Assert.Equal(customer.Name, kibontott.Name);
            Assert.Equal(customer.Email, kibontott.Email);
            Assert.NotNull(kibontott.Works);
        }

        [Fact]
        public async Task GetWorkById_ReturnsWork_WhenWorkExists()
        {
            var repository = Substitute.For<IWorksRepository>();
            var workService = new WorkService(repository);
            var work = new Works
            {
                Id = 1,
                LicensePlate = "ABC-123",
                ProductionYear = 2010,
                FaultDescription = "Nem indul",
                Category = "motor",
                seriousness = 2,
                CustomerId = 1
            };
            repository.GetByIdAsync(1).Returns(work);
            var eredmeny = await workService.GetWorkById(1);
            var kibontott = eredmeny.Value;
            Assert.NotNull(kibontott);
            Assert.Equal(work.Id, kibontott.Id);
            Assert.Equal(work.LicensePlate, kibontott.LicensePlate);
            Assert.Equal(work.ProductionYear, kibontott.ProductionYear);
            Assert.Equal(work.FaultDescription, kibontott.FaultDescription);
            Assert.Equal(work.Category, kibontott.Category);
            Assert.Equal(work.seriousness, kibontott.Seriousness);
            Assert.Equal(work.CustomerId, kibontott.CustomerId);
        }

        [Fact]
        public async Task GetWorkById_ThrowsException_WhenWorkDoesNotExist()
        {
            var repository = Substitute.For<IWorksRepository>();
            var workService = new WorkService(repository);
            var eredmeny = await Assert.ThrowsAsync<ArgumentException>(() => workService.GetWorkById(999));
            Assert.Equal("Hiba: Ilyen ID-jű munka nem létezik!", eredmeny.Message);
        }
    }
}
