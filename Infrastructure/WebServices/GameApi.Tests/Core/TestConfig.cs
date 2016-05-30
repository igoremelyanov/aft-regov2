using System.Configuration;

using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Infrastructure.DependencyResolution;
using AFT.RegoV2.Infrastructure.Providers;

using Microsoft.Practices.Unity;

namespace AFT.RegoV2.GameApi.Tests.Core
{
    public interface ITestConfig
    {
        string GameApiUrl { get; }
    }

    public sealed class TestConfig : ITestConfig
    {
        private ICommonSettingsProvider _settingsProvider;

        public TestConfig()
        {
            var container = new ApplicationContainerFactory().CreateWithRegisteredTypes();
            _settingsProvider = container.Resolve<ICommonSettingsProvider>();
        }

        string ITestConfig.GameApiUrl { get { return _settingsProvider.GetGameApiUrl(); } }
    }
}