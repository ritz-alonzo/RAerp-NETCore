using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace RAerp.Extensions
{
    public static class FluentValidationExtensions
    {
        public static string AddToModelState(this ValidationResult result, ModelStateDictionary modelState, string errorMessage)
        {
            var validationErrorMessages = "";
            int errorCount = 1;
            foreach (var error in result.Errors)
            {
                // Maps the property name and error message directly to ModelState
                modelState.AddModelError(error.PropertyName, error.ErrorMessage);
                validationErrorMessages = $"</br> {errorCount}. {error.PropertyName} - <small>{error.ErrorMessage}</small>";
                errorCount++;
            }
            return errorMessage + validationErrorMessages;
        }

        public static string RequestDtoValidationErrors(this ValidationResult result, string errorMessage)
        {
            var validationErrorMessages = "";
            int errorCount = 1;
            foreach (var error in result.Errors)
            {
                validationErrorMessages += $"</br> {errorCount}. {error.PropertyName} - <small>{error.ErrorMessage}</small>";
                errorCount++;
            }
            return errorMessage + validationErrorMessages;
        }

        //public static string EntityValidationErrors
    }
}
