using System;
using System.Linq;
using AFT.RegoV2.Core.Auth.Interface;
using AFT.RegoV2.Core.Common.Data.Admin;
using AFT.RegoV2.Core.Payment.Interface.Events;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Payment.Interface.ApplicationServices;
using AFT.RegoV2.Core.Payment.Interface.Data.Commands;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.RegoBus.Interfaces;
using AFT.RegoV2.Shared.Utils;
using Currency = AFT.RegoV2.Core.Payment.Data.Currency;

namespace AFT.RegoV2.Core.Payment.ApplicationServices
{ 
    public class CurrencyCommands : ICurrencyCommands, IApplicationService
    {
        private readonly IActorInfoProvider _actorInfoProvider;
        private readonly IEventBus _eventBus;

        private readonly IPaymentRepository _paymentRepository;
        private readonly IPaymentQueries _queries;

        public CurrencyCommands(
            IActorInfoProvider actorInfoProvider,
            IEventBus eventBus,
            IPaymentRepository paymentRepository,
            IPaymentQueries queries
            )
        {
            _queries = queries;
            _actorInfoProvider = actorInfoProvider;
            _eventBus = eventBus;
            _paymentRepository = paymentRepository;
        }

        [Permission(Permissions.Create, Module = Modules.CurrencyManager)]
        public CurrencyCRUDStatus Add(EditCurrencyData model)
        {
            if (_queries.GetCurrencies().Any(c => c.Code == model.Code))
            {
                return new CurrencyCRUDStatus { IsSuccess = false, Message = "codeUnique" };
            }

            if (_queries.GetCurrencies().Any(c => c.Name == model.Name))
            {
                return new CurrencyCRUDStatus { IsSuccess = false, Message = "nameUnique" };
            }

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var username = _actorInfoProvider.Actor.UserName;
                var currency = new Currency
                {
                    Code = model.Code,
                    CreatedBy = username,
                    DateCreated = DateTimeOffset.UtcNow,
                    Name = model.Name,
                    Remarks = model.Remarks
                };

                _paymentRepository.Currencies.Add(currency);
                _paymentRepository.SaveChanges();

                var currencyCreated = new CurrencyCreated
                {
                    Code = currency.Code,
                    Name = currency.Name,
                    Remarks = currency.Remarks,
                    Status = currency.Status
                };

                _eventBus.Publish(currencyCreated);

                scope.Complete();
            }

            return new CurrencyCRUDStatus { IsSuccess = true, Message = "created" };
        }

        [Permission(Permissions.Update, Module = Modules.CurrencyManager)]
        public CurrencyCRUDStatus Save(EditCurrencyData model)
        {
            var oldCode = model.OldCode;
            var oldName = model.OldName;

            var currency = _paymentRepository.Currencies.SingleOrDefault(c => c.Code == oldCode);

            if (currency == null)
            {
                return new CurrencyCRUDStatus { IsSuccess = false, Message = "invalidId" };
            }

            if (_queries.GetCurrencies().Any(c => c.Code == model.Code && c.Code != oldCode))
            {
                return new CurrencyCRUDStatus { IsSuccess = false, Message = "codeUnique" };
            }

            if (_queries.GetCurrencies().Any(c => c.Name == model.Name && c.Name != oldName))
            {
                return new CurrencyCRUDStatus { IsSuccess = false, Message = "nameUnique" };
            }

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var username = _actorInfoProvider.Actor.UserName;
                currency.UpdatedBy = username;
                currency.DateUpdated = DateTimeOffset.UtcNow;
                currency.Name = model.Name;
                currency.Remarks = model.Remarks;

                _paymentRepository.SaveChanges();

                var currencyUpdated = new CurrencyUpdated
                {
                    OldCode = model.OldCode,
                    OldName = model.OldName,
                    Code = currency.Code,
                    Name = currency.Name,
                    Remarks = currency.Remarks
                };

                _eventBus.Publish(currencyUpdated);

                scope.Complete();
            }

            return new CurrencyCRUDStatus { IsSuccess = true, Message = "updated" };
        }
    }

   
}