using System;
using System.Net.Http;
using System.Net.Http.Headers;
using AFT.RegoV2.ApplicationServices.Payment;
using AFT.RegoV2.ApplicationServices.Report;
using AFT.RegoV2.Bonus.Api.Interface;
using AFT.RegoV2.Bonus.Api.Interface.Proxy;
using AFT.RegoV2.BoundedContexts.Report;
using AFT.RegoV2.Core.Auth.Interface;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Common;
using AFT.RegoV2.Core.Common.EventStore;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Shared.Logging;
using AFT.RegoV2.Core.Fraud;
using AFT.RegoV2.Core.Fraud.ApplicationServices;
using AFT.RegoV2.Core.Fraud.ApplicationServices.Duplicate_mechanism;
using AFT.RegoV2.Core.Fraud.Interface.ApplicationServices;
using AFT.RegoV2.Core.Fraud.Services;
using AFT.RegoV2.Core.Fraud.Validations;
using AFT.RegoV2.Core.Game.ApplicationServices;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Core.Payment.ApplicationServices;
using AFT.RegoV2.Core.Payment.Interface.ApplicationServices;
using AFT.RegoV2.Core.Payment.Validators;
using AFT.RegoV2.Core.Player.ApplicationServices;
using AFT.RegoV2.Core.Security.ApplicationServices;
using AFT.RegoV2.Core.Security.ApplicationServices.IpRegulations;
using AFT.RegoV2.Core.Security.Interfaces;
using AFT.RegoV2.Core.Payment;
using AFT.RegoV2.Core.Security.Interface.ApplicationServices;
using AFT.RegoV2.Core.Settings.Interface.Interfaces;
using AFT.RegoV2.Core.Settings.Interface.Providers;
using AFT.RegoV2.Infrastructure.Aspects;
using AFT.RegoV2.Infrastructure.DataAccess.Base;
using AFT.RegoV2.Infrastructure.DataAccess.Documents;
using AFT.RegoV2.Infrastructure.DataAccess.Event;
using AFT.RegoV2.Infrastructure.DataAccess.Fraud;
using AFT.RegoV2.Infrastructure.DataAccess.Payment.Repository;
using AFT.RegoV2.Infrastructure.DataAccess.Report;
using AFT.RegoV2.Infrastructure.DataAccess.Security;
using AFT.RegoV2.Infrastructure.GameIntegration;
using AFT.RegoV2.Infrastructure.Mail;
using AFT.RegoV2.Infrastructure.Providers;
using AFT.RegoV2.Infrastructure.Sms;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Shared.Caching;
using AFT.RegoV2.Shared.Synchronization;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.InterceptionExtension;

namespace AFT.RegoV2.Infrastructure.DependencyResolution
{
    public interface IContainerFactory
    {
        IUnityContainer CreateWithRegisteredTypes();
        void RegisterTypes(IUnityContainer container);
    }

    /// <summary>
    /// Provides application-wide initialization logic for IoC container
    /// </summary>
    public class ApplicationContainerFactory
    {
        public IUnityContainer CreateWithRegisteredTypes()
        {
            var container = new UnityContainer();
            RegisterTypes(container);
            return container;
        }

        public virtual void RegisterTypes(IUnityContainer container)
        {
            RegisterInfrastructureTypes(container);

            RegisterReportTypes(container);

            RegisterBrandTypes(container);

            RegisterContentTranslationTypes(container);

            RegisterAuthTypes(container);

            RegisterSecurityTypes(container);

            RegisterPlayerTypes(container);

            RegisterPaymentTypes(container);

            RegisterFraudTypes(container);

            RegisterBonusTypes(container);

            RegisterMessagingTypes(container);

            RegisterWalletTypes(container);

            RegisterGameTypes(container);

            RegisterSettingsTypes(container);

            RegisterDocumentTypes(container);

            RegisterExternalTypes(container);

            //TODO: consider moving out this line to some other place as it has side effects
            ServiceLocator.SetLocatorProvider(() => new UnityServiceLocator(container));
        }

