using Microsoft.Practices.Unity;

namespace AFT.RegoV2.MemberApi.Tests.Integration.Core
{
    public class TestStartup : Startup
    {
        public static IUnityContainer Container;

        protected override IUnityContainer GetUnityContainer()
        {
            return Container;
        }
    }
}
