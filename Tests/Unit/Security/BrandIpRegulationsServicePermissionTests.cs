using System;
using System.Linq;
using AFT.RegoV2.Core.Common.Data.Admin;
using AFT.RegoV2.Core.Security.ApplicationServices.IpRegulations;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AutoMapper;
using Microsoft.Practices.Unity;
using NUnit.Framework;
using EditBrandIpRegulationData = AFT.RegoV2.Core.Security.ApplicationServices.Data.IpRegulations.EditBrandIpRegulationData;
using IpRegulationConstants = AFT.RegoV2.Infrastructure.Constants.IpRegulationConstants;

namespace AFT.RegoV2.Tests.Unit.Security
{
    class BrandIpRegulationsServicePermissionTests : PermissionsTestsBase
    {
        private BrandIpRegulationService _brandService;

        public override void BeforeEach()
        {
            base.BeforeEach();

            _brandService = Container.Resolve<BrandIpRegulationService>();
        }

        [Test]
        public void Cannot_execute_BackendIpRegulationService_without_permissions()
        {
            /* Arrange */
            LogWithNewAdmin(Modules.PlayerManager, Permissions.View);

            /* Act */
            Assert.Throws<InsufficientPermissionsException>(() => _brandService.GetIpRegulations());
            Assert.Throws<InsufficientPermissionsException>(() => _brandService.CreateIpRegulation(new AddBrandIpRegulationData()));
            Assert.Throws<InsufficientPermissionsException>(() => _brandService.UpdateIpRegulation(new EditBrandIpRegulationData()));
            Assert.Throws<InsufficientPermissionsException>(() => _brandService.DeleteIpRegulation(new Guid()));
        }

        [Test]
        public void Cannot_create_brand_ip_regulation_with_invalid_brand()
        {
            /*** Arrange ***/
            var licensee = BrandTestHelper.CreateLicensee();
            var brand = BrandTestHelper.CreateBrand(licensee, isActive: true);
            SecurityTestHelper.CreateBrand(brand.Id, brand.LicenseeId, brand.TimezoneId);

            var data = new AddBrandIpRegulationData
            {
                IpAddress = TestDataGenerator.GetRandomIpAddress(),
                BrandId = brand.Id,
                LicenseeId = licensee.Id,
                BlockingType = IpRegulationConstants.BlockingTypes.Redirection,
                RedirectionUrl = "google.com"
            };

            LogWithNewAdmin(Modules.BrandIpRegulationManager, Permissions.Create);

            /*** Act ***/
            Assert.Throws<InsufficientPermissionsException>(() => _brandService.CreateIpRegulation(data));
        }

        [Test]
        public void Cannot_update_brand_ip_regulation_with_invalid_brand()
        {
            /*** Arrange ***/
            var licensee = BrandTestHelper.CreateLicensee();
            var brand = BrandTestHelper.CreateBrand(licensee, isActive: true);
            SecurityTestHelper.CreateBrand(brand.Id, brand.LicenseeId, brand.TimezoneId);

            var addBrandIpRegulationData = new AddBrandIpRegulationData
            {
                IpAddress = TestDataGenerator.GetRandomIpAddress(),
                BrandId = brand.Id,
                LicenseeId = licensee.Id,
                BlockingType = IpRegulationConstants.BlockingTypes.Redirection,
                RedirectionUrl = "google.com"
            };

            _brandService.CreateIpRegulation(addBrandIpRegulationData);

            var editBrandIpRegulationData = Mapper.DynamicMap<EditBrandIpRegulationData>(addBrandIpRegulationData);

            LogWithNewAdmin(Modules.BrandIpRegulationManager, Permissions.Update);

            /*** Act ***/
            Assert.Throws<InsufficientPermissionsException>(() => _brandService.UpdateIpRegulation(editBrandIpRegulationData));
        }

        [Test]
        public void Cannot_delete_brand_ip_regulation_with_invalid_brand()
        {
            /*** Arrange ***/
            var licensee = BrandTestHelper.CreateLicensee();
            var brand = BrandTestHelper.CreateBrand(licensee, isActive: true);
            SecurityTestHelper.CreateBrand(brand.Id, brand.LicenseeId, brand.TimezoneId);

            var ipAddress = TestDataGenerator.GetRandomIpAddress();

            var data = new AddBrandIpRegulationData
            {
                IpAddress = ipAddress,
                BrandId = brand.Id,
                LicenseeId = licensee.Id,
                BlockingType = IpRegulationConstants.BlockingTypes.Redirection,
                RedirectionUrl = "google.com"
            };

            _brandService.CreateIpRegulation(data);

            var regulation = _brandService.GetIpRegulations().SingleOrDefault(ip => ip.IpAddress == ipAddress);

            LogWithNewAdmin(Modules.BrandIpRegulationManager, Permissions.Delete);

            /*** Act ***/
            Assert.NotNull(regulation);
            Assert.Throws<InsufficientPermissionsException>(() => _brandService.DeleteIpRegulation(regulation.Id));
        }
    }
}
