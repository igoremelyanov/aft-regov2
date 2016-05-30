using System.Linq;
using AFT.RegoV2.Core.Payment.Interface.Data;
using FluentValidation;

namespace AFT.RegoV2.Core.Payment.Validators
{
    public class OnlineDepositRequestValidator : AbstractValidator<OnlineDepositRequest>
    {
        public OnlineDepositRequestValidator(IPaymentRepository repository)
        {            
            RuleFor(x => x.PlayerId)
                .Must(x => repository.Players.FirstOrDefault(y=>x == y.Id) != null)
                .WithMessage("{\"text\": \"app:apiResponseCodes.PlayerDoesNotExist\"}");           
        }    
    }
}
