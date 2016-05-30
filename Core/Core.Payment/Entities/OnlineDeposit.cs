using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Payment;
using AFT.RegoV2.Core.Payment.Interface.Commands;
using AFT.RegoV2.Core.Payment.Interface.Data;
using AFT.RegoV2.Core.Payment.Interface.Events;
using AFT.RegoV2.Shared;

namespace AFT.RegoV2.Core.Payment.Entities
{
    /// <summary>
    /// </summary>
    public class OnlineDeposit
    {
        public Data.OnlineDeposit Data { get; private set; }

        public OnlineDeposit(Guid id,
            string transactionNumber,
            OnlineDepositRequest request,
            OnlineDepositParams depositParams,
            Guid brandId,
            DateTimeOffset now)
        {
            if (request.Amount <= 0)
            {
                throw new ArgumentException("Amount must be greater than 0.");
            }
            Data = new Data.OnlineDeposit
            {
                Id = id,
                BrandId = brandId,
                PlayerId = request.PlayerId,
                TransactionNumber = transactionNumber,
                Amount = request.Amount,                
                CreatedBy = request.RequestedBy,
                Created = now,
                Status = OnlineDepositStatus.New,
                Method = depositParams.Method,
                Channel = depositParams.Channel,
                MerchantId = depositParams.MerchantId,
                Currency = depositParams.Currency,
                Language = depositParams.Language,
                ReturnUrl = depositParams.ReturnUrl,
                NotifyUrl = depositParams.NotifyUrl,
                BonusCode = request.BonusCode,
                BonusId = request.BonusId
            };            
        }

        public OnlineDeposit(Data.OnlineDeposit onlineDeposit)
        {
            Data = onlineDeposit;
        }

        public DepositSubmitted Submit()
        {            
            ChangeState(OnlineDepositStatus.Processing);

            return new DepositSubmitted
            {
                DepositId = Data.Id,
                PlayerId = Data.PlayerId,
                TransactionNumber = Data.TransactionNumber,
                Amount = Data.Amount,
                Submitted = Data.Created,
                SubmittedBy = Data.CreatedBy,
                CurrencyCode = Data.Currency,
                PaymentMethod = Data.Method,
                Status = Data.Status.ToString(),
                Remarks = Data.Remarks,
                DepositType = DepositType.Online,
                BonusCode = Data.BonusCode,
                BonusId = Data.BonusId
            };
        }

        public Deposit Approve(OnlineDepositPayNotifyRequest notifyData, DateTimeOffset now, string approvedBy = "")
        {
            Data.Approved = now;
            Data.ApprovedBy = string.IsNullOrEmpty(approvedBy) ? Data.CreatedBy : approvedBy;
            Data.OrderIdOfGateway = notifyData.OrderIdOfGateway;
            Data.OrderIdOfRouter = notifyData.OrderIdOfRouter;
            ChangeState(OnlineDepositStatus.Approved);

            return new Deposit
            {
                ActorName = "System",
                DepositId = Data.Id,
                PlayerId = Data.PlayerId,
                CurrencyCode = Data.Currency,
                ReferenceCode = Data.TransactionNumber,
                Amount = Data.Amount,
                Remarks = Data.Remarks,
                DepositType = DepositType.Online,
                Approved = Data.Approved.Value,
                ApprovedBy = Data.ApprovedBy
            };
        }

