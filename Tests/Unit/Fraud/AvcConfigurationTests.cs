using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Fraud.Interface.ApplicationServices;
using AFT.RegoV2.Core.Fraud.Interface.Data;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;
using AFT.RegoV2.WinService.Workers;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Fraud
{
    public class AvcConfigurationTests : AdminWebsiteUnitTestsBase
    {
        private IAVCConfigurationCommands _avcConfigurationCommands;
        private IAVCConfigurationQueries _avcConfigurationQueries;
        private BrandQueries _brandQueries;

        public override void BeforeEach()
        {
            base.BeforeEach();

            _avcConfigurationCommands = Container.Resolve<IAVCConfigurationCommands>();
            _avcConfigurationQueries = Container.Resolve<IAVCConfigurationQueries>();
            _brandQueries = Container.Resolve<BrandQueries>();

            var securityTestHelper = Container.Resolve<SecurityTestHelper>();
            securityTestHelper.PopulatePermissions();
            securityTestHelper.CreateAndSignInSuperAdmin();

            Container.Resolve<RiskLevelWorker>().Start();
        }

        [Test]
        public void Can_create_Avc_configuration()
        {
            var id = Guid.NewGuid();
            Container.Resolve<BrandTestHelper>().CreateBrand(isActive: true);
            var avcConfiguration = new AVCConfigurationDTO
            {
                Id = id,
                Brand = _brandQueries.GetBrands().First().Id,
                Currency = _brandQueries.GetBrands().First().DefaultCurrency,
                HasFraudRiskLevel = false,
                HasWinnings = true,
                WinningRules = new List<WinningRuleDTO>
                {
                    FraudTestDataHelper.GenerateWinningRule(),
                    FraudTestDataHelper.GenerateWinningRule()
                }
            };

            _avcConfigurationCommands.Create(avcConfiguration);

            var latestAutoVerificationCheckCriteria = _avcConfigurationQueries.GetAutoVerificationCheckConfiguration(id);
            Assert.NotNull(latestAutoVerificationCheckCriteria);

            //The default state of criteria at creation must be Inactive
            Assert.AreEqual(AutoVerificationCheckStatus.Inactive, latestAutoVerificationCheckCriteria.Status);
        }

        [Test]
        public void Can_delete_Avc_configuration()
        {
            var id = Guid.NewGuid();
            Container.Resolve<BrandTestHelper>().CreateBrand(isActive: true);
            var avcConfiguration = new AVCConfigurationDTO()
            {
                Id = id,
                Brand = _brandQueries.GetBrands().First().Id,
                Currency = _brandQueries.GetBrands().First().DefaultCurrency,
                HasFraudRiskLevel = false,
                HasWinnings = true,
                WinningRules = new List<WinningRuleDTO>
                {
                    FraudTestDataHelper.GenerateWinningRule(),
                    FraudTestDataHelper.GenerateWinningRule()
                }
            };
            _avcConfigurationCommands.Create(avcConfiguration);
            _avcConfigurationCommands.Delete(id);

            Assert.IsEmpty(_avcConfigurationQueries.GetAutoVerificationCheckConfigurations());
        }

        [Test]
        public void Can_update_Avc_configuration()
        {
            var id = Guid.NewGuid();
            Container.Resolve<BrandTestHelper>().CreateBrand(isActive: true);
            var avcConfiguration = new AVCConfigurationDTO()
            {
                Id = id,
                Brand = _brandQueries.GetBrands().First().Id,
                Currency = _brandQueries.GetBrands().First().DefaultCurrency,
                HasFraudRiskLevel = false
            };
            _avcConfigurationCommands.Create(avcConfiguration);

            avcConfiguration.HasFraudRiskLevel = true;
            avcConfiguration.HasWinnings = true;

            avcConfiguration.WinningRules = new List<WinningRuleDTO>
            {
                FraudTestDataHelper.GenerateWinningRule()
            };

            _avcConfigurationCommands.Update(avcConfiguration);
            Assert.IsTrue(_avcConfigurationQueries.GetAutoVerificationCheckConfiguration(id).HasFraudRiskLevel);
        }

        [Test]
        public void Can_activate_Avc_configuration()
        {
            var id = Guid.NewGuid();
            Container.Resolve<BrandTestHelper>().CreateBrand(isActive: true);
            var avcConfiguration = new AVCConfigurationDTO
            {
                Id = id,
                Brand = _brandQueries.GetBrands().First().Id,
            };

            _avcConfigurationCommands.Create(avcConfiguration);

            var latestAutoVerificationCheckCriteria = _avcConfigurationQueries
                .GetAutoVerificationCheckConfiguration(id);

            //The default state of criteria at the moment of creation must be Inactive
            Assert.AreEqual(AutoVerificationCheckStatus.Inactive, latestAutoVerificationCheckCriteria.Status);

            //We activate the newly created AVC
            var activationCommand = new AvcChangeStatusCommand()
            {
                Id = avcConfiguration.Id
            };
            _avcConfigurationCommands.Activate(activationCommand);

            var latestAutoVerificationCheckCriteriaActivated = _avcConfigurationQueries
                .GetAutoVerificationCheckConfiguration(id);

            Assert.AreEqual(AutoVerificationCheckStatus.Active, latestAutoVerificationCheckCriteriaActivated.Status);
        }

        [Test]
        public void Can_deactivated_Avc_configuration()
        {
            //Here we create AVC with status of Active
            var id = Guid.NewGuid();
            Container.Resolve<BrandTestHelper>().CreateBrand(isActive: true);
            var avcConfiguration = new AVCConfigurationDTO
            {
                Id = id,
                Status = AutoVerificationCheckStatus.Active,
                Brand = _brandQueries.GetBrands().First().Id
            };

            _avcConfigurationCommands.Create(avcConfiguration);

            var latestAutoVerificationCheckCriteria = _avcConfigurationQueries
                .GetAutoVerificationCheckConfiguration(id);

            Assert.AreEqual(AutoVerificationCheckStatus.Active, latestAutoVerificationCheckCriteria.Status);

            //We deactivate the newly created AVC
            var activationCommand = new AvcChangeStatusCommand()
            {
                Id = avcConfiguration.Id
            };
            _avcConfigurationCommands.Deactivate(activationCommand);

            var latestAutoVerificationCheckCriteriaActivated = _avcConfigurationQueries
                .GetAutoVerificationCheckConfiguration(id);

            Assert.AreEqual(AutoVerificationCheckStatus.Inactive, latestAutoVerificationCheckCriteriaActivated.Status);
        }
    }
}