using System;
using System.Runtime.Serialization;
using AFT.RegoV2.GameApi.Classes;
using AFT.RegoV2.GameApi.Interfaces;

namespace AFT.RegoV2.GameApi.ServiceContracts
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
