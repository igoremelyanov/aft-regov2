using System.Runtime.Serialization;

namespace AFT.RegoV2.GameApi.Interface.ServiceContracts
{
    [DataContract, KnownType(typeof(BetCommandRequest))]
    public class WinBet : BetCommandRequest
    {
    }

    [DataContract, KnownType(typeof (BetCommandResponse))]
    public class WinBetResponse : BetCommandResponse
    {
    }
}
