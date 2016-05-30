using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.Core.Brand.Interface.Data;
using AFT.RegoV2.Core.Common.Data.Brand;
using AFT.RegoV2.Core.Common.Extensions;
using FluentValidation;

namespace AFT.RegoV2.Core.Brand.Validators
{
    public class AssignBrandCurrencyValidator : AbstractValidator<AssignBrandCurrencyRequest>
    {
        public AssignBrandCurrencyValidator(IBrandRepository repository)
        {
            CascadeMode = CascadeMode.StopOnFirstFailure;

            Interface.Data.Brand brand = null;

            RuleFor(x => x.Brand)
                .Must(x =>
                {
                    brand = repository.Brands
                        .Include(y => y.BrandCurrencies)
                        .SingleOrDefault(y => y.Id == x);

                    return brand != null;
                })
                .WithMessage(AssignBrandCurrencyResponseCodes.InvalidBrand)
                .DependentRules(x =>
                {
                    RuleFor(y => y.Currencies)
                        .NotNull()
                        .Must(y => y.Any())
                        .WithMessage(AssignBrandCurrencyResponseCodes.Required)
                        .Must(y =>
                        {
                            var licensee = repository.Licensees
                                .Include(z => z.Currencies)
                                .SingleOrDefault(z => z.Id == brand.LicenseeId);

                            var allowedCurrencies = licensee
                                .Currencies
                                .Select(z => z.Code)
                                .ToArray();

                            var allCurrenciesAllowed = y.All(allowedCurrencies.Contains);

                            return allCurrenciesAllowed;
                        })
                        .WithMessage(AssignBrandCurrencyResponseCodes.InvalidCurrency);

                    RuleFor(y => y.BaseCurrency)
                        .NotEmpty()
                        .WithMessage(AssignBrandCurrencyResponseCodes.Required)
                        .Must((data, y) => data.Currencies.Contains(y))
                        .WithMessage(AssignBrandCurrencyResponseCodes.InvalidBaseCurrency);

                    RuleFor(y => y.DefaultCurrency)
                        .NotEmpty()
                        .WithMessage(AssignBrandCurrencyResponseCodes.Required)
                        .Must((data, y) => data.Currencies.Contains(y))
                        .WithMessage(AssignBrandCurrencyResponseCodes.InvalidDefaultCurrency);
                });
        }
    }
}