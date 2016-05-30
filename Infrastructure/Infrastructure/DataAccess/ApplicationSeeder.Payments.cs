using System;
using System.Globalization;
using System.Linq;
using AFT.RegoV2.Core.Payment.Interface.Data.Commands;

namespace AFT.RegoV2.Infrastructure.DataAccess
{
    public partial class ApplicationSeeder
    {
        public void AddBank(Guid id, string bankId, string bankName, string country, Guid brandId)
        {
            if (_paymentRepository.Banks.Any(x => x.Id == id))
                return;

            _bankCommands.Add(new AddBankData
            {
                Id = id,
                BankId = bankId,
                BankName = bankName,
                BrandId = brandId,
                CountryCode = country,
                Remarks = "Created automatically while seeding database at first start"
            });
        }

        public void AddBankAccount(Guid id, string accountId, string accountName, string accountNumber, Guid accountType, Guid bankId, string branch,
            string province, string currencyCode, string supplierName, string contactNumber, string usbCode, Guid brandId, Guid licenseeId)
        {
            if (_paymentRepository.BankAccounts.Any(x => x.Id == id))
                return;

            _bankAccountCommands.Add(new AddBankAccountData
            {
                Id = id,
                AccountId = accountId,
                AccountName = accountName,
                AccountNumber = accountNumber,
                AccountType = accountType,
                Bank = bankId,
                Branch = branch,
                Province = province,
                Currency = currencyCode,
                SupplierName = supplierName,
                ContactNumber = contactNumber,
                USBCode = usbCode,
                PurchasedDate = DateTime.Now.AddMonths(-2).ToString(CultureInfo.InvariantCulture),
                UtilizationDate = DateTime.Now.ToString(CultureInfo.InvariantCulture),
                ExpirationDate = DateTime.Now.AddYears(1).ToString(CultureInfo.InvariantCulture),
                BrandId = brandId.ToString(),
                LicenseeId = licenseeId.ToString(),
            });

            _bankAccountCommands.Activate(id, "Activated when database has been seeded on first application start");
        }

        public Guid AddBankAccountType(Guid id, string name)
        {
            if (_paymentRepository.BankAccountTypes.Any(x => x.Id == id))
                return id;

            id = _bankAccountCommands.AddBankAccountType(new AFT.RegoV2.Core.Payment.Interface.Data.BankAccountType
            {
                Id = id,
                Name = name,
            });

            return id;
        }

        public Guid AddPaymentGatewaySettings(Guid brandId, string onlinePaymentMethodName, string paymentGatewayName, int channel)
        {
            var paymentGetwaySettings = _paymentRepository.PaymentGatewaySettings.FirstOrDefault(x =>
                x.OnlinePaymentMethodName == onlinePaymentMethodName && x.PaymentGatewayName == paymentGatewayName);
            if (paymentGetwaySettings != null)
            {
                return paymentGetwaySettings.Id;
            }

            string paymentUrl = _settingsProvider.GetPaymentProxyUrl() + "payment/issue";

            var result = _paymentGatewaySettingsCommands.Add(new SavePaymentGatewaysSettingsData
            {
                Brand = brandId,
                EntryPoint = paymentUrl,
                OnlinePaymentMethodName = onlinePaymentMethodName,
                PaymentGatewayName = paymentGatewayName,
                Channel = channel,
                Remarks = "Created automatically while seeding database at first start"
            });

            _paymentGatewaySettingsCommands.Activate(new ActivatePaymentGatewaySettingsData
            {
                Id = result.PaymentGatewaySettingsId,
                Remarks = "Activated when database has been seeded on first application start"
            });

            return result.PaymentGatewaySettingsId;
        }

        public void AddPaymentLevel(Guid id, Guid brandId, string currency, string name, string code,
            bool enableOfflineDeposit, bool enableOnlineDeposit, Guid bankAccountId, bool isDefault, Guid? paymentGatewaySettingsId = null)
        {
            if (_paymentRepository.PaymentLevels.Any(x => x.Id == id))
                return;

            _paymentLevelCommands.Save(new EditPaymentLevel
            {
                Id = id,
                Brand = brandId,
                Currency = currency,
                Name = name,
                Code = code,
                EnableOfflineDeposit = enableOfflineDeposit,
                EnableOnlineDeposit = enableOnlineDeposit,
                IsDefault = isDefault,
                BankAccounts = new[] { bankAccountId.ToString() },
                PaymentGatewaySettings = paymentGatewaySettingsId.HasValue ? new[] { paymentGatewaySettingsId.ToString() } : null
            });

            _paymentLevelCommands.Activate(new ActivatePaymentLevelCommand
            {
                Id = id,
                Remarks = "Activated when database has been seeded on first application start"
            });
        }

        public void AddCurrencyExchange(Guid brandId, string currencyToCode, decimal currentRate)
        {
            if (_paymentRepository.CurrencyExchanges.Any(x => x.BrandId == brandId && x.CurrencyToCode == currencyToCode))
                return;

            _currencyExchangeCommands.Add(new SaveCurrencyExchangeData
            {
                BrandId = brandId,
                Currency = currencyToCode,
                CurrentRate = currentRate,
            });
        }
    }
}
