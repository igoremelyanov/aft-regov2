using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Fraud.Exceptions;
using AFT.RegoV2.Core.Payment;
using IWalletQueries = AFT.RegoV2.Core.Payment.Interface.ApplicationServices.IWalletQueries;

namespace AFT.RegoV2.Core.Fraud
{
    public class FundsValidationService : IFundsValidationService
    {
        #region Fields

        private readonly IPaymentRepository _paymentRepository;
        private readonly IWalletQueries _walletQueries;

        #endregion

        #region Constructors

        public FundsValidationService(
            IPaymentRepository paymentRepository,
            IWalletQueries walletQueries)
        {
            _paymentRepository = paymentRepository;
            _walletQueries = walletQueries;
        }

        #endregion

        #region IFundsValidationService Members

        public async Task Validate(OfflineWithdrawRequest request)
        {
            var bankAccount =
                _paymentRepository
                    .PlayerBankAccounts
                    .Include(x => x.Player)
                    .FirstOrDefault(x => x.Id == request.PlayerBankAccountId);

            var wallet = await _walletQueries.GetPlayerBalance(bankAccount.Player.Id);

            if (wallet.Free < request.Amount)
            {
                throw new NotEnoughFundsException();
            }
        }

        #endregion
    }
}