namespace AFT.RegoV2.FakePaymentServer.Models
{
    public class NotifactionData
    {
        public string Action { get; set; }
        public string OrderIdOfMerchant { get; set; }
        public string OrderIdOfRouter { get; set; }
        public string OrderIdOfGateway { get; set; }
        public string Language { get; set; }
        public string PayMethod { get; set; }
        public string Signature { get; set; }

        public string SignParams
        {
            get
            {
                var plainText = OrderIdOfMerchant + OrderIdOfRouter + OrderIdOfGateway + Language;
                return plainText;
            }
        }
    }
}
