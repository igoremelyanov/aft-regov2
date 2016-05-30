using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using FakeUGS.Core.Classes;
using FakeUGS.Core.Interfaces;

namespace FakeUGS.Core.ServiceContracts.Base
{
    [DataContract, KnownType(typeof(GameApiResponseBase))]
    public class BatchCommandResponse : GameApiResponseBase
    {
        [DataMember(Name = "batchid")]
        public string BatchId { get; set; }

        [DataMember(Name = "dup")]
        public int IsDuplicate { get; set; }

        [DataMember(Name = "batchtime")]
        public string BatchTimestamp { get; set; }

        [DataMember(Name = "elapsed")]
        public long Elapsed { get; set; }

        [DataMember(Name = "transcount")]
        public int TransactionCount { get; set; }

        [DataMember(Name = "playerbalances")]
        public Dictionary<Guid, decimal> PlayerBalances { get; set; }

        [DataMember(Name = "errors")]
        public List<BatchCommandTransactionError> Errors { get; set; }
    }

    [DataContract]
    public class BatchCommandTransactionError : IGameApiErrorDetails
    {
        [DataMember(Name = "txid")]
        public string Id { get; set; }

        [DataMember(Name = "ptxid")]
        public Guid GameActionId { get; set; }

        [DataMember(Name = "dup")]
        public int IsDuplicate { get; set; }

        [DataMember(Name = "userid")]
        public Guid UserId { get; set; }

        [DataMember(Name = "err")]
        public GameApiErrorCode ErrorCode { get; set; }

        [DataMember(Name = "errdesc")]
        public string ErrorDescription { get; set; }
    }
}
