using System;
using System.Linq;
using AFT.RegoV2.Core.Fraud.ApplicationServices.Data;
using FluentValidation;
using FluentValidation.Results;

namespace AFT.RegoV2.Core.Fraud.Validations
{
    public class SignUpFraudTypeValidator : AbstractValidator<SignUpFraudTypeDTO>
    {
        private readonly IFraudRepository _repository;

        public SignUpFraudTypeValidator(IFraudRepository repository)
        {
            _repository = repository;

            Custom((dto, context) =>
            {
                var instance = context.InstanceToValidate;

                var fraudTypes = _repository.SignUpFraudTypes.AsQueryable();
                if (instance.Id != Guid.Empty)
                    fraudTypes = fraudTypes.Where(o => o.Id != instance.Id);

                var isPresent = fraudTypes
                    .Any(o => o.Name == instance.FraudTypeName);

                return isPresent
                    ? new ValidationFailure("Name", "uniqueName")
                    : null;
            });
        }
    }
}