using System;

namespace AFT.RegoV2.MemberApi.Interface.Player
{
    public class GetSecurityQuestionRequest
    {
        public Guid PlayerId { get; set; }
    }

    public class GetSecurityQuestionResponse
    {
        public string SecurityQuestion { get; set; }
    }

    public class SecurityAnswerRequest 
    {
        public Guid PlayerId { get; set; }
        public string Answer { get; set; }
    }

    public class SecurityAnswerCheckResponse
    {
    }
}
