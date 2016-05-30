namespace AFT.RegoV2.Core.Messaging.Interface.Data.MessageTemplateModels
{
    public class OnlineDepositUnsuccessfulModel : PlayerMessageTemplateModel
    {
        public string ReferenceCode { get; set; }
        public string Amount { get; set; }
    }
}