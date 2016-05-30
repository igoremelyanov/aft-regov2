namespace AFT.RegoV2.Core.Messaging.Interface.Data.MessageTemplateModels
{
    public class FirstOnlineDepositSuccessfulModel : PlayerMessageTemplateModel
    {
        public string ReferenceCode { get; set; }
        public string Amount { get; set; }
    }
}