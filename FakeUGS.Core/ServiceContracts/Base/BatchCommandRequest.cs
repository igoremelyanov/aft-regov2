using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using FakeUGS.Core.Interfaces;

namespace FakeUGS.Core.ServiceContracts.Base
{
    [DataContract]
    public class BatchCommandRequest
    {
        [DataMember(Name = "batchid")]
        public string BatchId { get; set; }

        [DataMember(Name = "securitykey")]
        public string SecurityKey { get; set; }

        [DataMember(Name = "timestamp")]
        public DateTimeOffset TimeStamp { get; set; }

        [DataMember(Name = "testmode")]
        public string TestMode { get; set; }

        [DataMember(Name = "transactions")]
        public List<BatchCommandTransactionRequest> Transactions { get; set; }

    }

    [DataContract]
    public class BatchCommandTransactionRequest : IBetCommandTransactionRequest
    {
        [DataMember(Name = "userid")]
        public Guid UserId { get; set; }

        [DataMember(Name = "tag")]
        public string BrandCode { get; set; }

        [DataMember(Name = "txid")]
        public string Id { get; set; }

        [DataMember(Name = "amt")]
        public decimal Amount { get; set; }

        [DataMember(Name = "cur")]
        public string CurrencyCode { get; set; }

        [DataMember(Name = "roundid")]
        public string RoundId { get; set; }

        [DataMember(Name = "roundclosed", IsRequired = false)]
        public bool RoundClosed { get; set; }

        [DataMember(Name = "betrecid")]
        public string BetRecId { get; set; }

        [DataMember(Name = "txtype")]
        public int TransactionType { get; set; }

        [DataMember(Name = "gameid")]
        public string GameId { get; set; }

        [DataMember(Name = "txrefid")]
        public string ReferenceId { get; set; }

        [DataMember(Name = "desc")]
        public string Description { get; set; }
    }
}
