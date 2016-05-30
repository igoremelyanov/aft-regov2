using System.Threading.Tasks;
using AFT.RegoV2.Core.Payment.Interface.Data;

namespace AFT.RegoV2.Core.Payment.Interface.ApplicationServices
{
    public interface IOnlineDepositCommands
    {
        Task<SubmitOnlineDepositRequestResult> SubmitOnlineDepositRequest(OnlineDepositRequest request);

        void ValidateOnlineDepositAmount(ValidateOnlineDepositAmountRequest amount);

        string PayNotify(OnlineDepositPayNotifyRequest request);

        void Verify(VerifyOnlineDepositRequest request);

        void Unverify(UnverifyOnlineDepositRequest request);

        void Approve(ApproveOnlineDepositRequest request);

        void Reject(RejectOnlineDepositRequest request);
    }
}