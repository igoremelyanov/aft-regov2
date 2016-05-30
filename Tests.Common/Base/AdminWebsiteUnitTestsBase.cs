using AFT.RegoV2.Tests.Common.Containers;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.Tests.Common.Base
{
    public abstract class AdminWebsiteUnitTestsBase : UnitTestsBase
    {
        protected override IUnityContainer CreateContainer()
        {
            return new AdminWebsiteUnitTestContainerFactory().CreateWithRegisteredTypes();
        }
    }
}