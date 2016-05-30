using System.Runtime.Serialization;

using FakeUGS.Core.ServiceContracts.Base;

namespace FakeUGS.Core.ServiceContracts
{
    [DataContract, KnownType(typeof(BatchCommandRequest))]
    public class BatchCancel : BatchCommandRequest
    {
    }

    [DataContract, KnownType(typeof(BatchCommandResponse))]
    public class CancelTransactionsResponse : BatchCommandResponse
    {
    }
}
