using System;
using System.Linq;
using AFT.RegoV2.Bonus.Core.ApplicationServices;
using AFT.RegoV2.Bonus.Core.Data;
using AFT.RegoV2.Bonus.Core.Models.Enums;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Payment;
using AFT.RegoV2.Core.Payment.Interface.Events;
using AFT.RegoV2.RegoBus.Interfaces;
using AFT.RegoV2.Shared.Utils;
using Microsoft.Practices.Unity;
using BonusRedemption = AFT.RegoV2.Bonus.Core.Entities.BonusRedemption;

namespace AFT.RegoV2.Bonus.Core.EventHandlers
{
    public class PaymentSubscriber
    {
        private readonly IUnityContainer _container;

        public PaymentSubscriber(IUnityContainer container)
        {
            _container = container;
        }

        public void Handle(TransferFundCreated @event)
        {
            if (@event.Type == TransferFundType.FundOut || @event.Status == TransferFundStatus.Rejected)
                return;

            var bonusCommands = _container.Resolve<BonusCommands>();
            var bonusQueries = _container.Resolve<BonusQueries>();
            var redemptionParams = new RedemptionParams { TransferAmount = @event.Amount, TransferWalletTemplateId = @event.DestinationWalletStructureId };

            var repository = _container.Resolve<IBonusRepository>();
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                bonusQueries
                    .GetQualifiedAutomaticBonusIds(@event.PlayerId, BonusType.FundIn, redemptionParams)
                    .ForEach(bonusId => bonusCommands.ActivateFundInBonus(@event.PlayerId, bonusId, redemptionParams));

                var wallet = repository.GetLockedWallet(@event.PlayerId);

                if (@event.Amount > 0)
                {
                    wallet.TransferFundCredit(@event.Amount);
                }
                else
                {
                    wallet.TransferFundDebit(@event.Amount);
                }

                repository.SaveChanges();
                scope.Complete();
            }
        }

        public void Handle(DepositSubmitted @event)
        {
            var bonusCommands = _container.Resolve<BonusCommands>();
            var bonusQueries = _container.Resolve<BonusQueries>();
            var bus = _container.Resolve<IEventBus>();
            var bonusRepository = _container.Resolve<IBonusRepository>();

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var player = bonusRepository.GetLockedPlayer(@event.PlayerId);
                var redemptionParams = new RedemptionParams
                {
                    TransferAmount = @event.Amount,
                    TransferExternalId = @event.DepositId,
                    TransferWalletTemplateId = player.Data.Brand.WalletTemplates.Single(wt => wt.IsMain).Id
                };

                bonusQueries
                    .GetQualifiedAutomaticBonusIds(@event.PlayerId, player.DepositQuailifiedBonusType, redemptionParams)
                    .ForEach(bonusId =>
                    {
                        var redemption = bonusCommands.RedeemBonus(@event.PlayerId, bonusId, redemptionParams);
                        redemption.Events.ForEach(bus.Publish);
                    });

                scope.Complete();
            }
        }

        public void Handle(DepositUnverified @event)
        {
            NegateDepositBonusRedemptions(@event.PlayerId, @event.DepositId, "Deposit unverified");
        }

        public void Handle(DepositRejected @event)
        {
            NegateDepositBonusRedemptions(@event.PlayerId, @event.DepositId, "Deposit rejected");
        }

        public void Handle(DepositApproved @event)
        {
            var bonusCommands = _container.Resolve<BonusCommands>();
            var repository = _container.Resolve<IBonusRepository>();
            var bus = _container.Resolve<IEventBus>();
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var wallet = repository.GetLockedWallet(@event.PlayerId);
                wallet.Deposit(@event.ActualAmount, @event.ReferenceCode);
                wallet.Events.ForEach(bus.Publish);
                var player = repository.GetLockedPlayer(@event.PlayerId);
                var depositBonusRedemptions = player.BonusesRedeemed
                    .Where(r => r.Parameters.TransferExternalId == @event.DepositId)
                    .Select(br => new BonusRedemption(br));
                foreach (var depositBonusRedemption in depositBonusRedemptions)
                {
                    depositBonusRedemption.Data.Parameters.TransferAmount = @event.ActualAmount;
                    bonusCommands.ProcessBonusRedemptionLifecycle(depositBonusRedemption);
                    depositBonusRedemption.Events.ForEach(bus.Publish);
                }

                bonusCommands.ProcessHighDepositBonus(player);

                repository.SaveChanges();
                scope.Complete();
            }
        }

        public void Handle(WithdrawalApproved @event)
        {
            var repository = _container.Resolve<IBonusRepository>();
            var bus = _container.Resolve<IEventBus>();
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var wallet = repository.GetLockedWallet(@event.WithdrawalMadeBy);
                wallet.Withdraw(@event.Amount, @event.TransactionNumber);
                wallet.Events.ForEach(bus.Publish);

                repository.SaveChanges();
                scope.Complete();
            }
        }

        private void NegateDepositBonusRedemptions(Guid playerId, Guid depositId, string reason)
        {
            var repository = _container.Resolve<IBonusRepository>();
            var bus = _container.Resolve<IEventBus>();

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var player = repository.GetLockedPlayer(playerId);
                var bonusRedemptions = player.BonusesRedeemed
                    .Where(r => r.Parameters.TransferExternalId == depositId)
                    .Select(brd => new BonusRedemption(brd));

                foreach (var redemption in bonusRedemptions)
                {
                    redemption.Negate(new[] { reason });
                    redemption.Events.ForEach(bus.Publish);
                }

                repository.SaveChanges();
                scope.Complete();
            }
        }
    }
}