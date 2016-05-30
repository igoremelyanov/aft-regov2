using System;
using System.Web;
using System.Web.Http;
using AFT.RegoV2.GameApi.Services;
using AFT.RegoV2.Infrastructure.DataAccess.Base;
using AFT.RegoV2.Infrastructure.Synchronization;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Shared.Constants;
using log4net.Config;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.GameApi
{
    public class Global : HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            var container = GameApiFactory.Default.Container;
            var synchronizationService = container.Resolve<SynchronizationService>();
            synchronizationService.Execute("WinService", () =>
            {
                var repositoryBase = container.Resolve<IRepositoryBase>();
                if (!repositoryBase.IsDatabaseSeeded())
                    throw new RegoException(Messages.DbExceptionMessage);
            });

            GlobalConfiguration.Configure(WebApiConfig.Register);
            XmlConfigurator.Configure();
        }
    }
}