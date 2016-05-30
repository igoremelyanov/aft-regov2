using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Core.Common.EventStore;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.RegoBus.Interfaces;
using AFT.RegoV2.Shared.Utils;

namespace AFT.RegoV2.Tests.Common.TestDoubles
{
    /// <summary>
    /// Dispatches Domain Events from in-process synchronous serviceBus.
    /// </summary>
    public class FakeServiceBus : IServiceBus
    {
        private readonly IEventRepository _eventRepository;

        private readonly Dictionary<Type, Dictionary<string, List<Action<IMessage>>>> _subscriptions;
        private readonly List<IMessage> _undispatchedMessages;

        private readonly Random _random;

        public List<ICommand> PublishedCommands { get; }
        public int PublishedEventCount { get; private set; }

        public FakeServiceBus(IEventRepository eventRepository)
        {
            _eventRepository = eventRepository;
            _subscriptions = new Dictionary<Type, Dictionary<string, List<Action<IMessage>>>>();
            _undispatchedMessages = new List<IMessage>();
            MessageOrder = new Queue<Type>();
            PublishedCommands = new List<ICommand>();

            _random = new Random();
        }

        /// <summary>
        /// For testing purposes, it allows to delay incoming messages from MessageOrder tail until head message arrive.
        /// For example, let MessageOrder is Queue { TypeA, TypeB, TypeC }, and messages of TypeB and TypeC
        /// are incoming before a message of TypeA. In that case FakeServiceBus waits for a message of TypeA,
        /// then fires the message of TypeA, then the message of TypeB, then the message of TypeC.
        /// If MessageOrder is not set, or incoming message type is not listed in MessageOrder, then the message is fired immediately.
        /// </summary>
        public Queue<Type> MessageOrder { get; set; }

        public void PublishMessage<T>(T message) where T : class, IMessage
        {
            PublishMessage(message, saveToEventStore: true);
        }

        public void PublishMessage<T>(T message, bool saveToEventStore) where T : class, IMessage
        {
            var item = message as ICommand;
            if (item != null)
            {
                PublishedCommands.Add(item);
            }

            IDomainEvent domainEvent;
            if ((domainEvent = message as IDomainEvent) != null)
            {
                if (saveToEventStore)
                    _eventRepository.SaveEvent(domainEvent);
                PublishedEventCount++;
            }

            _undispatchedMessages.Add(message);
            DispatchMessages();
        }



        private bool _dispatching;
        private bool _recurrentDispatching;
        public void DispatchMessages()
        {
            if (_dispatching)
            {
                _recurrentDispatching = true;
                return;
            }
            _dispatching = true;
            int messageCount;
            do
            {
                _recurrentDispatching = false;
                messageCount = _undispatchedMessages.Count;
                DispatchNextMessage();
            } while (_recurrentDispatching || messageCount != _undispatchedMessages.Count);
            _dispatching = false;
        }

        private void DispatchNextMessage()
        {
            if (!_undispatchedMessages.Any())
            {
                return;
            }
            var message = _undispatchedMessages.First();
            if (MessageOrder.Any() && _undispatchedMessages.Any(m => m.GetType() == MessageOrder.Peek()))
            {
                message = _undispatchedMessages.First(m => m.GetType() == MessageOrder.Dequeue());
            }
            else if (MessageOrder.Contains(message.GetType()))
            {
                return;
            }
            _undispatchedMessages.Remove(message);
            DispatchMessage(message);
        }

        private void DispatchMessage(IMessage message)
        {
            if (_subscriptions.ContainsKey(message.GetType()))
            {
                _subscriptions[message.GetType()].ForEach(subscription => subscription.Value[_random.Next(subscription.Value.Count)](message));
            }
        }

        public void Subscribe<TSubscriber>() where TSubscriber : IBusSubscriber, new()
        {
            Subscribe(() => new TSubscriber());
        }

        public void Subscribe<TSubscriber>(Func<TSubscriber> subscriberFactory) where TSubscriber : IBusSubscriber
        {
            var subscriber = subscriberFactory();
            var subscriberType = subscriber.GetType();
            var subscriptionName = subscriberType.Name;

            var types = subscriberType
                .GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IConsumes<>))
                .SelectMany(i => i.GetGenericArguments())
                .OrderBy(x => x.Name)
                .ToList();

            foreach (var type in types)
            {
                if (!_subscriptions.ContainsKey(type))
                {
                    _subscriptions.Add(type, new Dictionary<string, List<Action<IMessage>>>());
                }

                if (!_subscriptions[type].ContainsKey(subscriptionName))
                {
                    _subscriptions[type].Add(subscriptionName, new List<Action<IMessage>>());
                }

                Action<IMessage> subscription = (message) =>
                {
                    var subscriberMethod = subscriber.GetType().GetMethod("Consume", new Type[] { message.GetType() });
                    subscriberMethod.Invoke(subscriber, new object[] { message });
                };

                _subscriptions[type][subscriptionName].Add(subscription);
            }
        }

        public void Unsubscribe(Type subscriberType)
        {
            var subscriptionName = subscriberType.Name;
            foreach (var messageSubscription in _subscriptions)
            {
                foreach (var subscription in messageSubscription.Value)
                {
                    if (subscription.Key == subscriptionName)
                    {
                        messageSubscription.Value.Remove(subscription.Key);
                    }
                }
            }
        }

        public void Dispose()
        {
        }
    }
}