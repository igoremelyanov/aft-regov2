using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using AFT.RegoV2.GameApi.Interfaces;

namespace AFT.RegoV2.GameApi.ServiceContracts
{
    [DataContract]
    public class BetCommandRequest : GameApiRequestBase
    {
        [DataMember(Name = "transactions")]
        public List<BetCommandTransactionRequest> Transactions { get; set; }
    }

    [DataContract]
    public class BetCommandTransactionRequest : IBetCommandTransactionRequest
    {
        [DataMember(Name = "txid")]
        public string Id { get; set; }

        [DataMember(Name = "amt")]
        public decimal Amount { get; set; }

        [DataMember(Name = "cur")]
        public string CurrencyCode { get; set; }

        [DataMember(Name = "roundid")]
        public string RoundId { get; set; }

        [DataMember(Name="roundclosed", IsRequired = false)]
        public bool RoundClosed { get; set; }

        [DataMember(Name = "betrecid")]
        public string BetRecId { get; set; }

        [DataMember(Name = "gameid")]
        public string GameId { get; set; }

        [DataMember(Name = "txrefid")]
        public string ReferenceId { get; set; }

        [DataMember(Name = "timestamp")]
        public DateTimeOffset TimeStamp { get; set; }

        [DataMember(Name = "desc")]
        public string Description { get; set; }
    }
}
