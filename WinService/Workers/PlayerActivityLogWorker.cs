using AFT.RegoV2.Bonus.Core.Models.Events.Redemption;
using AFT.RegoV2.Core.Common.Events;
using AFT.RegoV2.Core.Common.Events.Player;
using AFT.RegoV2.Core.Domain.Player.Events;
using AFT.RegoV2.Core.Messaging.Interface.Events;
using AFT.RegoV2.Core.Payment.Interface.Events;
using AFT.RegoV2.Core.Player.ApplicationServices.EventHandlers.ActivityLog;
using AFT.RegoV2.Core.Report.ApplicationServices.EventHandlers.PlayerActivityLog;
using AFT.RegoV2.RegoBus.Interfaces;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.WinService.Workers
{
    public class PlayerActivityLogWorker : WorkerBase<PlayerActivityLogSubscriber>
    {
        public PlayerActivityLogWorker(IUnityContainer container, IServiceBus serviceBus) : base(container, serviceBus) { }
    }

    public class PlayerActivityLogSubscriber : IBusSubscriber,
        // Player category
        IConsumes<PlayerActivated>,
        IConsumes<PlayerDeactivated>,
        IConsumes<PlayerRegistered>,
        IConsumes<PlayerContactVerified>,
        IConsumes<PlayerPaymentLevelChanged>,

        // Deposit category
        IConsumes<DepositSubmitted>,
        IConsumes<DepositConfirmed>,
        IConsumes<DepositVerified>,
        IConsumes<DepositApproved>,
        IConsumes<DepositUnverified>,
        IConsumes<DepositRejected>,

        // Withdraw category
        IConsumes<WithdrawalCreated>,
        IConsumes<WithdrawalVerified>,
        IConsumes<WithdrawalUnverified>,
        IConsumes<WithdrawalWagerChecked>,
        IConsumes<WithdrawalInvestigated>,
        IConsumes<WithdrawalDocumentsChecked>,
        IConsumes<WithdrawalAccepted>,
        IConsumes<WithdrawalApproved>,
        IConsumes<WithdrawalCancelled>,
        IConsumes<WithdrawalReverted>,

        // Bonus category
        IConsumes<RedemptionClaimed>,

        // Player Record Edit category
        IConsumes<PlayerUpdated>,

        // Player Bank Account categry
        IConsumes<PlayerBankAccountVerified>,
        IConsumes<PlayerBankAccountRejected>,
        IConsumes<PlayerBankAccountCurrentSet>,

        // Player Transfer Fund categry
        IConsumes<TransferFundCreated>,

        // Messaging
        IConsumes<OnSiteMessageSentEvent>
    {
        private readonly PlayerActivityLogEventHandlers _playerHandlers;
        private readonly DepositActivityLogEventHandlers _depositHandlers;
        private readonly WithdrawActivityLogEventHandlers _withdrawHandlers;
        private readonly BonusActivityLogEventHandlers _bonusHandlers;
        private readonly PlayerRecordEditActivityLogEventHandlers _playerRecordEditHandlers;
        private readonly PlayerBankAccountLogEventHandlers _playerBankAccountHandlers;
        private readonly MessagingActivityLogEventHandlers _messagingHandlers;

        public PlayerActivityLogSubscriber(
            PlayerActivityLogEventHandlers playerHandlers,
            DepositActivityLogEventHandlers depositHandlers,
            WithdrawActivityLogEventHandlers withdrawHandlers,
            BonusActivityLogEventHandlers bonusHandlers,
            PlayerRecordEditActivityLogEventHandlers playerRecordEditHandlers,
            PlayerBankAccountLogEventHandlers playerBankAccountHandlers, 
            MessagingActivityLogEventHandlers messagingHandlers)
        {
            _playerHandlers = playerHandlers;
            _depositHandlers = depositHandlers;
            _withdrawHandlers = withdrawHandlers;
            _bonusHandlers = bonusHandlers;
            _playerRecordEditHandlers = playerRecordEditHandlers;
            _playerBankAccountHandlers = playerBankAccountHandlers;
            _messagingHandlers = messagingHandlers;
        }
        
        // Player category
        public void Consume(PlayerActivated message)
        {
            _playerHandlers.Handle(message); 
        }
        public void Consume(PlayerDeactivated message)
        {
            _playerHandlers.Handle(message);
        }
        public void Consume(PlayerRegistered message)
        {
            _playerHandlers.Handle(message);
            _playerRecordEditHandlers.Handle(message);
        }
        public void Consume(PlayerContactVerified message)
        {
            _playerHandlers.Handle(message);
        }
        public void Consume(PlayerPaymentLevelChanged message)
        {
            _playerHandlers.Handle(message);
        }

        // Deposit category
        public void Consume(DepositSubmitted message)
        {
            _depositHandlers.Handle(message);
        }
        public void Consume(DepositConfirmed message)
        {
            _depositHandlers.Handle(message);
        }
        public void Consume(DepositVerified message)
        {
            _depositHandlers.Handle(message);
        }
        public void Consume(DepositApproved message)
        {
            _depositHandlers.Handle(message);
        }
        public void Consume(DepositUnverified message)
        {
            _depositHandlers.Handle(message);
        }
        public void Consume(DepositRejected message)
        {
            _depositHandlers.Handle(message);
        }

        // Withdraw category
        public void Consume(WithdrawalCreated message)
        {
            _withdrawHandlers.Handle(message); 
        }
        public void Consume(WithdrawalVerified message)
        {
            _withdrawHandlers.Handle(message);
        }
        public void Consume(WithdrawalUnverified message)
        {
            _withdrawHandlers.Handle(message);
        }
        public void Consume(WithdrawalWagerChecked message)
        {
            _withdrawHandlers.Handle(message);
        }
        public void Consume(WithdrawalInvestigated message)
        {
            _withdrawHandlers.Handle(message);
        }
        public void Consume(WithdrawalDocumentsChecked message)
        {
            _withdrawHandlers.Handle(message);
        }
        public void Consume(WithdrawalAccepted message)
        {
            _withdrawHandlers.Handle(message);
        }
        public void Consume(WithdrawalApproved message)
        {
            _withdrawHandlers.Handle(message);
        }
        public void Consume(WithdrawalCancelled message)
        {
            _withdrawHandlers.Handle(message);
        }
        public void Consume(WithdrawalReverted message)
        {
            _withdrawHandlers.Handle(message);
        }

        // Bonus category
        public void Consume(RedemptionClaimed message)
        {
            _bonusHandlers.Handle(message);
        }

        // Player Record Edit category
        public void Consume(PlayerUpdated message)
        {
            _playerRecordEditHandlers.Handle(message);
        }

        // Player Bank Account categry
        public void Consume(PlayerBankAccountVerified message)
        {
            _playerBankAccountHandlers.Handle(message);
        }
        public void Consume(PlayerBankAccountRejected message)
        {
            _playerBankAccountHandlers.Handle(message);
        }
        public void Consume(PlayerBankAccountCurrentSet message)
        {
            _playerBankAccountHandlers.Handle(message);
        }

        // Player Transfer Fund categry
        public void Consume(TransferFundCreated message)
        {
            _playerHandlers.Handle(message);
        }

        // Messaging
        public void Consume(OnSiteMessageSentEvent message)
        {
            _messagingHandlers.Handle(message);
        }
    }
}
