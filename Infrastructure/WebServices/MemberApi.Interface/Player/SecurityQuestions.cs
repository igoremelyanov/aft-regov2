using System;
using System.Collections.Generic;

namespace AFT.RegoV2.MemberApi.Interface.Player
{
    public class SecurityQuestionsRequest 
    {

    }

    public class SecurityQuestionsResponse 
    {
        public List<SecurityQuestion> SecurityQuestions { get; set; }
    }

    public class SecurityQuestion
    {
        public Guid Id { get; set; }
        public string Question { get; set; }
    }
}
