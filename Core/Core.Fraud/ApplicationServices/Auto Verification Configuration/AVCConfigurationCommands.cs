using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using AFT.RegoV2.Core.Auth.Interface;
using AFT.RegoV2.Core.Common.Events.Fraud;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Fraud.Interface.ApplicationServices;
using AFT.RegoV2.Core.Fraud.Interface.Data;
using AFT.RegoV2.Core.Fraud.Interface.Events;
using AFT.RegoV2.Core.Fraud.Validations;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.RegoBus.Interfaces;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Shared.Utils;

namespace AFT.RegoV2.Core.Fraud.ApplicationServices
{
    public class AVCConfigurationCommands : MarshalByRefObject, IAVCConfigurationCommands, IApplicationService
    {
        private readonly IFraudRepository _fraudRepository;
        private readonly IActorInfoProvider _actorInfoProvider;
        private readonly IEventBus _eventBus;

        public AVCConfigurationCommands(IFraudRepository fraudRepository, IActorInfoProvider actorInfoProvider, IEventBus eventBus)
        {
            _fraudRepository = fraudRepository;
            _actorInfoProvider = actorInfoProvider;
            _eventBus = eventBus;
        }

        #region Public methods


        [Permission(Permissions.Create, Module = Modules.AutoVerificationConfiguration)]
        public AutoVerificationCheckConfiguration Create(AVCConfigurationDTO data)
        {
            ValidateAvcEntity(data, AvcConfigurationDtoQueriesEnum.Create);

            var entity = new AutoVerificationCheckConfiguration
            {
                Id = data.Id,
                HasFraudRiskLevel = data.HasFraudRiskLevel,
                BrandId = data.Brand,
                Brand = _fraudRepository.Brands.First(x => x.Id == data.Brand),
                Currency = data.Currency,
                DateCreated = DateTimeOffset.UtcNow,

                HasWinLoss = data.HasWinLoss,
                WinLossAmount = data.WinLossAmount,
                WinLossOperator = data.WinLossOperator,

                HasCompleteDocuments = data.HasCompleteDocuments,

                HasWithdrawalExemption = data.HasWithdrawalExemption,

                HasTotalDepositAmount = data.HasTotalDepositAmount,
                TotalDepositAmount = data.TotalDepositAmount,
                TotalDepositAmountOperator = data.TotalDepositAmountOperator,

                HasDepositCount = data.HasDepositCount,
                TotalDepositCountAmount = data.TotalDepositCountAmount,
                TotalDepositCountOperator = data.TotalDepositCountOperator,

                HasAccountAge = data.HasAccountAge,
                AccountAge = data.AccountAge,
                AccountAgeOperator = data.AccountAgeOperator,

                HasWithdrawalCount = data.HasWithdrawalCount,
                TotalWithdrawalCountAmount = data.TotalWithdrawalCountAmount,
                TotalWithdrawalCountOperator = data.TotalWithdrawalCountOperator,

                HasNoRecentBonus = data.HasNoRecentBonus,
                CreatedBy = _actorInfoProvider.Actor.Id,

                HasPaymentLevel = data.HasPaymentLevel,
                Status = data.Status
            };

            //ToDo: Add validation here
            if (entity.Id == Guid.Empty)
                entity.Id = Guid.NewGuid();

            if (data.HasFraudRiskLevel)
            {
                //Let's add all risk levels.
                var brand = _fraudRepository.Brands
                    .Single(x => x.Id == data.Brand);

                _fraudRepository.RiskLevels
                    .Where(x => x.BrandId == brand.Id)
                    .ForEach(x => entity.AllowedRiskLevels.Add(x));

                //Let's remove all ignored via UI
                foreach (var riskLevelId in data.RiskLevels)
                {
                    var riskLevel = entity.AllowedRiskLevels.FirstOrDefault(x => x.Id == riskLevelId);
                    if (riskLevel != null)
                        entity.AllowedRiskLevels.Remove(riskLevel);
                }
            }

            if (data.HasPaymentLevel)
            {
                entity.PaymentLevels.Clear();

                //Let's add all payment levels.
                var currentlySelectedPaymentLevels = _fraudRepository
                    .PaymentLevels
                    .Where(x => data.PaymentLevels.Contains(x.Id));

                currentlySelectedPaymentLevels.ForEach(x => entity.PaymentLevels.Add(x));

            }

            if (data.HasWinnings)
            {
                entity.HasWinnings = true;
                foreach (var winningRule in data.WinningRules)
                {
                    var rule = CreateWinningRuleEntity(winningRule, entity);

                    if (winningRule.Period == PeriodEnum.CustomDate)
                    {
                        rule.StartDate = winningRule.StartDate;
                        rule.EndDate = winningRule.EndDate;
                    }

                    entity.WinningRules.Add(rule);
                }
            }

            entity.VipLevels.Clear();
            data.VipLevels
                .ForEach(id =>
                {
                    var vipLevel = _fraudRepository.VipLevels
                        .Single(x => x.Id == id);

                    entity.VipLevels.Add(vipLevel);
                });

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                _fraudRepository.AutoVerificationCheckConfigurations.Add(entity);
                _fraudRepository.SaveChanges();
                scope.Complete();
            }

