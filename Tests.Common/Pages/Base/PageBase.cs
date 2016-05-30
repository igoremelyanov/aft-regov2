using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Infrastructure.DependencyResolution;

using Microsoft.Practices.Unity;

namespace AFT.RegoV2.Tests.Common.Pages.Base
{
    public class PageBase
    {
        protected string AdminWebsiteUrl { get; private set; }
        protected string MemberWebsiteUrl { get; private set; }

        public PageBase()
        {
            var container = new ApplicationContainerFactory().CreateWithRegisteredTypes();
            var settingsProvider = container.Resolve<ICommonSettingsProvider>();

            AdminWebsiteUrl = settingsProvider.GetAdminWebsiteUrl();
            MemberWebsiteUrl = settingsProvider.GetMemberWebsiteUrl();
        }

        public string GetAdminWebsiteUrl()
        {
            return AdminWebsiteUrl;
        }

        public string GetMemberWebsiteUrl()
        {
            return MemberWebsiteUrl;
        }
    }
}
