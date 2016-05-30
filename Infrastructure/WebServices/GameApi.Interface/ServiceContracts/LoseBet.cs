using System.Runtime.Serialization;

namespace AFT.RegoV2.GameApi.Interface.ServiceContracts
{
    [DataContract, KnownType(typeof(BetCommandRequest))]
    public class LoseBet : BetCommandRequest
    {
    }

    [DataContract, KnownType(typeof(BetCommandResponse))]
    public class LoseBetResponse : BetCommandResponse
    {
    }
}
