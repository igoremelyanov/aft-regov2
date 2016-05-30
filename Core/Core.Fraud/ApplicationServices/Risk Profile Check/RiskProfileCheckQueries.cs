using System;
using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.Core.Common.Events.Brand;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Fraud.Interface.ApplicationServices;
using AFT.RegoV2.Core.Fraud.Interface.Data;
using AFT.RegoV2.Core.Security.Common;

namespace AFT.RegoV2.Core.Fraud.ApplicationServices
{
    public class RiskProfileCheckQueries : MarshalByRefObject, IRiskProfileCheckQueries, IApplicationService
    {
        private readonly IFraudRepository _fraudRepository;

        public RiskProfileCheckQueries(IFraudRepository fraudRepository)
        {
            _fraudRepository = fraudRepository;
        }

        public IQueryable<RiskProfileConfiguration> GetConfigurations()
        {
            return _fraudRepository.RiskProfileConfigurations;
        }

        [Permission(Permissions.View, Module = Modules.RiskProfileCheckConfiguration)]
        public RiskProfileCheckDTO GetConfiguration(Guid id)
        {
            var config = _fraudRepository.RiskProfileConfigurations
                .Include(o => o.Brand)
                .Include(o => o.AllowedPaymentMethods)
                .Include(o => o.AllowedBonuses)
                .Include(o => o.AllowedRiskLevels)
                .Include(o => o.VipLevels)
                .Single(o => o.Id == id);

            return new RiskProfileCheckDTO
            {
                Licensee = config.Brand.LicenseeId,
                Brand = config.BrandId,
                Currency = config.Currency,
                VipLevels = config.VipLevels.Select(o=>o.Id).ToArray(),

                AccountAge = config.AccountAge,
                AccountAgeOperator = config.AccountAgeOperator,
                HasAccountAge = config.HasAccountAge,

                TotalDepositCountAmount = config.TotalDepositCountAmount,
                TotalDepositCountOperator = config.TotalDepositCountOperator,
                HasDepositCount = config.HasDepositCount,

                PaymentMethods = config.AllowedPaymentMethods.Select(o => o.Id),
                HasPaymentMethodCheck = config.HasPaymentMethodCheck,

                TotalWithdrawalCountAmount = config.TotalWithdrawalCountAmount,
                TotalWithdrawalCountOperator = config.TotalWithdrawalCountOperator,
                HasWithdrawalCount = config.HasWithdrawalCount,

                RiskLevels = config.AllowedRiskLevels.Select(o => o.Id),
                HasFraudRiskLevel = config.HasFraudRiskLevel,

                Bonuses = config.AllowedBonuses.Select(o => o.Id),
                HasBonusCheck = config.HasBonusCheck,

                WinLossAmount = config.WinLossAmount,
                WinLossOperator = config.WinLossOperator,
                HasWinLoss = config.HasWinLoss,

                WithdrawalAverageChangeAmount = config.WithdrawalAveragePercentage,
                WithdrawalAverageChangeOperator = config.WithdrawalAveragePercentageOperator,
                HasWithdrawalAverageChange = config.HasWithdrawalAveragePercentageCheck,

                WinningsToDepositIncreaseAmount = config.WinningsToDepositPercentageIncrease,
                WinningsToDepositIncreaseOperator = config.WinningsToDepositPercentageIncreaseOperator,
                HasWinningsToDepositIncrease = config.HasWinningsToDepositPercentageIncreaseCheck
            };
        }

        public object GetVipLevels(Guid? brandId)
        {
            var activeVipLevels = _fraudRepository.VipLevels.Where(o=>o.Status == VipLevelStatus.Active);

            var filteredVipLevels = brandId.HasValue
                ? activeVipLevels.Where(x => x.BrandId == brandId)
                : activeVipLevels;

            return filteredVipLevels.Select(x => new {x.Id, x.Name});
        }
    }
}