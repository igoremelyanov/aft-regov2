using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Payment.Interface.ApplicationServices;
using AFT.RegoV2.Core.Payment.Interface.Data;
using AutoMapper;
using PaymentGatewaySettings = AFT.RegoV2.Core.Payment.Interface.Data.PaymentGatewaySettings;
namespace AFT.RegoV2.Core.Payment.ApplicationServices
{
    public class PaymentGatewaySettingsQueries : IApplicationService, IPaymentGatewaySettingsQueries
    {
        private readonly IPaymentRepository _repository;        
        public PaymentGatewaySettingsQueries(IPaymentRepository repository)
        {
            _repository = repository;
        }

        static PaymentGatewaySettingsQueries()
        {
            MapperConfig.CreateMap();
        }

        public IEnumerable<PaymentGateway> GetPaymentGateways(Guid? brandId=null)
        {
            var paymentGateways = "XPAY0,SDPAY";
            if (ConfigurationManager.AppSettings["PaymentGateways"] != null)
                paymentGateways = ConfigurationManager.AppSettings["PaymentGateways"];

            var gateways = paymentGateways.Split(',').Select(x=>new PaymentGateway{Name=x,Id=x}).ToList();
            return gateways;         
        }

        public PaymentGatewaySettings GetPaymentGatewaySettingsById(Guid id)
        {
            var settings= _repository.PaymentGatewaySettings
               .Include(l => l.Brand)
               .SingleOrDefault(l => l.Id == id);

            var data = Mapper.Map<PaymentGatewaySettings>(settings);
            
            return data;
        }

        internal IEnumerable<AFT.RegoV2.Core.Payment.Data.PaymentGatewaySettings> GetPaymentGatewaySettingsByPlayer(Guid playerId)
        {
            var player = _repository.Players.Where(x=>x.Id==playerId);
            if (player == null)
            {
                throw new ArgumentException(@"Player was not found", "playerId");
            }

            var paymentLevels = _repository.PlayerPaymentLevels
                .Include(x => x.PaymentLevel.PaymentGatewaySettings)
                .Where(level => level.PlayerId == playerId)
                .Select(x => x.PaymentLevel).Where(
                    x => x.Status == PaymentLevelStatus.Active
                        && x.EnableOnlineDeposit
                        && x.PaymentGatewaySettings.Any(z => z.Status == Status.Active));

            var firstPaymentLevel = paymentLevels.FirstOrDefault();
            if (firstPaymentLevel == null)
            {
                return null;
            }
            var paymentGatewaySettings = firstPaymentLevel.PaymentGatewaySettings.Where(x => x.Status == Status.Active);
            return paymentGatewaySettings;
        }

        public IEnumerable<PaymentGatewaySettings> GetPaymentGatewaySettingsByPlayerId(Guid playerId)
        {
            var paymentGatewaySettings = GetPaymentGatewaySettingsByPlayer (playerId);
            return Mapper.Map<IEnumerable<AFT.RegoV2.Core.Payment.Data.PaymentGatewaySettings>, IEnumerable<PaymentGatewaySettings>>(paymentGatewaySettings);
        }

        public PaymentGatewaySettings GetOnePaymentGatewaySettingsByPlayerId(Guid playerId)
        {
            var paymentGatewaySettings = GetPaymentGatewaySettingsByPlayer(playerId);
            var settings = paymentGatewaySettings!=null?paymentGatewaySettings.FirstOrDefault():null;
            return Mapper.Map<PaymentGatewaySettings>(settings); 
        }
    }
   
}
