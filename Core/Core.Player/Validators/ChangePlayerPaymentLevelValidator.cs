using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.Core.Common.Data.Player;
using AFT.RegoV2.Core.Payment.Interface.ApplicationServices;
using AFT.RegoV2.Core.Payment.Interface.Data;
using FluentValidation;
using FluentValidation.Results;

namespace AFT.RegoV2.Core.Player.Validators
{
    public class ChangePlayerPaymentLevelValidator : AbstractValidator<ChangePaymentLevelData>
    {
        public ChangePlayerPaymentLevelValidator(IPlayerRepository repository,IPaymentQueries paymentQueries)
        {
            CascadeMode = CascadeMode.StopOnFirstFailure;

            Custom(data =>
            {
                var player =
                    repository.Players.Include(x => x.Brand).FirstOrDefault(x => x.Id == data.PlayerId);

                if (player == null)
                {
                    return new ValidationFailure("PlayerId", PaymentLevelErrors.PlayerNotFound.ToString());
                }

                var newPaymentLevel = paymentQueries.GetPaymentLevel(data.PaymentLevelId);
                if (newPaymentLevel == null)
                {
                    return new ValidationFailure("PaymentLevelId", PaymentLevelErrors.PaymentLevelNotFound.ToString());
                }
                if (newPaymentLevel.Status == PaymentLevelStatus.Inactive)
                {
                    return new ValidationFailure("PaymentLevelId", PaymentLevelErrors.PaymentLevelInactivate.ToString());
                }
                if(newPaymentLevel.CurrencyCode!=player.CurrencyCode || newPaymentLevel.BrandId!=player.BrandId )
                {
                    return new ValidationFailure("PaymentLevelId", PaymentLevelErrors.PaymentLevelAndPlayerNotMatch.ToString());
                }
                
                return null;
            });
        }
    }

    public enum PaymentLevelErrors
    {
        PlayerNotFound,
        PaymentLevelNotFound,
        PaymentLevelInactivate,
        PaymentLevelAndPlayerNotMatch
    }
}
