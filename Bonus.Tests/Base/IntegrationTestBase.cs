using AFT.RegoV2.Bonus.Core;
using AFT.RegoV2.Bonus.Infrastructure.DataAccess;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Bonus.Tests.Base
{
    [Category("Integration")]
    public abstract class IntegrationTestBase : BonusTestBase
    {
        protected override IUnityContainer CreateContainer()
        {
            var container = base.CreateContainer();

            container.RegisterType<IBonusRepository, BonusRepository>(new PerThreadLifetimeManager());

            return container;
        }
    }
}
