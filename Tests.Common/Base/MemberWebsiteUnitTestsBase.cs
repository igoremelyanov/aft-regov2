using Microsoft.Practices.Unity;

namespace AFT.RegoV2.Tests.Common.Base
{
    public abstract class MemberWebsiteUnitTestsBase : SingleProcessTestsBase
    {
        protected override IUnityContainer CreateContainer()
        {
            return new MemberWebsiteUnitTestContainerFactory().CreateWithRegisteredTypes();
        }
    }
}