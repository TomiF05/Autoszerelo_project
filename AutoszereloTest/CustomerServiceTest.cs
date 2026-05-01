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
    }
}
