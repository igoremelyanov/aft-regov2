using System.Linq;
using FluentValidation;

namespace AFT.RegoV2.Core.Fraud.Validations
{
    public class UpdateRiskLevelValidator : RiskLevelValidatorBase
    {
        public UpdateRiskLevelValidator(IFraudRepository repository)
            : base(repository)
        {
            RuleFor(x => x)
                .Must(rl => !repository.RiskLevels.Any(y => rl.BrandId == y.BrandId && rl.Name == y.Name && y.Id != rl.Id))
                .WithMessage("Fraud Risk Level name should be unique per brand.")
                .WithName("Name");

            RuleFor(x => x)
                .Must(rl => !repository.RiskLevels.Any(y => rl.BrandId == y.BrandId && rl.Level == y.Level && y.Id != rl.Id))
                .WithMessage("Fraud Risk Level should be unique per brand.")
                .WithName("Level");

            RuleFor(x => x.Id)
                .Must(id => repository.RiskLevels.Any(y => id == y.Id)).WithMessage("{\"text\": \"app:common.idDoesNotExist\"}");
        }
    }
}
