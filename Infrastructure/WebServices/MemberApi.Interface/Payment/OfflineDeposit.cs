using System;
using System.Collections.Generic;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Payment.Interface.Data;
using AFT.RegoV2.MemberApi.Interface.Bonus;

namespace AFT.RegoV2.MemberApi.Interface.Payment
{
    public class OfflineDepositFormDataRequest
    {
        public Guid BrandId { get; set; }
    }

    public class OfflineDepositFormDataResponse
    {
        public Dictionary<Guid, string> BankAccounts { get; set; }
    }

    public class DefaultPaymentSettingsRequest
    {
        public Guid BrandId { get; set; }
        public string CurrencyCode { get; set; }
    }

    public class PaymentSettingsResponse
    {
        public string CurrencyCode { get; set; }
        public decimal MinAmountPerTransaction { get; set; }
        public decimal MaxAmountPerTransaction { get; set; }
        public decimal MaxAmountPerDay { get; set; }
        public int MaxTransactionPerDay { get; set; }
        public int MaxTransactionPerWeek { get; set; }
        public int MaxTransactionPerMonth { get; set; }

        public PaymentSettingsResponse()
        {

        }

        public PaymentSettingsResponse(PaymentSettings paymentSetting)
        {
            if (paymentSetting == null)
                return;

            CurrencyCode = paymentSetting.CurrencyCode;
            MinAmountPerTransaction = paymentSetting.MinAmountPerTransaction;
            MaxAmountPerTransaction = paymentSetting.MaxAmountPerTransaction;
            MaxAmountPerDay = paymentSetting.MaxAmountPerDay;
            MaxTransactionPerDay = paymentSetting.MaxTransactionPerDay;
            MaxTransactionPerWeek = paymentSetting.MaxTransactionPerWeek;
            MaxTransactionPerMonth = paymentSetting.MaxTransactionPerMonth;
        }
    }

    public class OfflineDepositRequest
    {
        public Guid BankAccountId { get; set; }
        public decimal Amount { get; set; }
        public string PlayerRemarks { get; set; }
        public string NotificationMethod { get; set; }
        public string BankTime { get; set; }
        public string BankAccountTime { get; set; }
        public string BonusCode { get; set; }
        public Guid? BonusId { get; set; }
    }

    public class RegisterStep2Request
    {
        public double Amount { get; set; }
    }

    public class RegisterStep2Model
    {
        public decimal? Amount { get; set; }
        public decimal? Min { get; set; }
        public decimal? Max { get; set; }
        public IEnumerable<QualifiedBonus> Bonuses { get; set; }
    }

    public class OfflineDepositResponse
    {
        public Guid Id { get; set; }
        public Guid? BonusRedemptionId { get; set; }
        public string UriToSubmittedOfflineDeposit { get; set; }
    }

    public class PendingDepositsRequest
    {
    }

    public class PendingDepositsResponse
    {
        public IEnumerable<OfflineDeposit> PendingDeposits { get; set; }
    }

    public class OfflineDeposit
    {
        public Guid Id { get; set; }
        public string ReferenceCode { get; set; }
        public string DateCreated { get; set; }
        public decimal Amount { get; set; }
        public string AmountFormatted { get; set; }
        public string Status { get; set; }
        public string DepositType { get; set; }

        public string UnverifyReason { get; set; }

        public string TransferType { get; set; }

        public byte[] IdBack { get; set; }
        public byte[] IdFront { get; set; }
        public byte[] Receipt { get; set; }
        public Guid? BonusRedemptionId { get; set; }
    }

    public class GetOfflineDepositRequest
    {
        public Guid Id { get; set; }
    }
}
