using System;
using System.Collections.Generic;
using AFT.RegoV2.Core.Common.Data.Base;
using AFT.RegoV2.Core.Common.Data;

namespace AFT.RegoV2.AdminApi.Interface.Payment
{
    public class SavePaymentGatewaySettingsRequest
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

    public class SavePaymentGatewaySettingsResponse : ValidationResponseBase
    {
        public Guid Id { get; set; }        
    }

    public class ActivatePaymentGatewaySettingsRequest
    {
        public Guid Id { get; set; }
        public string Remarks { get; set; }
    }

    public class DeactivatePaymentGatewaySettingsRequest
    {
        public Guid Id { get; set; }
        public string Remarks { get; set; }
    }

    public class ActivatePaymentGatewaySettingsResponse : ValidationResponseBase
    {
    }

    public class DeactivatePaymentGatewaySettingsResponse : ValidationResponseBase
    {
    }

    public class GetPaymentGatewaysRequest
    {
        public Guid? BrandId { get; set; }
    }

    public class GetPaymentGatewaysResponse : ValidationResponseBase
    {
        public IEnumerable<PaymentGateway> PaymentGateways { get; set; }
    }

    public class PaymentGateway
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class PaymentGatewaySettingsViewDataResponse
    {
        public Guid Id { get; set; }
        public Guid BrandId { get; set; }
        public Guid LicenseeId { get; set; }   
        public string OnlinePaymentMethodName { get; set; }        
        public string PaymentGatewayName { get; set; }
        public int Channel { get; set; }
        public string EntryPoint { get; set; }
        public string Remarks { get; set; }
        public Status Status { get; set; }
        public string CreatedBy { get; set; }
        public DateTimeOffset DateCreated { get; set; }
        public string UpdatedBy { get; set; }
        public DateTimeOffset? DateUpdated { get; set; }
        public string ActivatedBy { get; set; }
        public DateTimeOffset? DateActivated { get; set; }
        public string DeactivatedBy { get; set; }
        public DateTimeOffset? DateDeactivated { get; set; }
    }
}
