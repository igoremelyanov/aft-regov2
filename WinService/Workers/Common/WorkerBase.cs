using System;
using AFT.RegoV2.RegoBus.Interfaces;
using Microsoft.Practices.Unity;
using WinService.Workers;

namespace AFT.RegoV2.WinService.Workers
{
    public abstract class WorkerBase<TSubscriber> : IWorker where TSubscriber : IBusSubscriber
    {
        private readonly IUnityContainer _container;
        private readonly IServiceBus _serviceBus;

        protected WorkerBase(IUnityContainer container, IServiceBus serviceBus)
        {
            _container = container;
            _serviceBus = serviceBus;
        }

        public void Start()
        {
            Subscribe();
        }

        public void Stop()
        {
            _serviceBus.Unsubscribe(this.GetType());
        }

        protected void Subscribe()
        {
            var factory = GetSubscriberFactory();
            _serviceBus.Subscribe(factory);
        }

        protected Func<TSubscriber> GetSubscriberFactory()
        {
            return () => (TSubscriber)_container.Resolve(typeof (TSubscriber));
        }
    }
}