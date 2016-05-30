using System.Runtime.Serialization;

using FakeUGS.Core.ServiceContracts.Base;

namespace FakeUGS.Core.ServiceContracts
{
    [DataContract, KnownType(typeof(BatchCommandRequest))]
    public class BatchAdjust : BatchCommandRequest
    {
    }

    [DataContract, KnownType(typeof(BatchCommandResponse))]
    public class BatchAdjustResponse : BatchCommandResponse
    {
    }
}
