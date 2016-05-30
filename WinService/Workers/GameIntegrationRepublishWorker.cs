using AFT.RegoV2.Core.Game.Interface.Interfaces;
using AFT.RegoV2.RegoBus.Interfaces;
using AFT.UGS.Core.BaseModels.Bus;
using Microsoft.Practices.Unity;

// ReSharper disable InconsistentNaming
namespace AFT.RegoV2.WinService.Workers
{
    public class GameIntegrationRepublishWorker : WorkerBase<UGSEventsSubscriber>
    {
        public GameIntegrationRepublishWorker(IUnityContainer container, IUgsServiceBus serviceBus) : base(container, serviceBus) { }
    }

    public class UGSEventsSubscriber : IConsumesExternal<GameEvent>
    {
        private readonly IUgsEventRetranslator _translator;

        public UGSEventsSubscriber(IUgsEventRetranslator translator)
        {
            _translator = translator;
        }

        public void Consume(GameEvent @event)
        {
            _translator.Retranslate(@event);
        }

        public void Consume(FinancialEvent @event)
        {
            _translator.Retranslate(@event);
        }
    }
}
