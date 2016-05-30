using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.Core.Common.Extensions;
using AFT.RegoV2.Core.Messaging.Interface.Data;
using AFT.RegoV2.Core.Messaging.Interface.Data.MassMessageCommands;
using FluentValidation;
using MassMessage = AFT.RegoV2.Core.Messaging.Data.MassMessage;

namespace AFT.RegoV2.Core.Messaging.Validators
{
    public class SendMassMessageValidator : AbstractValidator<SendMassMessageRequest>
    {
        public SendMassMessageValidator(
            IMessagingRepository repository)
        {
            CascadeMode = CascadeMode.StopOnFirstFailure;

            MassMessage massMessage;

            RuleFor(x => x.Id)
                .Must(x =>
                {
                    massMessage = repository.MassMessages
                        .Include(y => y.Recipients)
                        .Single(y => y.Id == x);
                    return massMessage != null;
                })
                .WithMessage(MessagingValidationError.InvalidId);
        }
    }
}