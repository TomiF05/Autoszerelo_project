using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Shared
{
    public class UpdateCustomerRequest
    {
        public int Id {  get; set; }

        [NoWhitespaceOnly]
        public string? Name { get; set; }

        [NoWhitespaceOnly]
        public string? Address { get; set; }
        
        [EmailAddress]
        [NoWhitespaceOnly]
        public string? Email {  get; set; }
    }
}
