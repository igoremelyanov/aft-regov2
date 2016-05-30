using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Bonus.Core;
using AFT.RegoV2.Bonus.Core.ApplicationServices;
using AFT.RegoV2.Bonus.Core.Data;
using AFT.RegoV2.Bonus.Core.Models.Commands;
using AFT.RegoV2.Bonus.Core.Models.Enums;
using AFT.RegoV2.Bonus.Tests.Doubles;
using AFT.RegoV2.Core.Auth.Interface;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Payment;
using AFT.RegoV2.Core.Common.Events;
using AFT.RegoV2.Core.Common.Events.Brand;
using AFT.RegoV2.Core.Common.Events.Fraud;
using AFT.RegoV2.Core.Common.Events.Player;
using AFT.RegoV2.Core.Common.EventStore;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Shared.Logging;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Game.Interface.Events;
using AFT.RegoV2.Core.Game.Interface.Interfaces;
using AFT.RegoV2.Core.Payment.Interface.Events;
using AFT.RegoV2.Core.Messaging.Interface.ApplicationServices;
using AFT.RegoV2.Domain.Brand.Events;
using AFT.RegoV2.RegoBus.Bus;
using AFT.RegoV2.RegoBus.EventStore;
using AFT.RegoV2.RegoBus.Interfaces;
using AFT.RegoV2.Shared.Synchronization;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.TestDoubles;
using AFT.RegoV2.WinService.Workers;
using Microsoft.Practices.Unity;
using Moq;
using Player = AFT.RegoV2.Bonus.Core.Data.Player;
using Template = AFT.RegoV2.Bonus.Core.Data.Template;

namespace AFT.RegoV2.Bonus.Tests.Base
{
    public abstract class BonusTestBase : TestBase
    {
        protected IUnityContainer Container { get; private set; }
        protected IBonusRepository BonusRepository { get; set; }
        protected BonusCommands BonusCommands { get; set; }
        protected IServiceBus ServiceBus { get; set; }
        private BonusManagementCommands BonusManagementCommands { get; set; }

        public override void BeforeAll()
        {
            base.BeforeAll();

            Container = CreateContainer();

            BonusCommands = Container.Resolve<BonusCommands>();
            BonusRepository = Container.Resolve<IBonusRepository>();
            ServiceBus = Container.Resolve<IServiceBus>();
            BonusManagementCommands = Container.Resolve<BonusManagementCommands>();

            Container.Resolve<BonusWorker>().Start();
        }

        protected virtual IUnityContainer CreateContainer()
        {
            var container = new UnityContainer();

            container.RegisterType<IServiceBus, FakeServiceBus>(new ContainerControlledLifetimeManager());
            container.RegisterType<IBonusRepository, FakeBonusRepository>(new ContainerControlledLifetimeManager());
            container.RegisterType<IActorInfoProvider, FakeActorInfoProvider>(new ContainerControlledLifetimeManager());
            container.RegisterType<IEventRepository, FakeEventRepository>(new ContainerControlledLifetimeManager());
            container.RegisterType<ISynchronizationService, FakeSynchronizationService>();

            var bus = new EventBus();
            bus.Subscribe(() => container.Resolve<EventStoreSubscriber>());
            container.RegisterInstance<IEventBus>(bus);

            container.RegisterInstance(new Mock<IBrandOperations>().Object);
            container.RegisterInstance(new Mock<IMessageTemplateService>().Object);
            container.RegisterInstance(new Mock<ILog>().Object);
            container.RegisterInstance(new Mock<ICommonSettingsProvider>().Object);

            return container;
        }

