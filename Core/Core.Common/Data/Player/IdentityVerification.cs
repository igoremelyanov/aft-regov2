using System;
using System.ComponentModel.DataAnnotations;
using AFT.RegoV2.Core.Common.Data.Player.Enums;

namespace AFT.RegoV2.Core.Common.Data.Player
{
    public class IdentityVerification
    {
        public Guid Id { get; set; }
        public Player Player { get; set; }
        public DocumentType DocumentType { get; set; }
        [MaxLength(20)]
        public string CardNumber { get; set; }
        public DateTimeOffset? ExpirationDate { get; set; }
        public VerificationStatus VerificationStatus { get; set; }
        public string VerifiedBy { get; set; }
        public DateTimeOffset? DateVerified { get; set; }
        public string UnverifiedBy { get; set; }
        public DateTimeOffset? DateUnverified { get; set; }
        public string UploadedBy { get; set; }
        public DateTimeOffset DateUploaded { get; set; }
        public Guid? FrontFile { get; set; }
        public Guid? BackFile { get; set; }
        [MaxLength(200)]
        public string Remarks { get; set; }
    }
}
