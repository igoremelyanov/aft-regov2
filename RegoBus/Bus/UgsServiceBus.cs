using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Settings.Interface.Interfaces;
using AFT.RegoV2.RegoBus.Interfaces;
using AFT.RegoV2.Shared.Logging;
using AFT.RegoV2.Shared.Synchronization;
using AFT.RegoV2.Shared.Utils;
using AFT.UGS.Core.BaseModels.Bus;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace AFT.RegoV2.RegoBus.Bus
{
    public class UgsServiceBus : IUgsServiceBus
    {
        private readonly ILog _logger;
        private readonly ISynchronizationService _syncService;
        private readonly IUgsServiceBusSettingsProvider _connectionStringProvider;

        private string _connectionString;
        protected Dictionary<Type, SubscriptionClient> SubscriberClients;

        private TopicClient _topicClient;
        private TopicClient TopicClient
        {
            get
            {
                if (_topicClient != null) return _topicClient;

                var connectionString = GetServiceBusConnectionString();
                _topicClient = TopicClient.CreateFromConnectionString(connectionString, TopicName);
                return _topicClient;
            }
        }

        private NamespaceManager _namespaceManager = null;

        private Dictionary<string, Type> _messageTypes = new Dictionary<string, Type>();

        protected const string TopicName = "topicrego";
        private const string SubscriptionName = "main";

        public UgsServiceBus(ILog logger, ISynchronizationService syncService,
            IUgsServiceBusSettingsProvider connectionStringProvider)
        {
            _logger = logger;
            _syncService = syncService;
            _connectionStringProvider = connectionStringProvider;

            SubscriberClients = new Dictionary<Type, SubscriptionClient>();
        }

        public void PublishMessage<T>(T message) where T : class, IMessage
        {
            PublishExternalMessage(message);
        }

        public void PublishExternalMessage<T>(T message) where T : class
        {
            var brokeredMessage = new BrokeredMessage(message)
            {
                ContentType = message.GetType().AssemblyQualifiedName
            };

            using (var scope = CustomTransactionScope.GetTransactionSuppressedScope())
            {
                TopicClient.Send(brokeredMessage);
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

            TryToManageServiceBus<TSubscriber>();

            // TODO: Use UGS' listener
            var subscriptionClient = SubscriptionClient.CreateFromConnectionString(
                    GetServiceBusConnectionString(),
                    TopicName,
                    SubscriptionName);

            var onMessageOptions = new OnMessageOptions
            {
                MaxConcurrentCalls = 1,
                AutoComplete = false
            };

            onMessageOptions.ExceptionReceived += MessageReceivingException;

            try
            {
                subscriptionClient.OnMessageAsync((message) => ProcessBrokeredMessage(subscriberFactory, message),
                    onMessageOptions);
            }
            catch (Exception ex)
            {
                _logger.Error("Unable to subscribe to UGS Service bus. Missing subscription?", ex);
                throw;
            }
            SubscriberClients.Add(subscriberType, subscriptionClient);
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

        public void Dispose()
        {
            foreach (var subscriberType in SubscriberClients)
            {
                Unsubscribe(subscriberType.Key);
            }

            TopicClient?.Close();
        }


        private string GetServiceBusConnectionString()
        {
            return _connectionString ?? (_connectionString = _connectionStringProvider.GetUgsBusConnectionString());
        }

        protected async Task ProcessBrokeredMessage<TSubscriber>(Func<TSubscriber> subscriberFactory, BrokeredMessage brokeredMessage) where TSubscriber : IBusSubscriber
        {
            Type messageType = null;
            var subscriberType = typeof(TSubscriber);

            await _syncService.ExecuteAsync("WindowsServiceBus_" + subscriberType.Name, async () =>
            {
                try
                {
                    messageType = GetMessageType(brokeredMessage.ContentType);
                    _logger.Debug(string.Format("{1}: processing '{0}' message ...", messageType.Name,
                        subscriberType.Name));

                    var message = GetBodyOfBase(brokeredMessage);

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
                    .Where(assembly => assembly.FullName.StartsWith("AFT.UGS.")).ToList();
                foreach (var a in assemblies)
                {
                    type = a.GetType(typeName);
                    if (type != null)
                    {
                        _messageTypes[typeName] = type;
                        break;
                    }
                }

                if (!_messageTypes.ContainsKey(typeName))
                {
                    throw new ArgumentException("Type {0} can not be created".Args(typeName));
                }
            }
            else { 
                _messageTypes[typeName] = type;
}

            return _messageTypes[typeName];
        }

        private void MessageReceivingException(object sender, ExceptionReceivedEventArgs e)
        {
            if (e.Exception != null)
            {
                _logger.Error("UgsSubscriptionClient raised an exception", e.Exception);
            }
        }

        private static readonly ConcurrentDictionary<Type, DataContractSerializer> _serializerByType =
            new ConcurrentDictionary<Type, DataContractSerializer>();

        private static BusEvent GetBodyOfBase(BrokeredMessage bm)
        {
            var ct = bm.ContentType;
            Type bodyType = Type.GetType(ct, true);
            using (var stream = bm.GetBody<Stream>())
            {
                DataContractSerializer serializer = null;
                if (!_serializerByType.TryGetValue(bodyType, out serializer))
                {
                    serializer = new DataContractSerializer(bodyType);
                    _serializerByType.AddOrUpdate(bodyType, serializer, (t, s) => s);
                }
                XmlDictionaryReader reader = XmlDictionaryReader.CreateBinaryReader(stream, XmlDictionaryReaderQuotas.Max);
                return (BusEvent)serializer.ReadObject(reader);
            }
        }



        private void TryToManageServiceBus<T>()
        {
            try
            {
                EnsureTopicClientExists(TopicName);
                EnsureTopicExists(TopicName);
                EnsureSubscriptionExists(TopicName, typeof (T));
            }
            catch (Exception)
            {
                _logger.Warn("Unable to manage Ugs Service Bus...");
            }
        }


        private NamespaceManager GetNamespaceManager()
        {
            if (_namespaceManager != null) return _namespaceManager;

            var connectionString = GetServiceBusConnectionString();
            _namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);

            return _namespaceManager;
        }

        private void EnsureTopicClientExists(string topicName)
        {
            if (_topicClient != null)
                return;

            var connectionString = GetServiceBusConnectionString();
            _topicClient = TopicClient.CreateFromConnectionString(connectionString, topicName);
        }

        private bool _topicExistanceChecked = false;
        private void EnsureTopicExists(string topicName)
        {
            if (_topicExistanceChecked)
                return;

            using (var scope = CustomTransactionScope.GetTransactionSuppressedScope())
            {
                var namespaceManager = GetNamespaceManager();
                if (!namespaceManager.TopicExists(topicName))
                {
                    _logger.Debug("Fake Ugs Service Bus: Attempting to create topic {0}".Args(topicName));

                    var topicDescription = new TopicDescription(topicName);
                    namespaceManager.CreateTopic(topicDescription);

                    _logger.Debug("Fake Ugs Service Bus: Topic {0} created".Args(topicName));
                }

                _topicExistanceChecked = true;
                scope.Complete();
            }
        }

        private readonly List<string> _subscriptionsWithCheckedExistance = new List<string>();
        private string EnsureSubscriptionExists(string topicName, Type subscriberType)
        {
            var subscriptionName = SubscriptionName;
            var key = topicName + subscriptionName;
            if (_subscriptionsWithCheckedExistance.Contains(key))
            {
                return subscriptionName;
            }

            using (var scope = CustomTransactionScope.GetTransactionSuppressedScope())
            {
                var types = subscriberType
                        .GetInterfaces()
                        .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IConsumesExternal<>))
                        .SelectMany(i => i.GetGenericArguments())
                        .OrderBy(x => x.FullName)
                        .ToList();

                var namespaceManager = GetNamespaceManager();
                if (!namespaceManager.SubscriptionExists(topicName, subscriptionName))
                {
                    _logger.Debug("Attempting to create Fake UGS subscription for {0} in topic {1}...".Args(subscriptionName, topicName));

                    var subscriptionDescription = new SubscriptionDescription(topicName, subscriptionName);
                    namespaceManager.CreateSubscription(subscriptionDescription);

                    _logger.Debug("Fake UGS Subscription for {0} in topic {1} created.".Args(subscriptionName, topicName));
                }
                UpsertSubscriptionRules(topicName, subscriptionName, types);

                _subscriptionsWithCheckedExistance.Add(key);
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
                var filter = new SqlFilter("1=1");
                var rule = new RuleDescription(type.Name, filter);
                subscriptionClient.AddRule(rule);
            });
        }

    }
}
