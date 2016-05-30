using System;
using AFT.RegoV2.Core.Common.Data.Payment;
using AFT.RegoV2.Core.Common.Data.Player;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Player.Data;
using AutoMapper;
using TransactionType = AFT.RegoV2.Core.Common.Data.Player.TransactionType;

namespace AFT.RegoV2.Core.Player.Events
{
    public class IdentificationDocumentSettingsCreated : DomainEventBase
    {
        public Guid Id { get; set; }
        public Guid LicenseeId { get; set; }
        public Guid BrandId { get; set; }
        public TransactionType? TransactionType { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }
        public bool IdFront { get; set; }
        public bool IdBack { get; set; }
        public bool CreditCardFront { get; set; }
        public bool CreditCardBack { get; set; }
        public bool POA { get; set; }
        public bool DCF { get; set; }
        public string CreatedBy { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
        public string UpdatedBy { get; set; }
        public DateTimeOffset? UpdatedOn { get; set; }

        public IdentificationDocumentSettingsCreated()
        {
        } // default constructor is required for publishing event to MQ

        public IdentificationDocumentSettingsCreated(IdentificationDocumentSettings setting)
        {
            Mapper.DynamicMap(setting, this);
        }
    }
}