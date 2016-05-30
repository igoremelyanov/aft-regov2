using System.ComponentModel.DataAnnotations;

namespace AFT.RegoV2.Core.Game.Interface.Data
{
    public class GameCulture
    {
        [Required]
        public string Code { get; set; }
    }
}
