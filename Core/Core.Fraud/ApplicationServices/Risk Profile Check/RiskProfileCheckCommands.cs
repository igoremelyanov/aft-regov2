using System;
using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.Core.Auth.Interface;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Fraud.Interface.ApplicationServices;
using AFT.RegoV2.Core.Fraud.Interface.Data;
using AFT.RegoV2.Core.Fraud.Interface.Events;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.RegoBus.Interfaces;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Shared.Utils;

namespace AFT.RegoV2.Core.Fraud.ApplicationServices
{
    public class RiskProfileCheckCommands : MarshalByRefObject, IRiskProfileCheckCommands, IApplicationService
    {
        private readonly IFraudRepository _fraudRepository;
        private readonly IActorInfoProvider _actorInfoProvider;
        private readonly IEventBus _eventBus;

        public RiskProfileCheckCommands(IFraudRepository fraudRepository,
            IActorInfoProvider actorInfoProvider
            , IEventBus eventBus)
        {
            _fraudRepository = fraudRepository;
            _actorInfoProvider = actorInfoProvider;
            _eventBus = eventBus;
        }

        [Permission(Permissions.Create, Module = Modules.RiskProfileCheckConfiguration)]
        public RiskProfileConfiguration Create(RiskProfileCheckDTO data)
        {
            var entity = new RiskProfileConfiguration();

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                ValidateData(data, RiskAction.Create);

                entity.Id = Guid.NewGuid();
                CopyFromModel(data, entity);

                entity.CreatedBy = _actorInfoProvider.Actor.Id;

                if (data.HasFraudRiskLevel)
                {
                    foreach (var riskLevelId in data.RiskLevels)
                    {
                        var riskLevel = _fraudRepository.RiskLevels
                            .Single(x => x.Id == riskLevelId);

                        entity.AllowedRiskLevels.Add(riskLevel);
                    }
                }

                if (data.HasPaymentMethodCheck)
                {
                    foreach (var paymentMethodId in data.PaymentMethods)
                    {
                        var method = _fraudRepository.PaymentMethods
                            .Single(o => o.Id == paymentMethodId);

                        entity.AllowedPaymentMethods.Add(method);
                    }
                }

                if (data.HasBonusCheck)
                {
                    foreach (var bonusId in data.Bonuses)
                    {
                        var bonus = _fraudRepository.Bonuses
                            .Single(o => o.Id == bonusId);

                        entity.AllowedBonuses.Add(bonus);
                    }
                }

                _fraudRepository.RiskProfileConfigurations.Add(entity);
                _fraudRepository.SaveChanges();
                scope.Complete();
            }

            _eventBus.Publish(new RiskProfileCheckConfigCreated
            {
                Id = entity.Id,
                CreatedBy = _actorInfoProvider.Actor.UserName,
                DateCreated = entity.DateCreated
            });

            return entity;
        }

        private void ValidateData(RiskProfileCheckDTO data, RiskAction action)
        {
            var riskProfileConfigurations = _fraudRepository.RiskProfileConfigurations
                .Include(o => o.VipLevels)
                .AsQueryable();

            if (action == RiskAction.Update)
                riskProfileConfigurations = riskProfileConfigurations
                    .Where(o => o.Id != data.Id);

            if (riskProfileConfigurations.ToList().Any(
                record =>
                    record.BrandId == data.Brand &&
                    record.Currency == data.Currency &&
                    record.VipLevels.Select(o => o.Id).Intersect(data.VipLevels).Any()))
                throw new RegoException(
                    "You have already set up Risk Profile Check Configuration with the selected Brand, Currency and Vip level. Please, update the existing one or change your form data.");
        }

