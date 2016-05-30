using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.AdminApi.Interface.Payment;
using AFT.RegoV2.AdminWebsite.Common;
using AFT.RegoV2.Core.Common.Utils;

using ServiceStack.Common;
using UnverifyReasons = AFT.RegoV2.Core.Common.Data.UnverifyReasons;

namespace AFT.RegoV2.AdminWebsite.ViewModels
{
    public class OfflineDepositViewModel
    {
        public OfflineDepositViewModel(OfflineDepositDto offlineDeposit)
        {
            Identifier = offlineDeposit.BankAccount.Id;
            Licensee = offlineDeposit.Brand.LicenseeName;
            Brand = offlineDeposit.Brand.Name;
            PlayerId = offlineDeposit.PlayerId;
            Username = offlineDeposit.Player.Username;
            FirstName = offlineDeposit.Player.FirstName;
            LastName = offlineDeposit.Player.LastName;
            PlayerName = offlineDeposit.Player.FullName;
            TransactionNumber = offlineDeposit.TransactionNumber;
            Status = offlineDeposit.Status.ToString();
            PlayerAccountName = offlineDeposit.PlayerAccountName;
            PlayerAccountNumber = offlineDeposit.PlayerAccountNumber;
            ReferenceNumber = offlineDeposit.BankReferenceNumber;
            Amount = offlineDeposit.Amount;
            CurrencyCode = offlineDeposit.CurrencyCode;
            BankName = offlineDeposit.BankAccount.Bank.BankName;
            BankAccountId = offlineDeposit.BankAccount.AccountId;
            BankAccountName = offlineDeposit.BankAccount.AccountName;
            BankAccountNumber = offlineDeposit.BankAccount.AccountNumber;
            BankProvince = offlineDeposit.BankAccount.Province;
            BankBranch = offlineDeposit.BankAccount.Branch;
            TransferType = offlineDeposit.TransferType.ToString("F");
            DepositType = offlineDeposit.DepositType.ToString("F");
            OfflineDepositType = LabelHelper.LabelOfflineDepositType(offlineDeposit.DepositMethod);
            PaymentMethod = LabelHelper.LabelPaymentMethod(offlineDeposit.PaymentMethod);
            Remark = offlineDeposit.Remark;
            PlayerRemark = offlineDeposit.PlayerRemark;
            Created = offlineDeposit.Created.ToString("yyyy/MM/dd HH:mm:ss zzz");
            CreatedBy = offlineDeposit.CreatedBy;
            Verified = offlineDeposit.Verified?.ToString("yyyy/MM/dd HH:mm:ss zzz") ?? "";
            VerifiedBy = offlineDeposit.VerifiedBy;
            IdFrontImage = offlineDeposit.IdFrontImage;
            IdBackImage = offlineDeposit.IdBackImage;
            ReceiptImage = offlineDeposit.ReceiptImage;
            UnverifyReason = offlineDeposit.UnverifyReason.HasValue
                ? GetUnverifyReasons().Single(o => o.Key == offlineDeposit.UnverifyReason.ToString()).Value
                : string.Empty;
            UnverifyReasons = GetUnverifyReasons().Select(o => new
            {
                Code = o.Key,
                Message = o.Value
            });
        }

        private Dictionary<string, string> GetUnverifyReasons()
        {
            var enumType = typeof(UnverifyReasons);

            return Enum.GetValues(enumType)
                .Cast<UnverifyReasons>()
                .ToDictionary(
                    value => value.ToString(),
                    value =>
                    {
                        var descr = value.ToDescription();
                        return descr;
                    });
        }

        public Guid Identifier { get; set; }
        public string Licensee { get; set; }
        public string Brand { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PlayerName { get; set; }
        public string TransactionNumber { get; set; }
        public string Status { get; set; }
        public string PlayerAccountName { get; set; }
        public string PlayerAccountNumber { get; set; }
        public string ReferenceNumber { get; set; }
        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; }
        public string BankName { get; set; }
        public string BankAccountId { get; set; }
        public string BankAccountName { get; set; }
        public string BankAccountNumber { get; set; }
        public string BankProvince { get; set; }
        public string BankBranch { get; set; }
        public string TransferType { get; set; }
        public string DepositType { get; set; }
        public string OfflineDepositType { get; set; }
        public string PaymentMethod { get; set; }
        public string Remark { get; set; }
        public string PlayerRemark { get; set; }
        public string Created { get; set; }
        public string CreatedBy { get; set; }
        public string Verified { get; set; }
        public string VerifiedBy { get; set; }
        public Guid? IdFrontImage { get; set; }
        public Guid? IdBackImage { get; set; }
        public Guid? ReceiptImage { get; set; }
        public string NotificationMethod { get; set; }
        public string BonusName { get; set; }
        public IEnumerable UnverifyReasons { get; set; }
        public string UnverifyReason { get; set; }
        public Guid PlayerId { get; set; }
    }
}