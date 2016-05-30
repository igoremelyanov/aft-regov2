using System.ComponentModel.DataAnnotations;

namespace FakeUGS.Core.Data
{
    public class GameCulture
    {
        [Required]
        public string Code { get; set; }
    }
}
