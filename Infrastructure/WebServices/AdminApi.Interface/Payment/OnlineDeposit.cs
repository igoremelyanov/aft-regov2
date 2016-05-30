using System;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Base;
namespace AFT.RegoV2.AdminApi.Interface.Payment
{
    public class VerifyOnlineDepositRequest
    {
        public Guid Id { get; set; }
        public string Remarks { get; set; }
    }

    public class UnverifyOnlineDepositRequest
    {
        public Guid Id { get; set; }
        public string Remarks { get; set; }
        public UnverifyReasons UnverifyReason { get; set; }
    }

    public class VerifyOnlineDepositResponse : ValidationResponseBase
    {
    }

    public class UnverifyOnlineDepositResponse : ValidationResponseBase
    {
    }

    public class ApproveOnlineDepositRequest
    {
        public Guid Id { get; set; }
        public string Remarks { get; set; }
    }

    public class ApproveOnlineDepositResponse : ValidationResponseBase
    {
    }

    public class RejectOnlineDepositRequest
    {
        public Guid Id { get; set; }
        public string Remarks { get; set; }
    }

    public class RejectOnlineDepositResponse : ValidationResponseBase
    {
    }

    public class OnlineDepositViewDataResponse
    {
        public Guid Id { get; set; }

        public string Licensee { get; set; }

        public Guid BrandId { get; set; }

        public string BrandName { get; set; }

        public string Username { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string ReferenceCode { get; set; }

        public string PaymentMethod { get; set; }

        public string CurrencyCode { get; set; }

        public decimal Amount { get; set; }

        public decimal UniqueDepositAmount { get; set; }

        public string Status { get; set; }

        public string DateSubmitted { get; set; }

        public string SubmittedBy { get; set; }

        public string DateVerified { get; set; }

        public string VerifiedBy { get; set; }

        public string DateRejected { get; set; }

        public string RejectedBy { get; set; }
        
        public string DepositType { get; set; }

        public string PlayerName { get; set; }
    }
}
