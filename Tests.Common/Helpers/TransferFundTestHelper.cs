using System;
using System.Linq;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Payment;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Payment.Data;
using AFT.RegoV2.Core.Payment.Interface.ApplicationServices;
using AFT.RegoV2.Core.Payment.Interface.Data.Commands;
using AFT.RegoV2.Core.Payment;

namespace AFT.RegoV2.Tests.Common.Helpers
{
    public class TransferFundTestHelper
    {
        private readonly ITransferSettingsCommands _transferSettingsCommands;

        public TransferFundTestHelper(ITransferSettingsCommands transferSettingsCommands)
        {
            _transferSettingsCommands = transferSettingsCommands;
        }

        public void CreateTransferSettings(
            Core.Brand.Interface.Data.Brand brand, 
            SaveTransferSettingsCommand saveTransferSettingsCommand, 
            Guid walletTemplateId,
            TransferFundType transferType = TransferFundType.FundIn
            )
        {
            var transferSettings = new SaveTransferSettingsCommand
            {
                Brand = brand.Id,
                Licensee = brand.Licensee.Id,
                TimezoneId = brand.TimezoneId,
                TransferType = TransferFundType.FundIn,
                Currency = brand.DefaultCurrency,
                //VipLevel = brand.VipLevels.Single().Code,
                VipLevel = brand.VipLevels.Single().Id,
                Wallet = walletTemplateId.ToString(),
                MinAmountPerTransaction = saveTransferSettingsCommand.MinAmountPerTransaction,
                MaxAmountPerTransaction = saveTransferSettingsCommand.MaxAmountPerTransaction,
                MaxAmountPerDay = saveTransferSettingsCommand.MaxAmountPerDay,
                MaxTransactionPerDay = saveTransferSettingsCommand.MaxTransactionPerDay,
                MaxTransactionPerWeek = saveTransferSettingsCommand.MaxTransactionPerWeek,
                MaxTransactionPerMonth = saveTransferSettingsCommand.MaxTransactionPerMonth
            };

            var transferSettingsId = _transferSettingsCommands.AddSettings(transferSettings);
            _transferSettingsCommands.Enable(transferSettingsId, brand.TimezoneId, "remark");
        }

        public void CreateTransferSettings(
            Core.Brand.Interface.Data.Brand brand,
            SaveTransferSettingsCommand saveTransferSettingsCommand,
            Guid walletTemplateId,
            TransferFundType transferType, 
            string currencyCode, 
            Guid vipLevel)
        {
            var transferSettings = new SaveTransferSettingsCommand
            {
                Brand = brand.Id,
                Licensee = brand.Licensee.Id,
                TimezoneId = brand.TimezoneId,
                TransferType = transferType,
                Currency = currencyCode,
                //VipLevel = vipLevel.ToString(),
                VipLevel = vipLevel,
                Wallet = walletTemplateId.ToString(),
                MinAmountPerTransaction = saveTransferSettingsCommand.MinAmountPerTransaction,
                MaxAmountPerTransaction = saveTransferSettingsCommand.MaxAmountPerTransaction,
                MaxAmountPerDay = saveTransferSettingsCommand.MaxAmountPerDay,
                MaxTransactionPerDay = saveTransferSettingsCommand.MaxTransactionPerDay,
                MaxTransactionPerWeek = saveTransferSettingsCommand.MaxTransactionPerWeek,
                MaxTransactionPerMonth = saveTransferSettingsCommand.MaxTransactionPerMonth
            };

            var transferSettingsId = _transferSettingsCommands.AddSettings(transferSettings);
            _transferSettingsCommands.Enable(transferSettingsId, brand.TimezoneId, "remark");
        }

        public void AddTransfer(
            Core.Brand.Interface.Data.Brand brand,
            Guid playerId, 
            Guid walletTemplateId,
            IPaymentRepository repository, 
            double timeShiftInDays = 0)
        {
            var transferFund = new TransferFund
            {
                Id = Guid.NewGuid(),
                Amount = 1,
                CreatedBy = playerId.ToString(),
                Created = DateTimeOffset.Now.ToBrandOffset(brand.TimezoneId) + TimeSpan.FromDays(timeShiftInDays),
                TransferType = TransferFundType.FundIn,
                WalletId = walletTemplateId.ToString(),
                Status = TransferFundStatus.Approved,
                TransactionNumber = "TF000000000"
            };

            repository.TransferFunds.Add(transferFund);
            repository.SaveChanges();
        }
    }
}
