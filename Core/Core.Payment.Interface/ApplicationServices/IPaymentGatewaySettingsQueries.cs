using System;
using System.Collections.Generic;
using AFT.RegoV2.Core.Payment.Interface.Data;

namespace AFT.RegoV2.Core.Payment.Interface.ApplicationServices
{
    public interface IPaymentGatewaySettingsQueries
    {
        IEnumerable<PaymentGateway> GetPaymentGateways(Guid? brandId = null);

        PaymentGatewaySettings GetPaymentGatewaySettingsById(Guid id);

        IEnumerable<PaymentGatewaySettings> GetPaymentGatewaySettingsByPlayerId(Guid playerId);

        PaymentGatewaySettings GetOnePaymentGatewaySettingsByPlayerId(Guid playerId);
    }
}