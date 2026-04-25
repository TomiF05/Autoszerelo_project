using System.Text.Json.Serialization;

namespace Autoszerelo.models
{
    public class Works
    {
        public int Id { get; set; }
        public string LicensePlate { get; set; } = string.Empty; // XXX-111
        public int ProductionYear { get; set; }
        public string FaultDescription { get; set; } = string.Empty;

        // Ez a kategória határozza meg a munkaidőt (Body, Engine, etc.)
        public string Category { get; set; } = string.Empty;
        public int EstimatedHours { get; set; }

        // Külső kulcs az ügyfélhez
        public int CustomerId { get; set; }
        
        
    }
}