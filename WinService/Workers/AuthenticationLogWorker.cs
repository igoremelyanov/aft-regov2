using AFT.RegoV2.ApplicationServices.Report.EventHandlers;
using AFT.RegoV2.Core.Security.Events;
using AFT.RegoV2.RegoBus.Interfaces;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.WinService.Workers
{
    public class AuthenticationLogWorker : WorkerBase<AuthenticationLogSubscriber>
    {
        public AuthenticationLogWorker(IUnityContainer container, IServiceBus serviceBus) : base(container, serviceBus) { }
    }

    public class AuthenticationLogSubscriber : IBusSubscriber,
        IConsumes<AdminAuthenticationSucceded>,
        IConsumes<AdminAuthenticationFailed>,
        IConsumes<MemberAuthenticationSucceded>,
        IConsumes<MemberAuthenticationFailed>
    {
        private readonly AuthenticationLogEventHandlers _eventHandlers;

        public AuthenticationLogSubscriber(AuthenticationLogEventHandlers eventHandlers)
        {
            _eventHandlers = eventHandlers;
        }

        public void Consume(AdminAuthenticationSucceded message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(AdminAuthenticationFailed message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(MemberAuthenticationSucceded message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(MemberAuthenticationFailed message)
        {
            _eventHandlers.Handle(message);
        }
    }
}
