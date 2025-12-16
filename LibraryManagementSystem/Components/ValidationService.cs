using System.ComponentModel.DataAnnotations;
using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Components
{
    public class ValidationService
    {
        public List<ValidationResult> ValidateModel<T>(T model)
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(model);
            Validator.TryValidateObject(model, context, results, true);
            return results;
        }

        public bool IsValid<T>(T model)
        {
            return ValidateModel(model).Count == 0;
        }
    }
}