        public virtual InterceptionBehavior GetSecurityInterceptionBehavior()
        {
            return new InterceptionBehavior<DummyInterceptionBehavior>();
        }

        protected virtual InterceptionBehavior GetBrandCheckAspect()
        {
            return new InterceptionBehavior<DummyInterceptionBehavior>();
        }

        private void RegisterContentTranslationTypes(IUnityContainer container)
        {
            container.RegisterType<ContentTranslationCommands>(
                new Interceptor<TransparentProxyInterceptor>(),
                GetSecurityInterceptionBehavior()
                );

            container.RegisterType<ContentTranslationQueries>(
                new Interceptor<TransparentProxyInterceptor>(),
                GetSecurityInterceptionBehavior()
                );
        }

        private void RegisterReportTypes(IUnityContainer container)
        {
            container.RegisterType<IReportRepository, ReportRepository>();
            container.RegisterType<ReportQueries>(
                new Interceptor<TransparentProxyInterceptor>(),
                GetSecurityInterceptionBehavior()
                );
        }

        private void RegisterBrandTypes(IUnityContainer container)
        {
            container.Resolve<BrandContainerFactory>().RegisterTypes(container);
        }

        private void RegisterAuthTypes(IUnityContainer container)
        {
            container.Resolve<AuthContainerFactory>().RegisterTypes(container);
        }

        private void RegisterSecurityTypes(IUnityContainer container)
        {
            container.RegisterType<ISecurityRepository, SecurityRepository>(new PerHttpRequestLifetime());
            
            container.AddNewExtension<Interception>();

            container.RegisterType<IAdminCommands, AdminCommands>(
                new PerHttpRequestLifetime(),
                new Interceptor<TransparentProxyInterceptor>(),
                GetSecurityInterceptionBehavior(),
                GetBrandCheckAspect()
                );

            container.RegisterType<IAdminQueries, AdminQueries>(
                new PerHttpRequestLifetime(),
                new Interceptor<TransparentProxyInterceptor>(),
                GetSecurityInterceptionBehavior(),
                GetBrandCheckAspect()
                );

            container.RegisterType<RoleService>(
                new PerHttpRequestLifetime(),
                new Interceptor<TransparentProxyInterceptor>(),
                GetSecurityInterceptionBehavior()
                );

            container.RegisterType<BackendIpRegulationService>(
                new PerHttpRequestLifetime(),
                new Interceptor<TransparentProxyInterceptor>(),
                GetSecurityInterceptionBehavior()
                );

            container.RegisterType<BrandIpRegulationService>(
                new PerHttpRequestLifetime(),
                new Interceptor<TransparentProxyInterceptor>(),
                GetSecurityInterceptionBehavior(),
                GetBrandCheckAspect()
                );

            container.RegisterType<BrandQueries>(
                new Interceptor<TransparentProxyInterceptor>(),
                GetSecurityInterceptionBehavior(),
                new InterceptionBehavior<BrandFilterAspect>()
                );

            container.RegisterType<BrandCommands>(
                new Interceptor<TransparentProxyInterceptor>(),
                GetSecurityInterceptionBehavior(),
                GetBrandCheckAspect()
                );

            container.RegisterType<IPaymentQueries,PaymentQueries>(
                new Interceptor<TransparentProxyInterceptor>(),
                GetSecurityInterceptionBehavior(),
                GetBrandCheckAspect()
                );

            container.RegisterType<PlayerQueries>(
                new Interceptor<TransparentProxyInterceptor>(),
                GetSecurityInterceptionBehavior(),
                GetBrandCheckAspect()
                );

            container.RegisterType<PlayerCommands>(
                new Interceptor<TransparentProxyInterceptor>(),
                GetSecurityInterceptionBehavior(),
                GetBrandCheckAspect()
                );

            container.RegisterType<LoggingService>(new PerHttpRequestLifetime());
            container.RegisterType<PlayerActivityLogEventHandlersBase>(
                new Interceptor<TransparentProxyInterceptor>(),
                GetSecurityInterceptionBehavior()
                );

            container.RegisterType<LicenseeCommands>(
                new Interceptor<TransparentProxyInterceptor>(),
                GetSecurityInterceptionBehavior()
                );

            container.RegisterType<LicenseeQueries>(
                new Interceptor<TransparentProxyInterceptor>(),
                GetSecurityInterceptionBehavior()
                );

            container.RegisterType<GameCommands>(
                new Interceptor<TransparentProxyInterceptor>(),
                GetSecurityInterceptionBehavior()
                );

            container.RegisterType<ReportQueries>(
                new Interceptor<TransparentProxyInterceptor>(),
                GetSecurityInterceptionBehavior()
                );

            container.RegisterType<IPaymentLevelQueries,PaymentLevelQueries>(
               new Interceptor<TransparentProxyInterceptor>(),
               GetSecurityInterceptionBehavior()
               );

            container.RegisterType<IPaymentLevelCommands,PaymentLevelCommands>(
                new Interceptor<TransparentProxyInterceptor>(),
                GetSecurityInterceptionBehavior(),
                GetBrandCheckAspect()
                );

            container.RegisterType<IPaymentSettingsQueries,PaymentSettingsQueries>(
                new Interceptor<TransparentProxyInterceptor>(),
                GetSecurityInterceptionBehavior(),
                GetBrandCheckAspect()
                );

            container.RegisterType<IPaymentSettingsCommands,PaymentSettingsCommands>(
                new Interceptor<TransparentProxyInterceptor>(),
                GetSecurityInterceptionBehavior(),
                GetBrandCheckAspect()
                );

            container.RegisterType<ITransferSettingsCommands,TransferSettingsCommands>(
                new Interceptor<TransparentProxyInterceptor>(),
                GetSecurityInterceptionBehavior(),
                GetBrandCheckAspect()
                );
        }

