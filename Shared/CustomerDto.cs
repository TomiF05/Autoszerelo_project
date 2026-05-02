using System;
using System.Collections.Generic;
using System.Text;

namespace Shared
{
    public class CustomerDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public List<WorkDto> Works { get; set; } = new List<WorkDto>();
    }
}
