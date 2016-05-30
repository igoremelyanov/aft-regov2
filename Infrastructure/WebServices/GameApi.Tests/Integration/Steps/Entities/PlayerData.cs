using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFT.RegoV2.GameApi.Tests.Integration.Steps.Entities
{
    public class PlayerData
    {
        public Guid PlayerId { get; set; }
        public string LastPlaceBetTransactionId { get; set; }
        public string RoundId { get; set; }
    }
}
