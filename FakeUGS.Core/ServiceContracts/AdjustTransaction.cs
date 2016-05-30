using System.Runtime.Serialization;

using FakeUGS.Core.ServiceContracts.Base;

namespace FakeUGS.Core.ServiceContracts
{
    [DataContract, KnownType(typeof(BetCommandRequest))]
    public class AdjustTransaction : BetCommandRequest
    {
    }


    [DataContract, KnownType(typeof(BetCommandResponse))]
    public class AdjustTransactionResponse : BetCommandResponse
    {
    }
}
