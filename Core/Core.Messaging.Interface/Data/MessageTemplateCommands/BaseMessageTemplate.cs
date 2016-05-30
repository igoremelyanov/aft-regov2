namespace AFT.RegoV2.Core.Messaging.Interface.Data.MessageTemplateCommands
{
    public abstract class BaseMessageTemplate
    {
        public string TemplateName { get; set; }
        public string Subject { get; set; }
        public string MessageContent { get; set; }
    }
}