using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFT.RegoV2.GameApi.Interface.Interfaces
{
    public interface IBetCommandTransactionRequest
    {
        string Id { get; set; }

        decimal Amount { get; set; }
        string CurrencyCode { get; set; }

        string RoundId { get; set; }
        string ReferenceId { get; set; }
        string Description { get; set; }
        bool RoundClosed { get; set; }
    }
}
