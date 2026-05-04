using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Shared
{
    public class UpdateWorkRequest
    {
        public int Id {  get; set; }
        public int? CustomerId { get; set; }

        [RegularExpression(@"^[A-Z]{3}-\d{3}$")]
        [NoWhitespaceOnly]
        public string? LicensePlate { get; set; }

        [ValidProductionYear]
        public int? ProductionYear { get; set; }

        public string? Category { get; set; }

        [NoWhitespaceOnly]
        public string? FaultDescription { get; set; }

        [Range(1,10)]
        public int? Seriousness { get; set; }
        public string? Status { get; set; }
    }
}
