using System;

namespace AFT.RegoV2.MemberApi.Interface.Common
{
    public class OfflineDepositConfirmRequest
    {
        public OfflineDepositConfirm DepositConfirm { get; set; }
        public byte[] IdFrontImage { get; set; }
        public byte[] IdBackImage { get; set; }
        public byte[] ReceiptImage { get; set; }
    }

    public class OfflineDepositConfirmResponse
    {
        public string UriToConfirmedDeposit { get; set; }
    }

    public class OfflineDepositConfirm
    {
        public Guid Id { get; set; }
        public string PlayerAccountName { get; set; }
        public string PlayerAccountNumber { get; set; }
        public string ReferenceNumber { get; set; }
        public decimal Amount { get; set; }
        public Guid BankId { get; set; }
        public TransferType TransferType { get; set; }
        public DepositMethod OfflineDepositType { get; set; }
        public string IdFrontImage { get; set; }
        public string IdBackImage { get; set; }
        public string ReceiptImage { get; set; }
        public string Remark { get; set; }
    }

    public enum TransferType
    {
        SameBank,
        DifferentBank
    }

    public enum DepositMethod
    {
        InternetBanking,
        ATM,
        CounterDeposit
    }
}
