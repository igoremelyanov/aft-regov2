using System;
using System.Linq;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Payment;
using AFT.RegoV2.Core.Payment.Interface.Data;
using AFT.RegoV2.Core.Payment.Interface.Data.Commands;

namespace AFT.RegoV2.Core.Payment.Interface.ApplicationServices
{
    public interface IWithdrawalService
    {
        IQueryable<OfflineWithdraw> GetWithdrawalsForVerification();
        
        IQueryable<OfflineWithdraw> GetWithdrawalsForAcceptance();

        IQueryable<OfflineWithdraw> GetWithdrawalsForVerificationQueue();

        IQueryable<OfflineWithdraw> GetWithdrawalsForApproval();

        IQueryable<OfflineWithdraw> GetWithdrawalsCanceled();

        IQueryable<OfflineWithdraw> GetWithdrawalsFailedAutoWagerCheck();

        IQueryable<OfflineWithdraw> GetWithdrawalsOnHold();

        IQueryable<OfflineWithdraw> GetWithdrawalsVerified();

        IQueryable<OfflineWithdraw> GetWithdrawalsAccepted();

        IQueryable<OfflineWithdraw> GetWithdrawals();

        OfflineWithdrawResponse Request(OfflineWithdrawRequest request);

        OfflineWithdrawResponse WithdrawalRequestSubmit(OfflineWithdrawRequest request);

        void WithdrawalStateMachine(Guid id);        
        
        void Verify(OfflineWithdrawId id, string remarks);

        void SetDocumentsState(Guid requestId, string remarks);
        void SetInvestigateState(Guid requestId, string remarks);

        void Unverify(OfflineWithdrawId id, string remarks);

        void Approve(OfflineWithdrawId id, string remarks);
        
        void Reject(OfflineWithdrawId id, string remarks);

        void PassWager(OfflineWithdrawId id, string remarks);

        void FailWager(OfflineWithdrawId id, string remarks);

        void PassInvestigation(OfflineWithdrawId id, string remarks);

        void FailInvestigation(OfflineWithdrawId id, string remarks);
        
        void Accept(OfflineWithdrawId id, string remarks);

        void Revert(OfflineWithdrawId id, string remarks);

        void Cancel(OfflineWithdrawId id, string remarks);
        
        void SaveExemption(Exemption exemption);
    }
}
