using System;
using System.Linq;
using AFT.RegoV2.Core.Auth.Interface;
using AFT.RegoV2.Core.Common.Data.Payment;
using AFT.RegoV2.Core.Payment.Interface.Events;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Payment.Interface.ApplicationServices;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.RegoBus.Interfaces;
using AFT.RegoV2.Shared.Utils;

namespace AFT.RegoV2.Core.Payment.ApplicationServices
{
    public class PaymentCommands : IPaymentCommands, IApplicationService
    {
        private readonly IPaymentRepository _repository;
        private readonly IEventBus _eventBus;
        private readonly IActorInfoProvider _actorInfoProvider;

        public PaymentCommands(
            IPaymentRepository repository,
            IEventBus eventBus,
            IActorInfoProvider actorInfoProvider
            )
        {
            _repository = repository;
            _eventBus = eventBus;
            _actorInfoProvider = actorInfoProvider;
        }

        [Permission(Permissions.Activate, Module = Modules.CurrencyManager)]
        public void ActivateCurrency(string code, string remarks)
        {
            UpdateCurrencyStatus(code, CurrencyStatus.Active, remarks);
        }

        [Permission(Permissions.Deactivate, Module = Modules.CurrencyManager)]
        public void DeactivateCurrency(string code, string remarks)
        {
            UpdateCurrencyStatus(code, CurrencyStatus.Inactive, remarks);
        }

        private void UpdateCurrencyStatus(string code, CurrencyStatus status, string remarks)
        {
            var currency = _repository.Currencies.First(x => x.Code == code);

            if (currency.Status == status)
                return;

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var user = _actorInfoProvider.Actor.UserName;
                currency.Status = status;
                currency.UpdatedBy = user;
                currency.DateUpdated = DateTimeOffset.UtcNow;

                if (status == CurrencyStatus.Active)
                {
                    currency.ActivatedBy = user;
                    currency.DateActivated = currency.DateUpdated;
                }
                else
                {
                    currency.DeactivatedBy = user;
                    currency.DateDeactivated = currency.DateUpdated;
                }

                currency.Remarks = remarks;

                _repository.SaveChanges();
                
                var currencyStatusChanged = new CurrencyStatusChanged
                {
                    Code = currency.Code,
                    Status = currency.Status,
                    Remarks = remarks
                };
                
                _eventBus.Publish(currencyStatusChanged);

                scope.Complete();
            }
        }

    }
}