        public void CreateActiveBrandWithProducts()
        {
            var brandId = Guid.NewGuid();
            ServiceBus.PublishMessage(new BrandRegistered
            {
                Id = brandId,
                Name = TestDataGenerator.GetRandomString(),
                LicenseeId = Guid.NewGuid(),
                LicenseeName = TestDataGenerator.GetRandomString(),
                TimeZoneId = TestDataGenerator.GetRandomTimeZone().Id
            });

            ServiceBus.PublishMessage(new BrandCurrenciesAssigned
            {
                BrandId = brandId,
                Currencies = new[] { "CAD", "RMB" }
            });

            ServiceBus.PublishMessage(new VipLevelRegistered
            {
                BrandId = brandId,
                Code = "Silver"
            });

            ServiceBus.PublishMessage(new WalletTemplateCreated
            {
                BrandId = brandId,
                WalletTemplates = new[]
                {
                    new WalletTemplateDto{Id = Guid.NewGuid(), CurrencyCode = "CAD", IsMain = true, ProductIds = new Guid[]{}},
                    new WalletTemplateDto{Id = Guid.NewGuid(), CurrencyCode = "RMB", IsMain = false, ProductIds = new Guid[]{}}
                }
            });

            ServiceBus.PublishMessage(new RiskLevelCreated
            {
                BrandId = brandId,
                Id = Guid.NewGuid(),
                Status = RiskLevelStatus.Active
            });
        }

        public Player CreatePlayer(Guid? referredBy = null)
        {
            var playerId = Guid.NewGuid();
            if (referredBy != null)
            {
                var referrer = BonusRepository.Players.Single(a => a.Id == referredBy);
                referredBy = referrer.ReferralId;
            }
            ServiceBus.PublishMessage(new PlayerRegistered
            {
                PlayerId = playerId,
                BrandId = BonusRepository.Brands.First().Id,
                UserName = TestDataGenerator.GetRandomString(),
                Email = TestDataGenerator.GetRandomEmail(),
                VipLevel = "Silver",
                CurrencyCode = "CAD",
                DateRegistered = DateTime.Now,
                ReferralId = referredBy,
                RefIdentifier = Guid.NewGuid()
            });

            return BonusRepository.Players.Single(p => p.Id == playerId);
        }

        public void MakeDeposit(Guid playerId, decimal depositAmount = 200, string bonusCode = null, Guid? bonusId = null)
        {
            var depositId = SubmitDeposit(playerId, depositAmount, bonusCode, bonusId);
            ApproveDeposit(depositId, playerId, depositAmount);
        }

        public Guid SubmitDeposit(Guid playerId, decimal depositAmount = 200, string bonusCode = null, Guid? bonusId = null)
        {
            var depositId = Guid.NewGuid();
            ServiceBus.PublishMessage(new DepositSubmitted
            {
                DepositId = depositId,
                PlayerId = playerId,
                Amount = depositAmount
            });

            if (bonusId.HasValue || string.IsNullOrWhiteSpace(bonusCode) == false)
            {
                BonusCommands.ApplyForBonus(new DepositBonusApplication
                {
                    PlayerId = playerId,
                    BonusId = bonusId,
                    BonusCode = bonusCode,
                    Amount = depositAmount,
                    DepositId = depositId
                });
            }

            return depositId;
        }

        public void ApproveDeposit(Guid depositId, Guid playerId, decimal depositAmount)
        {
            ServiceBus.PublishMessage(new DepositApproved
            {
                DepositId = depositId,
                PlayerId = playerId,
                ActualAmount = depositAmount
            });
        }

        public void UnverifyDeposit(Guid depositId, Guid playerId)
        {
            ServiceBus.PublishMessage(new DepositUnverified
            {
                PlayerId = playerId,
                DepositId = depositId
            });
        }

        public void MakeFundIn(Guid playerId, Guid destinationWalletStructureId, decimal amount, string bonusCode = null, Guid? bonusId = null)
        {
            if (bonusId.HasValue || string.IsNullOrWhiteSpace(bonusCode) == false)
            {
                BonusCommands.ApplyForBonus(new FundInBonusApplication
                {
                    PlayerId = playerId,
                    BonusId = bonusId,
                    BonusCode = bonusCode,
                    Amount = amount,
                    DestinationWalletTemplateId = destinationWalletStructureId
                });
            }

            ServiceBus.PublishMessage(new TransferFundCreated
            {
                PlayerId = playerId,
                DestinationWalletStructureId = destinationWalletStructureId,
                Amount = amount,
                BonusCode = bonusCode,
                Type = TransferFundType.FundIn,
                Status = TransferFundStatus.Approved
            });

            // Here we need somehow create an event to mimic a wallet fund-in
        }

        public void PlaceAndLoseBet(decimal amount, Guid playerId)
        {
            var gameId = Guid.NewGuid();
            var roundId = Guid.NewGuid();
            PlaceBet(amount, playerId, gameId, roundId);
            LoseBet(playerId, roundId, amount);
        }

