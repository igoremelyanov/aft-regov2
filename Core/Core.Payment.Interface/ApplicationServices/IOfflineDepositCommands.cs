using System;
using System.Threading.Tasks;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Payment.Interface.Data;
using AFT.RegoV2.Core.Payment.Interface.Data.Commands;

namespace AFT.RegoV2.Core.Payment.Interface.ApplicationServices
{
    public interface IOfflineDepositCommands
    {
        Task<OfflineDeposit> Submit(OfflineDepositRequest request);
     
        OfflineDeposit Confirm(
            OfflineDepositConfirm depositConfirm,
            string confirmedBy,
            byte[] idFrontImage,
            byte[] idBackImage,
            byte[] receiptImage);

        void Verify(OfflineDepositId id, Guid bankAccountId, string remark);

        void Unverify(OfflineDepositId id, string remark, UnverifyReasons unverifyReason);

        void Approve(OfflineDepositApprove approveCommand);

        void Reject(OfflineDepositId id, string remark);
    }
}
