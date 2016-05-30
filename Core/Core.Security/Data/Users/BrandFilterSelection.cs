using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AFT.RegoV2.Core.Security.Data.Users
{
    public class BrandFilterSelection
    {
        [Key, Column(Order = 0)]
        public Guid AdminId { get; set; }
        [Key, Column(Order = 1)]
        public Guid BrandId { get; set; }
        public virtual Admin Admin { get; set; }
    }
}
