using AFT.RegoV2.Bonus.Core.EventHandlers;
using AFT.RegoV2.Bonus.Infrastructure;
using AFT.RegoV2.Core.Common.Events;
using AFT.RegoV2.Core.Common.Events.Brand;
using AFT.RegoV2.Core.Common.Events.Fraud;
using AFT.RegoV2.Core.Payment.Interface.Events;
using AFT.RegoV2.Core.Common.Events.Player;
using AFT.RegoV2.Core.Game.Interface.Events;
using AFT.RegoV2.Domain.Brand.Events;
using AFT.RegoV2.RegoBus.Interfaces;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.WinService.Workers
{
    public class BonusWorker : WorkerBase<BonusSubscriber>
    {
        public BonusWorker(IUnityContainer container, IServiceBus serviceBus) : base(container, serviceBus) { }
    }

    public class BonusSubscriber : IBusSubscriber,
        IConsumes<DepositUnverified>,
        IConsumes<DepositSubmitted>,
        IConsumes<DepositApproved>,
        IConsumes<DepositRejected>,
        IConsumes<WithdrawalApproved>,
        IConsumes<TransferFundCreated>,

        IConsumes<PlayerRegistered>,
        IConsumes<PlayerContactVerified>,

        IConsumes<BrandRegistered>,
        IConsumes<BrandUpdated>,
        IConsumes<BrandCurrenciesAssigned>,
        IConsumes<VipLevelRegistered>,
        IConsumes<WalletTemplateCreated>,
        IConsumes<WalletTemplateUpdated>,

        IConsumes<RiskLevelStatusUpdated>,
        IConsumes<RiskLevelCreated>,
        IConsumes<RiskLevelTagPlayer>,
        IConsumes<RiskLevelUntagPlayer>,
        IConsumes<PlayerRegistrationChecked>,

        IConsumes<GameCreated>,
        IConsumes<GameUpdated>,
        IConsumes<GameDeleted>,
        IConsumes<BetPlaced>,
        IConsumes<BetPlacedFree>,
        IConsumes<BetTied>,
        IConsumes<BetWon>,
        IConsumes<BetLost>,
        IConsumes<BetCancelled>,
        IConsumes<BetAdjusted>,

        IConsumes<RegoHeadSeeded>
    {
        private readonly PaymentSubscriber _paymentSubscriber;
        private readonly GameSubscriber _gameSubscriber;
        private readonly BrandSubscriber _brandSubscriber;
        private readonly PlayerSubscriber _playerSubscriber;
        private readonly FraudSubscriber _fraudSubscriber;
        private readonly ApplicationSeeder _applicationSeeder;

        public BonusSubscriber(
            PaymentSubscriber paymentSubscriber,
            GameSubscriber gameSubscriber,
            BrandSubscriber brandSubscriber,
            PlayerSubscriber playerSubscriber,
            FraudSubscriber fraudSubscriber,
            ApplicationSeeder applicationSeeder)
        {
            _paymentSubscriber = paymentSubscriber;
            _gameSubscriber = gameSubscriber;
            _brandSubscriber = brandSubscriber;
            _playerSubscriber = playerSubscriber;
            _fraudSubscriber = fraudSubscriber;
            _applicationSeeder = applicationSeeder;
        }

        public void Consume(DepositUnverified message)
        {
            _paymentSubscriber.Handle(message);
        }

        public void Consume(DepositSubmitted message)
        {
            _paymentSubscriber.Handle(message);
        }

        public void Consume(DepositApproved message)
        {
            _paymentSubscriber.Handle(message);
        }

        public void Consume(DepositRejected message)
        {
            _paymentSubscriber.Handle(message);
        }
        
        public void Consume(WithdrawalApproved message)
        {
            _paymentSubscriber.Handle(message);
        }

        public void Consume(TransferFundCreated message)
        {
            _paymentSubscriber.Handle(message);
        }

        public void Consume(PlayerRegistered message)
        {
            _playerSubscriber.Handle(message);
        }

        public void Consume(PlayerContactVerified message)
        {
            _playerSubscriber.Handle(message);
        }

        public void Consume(BrandRegistered message)
        {
            _brandSubscriber.Handle(message);
        }

        public void Consume(BrandUpdated message)
        {
            _brandSubscriber.Handle(message);
        }

        public void Consume(BrandCurrenciesAssigned message)
        {
            _brandSubscriber.Handle(message);
        }

        public void Consume(VipLevelRegistered message)
        {
            _brandSubscriber.Handle(message);
        }

        public void Consume(WalletTemplateCreated message)
        {
            _brandSubscriber.Handle(message);
        }

        public void Consume(WalletTemplateUpdated message)
        {
            _brandSubscriber.Handle(message);
        }

        public void Consume(RiskLevelStatusUpdated message)
        {
            _fraudSubscriber.Handle(message);
        }

        public void Consume(RiskLevelCreated message)
        {
            _fraudSubscriber.Handle(message);
        }

        public void Consume(RiskLevelTagPlayer message)
        {
            _fraudSubscriber.Handle(message);
        }

        public void Consume(RiskLevelUntagPlayer message)
        {
            _fraudSubscriber.Handle(message);
        }

        public void Consume(PlayerRegistrationChecked message)
        {
            _fraudSubscriber.Handle(message);
        }

        public void Consume(GameCreated message)
        {
            _gameSubscriber.Handle(message);
        }

        public void Consume(GameUpdated message)
        {
            _gameSubscriber.Handle(message);
        }

        public void Consume(GameDeleted message)
        {
            _gameSubscriber.Handle(message);
        }

        public void Consume(BetPlaced message)
        {
            _gameSubscriber.Handle(message);
        }

        public void Consume(BetPlacedFree message)
        {
            _gameSubscriber.Handle(message);
        }

        public void Consume(BetTied message)
        {
            _gameSubscriber.Handle(message);
        }

        public void Consume(BetWon message)
        {
            _gameSubscriber.Handle(message);
        }

        public void Consume(BetLost message)
        {
            _gameSubscriber.Handle(message);
        }

        public void Consume(BetCancelled message)
        {
            _gameSubscriber.Handle(message);
        }

        public void Consume(BetAdjusted message)
        {
            _gameSubscriber.Handle(message);
        }

        public void Consume(RegoHeadSeeded message)
        {
            _applicationSeeder.Seed();
        }
    }
}