using System;
using AFT.RegoV2.Core.Game.Interface.Events;
using AFT.RegoV2.Core.Game.Interface.Interfaces;
using AFT.RegoV2.RegoBus.Interfaces;
using AFT.UGS.Core.BaseModels.Bus;

namespace AFT.RegoV2.Core.Game.Services
{
    public class UgsEventRetranslator : IUgsEventRetranslator
    {
        private readonly IEventBus _eventBus;

        public UgsEventRetranslator(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        void IUgsEventRetranslator.Retranslate<TEvent>(TEvent @event)
        {
            if (@event is GameEvent)
                _eventBus.Publish(new UgsGameEvent(@event as GameEvent));

            if (@event is FinancialEvent)
            {
                var financialEvent = @event as FinancialEvent;
                if (financialEvent.type == BusEventType.FundedIn)
                    _eventBus.Publish(new FundedIn
                    {
                        TransactionId = financialEvent.externaltxid,
                        Amount = financialEvent.amount,
                        PlayerId = Guid.Parse(financialEvent.userid)
                    });
                if (financialEvent.type == BusEventType.FundedOut)
                    _eventBus.Publish(new FundedOut
                    {
                        TransactionId = financialEvent.externaltxid,
                        Amount = financialEvent.amount,
                        PlayerId = Guid.Parse(financialEvent.userid)
                    });
            }
        }

    }
}
