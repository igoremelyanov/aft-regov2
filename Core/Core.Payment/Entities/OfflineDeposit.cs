using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Payment;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Payment.Interface.Data;
using AFT.RegoV2.Core.Payment.Interface.Data.Commands;
using AFT.RegoV2.Core.Payment.Interface.Events;
using Deposit = AFT.RegoV2.Core.Payment.Interface.Commands.Deposit;
namespace AFT.RegoV2.Core.Payment.Entities
{
    /// <summary>
    /// OfflineDepositData has DepositType which says "Online" and "Offline".
    /// Therefore, name of the DTO as well as name of the Entity is inconsistent and should be changed to 'Deposit': Entities.Deposit and Data.Deposit
    /// </summary>
    public class OfflineDeposit
    {
        public OfflineDeposit(
            OfflineDepositRequest request,
            Data.BankAccount bankAccount,
            AFT.RegoV2.Core.Payment.Data.Player player,
            string createdBy)
        {
            if (request.Amount <= 0)
            {
                throw new ArgumentException("Amount must be greater than 0.");
            }
            var random = new Random();
            var transactionNumber = "OD" + random.Next(10000000, 99999999);

            Data = new Data.OfflineDeposit
            {
                Id = Guid.NewGuid(),
                BrandId = player.BrandId,
                TransactionNumber = transactionNumber,
                Amount = request.Amount,
                PlayerId = player.Id,
                Player = player,
                Created = DateTimeOffset.Now.ToBrandOffset(player.Brand.TimezoneId),
                BankAccountId = bankAccount.Id,
                CurrencyCode = bankAccount.CurrencyCode,
                Status = OfflineDepositStatus.New,
                PaymentMethod = PaymentMethod.OfflineBank,
                DepositType = DepositType.Offline,
                BankAccount = bankAccount,
                DepositWagering = request.Amount,
                PlayerRemark = request.PlayerRemark,
                CreatedBy = createdBy
            };
        }

        public OfflineDeposit(Data.OfflineDeposit offlineDeposit)
        {
            Data = offlineDeposit;
            if (Data.Player == null)
                throw new NullReferenceException("Player is not loaded");
        }

        public Data.OfflineDeposit Data { get; private set; }

        public DepositSubmitted Submit()
        {
            var depositSubmitted = new DepositSubmitted
            {
                DepositId = Data.Id,
                PlayerId = Data.PlayerId,
                TransactionNumber = Data.TransactionNumber,
                Amount = Data.Amount,
                Submitted = Data.Created,
                SubmittedBy = Data.CreatedBy,
                CurrencyCode = Data.CurrencyCode,
                PaymentMethod = PaymentMethodDto.OfflinePayMethod,
                DepositType = Data.DepositType,
                Status = Data.Status.ToString(),
                Remarks = Data.Remark,
                EventCreated = Data.Created,
            };

            if (Data.BankAccount != null)
            {
                var bankAccount = Data.BankAccount;
                depositSubmitted.BankAccountName = bankAccount.AccountName;
                depositSubmitted.BankAccountNumber = bankAccount.AccountNumber;
                depositSubmitted.BankAccountId = bankAccount.AccountId;
                depositSubmitted.BankProvince = bankAccount.Province;
                depositSubmitted.BankBranch = bankAccount.Branch;
                depositSubmitted.BankName = bankAccount.Bank != null ? bankAccount.Bank.BankName : null;
                depositSubmitted.BankAccount = Data.BankAccount.Id;
            }

            return depositSubmitted;
        }

        public DepositConfirmed Confirm(
            string playerAccountName,
            string playerAccountNumber,
            string referenceNumber,
            decimal amount,
            TransferType transferType,
            DepositMethod depositMethod,
            string remark,
            string confirmedBy,
            Guid? idFrontImage = null,
            Guid? idBackImage = null,
            Guid? receiptImage = null)
        {
            Data.Confirmed = DateTimeOffset.Now.ToBrandOffset(Data.Player.Brand.TimezoneId);
            Data.ConfirmedBy = confirmedBy;
            Data.PlayerAccountName = playerAccountName;
            Data.PlayerAccountNumber = playerAccountNumber;
            Data.BankReferenceNumber = referenceNumber;
            Data.Amount = amount;
            Data.TransferType = transferType;
            Data.DepositMethod = depositMethod;
            Data.IdFrontImage = idFrontImage;
            Data.IdBackImage = idBackImage;
            Data.ReceiptImage = receiptImage;
            SetRemark(remark, false);
            ChangeState(OfflineDepositStatus.Processing);
            return new DepositConfirmed
            {
                DepositId = Data.Id,
                PlayerId = Data.PlayerId,
                Amount = Data.Amount,
                Remarks = Data.Remark,
                DepositType = Data.DepositType
            };
        }
        
