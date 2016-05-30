using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using AFT.RegoV2.Core.Common.Data.Payment;

namespace AFT.RegoV2.Core.Payment.Data
{
    public class Currency
    {
        [Key, Required, StringLength(3)]
        public string Code { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }

        [DefaultValue(CurrencyStatus.Active)]
        public CurrencyStatus Status { get; set; }

        public string CreatedBy { get; set; }
        public DateTimeOffset DateCreated { get; set; }
        public string UpdatedBy { get; set; }
        public DateTimeOffset? DateUpdated { get; set; }

        public string ActivatedBy { get; set; }
        public DateTimeOffset? DateActivated { get; set; }

        public string DeactivatedBy { get; set; }
        public DateTimeOffset? DateDeactivated { get; set; }

        public string Remarks { get; set; }
    }
}
