using System.Runtime.Serialization;

using FakeUGS.Core.ServiceContracts.Base;

namespace FakeUGS.Core.ServiceContracts
{
    [DataContract, KnownType(typeof(GameApiRequestBase))]
    public class GetBalance : GameApiRequestBase
    {
    }

    [DataContract, KnownType(typeof(GameApiResponseBase))]
    public class GetBalanceResponse : GameApiResponseBase
    {
        [DataMember(Name = "bal")]
        public decimal Balance { get; set; }

        [DataMember(Name = "cur")]
        public string CurrencyCode { get; set; }
    }
}