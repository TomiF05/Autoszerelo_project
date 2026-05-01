using Autoszerelo.Data;
using Autoszerelo.models;
using Autoszerelo.Services;
using NSubstitute;
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

            string keresettName = "Kállai";
            var fakeList = new List<Customer>
            {
                new Customer { Id = 1, Name = "Kállai Péter", Address = "Cím 1", Email = "email1@example.com" },
                new Customer { Id = 2, Name = "Varga Anna", Address = "Cím 2", Email = "email2@example.com" }
            };
            repository.GetCustomersAsync(keresettName, null, null).Returns(fakeList);
            var result = await customerService.GetCustomersAsync(keresettName, null, null);

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
            string name = "Kállai Péter";
            string address = "Cím 1";
            string email = "email1@example.com";

            repository.EmailExistsAsync(email).Returns(true);
            repository.AddressExistsAsync(address).Returns(true);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => customerService.CreateCustomerAsync(name, address, email));


        }
        [Fact]
        public async Task CreateCustomerAsync_ThrowsException_WhenNameIsEmpty()
        {
            var repository = Substitute.For<ICustomerRepository>();
            var customerService = new CustomerService(repository);
            string name = "";
            string address = "Cím 1";
            string email = "valami@gmail.com";
            
            var uzenet = await Assert.ThrowsAsync<ArgumentException>(() => customerService.CreateCustomerAsync(name, address, email));

            Assert.Equal("Nem adtál meg nevet!", uzenet.Message);
        }
    }
}