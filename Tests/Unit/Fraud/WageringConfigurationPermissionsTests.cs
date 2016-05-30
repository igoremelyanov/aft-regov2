using System;
using AFT.RegoV2.Core.Auth.Interface;
using AFT.RegoV2.Core.Brand.Interface.Data;
using AFT.RegoV2.Core.Fraud.Interface.ApplicationServices;
using AFT.RegoV2.Core.Fraud.Interface.Data;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;
using AFT.RegoV2.Tests.Common.TestDoubles;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Fraud
{
    class WageringConfigurationPermissionsTests : PermissionsTestsBase
    {
        private IWagerConfigurationCommands _wagerConfigurationCommands;
        private IWagerConfigurationQueries _wagerConfigurationQueries;
        private FakeBrandRepository _brandRepository;
        private IActorInfoProvider _actorInfoProvider;

        public override void BeforeEach()
        {
            base.BeforeEach();

            _wagerConfigurationCommands = Container.Resolve<IWagerConfigurationCommands>();
            _wagerConfigurationQueries = Container.Resolve<IWagerConfigurationQueries>();
            _brandRepository = Container.Resolve<FakeBrandRepository>();
            _actorInfoProvider = Container.Resolve<IActorInfoProvider>();

            foreach (var currencyCode in TestDataGenerator.CurrencyCodes)
            {
                _brandRepository.Currencies.Add(new Currency { Code = currencyCode });
            }
        }

        [Test]
        public void Cannot_execute_WagerConfigurationQueries_without_permissions()
        {
            // Arrange
            LogWithNewAdmin(Modules.WagerConfiguration, Permissions.Create);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _wagerConfigurationQueries.GetWagerConfigurations());
        }

        [Test]
        public void Cannot_execute_WagerConfigurationCommands_without_permissions()
        {
            // Arrange
            LogWithNewAdmin(Modules.WagerConfiguration, Permissions.View);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _wagerConfigurationCommands.ActivateWagerConfiguration(new Guid(), new Guid()));
            Assert.Throws<InsufficientPermissionsException>(() => _wagerConfigurationCommands.DeactivateWagerConfiguration(new Guid(), new Guid()));
            Assert.Throws<InsufficientPermissionsException>(() => _wagerConfigurationCommands.CreateWagerConfiguration(new WagerConfigurationDTO(), new Guid()));
            Assert.Throws<InsufficientPermissionsException>(() => _wagerConfigurationCommands.UpdateWagerConfiguration(new WagerConfigurationDTO(), new Guid()));
        }

        [Test]
        public void Cannot_activate_wager_configuration_with_invalid_brand()
        {
            // Arrange
            var wagerId = _wagerConfigurationCommands.CreateWagerConfiguration(CreateValidWagerConfigurationDto(), _actorInfoProvider.Actor.Id);
            LogWithNewAdmin(Modules.WagerConfiguration, Permissions.Activate);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _wagerConfigurationCommands.ActivateWagerConfiguration(wagerId, _actorInfoProvider.Actor.Id));
        }

        [Test]
        public void Cannot_deactivate_wager_configuration_with_invalid_brand()
        {
            // Arrange
            var wagerId = _wagerConfigurationCommands.CreateWagerConfiguration(CreateValidWagerConfigurationDto(), _actorInfoProvider.Actor.Id);
            LogWithNewAdmin(Modules.WagerConfiguration, Permissions.Deactivate);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _wagerConfigurationCommands.DeactivateWagerConfiguration(wagerId, _actorInfoProvider.Actor.Id));
        }

        [Test]
        public void Cannot_create_wager_configuration_with_invalid_brand()
        {
            // Arrange
            var data = CreateValidWagerConfigurationDto();
            LogWithNewAdmin(Modules.WagerConfiguration, Permissions.Create);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _wagerConfigurationCommands.CreateWagerConfiguration(data, _actorInfoProvider.Actor.Id));
        }

        [Test]
        public void Cannot_update_wager_configuration_with_invalid_brand()
        {
            // Arrange
            var data = CreateValidWagerConfigurationDto();
            LogWithNewAdmin(Modules.WagerConfiguration, Permissions.Update);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _wagerConfigurationCommands.UpdateWagerConfiguration(data, new Guid()));
        }

        private WagerConfigurationDTO CreateValidWagerConfigurationDto()
        {
            var brandTestHelper = Container.Resolve<BrandTestHelper>();

            var licensee = brandTestHelper.CreateLicensee();
            var brand = brandTestHelper.CreateBrand(licensee, isActive: true);

            var data = new WagerConfigurationDTO()
            {
                BrandId = brand.Id,
                IsDepositWageringCheck = true,
                Currency = brand.DefaultCurrency
            };

            return data;
        }
    }
}
