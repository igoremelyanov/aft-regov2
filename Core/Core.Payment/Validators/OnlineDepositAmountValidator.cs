using System.Linq;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Payment.Interface.ApplicationServices;
using AFT.RegoV2.Core.Payment.Interface.Data;
using AFT.RegoV2.Core.Player.Interface.ApplicationServices;
using FluentValidation;

namespace AFT.RegoV2.Core.Payment.Validators
{
    public class OnlineDepositAmountValidator : AbstractValidator<ValidateOnlineDepositAmountRequest>
    {
        private readonly IPlayerQueries _playerQueries;
        private readonly IPaymentQueries _paymentQueries;

        public OnlineDepositAmountValidator(
            IPlayerQueries playerQueries, 
            IPaymentQueries paymentQueries)
        {
            _playerQueries = playerQueries;
            _paymentQueries = paymentQueries;
            RuleFor(x => x.Amount)
                .Must(amount => amount > 0)
                .WithMessage("Amount has to be greater than zero.");
            RuleFor(request => request).Must(request =>
            {
                var paymentSetting = GetPaymentSetting(request);
                if (paymentSetting == null)
                    return true;

                return request.Amount >= paymentSetting.MinAmountPerTransaction &&
                       request.Amount <= paymentSetting.MaxAmountPerTransaction;
            })
            .WithMessage("Amount must be between a minimum deposit of {0} and a maximum deposit of {1}", 
                request =>
                {
                    var paymentSetting = GetPaymentSetting(request);
                    if (paymentSetting == null)
                        return "";
                    return paymentSetting.MinAmountPerTransaction;
                },
                request =>
                {
                    var paymentSetting = GetPaymentSetting(request);

                    if (paymentSetting == null)
                        return "";
                    return paymentSetting.MaxAmountPerTransaction;
                }
                )
            .WithName("Amount");
        }

        private PaymentSettings GetPaymentSetting(ValidateOnlineDepositAmountRequest request)
        {
            var defaultVipLevel = _playerQueries.GetDefaultVipLevel(request.BrandId);
            //TODO: we should take the currency into account
            var paymentSettings = _paymentQueries.GetPaymentSettings().ToList();
            var paymentSetting = paymentSettings
                .FirstOrDefault(x => x.VipLevel == defaultVipLevel.Id.ToString() && x.Enabled == Status.Active);
            return paymentSetting;
        }
    }
}
