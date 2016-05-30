using System;

namespace AFT.RegoV2.MemberApi.Interface.Player
{
    public class TimeOutResponse
    {
        public string UriToPlayerWhoWasTimeOuted { get; set; }
    }
    
    public class TimeOutRequest
    {
        public Guid PlayerId { get; set; }
        public int Option { get; set; }
    }
}
