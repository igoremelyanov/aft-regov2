using System;
using System.Runtime.Serialization;

using FakeUGS.Core.ServiceContracts.Base;

namespace FakeUGS.Core.ServiceContracts
{
    [DataContract, KnownType(typeof(GameApiRequestBase))]
    public class ValidateToken : GameApiRequestBase
    {
        
    }

    [DataContract, KnownType(typeof(GameApiResponseBase))]
    public class ValidateTokenResponse : GameApiResponseBase
    {
        [DataMember(Name = "username")]
        public string PlayerDisplayName { get; set; }
        
        [DataMember(Name = "userid")]
        public Guid PlayerId { get; set; }

        [DataMember(Name = "tag")]
        public string BrandCode { get; set; }

        [DataMember(Name = "cur")]
        public string CurrencyCode { get; set; }

        [DataMember(Name = "lang")]
        public string Language { get; set; }
        
        [DataMember(Name = "bal")]
        public decimal Balance { get; set; }
        
        [DataMember(Name = "betlimitid")] // current API spec says it's "betlimitid", please don't change without notice, game providers rely on this.
        public string BetLimitCode { get; set; }
    }


}
