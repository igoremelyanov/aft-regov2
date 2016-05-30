using System;
using System.ComponentModel.DataAnnotations;

namespace AFT.RegoV2.Core.Player.Data
{
    public class Bank
    {
        [Key]
        public Guid Id { get; set; }

        public string BankId { get; set; }
        public string BankName { get; set; }
        public Guid BrandId { get; set; }
    }
}
