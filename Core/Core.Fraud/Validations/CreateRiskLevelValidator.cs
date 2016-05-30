using System.Linq;
using FluentValidation;

namespace AFT.RegoV2.Core.Fraud.Validations
{
    public class CreateRiskLevelValidator : RiskLevelValidatorBase
    {
        public CreateRiskLevelValidator(IFraudRepository repository)
            : base(repository)
        {
            RuleFor(x => x)
                .Must(rl => !repository.RiskLevels.Any(y => rl.BrandId == y.BrandId && rl.Name == y.Name))
                .WithMessage("Fraud Risk Level name should be unique per brand.")
                .WithName("Name");

            RuleFor(x => x)
                .Must(rl => !repository.RiskLevels.Any(y => rl.BrandId == y.BrandId && rl.Level == y.Level))
                .WithMessage("Fraud Risk Level should be unique per brand.")
                .WithName("Level");
        }

    }
}
