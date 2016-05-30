using System;
using AFT.RegoV2.Core.Common.Data.Base;

namespace AFT.RegoV2.AdminApi.Interface.Payment
{
    #region Dto
    public class BankDto
    {
        public Guid Id { get; set; }
        public Guid BrandId { get; set; }
        public string BrandName { get; set; }
        public Guid LicenseeId { get; set; }
        public string LicenseeName { get; set; }
        public string BankId { get; set; }
        public string BankName { get; set; }
        public string CountryName { get; set; }
        public string Remarks { get; set; }
    }
    #endregion 

    #region Request/Response
    public class AddBankRequest
    {
        public Guid BrandId { get; set; }
        public string BankId { get; set; }
        public string BankName { get; set; }
        public string CountryCode { get; set; }
        public string Remarks { get; set; }
    }
    public class SaveBankResponse : ValidationResponseBase
    {
        public Guid Id { get; set; }
    }

    public class GetBankByIdResponse
    {
        public BankDto Bank { get; set; }
    }

    public class EditBankRequest
    {
        public Guid Id { get; set; }
        public Guid BrandId { get; set; }
        public string BankName { get; set; }
        public string CountryCode { get; set; }
        public string Remarks { get; set; }
    }
    #endregion
}
