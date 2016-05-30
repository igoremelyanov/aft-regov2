using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using AFT.RegoV2.Infrastructure.DataAccess.Base;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Shared.Constants;
using AFT.RegoV2.Shared.Logging;
using AFT.RegoV2.Shared.Synchronization;
using FakeUGS;
using FakeUGS.App_Start;
using FakeUGS.Core.DataAccess;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.FakeUGS
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        public static IUnityContainer Container { get; private set; }

        protected void Application_Start()
        {
            Container = UnityConfig.GetConfiguredContainer();

            var logger = Container.Resolve<ILog>();
            logger.Info("Starting FakeUGS application ...");

            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            

            var synchronizationService = Container.Resolve<ISynchronizationService>();
            synchronizationService.Execute("WinService", () =>
            {
                var repositoryBase = Container.Resolve<IRepositoryBase>();
                if (!repositoryBase.IsDatabaseSeeded())
                    throw new RegoException(Messages.DbExceptionMessage);

                Container.Resolve<DataSeeder>().Seed();
            });

            logger.Info("FakeUGS application started successfully.");
        }
    }
}
