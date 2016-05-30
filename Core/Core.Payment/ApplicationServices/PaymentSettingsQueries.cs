using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Core.Common.Data.Payment;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Payment.Interface.ApplicationServices;
using AFT.RegoV2.Core.Payment.Interface.Data;
using AFT.RegoV2.Core.Payment.Interface.Data.Commands;
using AFT.RegoV2.Core.Player.Interface.ApplicationServices;
using AFT.RegoV2.Core.Security.Common;

namespace AFT.RegoV2.Core.Payment.ApplicationServices
{ 
    public class PaymentSettingsQueries : IApplicationService, IPaymentSettingsQueries
    {
        private readonly IPaymentQueries _paymentQueries;
        private readonly IPlayerQueries _playerQueries;
        private readonly IPaymentSettingsCommands _settingsCommands;
        private readonly IPaymentGatewaySettingsQueries _paymentGatewaySettingsQueries;

        public PaymentSettingsQueries(IPaymentQueries paymentQueries, IPlayerQueries playerQueries, IPaymentSettingsCommands settingsCommands,
        IPaymentGatewaySettingsQueries paymentGatewaySettingsQueries)
        {
            _paymentQueries = paymentQueries;
            _playerQueries = playerQueries;
            _settingsCommands = settingsCommands;
            _paymentGatewaySettingsQueries = paymentGatewaySettingsQueries;
        }

        public object GetBankAccounts(Guid brandId, string currencyCode)
        {
            return _paymentQueries.GetBankAccounts(brandId, currencyCode)
                .Select(x => new
                {
                    x.Id,
                    Name = string.Format("Offline - {0}", x.AccountId)
                });
        }

        public object GetVipLevels(Guid? brandId)
        {
            return brandId.HasValue
                ? _playerQueries.VipLevels.Where(x => x.BrandId == brandId).Select(x => new { x.Id, x.Name })
                : _playerQueries.VipLevels.Select(x => new { x.Id, x.Name });
        }

        [Permission(Permissions.Create, Module = Modules.PaymentSettings)]
        [Permission(Permissions.Update, Module = Modules.PaymentSettings)]
        public PaymentSettingSaveResult SaveSetting(SavePaymentSettingsCommand model)
        {
            string message;
            var paymentSettingsId = model.Id;

            if (model.Id == Guid.Empty)
            {
                paymentSettingsId = _settingsCommands.AddSettings(model);
                message = "CreatedSuccessfully";
            }
            else
            {
                _settingsCommands.UpdateSettings(model);
                message = "UpdatedSuccessfully";
            }

            return new PaymentSettingSaveResult
            {
                Message = message,
                PaymentSettingsId = paymentSettingsId
            };
        }

        public IEnumerable<PaymentMethodDto> GetPaymentMethods(Guid? brandId = null)
        {
            var paymentMethods = new List<PaymentMethodDto>();
            paymentMethods.Add(new PaymentMethodDto
            {
                Id = PaymentMethodDto.OfflinePayMethod,
                Name = PaymentMethodDto.OfflinePayMethod,
                PaymentGatewayMethod = PaymentMethod.OfflineBank
            });

            var onlinePaymentGateway = _paymentGatewaySettingsQueries.GetPaymentGateways()
                .Select(
                x => new PaymentMethodDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    PaymentGatewayMethod = PaymentMethod.Online
                });

            paymentMethods.AddRange(onlinePaymentGateway);

            return paymentMethods;
        }
    }
}
