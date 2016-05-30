using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Core.Security.Data.Users;
using AFT.RegoV2.Tests.Common.Helpers;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.Tests.Common.Base
{
    public abstract class PermissionsTestsBase : AdminWebsiteUnitTestsBase
    {
        protected SecurityTestHelper SecurityTestHelper;
        protected BrandTestHelper BrandTestHelper;
        public override void BeforeEach()
        {
            base.BeforeEach();

            SecurityTestHelper = Container.Resolve<SecurityTestHelper>();
            BrandTestHelper = Container.Resolve<BrandTestHelper>();
            SecurityTestHelper.PopulatePermissions();

            var admin = SecurityTestHelper.CreateSuperAdmin();
            foreach (var adminBrandId in GetAllowedAdminBrands())
                admin.AllowedBrands.Add(new BrandId
                {
                    Admin = admin,
                    AdminId = admin.Id,
                    Id = adminBrandId
                });

            SecurityTestHelper.SignInAdmin(admin);
        }

        protected virtual IEnumerable<Guid> GetAllowedAdminBrands()
        {
            return new List<Guid>();
        }

        protected Admin CreateAdminWithPermissions(string category, string[] permissions)
        {
            var licensee = BrandTestHelper.CreateLicensee();
            var brand = BrandTestHelper.CreateBrand(licensee, isActive: true);
            var brands = new[] { brand };

            return SecurityTestHelper.CreateAdmin(category, permissions, brands);
        }

        protected Admin LogWithNewAdmin(string category, string permission)
        {
            var user = CreateAdminWithPermissions(category, new[] { permission });
            SecurityTestHelper.SignInAdmin(user);

            return user;
        }

        protected void LoginNewUserWithMultiplePermissions(IList<Tuple<string, string>> multiplePermissions)
        {
            var securityTestHelper = Container.Resolve<SecurityTestHelper>();
            var brandTestHelper = Container.Resolve<BrandTestHelper>();

            var licensee = brandTestHelper.CreateLicensee();
            var brand = brandTestHelper.CreateBrand(licensee, isActive: true);
            var permissions = multiplePermissions.Select(x => securityTestHelper.GetPermission(x.Item1, x.Item2)).ToList();

            var role = securityTestHelper.CreateRole(new[] { licensee.Id }, permissions);
            const string password = "123456";
            var user = securityTestHelper.CreateAdmin(licensee.Id, new[] { brand }, password: password, roleId: role.Id);
            securityTestHelper.SignInAdmin(user);
        }
    }
}
