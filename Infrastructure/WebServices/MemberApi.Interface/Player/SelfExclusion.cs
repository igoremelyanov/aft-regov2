using System;

namespace AFT.RegoV2.MemberApi.Interface.Player
{
    public class SelfExclusionResponse
    {
        public string UriToPlayerThatSelfExclusionWasAppliedTo { get; set; }
    }
    
    public class SelfExclusionRequest
    {
        public Guid PlayerId { get; set; }
        public int Option { get; set; }
    }
}
