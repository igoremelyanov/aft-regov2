using System.Linq;
using AFT.RegoV2.Core.Game.Interfaces;
using FluentValidation;
using AFT.RegoV2.Core.Game.Interface.Data;

namespace AFT.RegoV2.Core.Game.Validations
{
    public class AddGameProviderCurrencyValidator : AbstractValidator<GameProviderCurrency>
    {
        public AddGameProviderCurrencyValidator(IGameRepository gameRepository)
        {
            CascadeMode = CascadeMode.StopOnFirstFailure;

            RuleFor(x => x.GameProviderId)
                .Must(x => gameRepository.GameProviders.Any(y => y.Id == x))
                .WithMessage("{\"text\": \"app:gameIntegration.gameProviderNotFound\"}");

            RuleFor(x => x.CurrencyCode)
                .NotNull()
                .WithMessage("{\"text\": \"app:common.requiredField\"}")
                .Must(x => gameRepository.Currencies.Any(y => y.Code == x))
                .WithMessage("{\"text\": \"app:gameIntegration.currencyNotFound\"}");

            RuleFor(x => x.GameProviderCurrencyCode)
                .NotNull()
                .WithMessage("{\"text\": \"app:common.requiredField\"}");
        }
    }
}