        public void PlaceAndWinBet(decimal betAmount, decimal wonAmount, Guid playerId)
        {
            var gameId = Guid.NewGuid();
            var roundId = Guid.NewGuid();
            PlaceBet(betAmount, playerId, gameId, roundId);
            WinBet(wonAmount, playerId, gameId, roundId, betAmount);
        }

        public void PlaceBet(decimal amount, Guid playerId, Guid gameId, Guid roundId)
        {
            ServiceBus.PublishMessage(new BetPlaced
            {
                PlayerId = playerId,
                Amount = amount,
                GameId = gameId,
                RoundId = roundId
            });
        }

        public void LoseBet(Guid playerId, Guid roundId, decimal turnover)
        {
            ServiceBus.PublishMessage(new BetLost
            {
                PlayerId = playerId,
                RoundId = roundId,
                Turnover = turnover
            });
        }

        public void WinBet(decimal amount, Guid playerId, Guid gameId, Guid roundId, decimal turnover)
        {
            ServiceBus.PublishMessage(new BetWon
            {
                PlayerId = playerId,
                Amount = amount,
                Turnover = turnover,
                RoundId = roundId,
                GameId = gameId
            });
        }

        public void TagPlayerWithFraudRiskLevel(Guid playerId, Guid riskLevelId)
        {
            ServiceBus.PublishMessage(new RiskLevelTagPlayer
            {
                PlayerId = playerId,
                RiskLevelId = riskLevelId
            });
        }

        public void DeactivateRiskLevel(Guid riskLevelId)
        {
            ServiceBus.PublishMessage(new RiskLevelStatusUpdated
            {
                Id = riskLevelId,
                NewStatus = RiskLevelStatus.Inactive
            });
        }

        public Core.Data.Bonus CreateFirstDepositBonus(bool isActive = true, IssuanceMode mode = IssuanceMode.Automatic)
        {
            var bonusTemplate = CreateFirstDepositTemplate(mode: mode);

            return CreateBonus(bonusTemplate, isActive);
        }

        public Core.Data.Bonus CreateBonus(Template bonusTemplate, bool isActive = true)
        {
            var now = DateTimeOffset.Now.ToBrandOffset(bonusTemplate.Info.Brand.TimezoneId).Date;
            var model = new CreateUpdateBonus
            {
                Id = Guid.Empty,
                Name = TestDataGenerator.GetRandomString(5),
                Code = TestDataGenerator.GetRandomString(5),
                Description = TestDataGenerator.GetRandomString(20),
                TemplateId = bonusTemplate.Id,
                ActiveFrom = now,
                ActiveTo = now.AddDays(1),
                DurationStart = now,
                DurationEnd = now.AddDays(1)
            };
            var bonusId = BonusManagementCommands.AddUpdateBonus(model);
            if (isActive)
            {
                BonusManagementCommands.ChangeBonusStatus(new ToggleBonusStatus
                {
                    Id = bonusId,
                    IsActive = true
                });
            }

            return BonusRepository.GetCurrentVersionBonuses().Single(b => b.Id == bonusId);
        }

        public CreateUpdateTemplateInfo CreateTemplateInfo(BonusType bonusType, IssuanceMode mode = IssuanceMode.Automatic)
        {
            var brand = BonusRepository.Brands.First();
            return new CreateUpdateTemplateInfo
            {
                Name = TestDataGenerator.GetRandomString(),
                TemplateType = bonusType,
                BrandId = brand.Id,
                WalletTemplateId = brand.WalletTemplates.First().Id,
                Mode = mode
            };
        }

        public Template CreateFirstDepositTemplate(IssuanceMode mode = IssuanceMode.Automatic)
        {
            return CreateTemplate(CreateTemplateInfo(BonusType.FirstDeposit, mode));
        }

