using Autoszerelo.Data;
using Autoszerelo.models;
using Autoszerelo.Services;
using NSubstitute;
using Shared;
using Xunit;
namespace AutoszereloTest
{
    public class CustomerServiceTest
    {

        [Fact]
        public async Task GetCustomersAsync_ReturnsDataFromRepository()
        {
            var repository = Substitute.For<ICustomerRepository>();
            var customerService = new CustomerService(repository);

            var filter = new FilterCustomerDto { Name = "Kállai" };
            var fakeList = new List<Customer>
            {
                new Customer { Id = 1, Name = "Kállai Péter", Address = "Cím 1", Email = "email1@example.com" },
                new Customer { Id = 2, Name = "Varga Anna", Address = "Cím 2", Email = "email2@example.com" }
            };
            repository.GetCustomersAsync(null, filter.Name, null, null).Returns(fakeList);
            var result = await customerService.GetCustomersAsync(filter);

            Assert.NotNull(result);
            var resultList = result.ToList();

            Assert.Equal(2, resultList.Count);

            Assert.Equal("Kállai Péter", resultList[0].Name);
            Assert.Equal("Varga Anna", resultList[1].Name);
        }
        [Fact]
        public async Task CreateCustomerAsync_ThrowsException_WhenDuplicate()
        {
            var repository = Substitute.For<ICustomerRepository>();
            var customerService = new CustomerService(repository);
            var rosszCustomerDto = new CustomerDto
            {
                Name = "Kállai Péter",
                Address = "Cím 1",
                Email = "valami@gmail.com"
            };

            repository.EmailExistsAsync(rosszCustomerDto.Email).Returns(true);
            repository.AddressExistsAsync(rosszCustomerDto.Address).Returns(true);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => customerService.CreateCustomerAsync(rosszCustomerDto));


        }
        [Fact]
        public async Task CreateCustomerAsync_ThrowsException_WhenNameIsEmpty()
        {
            var repository = Substitute.For<ICustomerRepository>();
            var customerService = new CustomerService(repository);
            var rosszCustomerDto = new CustomerDto
            {
                Name = "",
                Address = "Cím 1",
                Email = "valami@gmail.com"
            };


            var uzenet = await Assert.ThrowsAsync<ArgumentException>(() => customerService.CreateCustomerAsync(rosszCustomerDto));

            Assert.Equal("Nem adtál meg nevet!", uzenet.Message);
        }

        [Fact]
        public async Task UpdateCustomer_ThrowsException_WhenCustomerNotFound()
        {
            var repository = Substitute.For<ICustomerRepository>();
            var customerService = new CustomerService(repository);
            var rosszCutomerDto = new UpdateCustomerRequest
            {
                Id = 100,
                Name = "Kállai Péter",
                Address = "Debrecen",
                Email = "KallaiPeter1@gmail.com"
            };
            await Assert.ThrowsAsync<InvalidOperationException>(() => customerService.UpdateCustomerAsync(rosszCutomerDto));

        }
        [Fact]
        public async Task UpdateCustomer_ThrowsException_WhenEmailExists()
        {
            var repository = Substitute.For<ICustomerRepository>();
            var customerService = new CustomerService(repository);
            var rosszCutomerDto = new UpdateCustomerRequest
            {
                Id = 1,
                Name = "Kállai Péter",
                Address = "Debrecen",
                Email = "KallaiPeter1@gmail.com"
            };
            var letezoCustomer = new Customer
            {
                Id = 1,
                Name = "Kállai Péter"
            };

            repository.GetByIdAsync(rosszCutomerDto.Id).Returns(letezoCustomer);

            repository.EmailExistsForOtherCustomerAsync(rosszCutomerDto.Id, rosszCutomerDto.Email).Returns(true);
            var uzenet = await Assert.ThrowsAsync<InvalidOperationException>(() => customerService.UpdateCustomerAsync(rosszCutomerDto));

            Assert.Equal("Hiba: Ez az email cím már egy másik ügyfélhez van rendelve!", uzenet.Message);
        }
    

        [Fact]
        public async Task DeleteCustomerAsync_ThrowsException_WhenCustomerNotFound()
        {
            var repository = Substitute.For<ICustomerRepository>();
            var customerService = new CustomerService(repository);
            var rosszCustomer = new Customer
            {
                Id = 100,
                Name = "Kállai Péter",
                Address = "Debrecen",
                Email = "KallaiPeter1@gmail.com"
            };
            var uzenet = await Assert.ThrowsAsync<InvalidOperationException>(() => customerService.DeleteCustomerAsync(rosszCustomer.Id));

            Assert.Equal("Nem található a törölni kívánt ügyfél", uzenet.Message);
        }
        [Fact]
        public async Task GetCustomerByIdAsync_ReturnsNull_WhenCustomerNotFound()
        {
            var repository = Substitute.For<ICustomerRepository>();
            var customerService = new CustomerService(repository);
            var rosszCustomerId = 100;
            var result = await customerService.GetCustomerByIdAsync(rosszCustomerId);
            Assert.Null(result);
        }
    
    } 

}