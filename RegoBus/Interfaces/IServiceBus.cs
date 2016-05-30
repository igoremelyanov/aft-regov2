using System;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.RegoBus.Interfaces
{
    public interface IServiceBus : IDisposable
    {
        /// <summary>
        /// Asynchronous inter-process events and commands publishing.
        /// It asynchronously triggers either event handlers, which are registered using RegisterEventHandler method of EventWorkerBase class,
        /// or command handlers, which are ProcessMessage method in classes, derived from generic BaseNotificationWorker class of appropriate command type T.
        /// </summary>
        /// <typeparam name="T">Either event type (derived from DomainEventBase), or command type (implemented ICommand interface).</typeparam>
        /// <param name="message">Either event or command to be published via Message Queue.</param>
        void PublishMessage<T>(T message) where T : class, IMessage;

        /// <summary>
        /// Is used for register message handler in Message Queue.
        /// Do not use this method directly if you want to create notification handler or event handler. Instead, create derived class
        /// either from BaseNotificationWorker, and override ProcessMessage method,
        /// or from EventWorkerBase, and use RegisterEventHandler inside overrided RegisterEventHandlers method.
        /// </summary>
        void Subscribe<TSubscriber>() where TSubscriber : IBusSubscriber, new();
        void Subscribe<TSubscriber>(Func<TSubscriber> subscriberFactory) where TSubscriber : IBusSubscriber;

        void Unsubscribe(Type subscriberType);
    }
}