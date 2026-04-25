using Autoszerelo.models;

namespace Autoszerelo.Models
{
    public class WorkOrder
    {
        public int Id { get; set; }
        public string LicensePlate { get; set; } = string.Empty; // XXX-111
        public int ProductionYear { get; set; }
        public string FaultDescription { get; set; } = string.Empty;

        // Ez a kategória határozza meg a munkaidőt (Body, Engine, etc.)
        public string Category { get; set; } = string.Empty;
        public int EstimatedHours { get; set; }

        // Külső kulcs az ügyfélhez
        public Guid CustomerId { get; set; }
        public Customer? Customer { get; set; }
    }
}