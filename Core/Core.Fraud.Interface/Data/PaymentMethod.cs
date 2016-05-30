using System;
using System.ComponentModel.DataAnnotations;

namespace AFT.RegoV2.Core.Fraud.Interface.Data
{
    public class PaymentMethod
    {
        [Key]
        public Guid Key { get; set; }

        public int Id { get; set; }

        public string Code { get; set; }
    }
}