using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Shared
{
    public class ValidProductionYearAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value is not int year)
            {
                return false;
            }


            return year >= 1900 && year <= DateTime.Now.Year;
        }
    }
}
