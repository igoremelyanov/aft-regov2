using AFT.RegoV2.Infrastructure.DataAccess;
using AFT.RegoV2.Infrastructure.DataAccess.Base;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.AdminApi.Tests.Integration.Core
{
    public class AdminApiTestsBase
    {
        public IUnityContainer Container { get; set; }

        public AdminApiTestsBase()
        {
            Container = new AdminApiIntegrationTestContainerFactory().CreateWithRegisteredTypes();
            var repositoryBase = Container.Resolve<IRepositoryBase>();

            if (!repositoryBase.IsDatabaseSeeded())
                Container.Resolve<ApplicationSeeder>().Seed();
        }
    }
}