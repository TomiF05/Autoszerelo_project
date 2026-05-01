using System;
using System.Collections.Generic;
using System.Text;

namespace Shared
{
    public class GetAllWorkResponse
    {
        public List<WorkDto> Works { get; set; } = new();
    }
}
