using System.ComponentModel.DataAnnotations;

namespace AuthenticationDemo.Validators;

public class AdultAttribute : ValidationAttribute
{
    public string GetErrorMessage() => "You must be at least 18 years old to register.";

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null) 
            return new ValidationResult("Value is empty");

        var birthDate = (DateTime)value;
        var age = DateTime.Now.Year - birthDate.Year;

        if (DateTime.Now.Month < birthDate.Month
         || (DateTime.Now.Month == birthDate.Month && DateTime.Now.Day < birthDate.Day))
        {
            age--;
        }

        if (age < 18)
        {
            return new ValidationResult(GetErrorMessage());
        }

        return ValidationResult.Success;
    }
}