        public DepositVerified Verify(string verifiedBy, string remark)
        {
            DateTimeOffset.Now.ToBrandOffset(Data.Player.Brand.TimezoneId);
            Data.VerifiedBy = verifiedBy;
            SetRemark(remark, isRemarkRequired: false);
            ChangeState(OfflineDepositStatus.Verified);
            return new DepositVerified
            {
                DepositId = Data.Id,
                PlayerId = Data.PlayerId,
                Verified = Data.Verified ?? DateTimeOffset.Now.ToBrandOffset(Data.Player.Brand.TimezoneId),
                VerifiedBy = Data.VerifiedBy,
                Remarks = Data.Remark,
                DepositType = Data.DepositType,
                ReferenceCode = Data.TransactionNumber
            };
        }

        public DepositUnverified Unverify(string unverifiedBy, string remark, UnverifyReasons unverifyReason)
        {
            SetRemark(remark);
            ChangeState(OfflineDepositStatus.Unverified);
            Data.UnverifyReason = unverifyReason;
            return new DepositUnverified
            {
                DepositId = Data.Id,
                PlayerId = Data.Player.Id,
                Status = Data.Status,
                Unverified = DateTimeOffset.Now.ToBrandOffset(Data.Player.Brand.TimezoneId),
                UnverifiedBy = unverifiedBy,
                Remarks = Data.Remark,
                UnverifyReason = unverifyReason
            };
        }

        public Deposit Approve(decimal actualAmount, decimal fee, string playerRemark, string approveBy, string remark)
        {
            Data.ActualAmount = actualAmount;
            Data.Fee = fee;
            Data.PlayerRemark = playerRemark;
            Data.Approved = DateTimeOffset.Now.ToBrandOffset(Data.Player.Brand.TimezoneId);
            Data.ApprovedBy = approveBy;
            SetRemark(remark);
            ChangeState(OfflineDepositStatus.Approved);
            return new Deposit
            {
                ActorName = approveBy,
                DepositId = Data.Id,
                PlayerId = Data.PlayerId,
                ReferenceCode = Data.TransactionNumber,
                CurrencyCode = Data.CurrencyCode,
                Amount = Data.ActualAmount,
                Fee = Data.Fee,
                Approved = Data.Approved.Value,
                ApprovedBy = Data.ApprovedBy,
                Remarks = Data.Remark,
                DepositWagering = Data.DepositWagering,
            };
        }

        public DepositRejected Reject(string rejectedBy, string remark)
        {
            SetRemark(remark);
            ChangeState(OfflineDepositStatus.Rejected);
            return new DepositRejected
            {
                DepositId = Data.Id,
                PlayerId = Data.Player.Id,
                Rejected = DateTimeOffset.Now.ToBrandOffset(Data.Player.Brand.TimezoneId),
                RejectedBy = rejectedBy,
                Remarks = Data.Remark,
                DepositType = Data.DepositType
            };
        }

        private void SetRemark(string remark = "", bool isRemarkRequired = true)
        {
            if (isRemarkRequired && string.IsNullOrWhiteSpace(remark))
            {
                throw new ArgumentException(@"Remark is required", "remark");
            }
            Data.Remark = remark;
        }

        private void ChangeState(OfflineDepositStatus newStatus)
        {
            var allowed = _statesMap[Data.Status].Contains(newStatus);
            if (allowed == false)
            {
                throw new InvalidOperationException(
                    string.Format("The deposit has \"{0}\" status, so it can't be {1}", Data.Status, newStatus));
            }

            Data.Status = newStatus;
        }

        private readonly Dictionary<OfflineDepositStatus, IEnumerable<OfflineDepositStatus>> _statesMap
            = new Dictionary<OfflineDepositStatus, IEnumerable<OfflineDepositStatus>>
        {
            { OfflineDepositStatus.New, new[] { OfflineDepositStatus.New, OfflineDepositStatus.Processing} },
            { OfflineDepositStatus.Processing, new[] { OfflineDepositStatus.Verified, OfflineDepositStatus.Unverified } },
            { OfflineDepositStatus.Verified, new[] { OfflineDepositStatus.Rejected, OfflineDepositStatus.Approved } },
            { OfflineDepositStatus.Unverified, new[] { OfflineDepositStatus.Processing} },
            { OfflineDepositStatus.Rejected, Enumerable.Empty<OfflineDepositStatus>() },
            { OfflineDepositStatus.Approved, Enumerable.Empty<OfflineDepositStatus>() },
        };

        public void SetBonusRedemption(Guid bonusRedemptionId)
        {
            Data.BonusRedemptionId = bonusRedemptionId;
        }

        public void ChangeBankAccount(Guid newBankAccountId)
        {
            Data.BankAccountId = newBankAccountId;
        }
    }
}