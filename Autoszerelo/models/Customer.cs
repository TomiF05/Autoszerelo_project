namespace Autoszerelo.models
{
    public class Customer
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
