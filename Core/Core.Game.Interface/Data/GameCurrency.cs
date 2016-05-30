using System.ComponentModel.DataAnnotations;

namespace AFT.RegoV2.Core.Game.Interface.Data
{
    public class GameCurrency
    {
        [Required, MaxLength(3)]
        public string Code { get; set; }
    }
}
