using System;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Player.Events
{
    public class SecurityQuestionCreated : DomainEventBase
    {
        public Guid Id { get; set; }
        public string Question { get; set; }

        public SecurityQuestionCreated()
        {
        }

        public SecurityQuestionCreated(SecurityQuestion securityQuestion)
        {
            Id = securityQuestion.Id;
            Question = securityQuestion.Question;
        }
    }
}
