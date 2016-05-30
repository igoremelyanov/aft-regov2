﻿using System.Runtime.Serialization;

namespace AFT.RegoV2.GameApi.Interface.ServiceContracts
{
    [DataContract, KnownType(typeof(BatchCommandRequest))]
    public class BatchSettle : BatchCommandRequest
    {
    }

    [DataContract, KnownType(typeof(BatchCommandResponse))]
    public class BatchSettleResponse : BatchCommandResponse
    {
    }
}
