using System.ComponentModel.DataAnnotations;

namespace AFT.RegoV2.Core.Payment.Data
{
    public class Country
    {
        [Key]
        public string Code { get; set; }
        public string Name { get; set; }
    }
}
