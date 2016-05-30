using AFT.RegoV2.BoundedContexts.Report;
using AFT.RegoV2.Core.Common;
using AFT.RegoV2.Core.Security.Aspects;
using AFT.RegoV2.Infrastructure.Aspects;
using AFT.RegoV2.Infrastructure.DataAccess.Report;
using AFT.RegoV2.Infrastructure.DependencyResolution;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.InterceptionExtension;

namespace AFT.RegoV2.AdminWebsite
{
    public class AdminWebsiteContainerFactory : ApplicationContainerFactory
    {
        public override void RegisterTypes(IUnityContainer container)
        {
            base.RegisterTypes(container);

            container.RegisterType<IFileStorage, FileSystemStorage>();
            container.RegisterType<IReportRepository, ReportRepository>();
        }

        public override InterceptionBehavior GetSecurityInterceptionBehavior()
        {
            return new InterceptionBehavior<SecurityInterceptionBehavior>();
        }

        protected override InterceptionBehavior GetBrandCheckAspect()
        {
            return new InterceptionBehavior<BrandCheckAspect>();
        }
    }
}