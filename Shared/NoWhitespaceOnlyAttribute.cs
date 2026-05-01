using System.ComponentModel.DataAnnotations;

namespace Shared
{
    public class NoWhitespaceOnlyAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return new ValidationResult(ErrorMessage ?? "A mező kitöltése kötelező.");
            }

            if (value is string strValue && string.IsNullOrWhiteSpace(strValue))
            {
                return new ValidationResult(ErrorMessage ?? "A mező nem állhat csak szóközökből (whitespace).");
            }

            return ValidationResult.Success;
        }
    }
}
