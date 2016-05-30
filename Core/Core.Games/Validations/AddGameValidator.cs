using System.Linq;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Core.Game.Interface.Data;
using FluentValidation;

namespace AFT.RegoV2.Core.Game.Validations
{
    public class AddGameValidator : AbstractValidator<GameDTO>
    {
        public AddGameValidator(
            IGameRepository gameRepository)
        {
            CascadeMode = CascadeMode.Continue;

            RuleFor(x => x.ProductId)
                .NotNull()
                .WithMessage("{\"text\": \"app:common.requiredField\"}");
            RuleFor(x => x.Name)
                .NotNull()
                .WithMessage("{\"text\": \"app:common.requiredField\"}");
            RuleFor(x => x.Code)
                .NotNull()
                .WithMessage("{\"text\": \"app:common.requiredField\"}");
            RuleFor(x => x.Url)
                .NotNull()
                .WithMessage("{\"text\": \"app:common.requiredField\"}");
            RuleFor(x => x)
                .Must(x => !gameRepository.Games.Any(y => y.Name == x.Name && x.ProductId == y.GameProviderId))
                .WithName("name")
                .WithMessage("{\"text\": \"app:gameIntegration.games.nameUnique\"}");
            RuleFor(x => x)
                .Must(x => !gameRepository.Games.Any(y => y.Code == x.Code && x.ProductId == y.GameProviderId))
                .WithName("code")
                .WithMessage("{\"text\": \"app:gameIntegration.games.codeUnique\"}");
        }
    }
}