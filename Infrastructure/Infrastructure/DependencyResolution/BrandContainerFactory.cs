using AFT.RegoV2.Core.Brand;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Brand.Interface.ApplicationServices;
using AFT.RegoV2.Core.Payment.ApplicationServices;
using AFT.RegoV2.Core.Payment.Interface.ApplicationServices;
using AFT.RegoV2.Infrastructure.DataAccess.Brand;
using AFT.RegoV2.Shared;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.Infrastructure.DependencyResolution
{
    public class BrandContainerFactory
    {
        public void RegisterTypes(IUnityContainer container)
        {
            container.RegisterType<IBrandRepository, BrandRepository>(new PerHttpRequestLifetime());
            container.RegisterType<ILicenseeCommands, LicenseeCommands>();
            container.RegisterType<IBrandCommands, BrandCommands>();
            container.RegisterType<ICultureCommands, CultureCommands>();
            container.RegisterType<IBrandQueries, BrandQueries>();
        }
    }
}
