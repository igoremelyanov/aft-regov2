using System.Runtime.Serialization;

using FakeUGS.Core.Classes;
using FakeUGS.Core.Interfaces;

namespace FakeUGS.Core.ServiceContracts.Base
{
    [DataContract]
    public class GameApiResponseBase : IGameApiResponse
    {
        [DataMember(Name = "err")]
        public GameApiErrorCode ErrorCode { get; set; }

        [DataMember(Name = "errdesc")]
        public string ErrorDescription { get; set; }

        public GameApiResponseBase()
        {
            ErrorCode = GameApiErrorCode.NoError;
            ErrorDescription = string.Empty;
        }
    }
}
