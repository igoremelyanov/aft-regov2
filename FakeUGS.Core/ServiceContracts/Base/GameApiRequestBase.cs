using System.Runtime.Serialization;

using FakeUGS.Core.Interfaces;

namespace FakeUGS.Core.ServiceContracts.Base
{
    [DataContract]
    public class GameApiRequestBase : IAuthTokenHolder
    {
        [DataMember(Name="authtoken")]
        public string AuthToken { get; set; }

        [DataMember(Name = "ipaddress")]
        public string PlayerIpAddress { get; set; }

        [DataMember(Name = "testmode")]
        public string TestMode { get; set; }
    }
}