        private void RegisterPlayerTypes(IUnityContainer container)
        {
            container.Resolve<PlayerContainerFactory>().RegisterTypes(container);
        }

        private void RegisterDocumentTypes(IUnityContainer container)
        {
            container.RegisterType<IDocumentsRepository, DocumentsRepository>();
            container.RegisterType<IDocumentService, DocumentsService>();
        }

        private void RegisterPaymentTypes(IUnityContainer container)
        {
            container.RegisterType<IPaymentRepository, PaymentRepository>(new PerHttpRequestLifetime());
            container.RegisterType<IPaymentQueries, PaymentQueries>();
            container.RegisterType<IPaymentSettingsValidationService, PaymentSettingsValidationService>();
            container.RegisterType<ITransferFundCommands, TransferFundCommands>();
            container.RegisterType<IWithdrawalService,WithdrawalService>(
                new Interceptor<TransparentProxyInterceptor>(),
                GetSecurityInterceptionBehavior(),
                GetBrandCheckAspect()
                );
            container.RegisterType<IBasePaymentQueries, PaymentQueries>();
            container.RegisterType<ICurrencyCommands, CurrencyCommands>();
            container.RegisterType<IOnlineDepositCommands, OnlineDepositCommands>();
            container.RegisterType<IOnlineDepositQueries, OnlineDepositQueries>();
            container.RegisterType<ICurrencyExchangeCommands, CurrencyExchangeCommands>();
            container.RegisterType<IPaymentGatewaySettingsCommands, PaymentGatewaySettingsCommands>();
            container.RegisterType<IPaymentGatewaySettingsQueries, PaymentGatewaySettingsQueries>();
            container.RegisterType<IWalletQueries, WalletQueries>();
            container.RegisterType<IBankAccountCommands, BankAccountCommands>();
            container.RegisterType<IBankAccountQueries, BankAccountQueries>();
            container.RegisterType<IBankCommands, BankCommands>();
            container.RegisterType<IBankQueries, BankQueries>();
            container.RegisterType<IPaymentCommands, PaymentCommands>();
            container.RegisterType<IPaymentLevelCommands, PaymentLevelCommands>();
            container.RegisterType<IPaymentLevelQueries, PaymentLevelQueries>();
            container.RegisterType<IPaymentSettingsCommands, PaymentSettingsCommands>();
            container.RegisterType<IPaymentSettingsQueries, PaymentSettingsQueries>();
            container.RegisterType<IPlayerBankAccountCommands, PlayerBankAccountCommands>();
            container.RegisterType<IPlayerBankAccountQueries, PlayerBankAccountQueries>();
            container.RegisterType<ITransferSettingsCommands, TransferSettingsCommands>();
            container.RegisterType<IOfflineDepositCommands, OfflineDepositCommands>();
            container.RegisterType<IOfflineDepositQueries, OfflineDepositQueries>();
        }

