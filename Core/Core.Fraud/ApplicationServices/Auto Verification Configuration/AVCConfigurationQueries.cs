using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Fraud.Interface.ApplicationServices;
using AFT.RegoV2.Core.Fraud.Interface.Data;
using AFT.RegoV2.Core.Fraud.Validations;
using AFT.RegoV2.Core.Security.Common;
using FluentValidation.Results;

namespace AFT.RegoV2.Core.Fraud.ApplicationServices
{
    public class AVCConfigurationQueries : MarshalByRefObject, IAVCConfigurationQueries, IApplicationService
    {
        private readonly IFraudRepository _fraudRepository;
        public AVCConfigurationQueries(IFraudRepository fraudRepository)
        {
            _fraudRepository = fraudRepository;
        }
        public AVCConfigurationDTO GetAutoVerificationCheckConfiguration(Guid id)
        {
            var avcConfiguration = _fraudRepository
                .AutoVerificationCheckConfigurations
                .Include(o => o.VipLevels)
                .SingleOrDefault(x => x.Id == id);

            var dto = new AVCConfigurationDTO
            {
                Id = avcConfiguration.Id,
                Brand = avcConfiguration.BrandId,
                Currency = avcConfiguration.Currency,
                VipLevels = avcConfiguration.VipLevels.Select(o => o.Id),
                HasFraudRiskLevel = avcConfiguration.HasFraudRiskLevel,
                Licensee = avcConfiguration.Brand.LicenseeId,
                HasWinnings = avcConfiguration.HasWinnings,
                WinningRules = avcConfiguration.WinningRules.Select(o => new WinningRuleDTO
                {
                    Id = o.Id,
                    Amount = o.Amount,
                    Period = o.Period,
                    Comparison = o.Comparison,
                    ProductId = o.ProductId,
                    StartDate = o.StartDate,
                    EndDate = o.EndDate
                }),
                RiskLevels = avcConfiguration.AllowedRiskLevels.Select(x => x.Id).AsEnumerable(),
                PaymentLevels = avcConfiguration.PaymentLevels.Select(x => x.Id).AsEnumerable(),
                HasWinLoss = avcConfiguration.HasWinLoss,
                WinLossAmount = avcConfiguration.WinLossAmount,
                WinLossOperator = avcConfiguration.WinLossOperator,
                HasDepositCount = avcConfiguration.HasDepositCount,
                TotalDepositCountAmount = avcConfiguration.TotalDepositCountAmount,
                TotalDepositCountOperator = avcConfiguration.TotalDepositCountOperator,
                HasAccountAge = avcConfiguration.HasAccountAge,
                AccountAge = avcConfiguration.AccountAge,
                AccountAgeOperator = avcConfiguration.AccountAgeOperator,
                HasTotalDepositAmount = avcConfiguration.HasTotalDepositAmount,
                TotalDepositAmount = avcConfiguration.TotalDepositAmount,
                TotalDepositAmountOperator = avcConfiguration.TotalDepositAmountOperator,
                HasWithdrawalCount = avcConfiguration.HasWithdrawalCount,
                TotalWithdrawalCountAmount = avcConfiguration.TotalWithdrawalCountAmount,
                TotalWithdrawalCountOperator = avcConfiguration.TotalWithdrawalCountOperator,
                HasWithdrawalExemption = avcConfiguration.HasWithdrawalExemption,
                HasNoRecentBonus = avcConfiguration.HasNoRecentBonus,
                HasCompleteDocuments = avcConfiguration.HasCompleteDocuments,
                HasPaymentLevel = avcConfiguration.HasPaymentLevel,
                Status = avcConfiguration.Status

            };
            return dto;
        }

        [Permission(Permissions.View, Module = Modules.AutoVerificationConfiguration)]
        public IEnumerable<AutoVerificationCheckConfiguration> GetAutoVerificationCheckConfigurations()
        {
            return _fraudRepository
                .AutoVerificationCheckConfigurations
                .Include(x => x.Brand)
                .Include(o => o.VipLevels)
                .ToList();
        }

        public IEnumerable<AutoVerificationCheckConfiguration> GetAutoVerificationCheckConfigurations(Guid brandId)
        {
            return _fraudRepository
                .AutoVerificationCheckConfigurations
                .Include(x => x.Brand)
                .Where(x => x.Brand.Id == brandId);
        }

        //Validation calls
        public ValidationResult GetValidationResult(AvcChangeStatusCommand model, AutoVerificationCheckStatus status)
        {
            return new AVCStatusValidator(_fraudRepository, status).Validate(model);
        }
    }
}
