using System.Linq;
using AFT.RegoV2.Core.Common.Extensions;
using AFT.RegoV2.Core.Payment.Interface.Data;
using AFT.RegoV2.Core.Payment.Interface.Data.Commands;
using FluentValidation;
using PaymentLevel = AFT.RegoV2.Core.Payment.Data.PaymentLevel;

namespace AFT.RegoV2.Core.Payment.Validators
{
    public class ActivatePaymentLevelValidator : AbstractValidator<ActivatePaymentLevelCommand>
    {
        public ActivatePaymentLevelValidator(IPaymentRepository paymentRepository)
        {
            PaymentLevel paymentLevel = null;

            CascadeMode = CascadeMode.StopOnFirstFailure;

            RuleFor(x => x.Id)
                .Must(x =>
                {
                    paymentLevel = paymentRepository.PaymentLevels.SingleOrDefault(y => y.Id == x);
                    return paymentLevel != null;
                })
                .WithMessage(ActivatePaymentLevelErrors.NotFound);

            When(x => paymentLevel != null, () => 
                RuleFor(x => x.Id)
                    .Must(x => paymentLevel.Status == PaymentLevelStatus.Inactive)
                    .WithMessage(ActivatePaymentLevelErrors.AlreadyActive));
        }
    }
}
