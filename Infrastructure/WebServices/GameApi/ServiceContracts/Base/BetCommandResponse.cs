using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using AFT.RegoV2.GameApi.Classes;

namespace AFT.RegoV2.GameApi.ServiceContracts
{
    [DataContract]
    public class BetCommandResponse : GameApiResponseBase
    {
        [DataMember(Name = "bal")]
        public decimal Balance { get; set; }

        [DataMember(Name = "cur")]
        public string CurrencyCode { get; set; }

        [DataMember(Name = "transactions")]
        public List<BetCommandResponseTransaction> Transactions { get; set; }
    }

    [DataContract]
    public class BetCommandResponseTransaction
    {
        [DataMember(Name = "txid")]
        public string Id { get; set; }

        [DataMember(Name = "ptxid")]
        public Guid GameActionId { get; set; }

        [DataMember(Name = "dup")]
        public int IsDuplicate { get; set; }
    }
}
