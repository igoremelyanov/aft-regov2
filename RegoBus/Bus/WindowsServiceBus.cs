using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Shared.Logging;
using AFT.RegoV2.Core.Settings.Interface.Interfaces;
using AFT.RegoV2.RegoBus.Interfaces;
using AFT.RegoV2.Shared.Synchronization;
using AFT.RegoV2.Shared.Utils;

using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;

namespace AFT.RegoV2.RegoBus.Bus
{
    public class WindowsServiceBus : IServiceBus 
    {
        private ILog _logger;
        
        private NamespaceManager _namespaceManager = null;
        private TopicClient _topicClient = null;
        private string _serviceBusConnectionString = null;
        private bool _topicExistanceChecked = false;
        private List<string> _subscriptionsWithCheckedExistance = new List<string>();
        private Dictionary<string, Type> _messageTypes = new Dictionary<string, Type>();
        private ISynchronizationService _syncService;
        private readonly IServiceBusConnectionStringProvider _connectionStringProvider;

        protected Dictionary<Type, SubscriptionClient> SubscriberClients;

        protected const string PropertyForTypeName = "MessageType";
        protected const string PropertyForTypeFullName = "MessageFullType";
        protected const string TopicName = "Events";

        public WindowsServiceBus(ILog logger, ISynchronizationService syncService, IServiceBusConnectionStringProvider connectionStringProvider)
        {
            _logger = logger;
            _syncService = syncService;
            _connectionStringProvider = connectionStringProvider;

            SubscriberClients = new Dictionary<Type, SubscriptionClient>();
        }

        public void PublishMessage<T>(T message) where T : class, IMessage
        {
            EnsureTopicExists(TopicName);
            EnsureTopicClientExists(TopicName);

            var json = JsonConvert.SerializeObject(message);

            var brokeredMessage = new BrokeredMessage(json)
            {
                ContentType = "application/json",
                Label = message.GetType().FullName
            };

            brokeredMessage.Properties[PropertyForTypeName] = message.GetType().Name;
            brokeredMessage.Properties[PropertyForTypeFullName] = message.GetType().FullName;

            using (var scope = CustomTransactionScope.GetTransactionSuppressedScope()) 
            {
                _topicClient.Send(brokeredMessage);
                scope.Complete();
            }
        }

        public void Subscribe<TSubscriber>() where TSubscriber : IBusSubscriber, new()
        {
            Subscribe(() => new TSubscriber());
        }

        public void Subscribe<TSubscriber>(Func<TSubscriber> subscriberFactory) where TSubscriber : IBusSubscriber
        {
            var subscriberType = typeof(TSubscriber);
            EnsureTopicExists(TopicName);
            
            var subscriptionName = EnsureSubscriptionExists(TopicName, subscriberType);

            var subscriptionClient = SubscriptionClient.CreateFromConnectionString(
                    GetServiceBusConnectionString(),
                    TopicName,
                    subscriptionName);

            var onMessageOptions = new OnMessageOptions
            {
                MaxConcurrentCalls = 1,
                AutoComplete = false
            };

            onMessageOptions.ExceptionReceived += MessageReceivingException;

            subscriptionClient.OnMessageAsync((message) => ProcessBrokeredMessage(subscriberFactory, message), onMessageOptions);
            SubscriberClients.Add(subscriberType, subscriptionClient);
        }

        private void MessageReceivingException(object sender, ExceptionReceivedEventArgs e)
        {
            if (e.Exception != null)
            {
                _logger.Error("SubscriptionClient raised an exception", e.Exception);
            }
        }

        public void Dispose()
        {
            foreach (var subscriberType in SubscriberClients)
            {
                Unsubscribe(subscriberType.Key);
            }

            if (_topicClient != null)
            {
                _topicClient.Close();
            }
        }

        public void Unsubscribe(Type subscriberType)
        {
            // this works in assumption that we will have only one subscriber instance with that type in current process
            if (SubscriberClients.ContainsKey(subscriberType))
            {
                var subscriptionClient = SubscriberClients[subscriberType];
                subscriptionClient.Close();
            }
        }

        protected async Task ProcessBrokeredMessage<TSubscriber>(Func<TSubscriber> subscriberFactory, BrokeredMessage brokeredMessage) where TSubscriber : IBusSubscriber
        {
            Type messageType = null;
            var subscriberType = typeof(TSubscriber);

            await _syncService.ExecuteAsync("WindowsServiceBus_" + subscriberType.Name, async () =>
            {
                try
                {
                    messageType = GetMessageType(brokeredMessage.Properties[PropertyForTypeFullName].ToString());
                    _logger.Debug(string.Format("{1}: processing '{0}' message ...", messageType.Name,
                        subscriberType.Name));

                    var message = JsonConvert.DeserializeObject(brokeredMessage.GetBody<string>(), messageType);

                    var subscriber = subscriberFactory();
                    var subscriberMethod = subscriber.GetType().GetMethod("Consume", new[] { messageType });
                    subscriberMethod.Invoke(subscriber, new object[] { message });

                    await brokeredMessage.CompleteAsync();

                    _logger.Debug(string.Format("{1}: processing '{0}' message finished.", message.GetType().Name,
                        subscriberType.Name));
                }
                catch (Exception ex)
                {
                    _logger.Error(string.Format("{1}: error processing '{0}' message: {2}.",
                        messageType == null ? "unknown" : messageType.Name, subscriberType.Name, ex));
                    await brokeredMessage.AbandonAsync();
                    throw;
                }
            });
        }