        public Deposit Approve(string approveBy, string remarks, DateTimeOffset now)
        {
            Data.Approved = now;
            Data.ApprovedBy = approveBy;
            SetRemarks(remarks);
            ChangeState(OnlineDepositStatus.Approved);

            return new Deposit
            {
                ActorName = "System",
                DepositId = Data.Id,
                ReferenceCode = Data.TransactionNumber,
                PlayerId = Data.PlayerId,
                CurrencyCode = Data.Currency,
                Amount = Data.Amount,
                Remarks = Data.Remarks,
                DepositType = DepositType.Online,
                Approved = Data.Approved.Value,
                ApprovedBy = Data.ApprovedBy
            };
        }
        public DepositVerified Verify(string verifiedBy, string remarks, DateTimeOffset now)
        {
            Data.Verified = now;
            Data.VerifiedBy = verifiedBy;
            SetRemarks(remarks);
            ChangeState(OnlineDepositStatus.Verified);
            return new DepositVerified
            {
                DepositId = Data.Id,
                DepositType = DepositType.Online,
                PlayerId = Data.PlayerId,
                Verified = Data.Verified.Value,
                VerifiedBy = Data.VerifiedBy,
                ReferenceCode = Data.TransactionNumber,
                Remarks = Data.Remarks
            };
        }

        public DepositUnverified Unverify(string unverifiedBy, string remarks, DateTimeOffset now, UnverifyReasons reason)
        {
            Data.Unverified = now;
            Data.UnverifiedBy = unverifiedBy;
            SetRemarks(remarks);
            ChangeState(OnlineDepositStatus.Unverified);
            return new DepositUnverified()
            {
                DepositId = Data.Id,
                DepositType = DepositType.Online,
                PlayerId = Data.PlayerId,
                UnverifyReason = reason,
                Status = OfflineDepositStatus.Unverified,
                Unverified = Data.Unverified.Value,
                UnverifiedBy = Data.UnverifiedBy               
            };
        }

        public DepositRejected Reject(string rejectedBy, string remarks, DateTimeOffset now)
        {
            Data.Rejected = now;
            Data.RejectedBy = rejectedBy;
            SetRemarks(remarks);
            ChangeState(OnlineDepositStatus.Rejected);
            return new DepositRejected
            {
                DepositType = DepositType.Online,
                DepositId = Data.Id,
                PlayerId = Data.PlayerId,
                ReferenceCode = Data.TransactionNumber,
                Rejected = Data.Rejected.Value,
                RejectedBy = rejectedBy,
                Remarks = Data.Remarks
            };
        }

        public bool IsApproved()
        {
            return Data.Status == OnlineDepositStatus.Approved;
        }

        private void ChangeState(OnlineDepositStatus newStatus)
        {
            var allowed = _statesMap[Data.Status].Contains(newStatus);
            if (allowed == false)
            {
                throw new RegoException(
                    string.Format("The deposit has '{0}' status, so it can't be {1}", Data.Status, newStatus));
            }

            Data.Status = newStatus;
        }

        private void SetRemarks(string remarks = "", bool isRemarkRequired = true)
        {
            if (isRemarkRequired && string.IsNullOrWhiteSpace(remarks))
            {
                throw new ArgumentException(@"Remarks is required", "remarks");
            }
            Data.Remarks = remarks;
        }

        private readonly Dictionary<OnlineDepositStatus, IEnumerable<OnlineDepositStatus>> _statesMap
         = new Dictionary<OnlineDepositStatus, IEnumerable<OnlineDepositStatus>>
        {
            { OnlineDepositStatus.New, new[] { OnlineDepositStatus.New, OnlineDepositStatus.Processing} },
            { OnlineDepositStatus.Processing, new[] {OnlineDepositStatus.Approved, OnlineDepositStatus.Verified, OnlineDepositStatus.Unverified, OnlineDepositStatus.Rejected } },
            { OnlineDepositStatus.Verified, new[] { OnlineDepositStatus.Rejected, OnlineDepositStatus.Approved } },
            { OnlineDepositStatus.Unverified, Enumerable.Empty<OnlineDepositStatus>() },
            { OnlineDepositStatus.Rejected, Enumerable.Empty<OnlineDepositStatus>() },
            { OnlineDepositStatus.Approved, Enumerable.Empty<OnlineDepositStatus>() },
        };

        public void SetBonusRedemption(Guid bonusRedemptionId)
        {
            Data.BonusRedemptionId = bonusRedemptionId;
        }
    }
}