using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Shared
{
    public  class AddCustomerRequest
    {
        [Required]
        [NoWhitespaceOnly]
        public string Name { get; set; }

        [Required]
        [NoWhitespaceOnly]
        public string Address { get; set; }

        [Required]
        [EmailAddress]
        [NoWhitespaceOnly]
        public string Email { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        // A Role maradhat így, az alapértelmezett "User" tökéletes!
        public string Role { get; set; } = "User";
    }
}
