using System;
using System.Linq;
using AFT.RegoV2.Core.Security.Interfaces;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Admin
{
    public class LicenseeBrandFilterTests : AdminWebsiteUnitTestsBase
    {
        private IAdminQueries _adminQueries;
        private IAdminCommands _adminCommands;

        public override void BeforeEach()
        {
            base.BeforeEach();

            _adminQueries = Container.Resolve<IAdminQueries>();
            _adminCommands = Container.Resolve<IAdminCommands>();
            Container.Resolve<SecurityTestHelper>().CreateAndSignInSuperAdmin();   
        }

        [Test]
        public void Can_set_and_get_licensee_filter_selections()
        {
            var licenseeIds = GetGuids();
            _adminCommands.SetLicenseeFilterSelections(licenseeIds);   

            var licenseeFilterSelections = _adminQueries.GetLicenseeFilterSelections().ToArray();

            Assert.That(licenseeFilterSelections.Length, Is.EqualTo(licenseeIds.Length));
            Assert.That(licenseeFilterSelections.All(licenseeIds.Contains), Is.True);
        }

        [Test]
        public void Can_set_and_get_brand_filter_selections()
        {
            var brandIds = GetGuids();
            _adminCommands.SetBrandFilterSelections(brandIds);

            var brandFilterSelections = _adminQueries.GetBrandFilterSelections().ToArray();

            Assert.That(brandFilterSelections.Length, Is.EqualTo(brandIds.Length));
            Assert.That(brandFilterSelections.All(x => brandIds.Contains(x)), Is.True);
        }

        private Guid[] GetGuids(int number = 5)
        {
            var guids = new Guid[number];

            for (var i = 0; i < number; i++)
            {
                guids[i] = Guid.NewGuid();
            }

            return guids;
        }
    }
}