        private Type GetMessageType(string typeName)
        {
            if (_messageTypes.ContainsKey(typeName))
            {
                return _messageTypes[typeName];
            }

            var type = Type.GetType(typeName);
            if (type == null)
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(assembly => assembly.FullName.StartsWith("AFT.RegoV2.")).ToList();
                foreach (var a in assemblies)
                {
                    type = a.GetType(typeName);
                    if (type != null)
                    {
                        _messageTypes[typeName] = type;
                        break;
                    }
                }
            }

            if (!_messageTypes.ContainsKey(typeName))
            {
                throw new ArgumentException("Type {0} can not be created".Args(typeName));
            }

            return _messageTypes[typeName];
        }

        private NamespaceManager GetNamespaceManager()
        {
            if (_namespaceManager == null)
            {
                var connectionString = GetServiceBusConnectionString();
                _namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);
            }

            return _namespaceManager;
        }

        private string GetServiceBusConnectionString()
        {
            return _serviceBusConnectionString ??
                   (_serviceBusConnectionString = _connectionStringProvider.GetConnectionString());


        }

        private void EnsureTopicExists(string topicName)
        {
            if (_topicExistanceChecked)
            {
                return;
            }

            using (var scope = CustomTransactionScope.GetTransactionSuppressedScope())
            {
                _syncService.Execute("WindowsServiceBus_" + topicName, () =>
                {
                    var namespaceManager = GetNamespaceManager();
                    if (!namespaceManager.TopicExists(topicName))
                    {
                        _logger.Debug("Attempting to create topic {0}...".Args(topicName));

                        var topicDescription = new TopicDescription(topicName);
                        namespaceManager.CreateTopic(topicDescription);

                        _logger.Debug("Topic {0} created.".Args(topicName));
                    }

                    _topicExistanceChecked = true;
                });

                scope.Complete();
            }
        }

        private void EnsureTopicClientExists(string topicName)
        {
            if (_topicClient != null)
            {
                return;
            }

            var connectionString = GetServiceBusConnectionString();
            _topicClient = TopicClient.CreateFromConnectionString(connectionString, topicName);
        }

        private string EnsureSubscriptionExists(string topicName, Type subscriberType)
        {
            string subscriptionName = subscriberType.Name;

            var key = topicName + subscriptionName;
            if (_subscriptionsWithCheckedExistance.Contains(key))
            {
                return subscriptionName;
            }
            
            using (var scope = CustomTransactionScope.GetTransactionSuppressedScope())
            {
                _syncService.Execute("WindowsServiceBus_" + subscriptionName, () =>
                {
                    var types = subscriberType
                        .GetInterfaces()
                        .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IConsumes<>))
                        .SelectMany(i => i.GetGenericArguments())
                        .OrderBy(x => x.FullName)
                        .ToList();

                    var namespaceManager = GetNamespaceManager();
                    if (!namespaceManager.SubscriptionExists(topicName, subscriptionName))
                    {
                        _logger.Debug("Attempting to create subscription for {0} in topic {1}...".Args(subscriptionName, topicName));

                        var subscriptionDescription = new SubscriptionDescription(topicName, subscriptionName);
                        namespaceManager.CreateSubscription(subscriptionDescription);

                        _logger.Debug("Subscription for {0} in topic {1} created.".Args(subscriptionName, topicName));
                    }

                    UpsertSubscriptionRules(topicName, subscriptionName, types);
                    _subscriptionsWithCheckedExistance.Add(key);

                });

                scope.Complete();
            }

            return subscriptionName;
        }

        private void UpsertSubscriptionRules(string topicName, string subscriptionName, List<Type> filteredMessageTypes)
        {
            var namespaceManager = GetNamespaceManager();
            var connectionString = GetServiceBusConnectionString();
            var subscriptionClient = SubscriptionClient.CreateFromConnectionString(connectionString, topicName, subscriptionName);

            var rules = namespaceManager.GetRules(topicName, subscriptionName).ToList();
            var obsoleteRules = rules.Where(rule => filteredMessageTypes.All(type => type.Name != rule.Name));
            var newTypes = filteredMessageTypes.Where(type => rules.All(rule => rule.Name != type.Name));

            obsoleteRules.ForEach(rule => subscriptionClient.RemoveRule(rule.Name));
            newTypes.ForEach(type =>
            {
                var filter = new SqlFilter(string.Format("{0} = '{1}'", PropertyForTypeName, type.Name));
                var rule = new RuleDescription(type.Name, filter);
                subscriptionClient.AddRule(rule);
            });
        }
    }
}