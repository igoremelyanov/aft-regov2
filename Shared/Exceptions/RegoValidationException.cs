using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using FluentValidation.Results;

namespace AFT.RegoV2.Shared
{
    public class RegoValidationException : RegoException
    {
        public IList<RegoValidationError> ValidationErrors { get; private set; }

        public RegoValidationException(string message) : base(message)
        {
        }

        public RegoValidationException(ValidationResult validationResult)
            : base(validationResult.Errors.First().ErrorMessage)
        {
            ValidationErrors = new List<RegoValidationError>();

            foreach (var error in validationResult.Errors)
            {
                ValidationErrors.Add(new RegoValidationError
                {
                    FieldName = error.PropertyName,
                    ErrorMessage = error.ErrorMessage
                });
            }
        }
    }

    public class RegoValidationError
    {
        public string FieldName { get; set; }

        public string ErrorMessage { get; set; }
    }

    public static class RegoValidatorEx
    {
        public static void RegoValidateAndThrow<T>(this IValidator<T> validator, T instance)
        {
            var validationResult = validator.Validate(instance);

            if (!validationResult.IsValid)
            {
                throw new RegoValidationException(validationResult);
            }
        }
    }
}