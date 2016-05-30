﻿using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.Core.Brand.Interface.Data;
using FluentValidation;

namespace AFT.RegoV2.Core.Brand.Validators
{
    public class EditLicenseeValidator : AbstractValidator<EditLicenseeData>
    {
        public EditLicenseeValidator(IBrandRepository brandRepository)
        {
            CascadeMode = CascadeMode.StopOnFirstFailure;

            RuleFor(x => x.Name)
                .NotNull()
                .WithMessage("{\"text\": \"app:common.requiredField\"}")
                .Length(1, 50)
                .WithMessage("{{\"text\": \"app:common.exceedMaxLength\", \"variables\": {{\"length\": \"{0}\"}}}}", 50)
                .Must((data, x) => !brandRepository.Licensees.Any(y => y.Name == x && y.Id != data.Id))
                .WithMessage("{\"text\": \"app:common.nameUnique\"}");

            RuleFor(x => x.CompanyName)
                .NotNull()
                .WithMessage("{\"text\": \"app:common.requiredField\"}")
                .Length(1, 50)
                .WithMessage("{{\"text\": \"app:common.exceedMaxLength\", \"variables\": {{\"length\": \"{0}\"}}}}", 50);

            RuleFor(x => x.ContractStart)
                .Must((data, x) =>
                {
                    var licensee = brandRepository.Licensees.Include(y => y.Contracts).Single(y => y.Id == data.Id);

                    if (licensee.Status == LicenseeStatus.Inactive || licensee.ContractStart == x) 
                        return true;

                    return licensee.Contracts.Where(y => !y.IsCurrentContract).All(y => x > y.EndDate);
                })
                .WithMessage("{\"text\": \"app:licensee.contractStartNotAfterPreviousContracts\"}");

            RuleFor(x => x.ContractEnd)
                .Must((data, x) => x.HasValue || data.OpenEnded)
                .WithMessage("{\"text\": \"app:licensee.contractEndRequired\"}")
                .Must((data, x) => !x.HasValue || x.Value > data.ContractStart)
                .WithMessage("{\"text\": \"app:licensee.contractEndNotAftereStart\"}");

            RuleFor(x => x.Email)
                .NotNull()
                .WithMessage("{\"text\": \"app:common.requiredField\"}");

            RuleFor(x => x.BrandCount)
                .GreaterThanOrEqualTo(0)
                .LessThan(9999)
                .WithMessage(
                    "{{\"text\": \"app:common.numberOutOfRange\", \"variables\": {{\"minimum\": \"{1}\", \"maximum\": \"{2}\"}}}}",
                    0, 9999);

            RuleFor(x => x.WebsiteCount)
                .GreaterThanOrEqualTo(0)
                .LessThan(9999)
                .WithMessage(
                    "{{\"text\": \"app:common.numberOutOfRange\", \"variables\": {{\"minimum\": \"{1}\", \"maximum\": \"{2}\"}}}}",
                    0, 9999);

            RuleFor(x => x.Products)
                .NotNull()
                .NotEmpty()
                .WithMessage("{\"text\": \"app:common.requiredField\"}");

            RuleFor(x => x.Currencies)
                .NotNull()
                .NotEmpty()
                .WithMessage("{\"text\": \"app:common.requiredField\"}");

            RuleFor(x => x.Countries)
                .NotNull()
                .NotEmpty()
                .WithMessage("{\"text\": \"app:common.requiredField\"}");

            RuleFor(x => x.Languages)
                .NotNull()
                .NotEmpty()
                .WithMessage("{\"text\": \"app:common.requiredField\"}");

            RuleFor(x => x.Remarks)
                .NotNull()
                .WithMessage("{\"text\": \"app:common.requiredField\"}");
        }
    }
}