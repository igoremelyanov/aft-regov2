using System;
using System.Linq;
using AFT.RegoV2.Core.Common.Extensions;
using AFT.RegoV2.Core.Messaging.Interface.Data;
using AFT.RegoV2.Core.Messaging.Interface.Data.MassMessageCommands;
using FluentValidation;

namespace AFT.RegoV2.Core.Messaging.Validators
{
    public class UpdateMassMessageRecipientsValidator : AbstractValidator<UpdateRecipientsRequest>
    {
        public UpdateMassMessageRecipientsValidator(
            IMessagingRepository repository)
        {
            CascadeMode = CascadeMode.StopOnFirstFailure;

            When(x => x.Id.HasValue, () => 
                RuleFor(x => x.Id)
                .Must(x =>
                {
                    var massMessage = repository.MassMessages.SingleOrDefault(y => y.Id == x);
                    return massMessage != null;
                })
                .WithMessage(MessagingValidationError.InvalidId));

            RuleFor(x => x.UpdateRecipientsType)
                .Must(x => Enum.IsDefined(typeof (UpdateRecipientsType), x))
                .WithMessage(MessagingValidationError.InvalidUpdateRecipientsType);

            When(x => 
                x.UpdateRecipientsType == UpdateRecipientsType.SelectSingle || 
                x.UpdateRecipientsType == UpdateRecipientsType.UnselectSingle, () => 
                RuleFor(x => x.PlayerId)
                .Must(x => x.HasValue)
                .WithMessage(MessagingValidationError.Required)
                .Must(x =>
                {
                    var player = repository.Players.SingleOrDefault(y => y.Id == x.Value);
                    return player != null;
                })
                .WithMessage(MessagingValidationError.InvalidPlayerId));

        }
    }
}