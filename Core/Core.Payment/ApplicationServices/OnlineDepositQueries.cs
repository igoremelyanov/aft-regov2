﻿using System;
﻿using System.Collections.Generic;
﻿using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.Core.Common.Data.Payment;
using AFT.RegoV2.Core.Payment.Entities;
using AFT.RegoV2.Core.Payment.Interface.ApplicationServices;
using AFT.RegoV2.Core.Payment.Interface.Data;

namespace AFT.RegoV2.Core.Payment.ApplicationServices
{
    public class OnlineDepositQueries : IOnlineDepositQueries
    {
        private readonly IPaymentRepository _repository;

        public OnlineDepositQueries(
            IPaymentRepository repository)
        {
            _repository = repository;
        }

        public CheckStatusResponse CheckStatus(CheckStatusRequest request)
        {
            var depositStatus = new CheckStatusResponse
            {
                IsPaid = false
            };
            var onlineDeposit = _repository.OnlineDeposits.FirstOrDefault(x => x.TransactionNumber == request.TransactionNumber);
            if (onlineDeposit != null)
            {
                var onlineDepositEntity = new OnlineDeposit(onlineDeposit);
                if (onlineDepositEntity.IsApproved())
                {
                    depositStatus.IsPaid = true;
                    depositStatus.Amount = onlineDeposit.Amount;
                    depositStatus.Bonus = 0;//TODO:ONLINEDEPOSIT how to get Bonus
                    depositStatus.TotalAmount = depositStatus.Amount + depositStatus.Bonus;
                }
            }
            return depositStatus;
        }

        public DepositDto GetOnlineDepositById(Guid id)
        {
            var deposit = _repository.OnlineDeposits
                .Include(x => x.Brand)
                .Include(x => x.Player)
                .Select(obj => new DepositDto
                {
                    Id = obj.Id,
                    Licensee = obj.Brand.LicenseeName,
                    BrandId = obj.BrandId,
                    BrandName = obj.Brand.Name,
                    Username = obj.Player.Username,
                    PlayerId = obj.PlayerId,
                    FirstName = obj.Player.FirstName,
                    LastName = obj.Player.LastName,
                    ReferenceCode = obj.TransactionNumber,
                    PaymentMethod = obj.Method,
                    CurrencyCode = obj.Currency,
                    Amount = obj.Amount,
                    Status = obj.Status.ToString(),
                    DateSubmitted = obj.Created,
                    SubmittedBy = obj.CreatedBy,
                    DateApproved = obj.Approved,
                    ApprovedBy = obj.ApprovedBy,
                    DateVerified = obj.Verified,
                    VerifiedBy = obj.VerifiedBy,
                    DateRejected = obj.Rejected,
                    RejectedBy = obj.RejectedBy,
                    DepositType = DepositType.Online
                })
                  .SingleOrDefault(x => x.Id == id);
            if (deposit == null)
            {
                throw new ArgumentException(string.Format("OnlineDeposit with Id {0} was not found", id));
            }
            return deposit;
        }

        public IEnumerable<DepositDto> GetOnlineDepositsByPlayerId(Guid playerId)
        {
            var deposits = _repository.OnlineDeposits
                .Include(x => x.Brand)
                .Include(x => x.Player)
                .Where(x => x.PlayerId == playerId)
                .Select(obj => new DepositDto
                {
                    Id = obj.Id,
                    Licensee = obj.Brand.LicenseeName,
                    BrandId = obj.BrandId,
                    BrandName = obj.Brand.Name,
                    Username = obj.Player.Username,
                    PlayerId = obj.PlayerId,
                    FirstName = obj.Player.FirstName,
                    LastName = obj.Player.LastName,
                    ReferenceCode = obj.TransactionNumber,
                    PaymentMethod = obj.Method,
                    CurrencyCode = obj.Currency,
                    Amount = obj.Amount,
                    Status = obj.Status.ToString(),
                    DateSubmitted = obj.Created,
                    SubmittedBy = obj.CreatedBy,
                    DateApproved = obj.Approved,
                    ApprovedBy = obj.ApprovedBy,
                    DateVerified = obj.Verified,
                    VerifiedBy = obj.VerifiedBy,
                    DateRejected = obj.Rejected,
                    RejectedBy = obj.RejectedBy,
                    DepositType = DepositType.Online,
                    BonusCode = obj.BonusCode,
                    BonusId = obj.BonusId,
                    BonusRedemptionId = obj.BonusRedemptionId
                });
            
            return deposits;
        }
    }
}