using AFT.RegoV2.RegoBus.Interfaces;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Common.Base
{
    /// <summary>
    /// Use this base class for tests which require multiprocess communication with third-party components like database or message bus
    /// Such tests are integration tests by nature, this is why we're marking them with "Integration" category.
    /// </summary>
    [Category("Integration"), Category("Multiprocess")]
    public abstract class MultiprocessTestsBase : ContainerTestsBase
    {
        protected override IUnityContainer CreateContainer()
        {
            return new MultiprocessProcessTestContainerFactory().CreateWithRegisteredTypes();
        }

        public override void AfterEach()
        {
            base.AfterEach();

            Container.Resolve<IServiceBus>().Dispose();
        }
    }
}