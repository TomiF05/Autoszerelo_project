using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Shared
{
    public class AddWorkRequest
    {
        [Required]
        public int CustomerId { get; set; }

        [Required]
        [RegularExpression(@"^[A-Z]{3}-\d{3}$")]
        [NoWhitespaceOnly]
        public string LicensePlate { get; set; }

        [Required]
        [ValidProductionYear]
        public int ProductionYear { get; set; }

        [Required]
        [RegularExpression(@"^(Karosszéria|Motor|Futómű|Fékberendezés)$")]
        public string Category { get; set; }

        [Required]
        [NoWhitespaceOnly]
        public string FaultDescription { get; set; }

        [Required]
        [Range(1,10)]
        public int Seriousness { get; set; }

        public string Status { get; set; } = "Felvett Munka";
    }
}
