using System;
using System.Collections.Generic;
using AFT.RegoV2.Core.Common.Data.Base;

namespace AFT.RegoV2.AdminApi.Interface.Payment
{
    #region Dto
    public class BankAccountDto
    {
        public Guid Id { get; set; }

        public string AccountId { get; set; }

        public string AccountName { get; set; }

        public string AccountNumber { get; set; }

        public BankAccountTypeDto AccountType { get; set; }

        public string Province { get; set; }

        public string Branch { get; set; }

        public string CurrencyCode { get; set; }

        public BankDto Bank { get; set; }

        public string SupplierName { get; set; }

        public string ContactNumber { get; set; }

        public string USBCode { get; set; }

        public DateTime? PurchasedDate { get; set; }

        public DateTime? UtilizationDate { get; set; }

        public DateTime? ExpirationDate { get; set; }

        public Guid? IdFrontImage { get; set; }

        public Guid? IdBackImage { get; set; }

        public Guid? ATMCardImage { get; set; }

        public string Remarks { get; set; }

        public BankAccountStatus Status { get; set; }

        public string CreatedBy { get; set; }

        public DateTimeOffset Created { get; set; }

        public string UpdatedBy { get; set; }

        public DateTimeOffset? Updated { get; set; }

        public bool InternetSameBank { get; set; }

        public bool AtmSameBank { get; set; }

        public bool CounterDepositSameBank { get; set; }

        public bool InternetDifferentBank { get; set; }

        public bool AtmDifferentBank { get; set; }

        public bool CounterDepositDifferentBank { get; set; }
    }

    public class BankAccountTypeDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }

    public class BrandDto
    {
        public string Name { get; set; }
        public string LicenseeName { get; set; }
    }

    public enum BankAccountStatus
    {
        Pending,
        Verified,
        Rejected,
        Active
    }
    #endregion

    #region Request/Response
    public class AddBankAccountRequest
    {
        public Guid? Id { get; set; }
        public Guid Bank { get; set; }
        public string BrandId { get; set; }
        public string LicenseeId { get; set; }
        public string Currency { get; set; }
        public string AccountId { get; set; }
        public string AccountName { get; set; }
        public string AccountNumber { get; set; }
        public Guid AccountType { get; set; }
        public string Province { get; set; }
        public string Branch { get; set; }
        public string Remarks { get; set; }

        public string SupplierName { get; set; }
        public string ContactNumber { get; set; }
        public string USBCode { get; set; }

        public string PurchasedDate { get; set; }
        public string UtilizationDate { get; set; }
        public string ExpirationDate { get; set; }

        public string IdFrontImage { get; set; }
        public string IdBackImage { get; set; }
        public string AtmCardImage { get; set; }

        public byte[] IdFrontImageFile { get; set; }
        public byte[] IdBackImageFile { get; set; }
        public byte[] AtmCardImageFile { get; set; }
    }

    public class SaveBankAccountResponse : ValidationResponseBase
    {
        public Guid Id { get; set; }
    }

    public class GetBankAccountByIdResponse
    {
        public BankAccountDto BankAccount { get; set; }
    }

    public class EditBankAccountRequest
    {
        public Guid Id { get; set; }
        public Guid BrandId { get; set; }
        public Guid Bank { get; set; }
        public string Currency { get; set; }
        public string AccountId { get; set; }
        public string AccountName { get; set; }
        public string AccountNumber { get; set; }
        public Guid AccountType { get; set; }
        public string Province { get; set; }
        public string Branch { get; set; }
        public string Remarks { get; set; }

        public string SupplierName { get; set; }
        public string ContactNumber { get; set; }
        public string USBCode { get; set; }

        public string PurchasedDate { get; set; }
        public string UtilizationDate { get; set; }
        public string ExpirationDate { get; set; }

        public string IdFrontImage { get; set; }
        public string IdBackImage { get; set; }
        public string AtmCardImage { get; set; }


        public byte[] IdFrontImageFile { get; set; }
        public byte[] IdBackImageFile { get; set; }
        public byte[] AtmCardImageFile { get; set; }
    }

    public class GetBankAccountCurrencyResponse
    {
        public IEnumerable<string> Currency { get; set; }
    }

    public class GetBankAccountTypesResponse
    {
        public IEnumerable<BankAccountTypeDto> BankAccountTypes { get; set; }
    }

    public class GetBankListByBrandIdResponse
    {
        public IEnumerable<BankDto> Banks { get; set; }
    }

    public class ActivateBankAccountRequest
    {
        public Guid Id { get; set; }
        public string Remarks { get; set; }
    }

    public class ActivateBankAccountResponse : ValidationResponseBase
    {
    }

    public class DeactivateBankAccountRequest
    {
        public Guid Id { get; set; }
        public string Remarks { get; set; }
    }    

    public class DeactivateBankAccountResponse : ValidationResponseBase
    {
    }

    public class GetBankAccountsByPlayerIdRequest
    {
        public Guid PlayerId { get; set; }
    }

    public class GetBankAccountsByPlayerIdResponse
    {
        public IEnumerable<BankAccountDto> BankAccounts { get; set; }
    }
    #endregion    
}
