using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Common.Commands
{
    public class SmsCommandMessage : ICommand
    {
        public string From { get; set; }
        public string To { get; set; }
        public string Body { get; set; }

        public SmsCommandMessage(string from, string to, string body)
        {
            From = from;
            To = to;
            Body = body;
        }

        public SmsCommandMessage()
        {
        }
    }
}