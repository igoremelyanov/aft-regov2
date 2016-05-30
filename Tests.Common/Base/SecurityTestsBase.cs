using AFT.RegoV2.Core.Brand;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Brand.Interface.Data;
using AFT.RegoV2.Core.Security.Interfaces;
using AFT.RegoV2.Tests.Common.Helpers;
using Microsoft.Practices.Unity;
using Licensee = AFT.RegoV2.Core.Brand.Interface.Data.Licensee;

namespace AFT.RegoV2.Tests.Common.Base
{
    public abstract class SecurityTestsBase : AdminWebsiteUnitTestsBase
    {
        protected ISecurityRepository SecurityRepository;
        protected IBrandRepository BrandRepository;
        protected SecurityTestHelper SecurityTestHelper;
        protected BrandQueries BrandQueries;
        protected Brand Brand;
        protected Licensee Licensee;
        protected IAdminCommands AdminCommands;
        protected IAdminQueries AdminQueries;
        protected BrandTestHelper BrandHelper;

        public override void BeforeEach()
        {
            base.BeforeEach();

            SecurityRepository = Container.Resolve<ISecurityRepository>();
            BrandRepository = Container.Resolve<IBrandRepository>();

            SecurityTestHelper = Container.Resolve<SecurityTestHelper>();
            SecurityTestHelper.PopulatePermissions();
            SecurityTestHelper.CreateAndSignInSuperAdmin();

            BrandQueries = Container.Resolve<BrandQueries>();

            BrandHelper = Container.Resolve<BrandTestHelper>();
            Brand = BrandHelper.CreateBrand();
            SecurityTestHelper.CreateBrand(Brand.Id, Brand.LicenseeId, Brand.TimezoneId);
            Licensee = Brand.Licensee;

            AdminCommands = Container.Resolve<IAdminCommands>();
            AdminQueries = Container.Resolve<IAdminQueries>();
        }
    }
}