        private void RegisterFraudTypes(IUnityContainer container)
        {
            container.RegisterType<IFraudRepository, FraudRepository>(new PerHttpRequestLifetime());
            container.RegisterType<IWagerConfigurationQueries, WagerConfigurationQueries>(
                new Interceptor<TransparentProxyInterceptor>(),
                GetSecurityInterceptionBehavior());
            container.RegisterType<IWagerConfigurationCommands, WagerConfigurationCommands>(
                new Interceptor<TransparentProxyInterceptor>(),
                GetSecurityInterceptionBehavior(),
                GetBrandCheckAspect());
            container.RegisterType<ITransferFundValidationService, TransferFundValidationService>();
            container.RegisterType<IOfflineWithdrawalValidationService, OfflineWithdrawalValidationService>(new PerHttpRequestLifetime());
            container.RegisterType<IFundsValidationService, FundsValidationService>();
            container.RegisterType<IAWCValidationService, AWCValidationService>();
            container.RegisterType<IAVCValidationService, AVCValidationService>();
            container.RegisterType<IRiskProfileCheckValidationService, RiskProfileCheckValidationService>();
            container.RegisterType<IRiskProfileCheckQueries, RiskProfileCheckQueries>();
            container.RegisterType<IWithdrawalVerificationLogsQueues, WithdrawalVerificationLogsQueues>();
            container.RegisterType<IWithdrawalVerificationLogsCommands, WithdrawalVerificationLogsCommands>();
            container.RegisterType<IDuplicationMatchingService, ExactDuplicationMatchingService>();
            container.RegisterType<IDuplicateScoreService, DuplicateScoreService>();
            container.RegisterType<IDuplicationService, DuplicationService>();
            container.RegisterType<IFraudPlayerQueries, FraudPlayerQueries>();
            container.RegisterType<IFraudTypeCommands, FraudTypeCommands>();
            container.RegisterType<IAVCConfigurationCommands, AVCConfigurationCommands>(
                new Interceptor<TransparentProxyInterceptor>(),
                GetSecurityInterceptionBehavior(),
                GetBrandCheckAspect()
                );
            container.RegisterType<IAVCConfigurationQueries, AVCConfigurationQueries>(
                new Interceptor<TransparentProxyInterceptor>(),
                GetSecurityInterceptionBehavior()
                );
            container.RegisterType<IRebateWageringValidationService, RebateWageringValidationService>();
            container.RegisterType<IManualAdjustmentWageringValidationService, ManualAdjustmentWageringValidationService>();
            container.RegisterType<IBonusWageringWithdrawalValidationService, BonusWageringWithdrawalValidationService>();
            container.RegisterType<IRiskLevelQueries, RiskLevelQueries>(
                new Interceptor<TransparentProxyInterceptor>(),
                GetSecurityInterceptionBehavior()
                );
            container.RegisterType<IRiskLevelCommands, RiskLevelCommands>(
                new Interceptor<TransparentProxyInterceptor>(),
                GetSecurityInterceptionBehavior(),
                GetBrandCheckAspect()
                );
            container.RegisterType<IWinningRuleQueries, WinningRuleQueries>();
            container.RegisterType<IPaymentSettingsValidator, PaymentSettingsValidator>();
            container.RegisterType<IOnlineDepositValidator, OnlineDepositValidator>();
            container.RegisterType<IRiskProfileCheckQueries, RiskProfileCheckQueries>();
            container.RegisterType<IRiskProfileCheckCommands, RiskProfileCheckCommands>();
            container.RegisterType<DuplicateMechanismCommands>();
            container.RegisterType<DuplicateMechanismQueries>();

            container.RegisterType<FraudPlayerQueries>();
            container.RegisterType<SignUpFraudTypeCommands>();
            container.RegisterType<SignUpFraudTypeQueries>();
            container.RegisterType<SignUpFraudTypeValidator>();
        }

