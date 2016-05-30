using System;
using System.ComponentModel.DataAnnotations;

namespace AFT.RegoV2.Core.Common
{
    public class FileMetadata
    {
        [Key]
        public Guid FileId { get; set; }

        public Guid PlayerId { get; set; }

        public Guid LicenseeId { get; set; }

        public Guid BrandId { get; set; }
    }
}