using System.Runtime.Serialization;

using FakeUGS.Core.ServiceContracts.Base;

namespace FakeUGS.Core.ServiceContracts
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
