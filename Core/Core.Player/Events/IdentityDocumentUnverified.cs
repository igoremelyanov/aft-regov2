using System;
using AFT.RegoV2.Core.Common.Data.Player;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Player.Events
{
    public class IdentityDocumentUnverified : DomainEventBase
    {
        public Guid Id { get; set; }
        public Guid PlayerId { get; set; }
        public DocumentType DocumentType { get; set; }
        public string CardNumber { get; set; }
        public DateTimeOffset? ExpirationDate { get; set; }
        public string UploadedBy { get; set; }
        public DateTimeOffset DateUploaded { get; set; }
        public DateTimeOffset? DateUnverified { get; set; }
        public string UnverifiedBy { get; set; }
        public string Remarks { get; set; }

        public IdentityDocumentUnverified()
        {

        }

        public IdentityDocumentUnverified(IdentityVerification identity)
        {
            Id = identity.Id;
            PlayerId = identity.Player.Id;
            DocumentType = identity.DocumentType;
            CardNumber = identity.CardNumber;
            ExpirationDate = identity.ExpirationDate;
            UploadedBy = identity.UploadedBy;
            DateUploaded = identity.DateUploaded;
            UnverifiedBy = identity.UnverifiedBy;
            DateUnverified = identity.DateUnverified;
            Remarks = identity.Remarks;
        }
    }
}