using System;
using AFT.RegoV2.Core.Common.Data;

namespace AFT.RegoV2.Core.Payment.Interface.Data.Commands
{
    public class SavePaymentGatewaysSettingsData
    {
        public Guid Id { get; set; }
        public Guid Brand { get; set; }             
        public string OnlinePaymentMethodName { get; set; }        
        public string PaymentGatewayName { get; set; }
        public int Channel { get; set; }
        public string EntryPoint { get; set; }
        public string Remarks { get; set; }
        public Status Status { get; set; }      
    }

    public class SavePaymentGatewaysSettingsResult
    {
        public Guid PaymentGatewaySettingsId { get; set; }
    }

    public class ActivatePaymentGatewaySettingsData
    {
        public Guid Id { get; set; }
        public string Remarks { get; set; }
    }

    public class DeactivatePaymentGatewaySettingsData
    {
        public Guid Id { get; set; }
        public string Remarks { get; set; }
    }
}