        public Template CreateTemplate(
            CreateUpdateTemplateInfo info = null,
            CreateUpdateTemplateAvailability availability = null,
            CreateUpdateTemplateRules rules = null,
            CreateUpdateTemplateWagering wagering = null,
            CreateUpdateTemplateNotification notification = null
            )
        {
            var model = new CreateUpdateTemplate
            {
                Id = Guid.Empty,
                Info = info ?? CreateTemplateInfo(BonusType.FirstDeposit)
            };
            var identifier = BonusManagementCommands.AddUpdateTemplate(model);

            model = new CreateUpdateTemplate
            {
                Id = identifier.Id,
                Version = identifier.Version,
                Availability = availability ?? new CreateUpdateTemplateAvailability(),
                Rules = rules ?? new CreateUpdateTemplateRules
                {
                    RewardTiers = new List<CreateUpdateRewardTier>
                    {
                        new CreateUpdateRewardTier
                        {
                            CurrencyCode = "CAD",
                            BonusTiers = new List<CreateUpdateTemplateTier> {new CreateUpdateTemplateTier {Reward = 27}}
                        }
                    }
                },
                Wagering = wagering ?? new CreateUpdateTemplateWagering(),
                Notification = notification ?? new CreateUpdateTemplateNotification()
            };
            identifier = BonusManagementCommands.AddUpdateTemplate(model);

            return BonusRepository.Templates.Single(t => t.Id == identifier.Id && t.Version == identifier.Version);
        }

        public Core.Data.Bonus CreateBonusWithReferFriendTiers()
        {
            var bonus = CreateFirstDepositBonus();

            bonus.Template.Info.TemplateType = BonusType.ReferFriend;
            bonus.Template.Rules.ReferFriendMinDepositAmount = 100;
            bonus.Template.Rules.ReferFriendWageringCondition = 1;
            bonus.Template.Rules.RewardTiers.Single().BonusTiers = new List<TierBase>
            {
                new BonusTier {From = 1, Reward = 10},
                new BonusTier {From = 4, Reward = 20},
                new BonusTier {From = 7, Reward = 30}
            };

            return bonus;
        }

        public void CompleteReferAFriendRequirments(Guid referrerId)
        {
            var referredPlayer = CreatePlayer(referrerId);
            MakeDeposit(referredPlayer.Id);
            PlaceAndLoseBet(200, referredPlayer.Id);
        }

        public Core.Data.Bonus CreateBonusWithBonusTiers(BonusRewardType rewardType)
        {
            var bonus = CreateFirstDepositBonus();

            bonus.Template.Rules.RewardType = rewardType;
            bonus.Template.Rules.RewardTiers.Single().BonusTiers = (rewardType == BonusRewardType.TieredPercentage
                ? new List<TierBase>
                {
                    new BonusTier {From = 1, Reward = 0.1m, MaxAmount = 40},
                    new BonusTier {From = 101, Reward = 0.2m, MaxAmount = 50}
                }
                : new List<TierBase>
                {
                    new BonusTier {From = 1, Reward = 10},
                    new BonusTier {From = 101, Reward = 20},
                    new BonusTier {From = 201, Reward = 30}
                });

            return bonus;
        }

        public Core.Data.Bonus CreateBonusWithHighDepositTiers(bool isAutoGenerate = true)
        {
            var bonus = CreateFirstDepositBonus();

            bonus.Template.Rules.RewardType = BonusRewardType.TieredAmount;
            bonus.Template.Info.TemplateType = BonusType.HighDeposit;
            bonus.Template.Rules.RewardTiers.Single().BonusTiers = new List<TierBase>
            {
                new HighDepositTier
                {
                    From = 500,
                    Reward = 50,
                    NotificationPercentThreshold = 0.9m
                }
            };
            if (isAutoGenerate)
            {
                bonus.Template.Rules.IsAutoGenerateHighDeposit = true;
            }
            else
            {
                bonus.Template.Rules.RewardTiers.Single().BonusTiers.Add(new HighDepositTier
                {
                    From = 1000,
                    Reward = 100,
                    NotificationPercentThreshold = 0.9m
                });
            }

            return bonus;
        }

        public void VerifyPlayerContact(Guid playerId, ContactType type)
        {
            ServiceBus.PublishMessage(new PlayerContactVerified(playerId, type));
        }

        public void DisableBonusesForPlayer(Guid playerId)
        {
            ServiceBus.PublishMessage(new PlayerRegistrationChecked { Action = SystemAction.DisableBonus, PlayerId = playerId });
        }
    }
}
