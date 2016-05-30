using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Base;

namespace AFT.RegoV2.AdminApi.Interface.Payment
{
    #region Dto
    public class OfflineDepositDto
    {
        public Guid Id { get; set; }

        public Guid BrandId { get; set; }
        public BrandDto Brand { get; set; }

        public Guid PlayerId { get; set; }
        public PlayerDto Player { get; set; }

        public Guid BankAccountId { get; set; }
        public BankAccountDto BankAccount { get; set; }

        public string CurrencyCode { get; set; }

        public string TransactionNumber { get; set; }

        public DateTimeOffset Created { get; set; }

        public string CreatedBy { get; set; }

        public DateTimeOffset? Verified { get; set; }

        public string VerifiedBy { get; set; }

        public DateTimeOffset? Approved { get; set; }

        public string ApprovedBy { get; set; }

        public OfflineDepositStatus Status { get; set; }

        public string PlayerAccountName { get; set; }

        public string PlayerAccountNumber { get; set; }

        public string BankReferenceNumber { get; set; }

        public decimal Amount { get; set; }

        public decimal ActualAmount { get; set; }

        public decimal Fee { get; set; }

        public PaymentMethod PaymentMethod { get; set; }

        public TransferType TransferType { get; set; }

        public DepositMethod DepositMethod { get; set; }

        public DepositType DepositType { get; set; }

        public Guid? IdFrontImage { get; set; }

        public Guid? IdBackImage { get; set; }

        public Guid? ReceiptImage { get; set; }

        public string Remark { get; set; }

        public string PlayerRemark { get; set; }

        public decimal DepositWagering { get; set; }

        public Guid? BonusRedemptionId { get; set; }

        public UnverifyReasons? UnverifyReason { get; set; }
    }

    public enum DepositType
    {
        Offline,
        Online
    }
    public enum DepositMethod
    {
        InternetBanking,
        ATM,
        CounterDeposit
    }
    public enum TransferType
    {
        SameBank,
        DifferentBank
    }
    public enum PaymentMethod
    {
        OfflineBank,
        Online
    }
    public enum OfflineDepositStatus
    {
        New,
        Processing,
        Verified,
        Unverified,
        Rejected,
        Approved
    }
    #endregion

    #region Request/Response  
    public class GetOfflineDepositByIdResponse
    {
        public OfflineDepositDto OfflineDeposit { get; set; }
    }
    public class CreateOfflineDepositRequest
    {
        public Guid PlayerId { get; set; }
        public Guid BankAccountId { get; set; }
        public decimal Amount { get; set; }
        public Guid? BonusId { get; set; }
        public string BonusCode { get; set; }
        public string PlayerRemark { get; set; }
    }
    public class CreateOfflineDepositResponse : ValidationResponseBase
    {
        public Guid Id { get; set; }
    }

    public class ConfirmOfflineDepositRequest
    {
        [Required]
        public Guid Id { get; set; }

        [Required, MinLength(2), MaxLength(100)]
        public string PlayerAccountName { get; set; }

        [Required, MinLength(1), MaxLength(50)]
        public string PlayerAccountNumber { get; set; }

        [MinLength(2), MaxLength(50)]
        public string ReferenceNumber { get; set; }

        [Range(0.01, int.MaxValue)]
        public decimal Amount { get; set; }

        public Guid BankId { get; set; }

        public TransferType TransferType { get; set; }

        public DepositMethod OfflineDepositType { get; set; }

        public string IdFrontImage { get; set; }

        public string IdBackImage { get; set; }

        public string ReceiptImage { get; set; }

        [MaxLength(200)]
        public string Remark { get; set; }

        public string CurrentUser { get; set; }
        public byte[] IdFrontImageFile { get; set; }
        public byte[] IdBackImageFile { get; set; }
        public byte[] ReceiptImageFile { get; set; }
    }
    public class ConfirmOfflineDepositResponse : ValidationResponseBase
    {
        public Guid PlayerId { get; set; }
        public Guid? IdFrontImageId { get; set; }
        public Guid? IdBackImageId { get; set; }
        public Guid? ReceiptImageId { get; set; }
    }
    public class VerifyOfflineDepositRequest
    {
        public Guid Id { get; set; }
        public Guid BankAccountId { get; set; }
        public string Remarks { get; set; }
    }
    public class VerifyOfflineDepositResponse : ValidationResponseBase
    {
    }

    public class RejectOfflineDepositRequest
    {
        public Guid Id { get; set; }
        public string Remarks { get; set; }
    }

    public class RejectOfflineDepositResponse : ValidationResponseBase
    {
    }

    public class UnverifyOfflineDepositRequest
    {
        public Guid Id { get; set; }
        public string Remarks { get; set; }
        public UnverifyReasons UnverifyReason { get; set; }
    }

    public class UnverifyOfflineDepositResponse : ValidationResponseBase
    {
    }

    public class ApproveOfflineDepositRequest
    {
        [Required]
        public Guid Id { get; set; }

        [Range(0.01, int.MaxValue)]
        public decimal ActualAmount { get; set; }

        public decimal Fee { get; set; }

        [MaxLength(200)]
        public string PlayerRemark { get; set; }

        [MaxLength(200)]
        public string Remark { get; set; }
    }
    
    public class ApproveOfflineDepositResponse : ValidationResponseBase
    {
    }
    #endregion
}
