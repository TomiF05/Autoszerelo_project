using System;
using System.Collections.Generic;
using System.Text;

namespace Shared
{
    public class GetWorkDto
    {
        
        public int? CustomerId { get; set; }
        public string? LicensePlate { get; set; }
        public int? ProductionYear { get; set; }
        public string? Category { get; set; }
        public string? FaultDescription { get; set; }
        public int? Seriousness { get; set; }
        public string? Status { get; set; }

        //ezt a backend számolja 
        
    }
}
