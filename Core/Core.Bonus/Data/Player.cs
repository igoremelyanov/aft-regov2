using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Core.Common.Events;
using AFT.RegoV2.Core.Common.Utils;

namespace AFT.RegoV2.Bonus.Core.Data
{
    public class Player : Identity
    {
        public Player() { }

        public Player(PlayerRegistered registrationData, Brand brand)
        {
            Id = registrationData.PlayerId;
            Name = registrationData.UserName;
            Email = registrationData.Email;
            PhoneNumber = registrationData.PhoneNumber;
            VipLevel = registrationData.VipLevel;
            CurrencyCode = registrationData.CurrencyCode;
            DateRegistered = registrationData.DateRegistered.ToBrandOffset(brand.TimezoneId);
            ReferralId = registrationData.RefIdentifier;
            Brand = brand;
            Wallets = brand.WalletTemplates.Select(wt => new Wallet { Player = this, Template = wt}).ToList();
            RiskLevels = new List<RiskLevel>();
        }

        public string Name { get; set; }
        public string VipLevel { get; set; }
        public string CurrencyCode { get; set; }
        public DateTimeOffset DateRegistered { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        /// <summary>
        /// Player's referral identifier
        /// </summary>
        public Guid ReferralId { get; set; }
        /// <summary>
        /// Id of a player that referred THIS player
        /// </summary>
        public Guid? ReferredBy { get; set; }
        /// <summary>
        /// Bonus, that was used during referral process
        /// </summary>
        public virtual Bonus ReferredWith { get; set; }
        public decimal AccumulatedWageringAmount { get; set; }
        public bool IsMobileVerified { get; set; }
        public bool IsEmailVerified { get; set; }
        /// <summary>
        /// Player can not redeem, claim, view bonuses
        /// </summary>
        public bool IsFraudulent { get; set; }

        public virtual List<Wallet> Wallets { get; set; }
        public virtual List<RiskLevel> RiskLevels { get; set; }
        public virtual Brand Brand { get; set; }
    }
}