using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.Bonus.Api.Interface;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Fraud.Exceptions;
using AFT.RegoV2.Core.Payment;

namespace AFT.RegoV2.Core.Fraud.Services
{
    public class BonusWageringWithdrawalValidationService : IBonusWageringWithdrawalValidationService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IBonusApiProxy _bonusApiProxy;

        public BonusWageringWithdrawalValidationService(
            IPaymentRepository paymentRepository,
            IBonusApiProxy bonusApiProxy)
        {
            _paymentRepository = paymentRepository;
            _bonusApiProxy = bonusApiProxy;
        }

        public async void Validate(OfflineWithdrawRequest request)
        {
            var bankAccount = _paymentRepository.PlayerBankAccounts.Include(x => x.Player).First(x => x.Id == request.PlayerBankAccountId);

            var wageringBalances = await _bonusApiProxy.GetWageringBalancesAsync(bankAccount.Player.Id);
            if (wageringBalances.Remaining > 0)
                throw new BonusWageringValidationException();
        }
    }
}