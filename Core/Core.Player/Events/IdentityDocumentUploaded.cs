using System;
using AFT.RegoV2.Core.Common.Data.Player;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Player.Events
{
    public class IdentityDocumentUploaded : DomainEventBase
    {
        public Guid Id { get; set; }
        public Guid PlayerId { get; set; }
        public DocumentType DocumentType { get; set; }
        public string CardNumber { get; set; }
        public DateTimeOffset? ExpirationDate { get; set; }
        public string UploadedBy { get; set; }
        public DateTimeOffset DateUploaded { get; set; }
        public string Remarks { get; set; }

        public IdentityDocumentUploaded()
        {

        }

        public IdentityDocumentUploaded(IdentityVerification identity)
        {
            Id = identity.Id;
            PlayerId = identity.Player.Id;
            DocumentType = identity.DocumentType;
            CardNumber = identity.CardNumber;
            ExpirationDate = identity.ExpirationDate;
            UploadedBy = identity.UploadedBy;
            DateUploaded = identity.DateUploaded;
            Remarks = identity.Remarks;
        }
    }
}