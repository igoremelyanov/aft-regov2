using System;
using System.ComponentModel.DataAnnotations;
using AFT.RegoV2.Core.Common.Data;

namespace AFT.RegoV2.Core.Player.Data
{
    public class BankAccount
    {
        [Key]
        public Guid Id { get; set; }
            
        [Required, MinLength(1), MaxLength(20)]
        public string AccountId { get; set; }

        public Bank Bank { get; set; }

        public Guid BankId { get; set; }

        public BankAccountStatus BankAccountStatus { get; set; }
    }
}