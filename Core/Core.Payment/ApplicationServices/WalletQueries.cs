using System;
using System.Threading;
using System.Threading.Tasks;
using AFT.RegoV2.Bonus.Api.Interface;
using AFT.RegoV2.Core.Game.Interface.Interfaces;
using AFT.RegoV2.Core.Payment.Interface.ApplicationServices;
using AFT.RegoV2.Core.Payment.Interface.Data;
using IWalletQueries = AFT.RegoV2.Core.Payment.Interface.ApplicationServices.IWalletQueries;

namespace AFT.RegoV2.Core.Payment.ApplicationServices
{
    public class WalletQueries : IWalletQueries
    {
        private const int RetryCount = 10;
        private readonly IPaymentQueries _paymentQueries;
        private readonly IBrandOperations _brandOperations;
        private readonly IBonusApiProxy _bonusApiProxy;

        public WalletQueries(IPaymentQueries paymentQueries,
            IBrandOperations brandOperations,
            IBonusApiProxy bonusApiProxy
            )
        {
            _paymentQueries = paymentQueries;
            _brandOperations = brandOperations;
            _bonusApiProxy = bonusApiProxy;
        }

        public async Task<PlayerBalance> GetPlayerBalance(Guid playerId, Guid? walletTemplateId = null)
        {          
            var withdrawalLockBalance = _paymentQueries.GetWithdrawalLockBalance(playerId);

            var bonusBalanceData = await _bonusApiProxy.GetPlayerBalanceAsync(playerId, walletTemplateId);

            //the Main in bonus domain include WithdrawalLock,so minus it
            var mainBalance = bonusBalanceData.Main - withdrawalLockBalance;
            var playableBalance = mainBalance + bonusBalanceData.Bonus;

            var player = _paymentQueries.GetPlayer(playerId);
            

            bool isBalanceConsistent = false;
            var gameBalance = 0m;
            for (int i = 0; i < RetryCount; i++)
            {
                gameBalance = await _brandOperations.GetPlayerBalanceAsync(playerId, player.CurrencyCode);
                isBalanceConsistent = gameBalance == playableBalance;

                if(isBalanceConsistent)
                    break;

                Thread.Sleep(500);
            }
            if (false == isBalanceConsistent)
                throw new ApplicationException(
                    "Playable balance is not consistent across the application. \n" +
                    $"gameBalance:{gameBalance} is not equal to mainBalance:{mainBalance} + bonusBalance:{bonusBalanceData.Bonus}");

            return new PlayerBalance
            {
                Playable = playableBalance,
                Total = playableBalance + withdrawalLockBalance,
                Main = mainBalance,
                Bonus = bonusBalanceData.Bonus,
                WithdrawalLock = withdrawalLockBalance,
                BonusLock = bonusBalanceData.BonusLock,
                CurrencyCode = player.CurrencyCode
            };
        }
    }
}
