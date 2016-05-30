using System.Web.Mvc;

namespace AFT.RegoV2.AdminWebsite.ViewModels.Messaging
{
    public abstract class BaseMessageTemplateModel
    {

        public string TemplateName { get; set; }
        public string Subject { get; set; }
        [AllowHtml]
        public string MessageContent { get; set; }
    }
}