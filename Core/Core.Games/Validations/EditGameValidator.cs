using AFT.RegoV2.Core.Game.Interface.Data;
using FluentValidation;

namespace AFT.RegoV2.Core.Game.Validations
{
    public class EditGameValidator : AbstractValidator<GameDTO>
    {
        public EditGameValidator()
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
        }
    }
}