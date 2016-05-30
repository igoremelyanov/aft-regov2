using System;
using System.Linq;
using AFT.RegoV2.Bonus.Core;
using AFT.RegoV2.Bonus.Infrastructure.DataAccess;
using AFT.RegoV2.Core.Auth;
using AFT.RegoV2.Core.Brand;
using AFT.RegoV2.Core.Messaging;
using AFT.RegoV2.Core.Fraud;
using AFT.RegoV2.Core.Game.Interface.Interfaces;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Core.Game.Services;
using AFT.RegoV2.Core.Messaging.ApplicationServices;
using AFT.RegoV2.Core.Messaging.Interface.ApplicationServices;
using AFT.RegoV2.Core.Security.ApplicationServices;
using AFT.RegoV2.Core.Security.Interfaces;
using AFT.RegoV2.Core.Payment;
using AFT.RegoV2.Infrastructure.DataAccess.Auth;
using AFT.RegoV2.Infrastructure.DataAccess.Brand;
using AFT.RegoV2.Infrastructure.DataAccess.Messaging;
using AFT.RegoV2.Infrastructure.DataAccess.Fraud;
using AFT.RegoV2.Infrastructure.DataAccess.Game;
using AFT.RegoV2.Infrastructure.DataAccess.Payment.Repository;
using AFT.RegoV2.Infrastructure.DataAccess.Security;
using AFT.RegoV2.Infrastructure.DependencyResolution;
using AFT.RegoV2.WinService.Workers;

using Microsoft.Practices.Unity;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using WinService.Workers;
using WinService.Workers.Fraud;

namespace AFT.RegoV2.WinService
{
    class WinServiceContainerFactory : ApplicationContainerFactory
    {
        public override void RegisterTypes(IUnityContainer container)
        {
            base.RegisterTypes(container);

            container.RegisterType<IJobFactory, UnityJobFactory>();
            container.RegisterInstance<ISchedulerFactory>(new StdSchedulerFactory());

            container.RegisterType<IWorker, SmsNotificationWorker>("SmsNotifications");
            container.RegisterType<IWorker, EmailNotificationWorker>("EmailNotifications");
            container.RegisterType<IWorker, BonusWorker>("BonusWorker");

            // Player Reports
            container.RegisterType<IWorker, PlayerReportWorker>("PlayerReport");
            container.RegisterType<IWorker, PlayerBetHistoryReportWorker>("PlayerBetHistoryReport");

            // Payment Reports
            container.RegisterType<IWorker, DepositReportWorker>("DepositReport");

            // Transaction Reports
            container.RegisterType<IWorker, PlayerTransactionReportWorker>("PlayerTransactionReport");

            // Brand Reports
            container.RegisterType<IWorker, BrandReportWorker>("BrandReport");
            container.RegisterType<IWorker, LicenseeReportWorker>("LicenseeReport");
            container.RegisterType<IWorker, LanguageReportWorker>("LanguageReport");
            container.RegisterType<IWorker, VipLevelReportWorker>("VipLevelReport");

            //=========================================
            // Fraud workers
            container.RegisterType<IWorker, FraudPaymentLevelWorker>("FraudPaymentLevelWorker");
            container.RegisterType<IWorker, FraudPlayerWorker>("FraudPlayerWorker");
            //=========================================

            //=========================================
            container.RegisterType<IUgsEventRetranslator, UgsEventRetranslator>();
            container.RegisterType<IWorker, GameIntegrationRepublishWorker>("Game Integration Republisher");

            //=========================================
            // Payment workers
            container.RegisterType<IWorker, PaymentWorker>("PaymentWorker");
            //=========================================

            container.RegisterType<IWorker, AdminActivityLogWorker>("AdminActivityLog");
            container.RegisterType<IWorker, AuthenticationLogWorker>("AuthenticationLog");
            container.RegisterType<IWorker, PlayerActivityLogWorker>("PlayerActivityLog");

            //=========================================
            container.RegisterType<IWorker, ErrorsLogWorker>("ErrorsLogWorker");

            //=========================================
            container.RegisterType<IWorker, EventPublisherWorker>("EventPublisher");

            ReregisterWithAppropriateLifetimeManagers(container);

            container.RegisterType<Func<IWorker[]>>(new InjectionFactory(unityContainer => new Func<IWorker[]>(() => unityContainer.ResolveAll<IWorker>().ToArray())));
        }

        private void ReregisterWithAppropriateLifetimeManagers(IUnityContainer container)
        {
            container.RegisterType<IBrandRepository, BrandRepository>(new PerResolveLifetimeManager());
            container.RegisterType<IPaymentRepository, PaymentRepository>(new PerResolveLifetimeManager());
            container.RegisterType<IBonusRepository, BonusRepository>(new PerResolveLifetimeManager());
            container.RegisterType<IFraudRepository, FraudRepository>(new PerResolveLifetimeManager());
            container.RegisterType<ISecurityRepository, SecurityRepository>(new PerResolveLifetimeManager());
            container.RegisterType<IAuthRepository, AuthRepository>(new PerResolveLifetimeManager());
            container.RegisterType<IFraudRepository, FraudRepository>(new PerResolveLifetimeManager());
            container.RegisterType<IMessagingRepository, MessagingRepository>(new PerResolveLifetimeManager());
            container.RegisterType<IGameRepository, GameRepository>(new PerResolveLifetimeManager());
            container.RegisterType<LoggingService>(new PerResolveLifetimeManager());
            container.RegisterType<IMessageTemplateService, MessageTemplateService>(new PerResolveLifetimeManager());
        }
    }
}