        private void CopyFromModel(RiskProfileCheckDTO data, RiskProfileConfiguration entity)
        {
            entity.BrandId = data.Brand;
            entity.Brand = _fraudRepository.Brands.First(x => x.Id == data.Brand);
            entity.Currency = data.Currency;

            entity.VipLevels.Clear();
            data.VipLevels
                .ForEach(id =>
                {
                    var vipLevel = _fraudRepository.VipLevels
                        .Single(x => x.Id == id);

                    entity.VipLevels.Add(vipLevel);
                });

            entity.DateCreated = DateTimeOffset.UtcNow;

            entity.HasWinLoss = data.HasWinLoss;
            entity.WinLossAmount = data.WinLossAmount;
            entity.WinLossOperator = data.WinLossOperator;

            entity.HasDepositCount = data.HasDepositCount;
            entity.TotalDepositCountAmount = data.TotalDepositCountAmount;
            entity.TotalDepositCountOperator = data.TotalDepositCountOperator;

            entity.HasAccountAge = data.HasAccountAge;
            entity.AccountAge = data.AccountAge;
            entity.AccountAgeOperator = data.AccountAgeOperator;

            entity.HasWithdrawalCount = data.HasWithdrawalCount;
            entity.TotalWithdrawalCountAmount = data.TotalWithdrawalCountAmount;
            entity.TotalWithdrawalCountOperator = data.TotalWithdrawalCountOperator;

            entity.HasWithdrawalAveragePercentageCheck = data.HasWithdrawalAverageChange;
            entity.WithdrawalAveragePercentageOperator = data.WithdrawalAverageChangeOperator;
            entity.WithdrawalAveragePercentage = data.WithdrawalAverageChangeAmount;

            entity.HasWinningsToDepositPercentageIncreaseCheck = data.HasWinningsToDepositIncrease;
            entity.WinningsToDepositPercentageIncreaseOperator = data.WinningsToDepositIncreaseOperator;
            entity.WinningsToDepositPercentageIncrease = data.WinningsToDepositIncreaseAmount;

            entity.HasPaymentMethodCheck = data.HasPaymentMethodCheck;

            entity.HasBonusCheck = data.HasBonusCheck;

            entity.HasFraudRiskLevel = data.HasFraudRiskLevel;
        }

        [Permission(Permissions.Update, Module = Modules.RiskProfileCheckConfiguration)]
        public void Update(RiskProfileCheckDTO data)
        {
            var entity = _fraudRepository.RiskProfileConfigurations
                    .Include(o => o.AllowedPaymentMethods)
                    .Include(o => o.AllowedBonuses)
                    .Include(o => o.AllowedRiskLevels)
                    .Include(o => o.VipLevels)
                    .Single(o => o.Id == data.Id);

            ValidateData(data, RiskAction.Update);

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                CopyFromModel(data, entity);

                entity.AllowedRiskLevels.Clear();
                if (data.HasFraudRiskLevel)
                {
                    foreach (var riskLevelId in data.RiskLevels)
                    {
                        var riskLevel = _fraudRepository.RiskLevels
                            .Single(x => x.Id == riskLevelId);

                        entity.AllowedRiskLevels.Add(riskLevel);
                    }
                }

                entity.AllowedPaymentMethods.Clear();
                if (data.HasPaymentMethodCheck)
                {
                    foreach (var paymentMethodId in data.PaymentMethods)
                    {
                        var method = _fraudRepository.PaymentMethods
                            .Single(o => o.Id == paymentMethodId);

                        entity.AllowedPaymentMethods.Add(method);
                    }
                }

                entity.AllowedBonuses.Clear();
                if (data.HasBonusCheck)
                {
                    foreach (var bonusId in data.Bonuses)
                    {
                        var bonus = _fraudRepository.Bonuses
                            .Single(o => o.Id == bonusId);

                        entity.AllowedBonuses.Add(bonus);
                    }
                }

                _fraudRepository.SaveChanges();
                scope.Complete();
            }

            _eventBus.Publish(new RiskProfileCheckConfigUpdated
            {
                Id = entity.Id,
                UpdatedBy = _actorInfoProvider.Actor.UserName,
                DateUpdated = DateTimeOffset.Now
            });
        }

        private enum RiskAction
        {
            Create,
            Update
        }
    }
}