        private void RegisterBonusTypes(IUnityContainer container)
        {
            const string httpClientName = "bonusHttpClient";
            container.RegisterType<HttpClient>(httpClientName, new ContainerControlledLifetimeManager(), 
                new InjectionFactory(unityContainer =>
                {
                    var bonusApiUrl = unityContainer.Resolve<ICommonSettingsProvider>().GetBonusApiUrl();
                    return new HttpClient
                    {
                        BaseAddress = new Uri(bonusApiUrl),
                        DefaultRequestHeaders =
                        {
                            Accept = { new MediaTypeWithQualityHeaderValue("application/json") }
                        }
                    };
                }));
            container.RegisterType<IBonusApiProxy, ApiProxy>(new InjectionFactory(unityContainer =>
            {
                var httpClient = unityContainer.Resolve<HttpClient>(httpClientName);
                var credentials = unityContainer.Resolve<ICommonSettingsProvider>().GetBonusApiCredentials();
                var actorId = unityContainer.Resolve<IActorInfoProvider>().Actor.Id;
                return new ApiProxy(httpClient, credentials.ClientId, credentials.ClientSecret, actorId);
            }));
        }

        private void RegisterMessagingTypes(IUnityContainer container)
        {
            container.Resolve<MessagingContainerFactory>().RegisterTypes(container);
        }

        private void RegisterWalletTypes(IUnityContainer container)
        {
            container.Resolve<WalletContainerFactory>().RegisterTypes(container);
        }

        private void RegisterGameTypes(IUnityContainer container)
        {
            container.Resolve<GameContainerFactory>().RegisterTypes(container);
        }

        private void RegisterSettingsTypes(IUnityContainer container)
        {
            container.Resolve<SettingsContainerFactory>().RegisterTypes(container);
        }

        private void RegisterInfrastructureTypes(IUnityContainer container)
        {
            container.RegisterType<ILog, LogDecorator>(new ContainerControlledLifetimeManager());
            container.RegisterType<ICacheManager, CacheManager>(new ContainerControlledLifetimeManager());

            RegisterEventBus(container);
            RegisterServiceBus(container);

            container.RegisterType<IFileStorage, FileSystemStorage>();
            container.RegisterType<IEmailNotifier, EmailNotifier>();
            container.RegisterType<ISmsNotifier, SmsNotifier>();
            container.RegisterType<IEventRepository, EventRepository>();

            container.RegisterType<ISynchronizationService, SynchronizationService>();
            container.RegisterType<IRepositoryBase, RepositoryBase>();

            container.RegisterType<ICommonSettingsProvider, CommonSettingsProvider>();
            container.RegisterType<IServiceBusSettingsProvider, ServiceBusSettingsProvider>();
        }

        private void RegisterExternalTypes(IUnityContainer container)
        {
            container.RegisterType<IBrandApiClientFactory, BrandApiClientFactory>();
            container.RegisterType<IProductApiClientFactory, ProductApiClientFactory>();
            container.RegisterType<ILoggingService, LoggingService>();
        }

        private void RegisterServiceBus(IUnityContainer container)
        {
            container.Resolve<ServiceBusContainerFactory>().RegisterTypes(container);
        }

        private void RegisterEventBus(IUnityContainer container)
        {
            container.Resolve<EventBusContainerFactory>().RegisterTypes(container);
        }
    }
}