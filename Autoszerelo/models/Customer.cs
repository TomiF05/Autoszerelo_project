using System.Text.Json.Serialization;

namespace Autoszerelo.models
{
    public class Customer
    {
        public int Id { get; set; } 
        public string Name { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        [JsonIgnore]
        public virtual ICollection<Works> Works { get; set; } = new List<Works>();
    }
}
