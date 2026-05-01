using System.ComponentModel.DataAnnotations;

namespace Shared
{
    public class NoWhitespaceOnlyAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value == null)
            {
                return true;
            }

            if (value is not string strValue)
            {
                return false;
            }

            return !string.IsNullOrWhiteSpace(strValue);
        }
    }
}
