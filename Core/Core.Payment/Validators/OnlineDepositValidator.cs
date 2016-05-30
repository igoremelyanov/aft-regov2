using System;
using System.Linq;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Payment.Interface.ApplicationServices;
using AFT.RegoV2.Core.Payment.Interface.Data;
using AFT.RegoV2.Core.Payment.Interface.Exceptions;
using FluentValidation;
using FluentValidation.Results;

namespace AFT.RegoV2.Core.Payment.Validators
{
    public class OnlineDepositValidator : IOnlineDepositValidator
    {
        private readonly IPaymentRepository _repository;
        private readonly IPaymentQueries _paymentQueries;

        public OnlineDepositValidator()
        {
        }

        public OnlineDepositValidator(IPaymentQueries paymentQueries, IPaymentRepository repository)
        {
            _paymentQueries = paymentQueries;
            _repository = repository;
        }

        public void ValidatePaymentSetting(Guid playerId, string payMethod, decimal amount, string currencyCode)
        {
	        if (amount <= 0)
				throw new ValidationException(new[]
				{
					new ValidationFailure("amount", String.Format("Amount must be greater than zero."))
				});

			var player = _paymentQueries.GetPlayer(playerId);

            var vipLevelId = player.VipLevelId == Guid.Empty
                ? "10000000-0000-0000-0000-000000000000"
                : player.VipLevelId.ToString();

            var gatewaySettings = _paymentQueries.GetOnlinePaymentSettings(player.BrandId, PaymentType.Deposit, vipLevelId,
              payMethod, currencyCode);
           
            if (gatewaySettings == null)
                return;

	        if (amount > gatewaySettings.MaxAmountPerTransaction && gatewaySettings.MaxAmountPerTransaction > 0)
		        throw new ValidationException(new ValidationFailure[]
		        {
			        new ValidationFailure("amount", String.Format("Amount must be between a minimum deposit of {0} and a maximum deposit of {1}", gatewaySettings.MinAmountPerTransaction, gatewaySettings.MaxAmountPerTransaction))
		        });

            if (amount < gatewaySettings.MinAmountPerTransaction && gatewaySettings.MinAmountPerTransaction > 0)
				throw new ValidationException(new[]
				{
					new ValidationFailure("amount", String.Format("Amount must be between a minimum deposit of {0} and a maximum deposit of {1}", gatewaySettings.MinAmountPerTransaction, gatewaySettings.MaxAmountPerTransaction))
				});

			var now = DateTime.Now;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);
            var startOfWeek = now.StartOfWeek();
            var startOfDay = now.Date;
            var earilestDay = startOfMonth > startOfWeek ? startOfWeek : startOfMonth;//startOfWeek might be not in the same week
            //check by Pay Method
            var onlineDeposits = _repository.OnlineDeposits
                .Where(x => x.Status == OnlineDepositStatus.Approved
//                            && x.Method == payMethod
                            && x.Approved >= earilestDay
                            && x.PlayerId == playerId)
                .ToList();

            var amountPerDay = onlineDeposits
                .Where(x => x.Approved >= startOfDay && x.PlayerId == playerId)
                .Sum(x => x.Amount) + amount;

            if (amountPerDay > gatewaySettings.MaxAmountPerDay && gatewaySettings.MaxAmountPerDay > 0)
				throw new ValidationException(new[]
				{
					new ValidationFailure("amount", String.Format("Total entered amounts exceed the daily limit. Maximum value is {0}.", gatewaySettings.MaxAmountPerDay))
				});

            var countMonth = onlineDeposits.Count(x => x.Approved >= startOfMonth);

            if (gatewaySettings.MaxTransactionPerMonth > 0 && countMonth >= gatewaySettings.MaxTransactionPerMonth)
				throw new ValidationException(new[]
				{
					new ValidationFailure("amount", String.Format("Total number of transactions per day exceed monthly limit. Maximum value is {0}.", gatewaySettings.MaxTransactionPerMonth))
				});

			var countWeek = onlineDeposits.Count(x => x.Approved >= startOfWeek);

            if (gatewaySettings.MaxTransactionPerWeek > 0 && countWeek >= gatewaySettings.MaxTransactionPerWeek)
				throw new ValidationException(new[]
				{
					new ValidationFailure("amount", String.Format("Total number of transactions per day exceed weekly limit. Maximum value is {0}.", gatewaySettings.MaxTransactionPerWeek))
				});

			var countDay = onlineDeposits.Count(x => x.Approved >= startOfDay);

            if (gatewaySettings.MaxTransactionPerDay > 0 && countDay >= gatewaySettings.MaxTransactionPerDay)
				throw new ValidationException(new[]
				{
					new ValidationFailure("amount", String.Format("Total number of transactions per day exceed daily limit. Maximum value is {0}.", gatewaySettings.MaxTransactionPerDay))
				});
		}
    }

    public interface IOnlineDepositValidator
    {
        void ValidatePaymentSetting(Guid playerId, string payMethod, decimal amount, string currencyCode);
    }
}