            _eventBus.Publish(new AutoVerificationCheckCreated()
            {
                CreatedBy = _actorInfoProvider.Actor.UserName,
                DateCreated = DateTime.UtcNow
            });

            return entity;
        }

        private static WinningRule CreateWinningRuleEntity(WinningRuleDTO winningRule, AutoVerificationCheckConfiguration entity)
        {
            return new WinningRule
            {
                Id = Guid.NewGuid(),
                ProductId = winningRule.ProductId,
                Comparison = winningRule.Comparison,
                Amount = winningRule.Amount,
                Period = winningRule.Period,
                AutoVerificationCheckConfigurationId = entity.Id,
                StartDate = winningRule.StartDate,
                EndDate = winningRule.EndDate
            };
        }


        //[Permission(Permissions.Delete, Module = Modules.AutoVerificationConfiguration)]
        public void Delete(Guid id)
        {
            var avcConfiguratoin = _fraudRepository.AutoVerificationCheckConfigurations.Single(x => x.Id == id);
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                _fraudRepository.AutoVerificationCheckConfigurations.Remove(avcConfiguratoin);
                _fraudRepository.SaveChanges();
                scope.Complete();
            }
        }


        [Permission(Permissions.Update, Module = Modules.AutoVerificationConfiguration)]
        public void Update(AVCConfigurationDTO data)
        {
            this.ValidateAvcEntity(data, AvcConfigurationDtoQueriesEnum.Update);

            var entity = _fraudRepository.AutoVerificationCheckConfigurations
                .Include(o => o.VipLevels)
                .Single(x => x.Id == data.Id);

            entity.HasFraudRiskLevel = data.HasFraudRiskLevel;
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

            entity.HasPaymentLevel = data.HasPaymentLevel;

            entity.HasWinnings = data.HasWinnings;
            UpdateWinningRules(entity, data);
            // entity.WinningRules = data.WinningRules.Select(Mapper.Map<WinningRule>) as ICollection<WinningRule>;

            entity.HasCompleteDocuments = data.HasCompleteDocuments;

            entity.HasWinLoss = data.HasWinLoss;
            entity.WinLossAmount = data.WinLossAmount;
            entity.WinLossOperator = data.WinLossOperator;

            entity.HasWithdrawalCount = data.HasWithdrawalCount;
            entity.TotalWithdrawalCountAmount = data.TotalWithdrawalCountAmount;
            entity.TotalWithdrawalCountOperator = data.TotalWithdrawalCountOperator;

            entity.HasAccountAge = data.HasAccountAge;
            entity.AccountAge = data.AccountAge;
            entity.AccountAgeOperator = data.AccountAgeOperator;

            entity.HasTotalDepositAmount = data.HasTotalDepositAmount;
            entity.TotalDepositAmount = data.TotalDepositAmount;
            entity.TotalDepositAmountOperator = data.TotalDepositAmountOperator;

            entity.HasDepositCount = data.HasDepositCount;
            entity.TotalDepositCountAmount = data.TotalDepositCountAmount;
            entity.TotalDepositCountOperator = data.TotalDepositCountOperator;

            entity.HasNoRecentBonus = data.HasNoRecentBonus;

            entity.HasWithdrawalExemption = data.HasWithdrawalExemption;

            if (data.HasFraudRiskLevel)
            {
                entity.AllowedRiskLevels.Clear();

                //Let's add all risk levels.
                _fraudRepository
                    .RiskLevels
                    .Include(x => x.Brand)
                    .Where(x => x.Brand != null)
                    .Where(x => x.Brand.Id == data.Brand)
                    .ForEach(x => entity.AllowedRiskLevels.Add(x));

                //Let's remove all ignored via UI
                foreach (var riskLevelId in data.RiskLevels)
                {
                    var riskLevel = entity.AllowedRiskLevels.FirstOrDefault(x => x.Id == riskLevelId);
                    if (riskLevel != null)
                        entity.AllowedRiskLevels.Remove(riskLevel);
                }
            }

            if (data.HasPaymentLevel)
            {
                entity.PaymentLevels.Clear();

                //Let's add all payment levels.
                var currentlySelectedPaymentLevels = _fraudRepository
                    .PaymentLevels
                    .Where(x => data.PaymentLevels.Contains(x.Id));

                currentlySelectedPaymentLevels.ForEach(x => entity.PaymentLevels.Add(x));
            }

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                _fraudRepository.AutoVerificationCheckConfigurations.AddOrUpdate(entity);
                _fraudRepository.SaveChanges();
                scope.Complete();
            }

            _eventBus.Publish(new AutoVerificationCheckUpdated()
            {
                UpdatedBy = _actorInfoProvider.Actor.UserName,
                DateUpdated = DateTime.UtcNow
            });
        }

        [Permission(Permissions.Activate, Module = Modules.AutoVerificationConfiguration)]
        public void Activate(AvcChangeStatusCommand command)
        {
            var avcToUpdate = _fraudRepository
                .AutoVerificationCheckConfigurations
                .FirstOrDefault(avc => avc.Id == command.Id);

            if (avcToUpdate == null)
                return;

            avcToUpdate.Status = AutoVerificationCheckStatus.Active;
            avcToUpdate.ActivatedBy = _actorInfoProvider.Actor.UserName;
            avcToUpdate.DateActivated = DateTime.UtcNow;

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                _fraudRepository.AutoVerificationCheckConfigurations.AddOrUpdate(avcToUpdate);
                _fraudRepository.SaveChanges();
                scope.Complete();
            }

            _eventBus.Publish(new AutoVerificationCheckActivated()
            {
                ActivatedBy = avcToUpdate.ActivatedBy,
                DateActivated = avcToUpdate.DateActivated
            });
        }
        [Permission(Permissions.Deactivate, Module = Modules.AutoVerificationConfiguration)]
        public void Deactivate(AvcChangeStatusCommand command)
        {
            var avcToUpdate = _fraudRepository
                .AutoVerificationCheckConfigurations
                .FirstOrDefault(avc => avc.Id == command.Id);

            if (avcToUpdate == null)
                return;

            avcToUpdate.Status = AutoVerificationCheckStatus.Inactive;
            avcToUpdate.DeactivatedBy = _actorInfoProvider.Actor.UserName; ;
            avcToUpdate.DateDeactivated = DateTime.UtcNow;

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                _fraudRepository.AutoVerificationCheckConfigurations.AddOrUpdate(avcToUpdate);
                _fraudRepository.SaveChanges();
                scope.Complete();
            }

            _eventBus.Publish(new AutoVerificationCheckDeactivated()
            {
                DeactivatedBy = _actorInfoProvider.Actor.UserName,
                DateDeactivated = avcToUpdate.DateDeactivated
            });
        }

        /// <summary>
        /// The method is used to validate AVC entity depending on the query type
        /// </summary>
        /// <param name="data">The entity populated in the form</param>
        /// <param name="queryType">Type of the query: Create, Update etc.</param>
        private void ValidateAvcEntity(AVCConfigurationDTO data, AvcConfigurationDtoQueriesEnum queryType)
        {
            var validationResult = new AVCConfigurationDTOValidator(_fraudRepository, queryType).Validate(data);

            if (validationResult.IsValid)
                return;

            var validationError = validationResult.Errors.FirstOrDefault();

            if (validationError != null)
                throw new RegoException(validationError.ToString());
        }

        private void UpdateWinningRules(AutoVerificationCheckConfiguration entity, AVCConfigurationDTO data)
        {
            foreach (var rule in entity.WinningRules.ToList())
                _fraudRepository.WinningRules.Remove(rule);

            foreach (var rule in data.WinningRules)
                _fraudRepository.WinningRules.Add(CreateWinningRuleEntity(rule, entity));

            _fraudRepository.SaveChanges();
        }

        #endregion
    }
}