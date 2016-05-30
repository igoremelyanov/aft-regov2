using System;
using System.Collections.Generic;

namespace AFT.RegoV2.MemberApi.Interface.Player
{
    public class WalletsDataRequest
    {
        public Guid BrandId { get; set; }
    }

    public class WalletsDataResponse
    {
        public Dictionary<Guid, string> Wallets { get; set; }
    }
}
