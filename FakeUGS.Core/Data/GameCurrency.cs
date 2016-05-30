using System.ComponentModel.DataAnnotations;

namespace FakeUGS.Core.Data
{
    public class GameCurrency
    {
        [Required, MaxLength(3)]
        public string Code { get; set; }
    }
}
