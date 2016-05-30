using System.Runtime.Serialization;

namespace AFT.RegoV2.GameApi.Interface.ServiceContracts
{
    [DataContract, KnownType(typeof(BetCommandRequest))]
    public class CancelTransaction : BetCommandRequest
    {
    }

    [DataContract, KnownType(typeof(BetCommandResponse))]
    public class CancelTransactionResponse : BetCommandResponse
    {
    }
}
