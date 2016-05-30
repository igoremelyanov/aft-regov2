using AFT.RegoV2.Core.Messaging;
using AFT.RegoV2.Core.Messaging.ApplicationServices;
using AFT.RegoV2.Core.Messaging.Interface.ApplicationServices;
using AFT.RegoV2.Infrastructure.DataAccess.Messaging;
using AFT.RegoV2.Shared;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.Infrastructure.DependencyResolution
{
    public class MessagingContainerFactory
    {
        public void RegisterTypes(IUnityContainer container)
        {
            container.RegisterType<IMessagingRepository, MessagingRepository>(new PerHttpRequestLifetime());
            container.RegisterType<IMessageTemplateService, MessageTemplateService>(new PerHttpRequestLifetime());
            container.RegisterType<IMessageTemplateCommands, MessageTemplateCommands>();
            container.RegisterType<IMessageTemplateQueries, MessageTemplateQueries>();
            container.RegisterType<IMassMessageQueries, MassMessageQueries>();
            container.RegisterType<IMassMessageCommands, MassMessageCommands>();
        }
    }
}
