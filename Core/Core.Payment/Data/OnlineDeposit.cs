using System;
using System.ComponentModel.DataAnnotations;
using AFT.RegoV2.Core.Payment.Interface.Data;

namespace AFT.RegoV2.Core.Payment.Data
{
    public class OnlineDeposit
    {
        public Guid Id { get; set; }

        public Guid BrandId { get; set; }
        public RegoV2.Core.Payment.Data.Brand Brand { get; set; }

        public Guid PlayerId { get; set; }
        public Player Player { get; set; }

        public string TransactionNumber { get; set; }

        public decimal Amount { get; set; }

        public DateTimeOffset Created { get; set; }

        public string CreatedBy { get; set; }

        public DateTimeOffset? Verified { get; set; }

        public string VerifiedBy { get; set; }

        public DateTimeOffset? Unverified { get; set; }

        public string UnverifiedBy { get; set; }

        public DateTimeOffset? Approved { get; set; }

        public string ApprovedBy { get; set; }

        public DateTimeOffset? Rejected { get; set; }

        public string RejectedBy { get; set; }

        public OnlineDepositStatus Status { get; set; }

        public string BonusCode { get; set; }

        public Guid? BonusId { get; set; }

        public Guid? BonusRedemptionId { get; set; }

        [MaxLength(200)]
        public string Remarks { get; set; }

        #region Payment Proxy Parameter
        /// <summary>
        /// Payment Gateway
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// Payment Gateway's Channel
        /// </summary>
        public int Channel { get; set; }

        /// <summary>        
        /// Brand Id in Payment Proxy
        /// </summary>
        public string MerchantId { get; set; }                

        /// <summary>
        /// Currency
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Language
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// Use to show depoist complated page
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        /// Use to receive depoist complated notify from payment proxy
        /// </summary>
        public string NotifyUrl { get; set; }
        #endregion

        #region Return Data from Payment Proxy
        /// <summary>
        /// ref no of Payment Proxy
        /// </summary>
        public string OrderIdOfRouter { get; set; }

        /// <summary>
        /// ref no of Payment Gateway
        /// </summary>
        public string OrderIdOfGateway { get; set; }
        #endregion 
    }
}