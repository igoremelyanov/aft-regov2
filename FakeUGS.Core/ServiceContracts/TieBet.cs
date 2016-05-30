using System.Runtime.Serialization;

using FakeUGS.Core.ServiceContracts.Base;

namespace FakeUGS.Core.ServiceContracts
{
    [DataContract, KnownType(typeof(BetCommandRequest))]
    public class TieBet : BetCommandRequest
    {
    }

    [DataContract, KnownType(typeof(BetCommandResponse))]
    public class TieBetResponse : BetCommandResponse
    {
    }
}
