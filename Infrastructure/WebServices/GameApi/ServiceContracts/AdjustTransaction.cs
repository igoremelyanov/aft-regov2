using System.Runtime.Serialization;

namespace AFT.RegoV2.GameApi.ServiceContracts
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
