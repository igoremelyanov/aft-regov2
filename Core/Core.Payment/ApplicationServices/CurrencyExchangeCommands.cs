using System;
using System.Linq;
using AFT.RegoV2.Core.Auth.Interface;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Payment.Data;
using AFT.RegoV2.Core.Payment.Interface.ApplicationServices;
using AFT.RegoV2.Core.Payment.Interface.Data.Commands;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Shared.Utils;

namespace AFT.RegoV2.Core.Payment.ApplicationServices
{    
    public class CurrencyExchangeCommands : ICurrencyExchangeCommands,IApplicationService
    {
        private readonly IActorInfoProvider _actorInfoProvider;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IPaymentQueries _queries;

        public CurrencyExchangeCommands(
            IActorInfoProvider actorInfoProvider,
            IPaymentRepository paymentRepository,
            IPaymentQueries queries
            )
        {
            _queries = queries;
            _actorInfoProvider = actorInfoProvider;
            _paymentRepository = paymentRepository;
        }
        
        public string Add(SaveCurrencyExchangeData model)
        {
            string message;

            if (_queries.GetCurrencyExchanges().Any(c => c.Brand.Id == model.BrandId && c.IsBaseCurrency && c.Brand.BaseCurrencyCode == model.Currency))
            {
                throw new RegoException("Base currency can't set as Currency To");
            }

            if (_queries.GetCurrencyExchanges().Any(c => c.BrandId == model.BrandId &&  !c.IsBaseCurrency && c.CurrencyToCode == model.Currency))
            {
                throw new RegoException("Currency Exchange Rate already exists");
            }

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var currencyExchange = new CurrencyExchange
                {
                    BrandId = model.BrandId,
                    CurrencyToCode = model.Currency,
                    CurrentRate = model.CurrentRate,
                    CreatedBy = _actorInfoProvider.Actor.UserName,
                    DateCreated = DateTimeOffset.UtcNow,
                };

                _paymentRepository.CurrencyExchanges.Add(currencyExchange);
                _paymentRepository.SaveChanges();
                
                scope.Complete();

                message = "app:currencies.created";
            }

            return message;
        }

        public string Save(SaveCurrencyExchangeData model)
        {
            string message;

            var currencyExchange = _paymentRepository.CurrencyExchanges.SingleOrDefault(c => c.Brand.Id == model.BrandId && c.CurrencyTo.Code == model.Currency);
            if (currencyExchange == null)
            {
                throw new RegoException("Currency Exchange not found");
            }

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                currencyExchange.PreviousRate = currencyExchange.CurrentRate;
                currencyExchange.CurrentRate = model.CurrentRate;
                currencyExchange.DateUpdated = DateTimeOffset.UtcNow;
                currencyExchange.UpdatedBy = _actorInfoProvider.Actor.UserName;

                _paymentRepository.SaveChanges();

                scope.Complete();

                message = "app:currencies.updated";
            }

            return message;
        }

        public string Revert(SaveCurrencyExchangeData model)
        {
            string message;
            
            var currencyExchange = _paymentRepository.CurrencyExchanges.SingleOrDefault(c => c.Brand.Id == model.BrandId && c.CurrencyTo.Code == model.Currency);
            if (currencyExchange == null)
            {
                throw new RegoException("Currency Exchange not found");
            }
            
            if (currencyExchange.PreviousRate == null)
            {
                throw new RegoException("Currency Exchange Previous Rate not found");
            }

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                currencyExchange.PreviousRate = currencyExchange.CurrentRate;
                currencyExchange.CurrentRate = model.PreviousRate;
                currencyExchange.DateUpdated = DateTimeOffset.UtcNow;
                currencyExchange.UpdatedBy = _actorInfoProvider.Actor.UserName;

                _paymentRepository.SaveChanges();

                scope.Complete();

                message = "app:currencies.updated";
            }

            return message;
        }
    }

   
}
