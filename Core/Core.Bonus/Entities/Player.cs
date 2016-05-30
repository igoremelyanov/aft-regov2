using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Bonus.Core.Data;
using AFT.RegoV2.Bonus.Core.DomainServices;
using AFT.RegoV2.Bonus.Core.Models.Enums;
using AFT.RegoV2.Core.Common.Utils;

namespace AFT.RegoV2.Bonus.Core.Entities
{
    public class Player
    {
        public Data.Player Data { get; }

        internal BonusType DepositQuailifiedBonusType
        {
            get
            {
                var depositCount = Data.Wallets.SelectMany(w => w.Transactions).Count(t => t.Type == TransactionType.Deposit);
                return depositCount == 0
                    ? BonusType.FirstDeposit
                    : BonusType.ReloadDeposit;
            }
        }

        public List<Data.BonusRedemption> BonusesRedeemed => Data.Wallets.SelectMany(w => w.BonusesRedeemed).ToList();

        public decimal MonthlyAccumulatedDepositAmount
        {
            get
            {
                var now = SystemTime.Now.ToBrandOffset(Data.Brand.TimezoneId);
                return Data.Wallets
                .SelectMany(w => w.Transactions)
                .Where(t => t.Type == TransactionType.Deposit)
                .Where(t => t.CreatedOn.Month == now.Month && t.CreatedOn.Year == now.Year)
                .Sum(t => t.TotalAmount);
            }
        }

        public Player(Data.Player data)
        {
            Data = data;
        }

        public BonusRedemption Redeem(Bonus bonus, RedemptionParams redemptionParams)
        {
            var redemption = new BonusRedemption(this, bonus, redemptionParams);

            Data
                .Wallets
                .Single(w => w.Template.Id == bonus.Data.Template.Info.WalletTemplateId)
                .BonusesRedeemed
                .Add(redemption.Data);

            return redemption;
        }

        public List<Data.BonusRedemption> GetClaimableRedemptions()
        {
            if (Data.IsFraudulent)
                return new List<Data.BonusRedemption>();

            return BonusesRedeemed
                .Where(r => r.ActivationState == ActivationStatus.Claimable)
                .ToList();
        }

        public void VerifyMobileNumber()
        {
            Data.IsMobileVerified = true;
        }

        public void VerifyEmailAddress()
        {
            Data.IsEmailVerified = true;
        }

        public bool CompletedReferralRequirements()
        {
            var referFriendBonus = Data.ReferredWith;

            if (referFriendBonus == null)
                return false;

            var firstDepositAmount =
                Data.Wallets.SelectMany(w => w.Transactions)
                    .Where(t => t.Type == TransactionType.Deposit)
                    .OrderBy(t => t.CreatedOn)
                    .First()
                    .TotalAmount;
            var requiredRollover = firstDepositAmount * referFriendBonus.Template.Rules.ReferFriendWageringCondition;
            return firstDepositAmount >= referFriendBonus.Template.Rules.ReferFriendMinDepositAmount &&
                   Data.AccumulatedWageringAmount >= requiredRollover;
        }

        public void CompleteReferralRequirements()
        {
            Data.ReferredWith = null;
        }

        public List<BonusRedemption> GetRedemptionsWithActiveRollover(Guid? walletStructureId = null)
        {
            return Data
                .Wallets
                .Single(w => walletStructureId != null
                    ? w.Template.Id == walletStructureId
                    : w.Template.IsMain)
                .BonusesRedeemed
                            .Where(redemption => redemption.RolloverState == RolloverStatus.Active)
                            .OrderByDescending(redemption => redemption.Contributions.Count)
                            .ThenBy(redemption => redemption.CreatedOn)
                            .Select(br => new BonusRedemption(br))
                            .ToList();
        }

        public List<BonusRedemption> GetCompletedRedemptions(Guid? walletStructureId = null)
        {
            return Data
                .Wallets
                .Single(w => walletStructureId != null
                    ? w.Template.Id == walletStructureId
                    : w.Template.IsMain)
                .BonusesRedeemed
                            .Select(br => new BonusRedemption(br))
                            .Where(redemption => redemption.Data.ActivationState == ActivationStatus.Activated
                                || (redemption.Data.ActivationState == ActivationStatus.Claimable && redemption.IsExpired))
                            .OrderBy(redemption => redemption.Data.CreatedOn)
                            .ToList();
        }

        public void DisableBonuses()
        {
            Data.IsFraudulent = true;
        }
    }
}