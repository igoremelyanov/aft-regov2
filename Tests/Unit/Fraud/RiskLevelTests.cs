using System;
using System.Linq;
using AFT.RegoV2.Core.Brand;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Fraud.Interface.ApplicationServices;
using AFT.RegoV2.Core.Fraud.Interface.Data;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;
using AFT.RegoV2.WinService.Workers;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Fraud
{
    public class RiskLevelTests : AdminWebsiteUnitTestsBase
    {
        private IRiskLevelQueries _riskQueries;
        private IRiskLevelCommands _riskCommands;

        public override void BeforeEach()
        {
            base.BeforeEach();

            _riskQueries = Container.Resolve<IRiskLevelQueries>();
            _riskCommands = Container.Resolve<IRiskLevelCommands>();

            var securityTestHelper = Container.Resolve<SecurityTestHelper>();
            securityTestHelper.PopulatePermissions();
            securityTestHelper.CreateAndSignInSuperAdmin();
            Container.Resolve<RiskLevelWorker>().Start();
        }

        private void CreateRiskLevel(Guid id)
        {
            Container.Resolve<BrandTestHelper>().CreateBrand(isActive: true);

            var entity = new RiskLevel();
            entity.Id = id;
            entity.BrandId = Container.Resolve<IBrandRepository>().Brands.First().Id;
            entity.Name = "dao_test";
            entity.Level = 1001;
            entity.Description = "remarks";

            _riskCommands.Create(entity);
        }

        private void UpdateStatusTest(Guid id, Status expectedStatus, string expectedRemarks)
        {
            if (expectedStatus == Status.Active)
            {
                _riskCommands.Activate(id, expectedRemarks);
            }
            else
            {
                _riskCommands.Deactivate(id, expectedRemarks);
            }

            var newRisk = _riskQueries.GetById(id);

            Assert.IsNotNull(newRisk);
            Assert.AreEqual(expectedStatus, (Status)newRisk.Status);
            Assert.AreEqual(expectedRemarks, newRisk.Description);
        }

        [Test]
        public void Can_Create_RiskLevel()
        {
            var id = Guid.NewGuid();
            CreateRiskLevel(id);

            Assert.IsNotNull(_riskQueries.GetById(id));
        }

        [Test]
        public void Can_Update_RiskLevel()
        {
            Guid id = Guid.NewGuid();
            CreateRiskLevel(id);

            var risk = _riskQueries.GetById(id);
            risk.Name = "dao_edit";
            risk.Description = "edit remarks";

            _riskCommands.Update(risk);

            var newRisk = _riskQueries.GetById(id);
            Assert.IsNotNull(newRisk);
            Assert.AreEqual(risk.Name, newRisk.Name);
            Assert.AreEqual(risk.Description, newRisk.Description);
        }

        [Test, ExpectedException(typeof(RegoValidationException))]
        public void Cannot_Update_RiskLevel()
        {
            Guid id = Guid.NewGuid();
            CreateRiskLevel(id);

            var risk = _riskQueries.GetById(id);
            risk.Description = "\"Long, Long Ago\" is a song dealing with nostalgia, written in 1833 by English composer Thomas Haynes Bayly. Originally called \"The Long Ago\", its name was apparently changed by the editor Rufus Wilmot Griswold when it was first published, posthumously, in a Philadelphia magazine, along with a collection of other songs and poems by Bayly. The song was well received, and became one of the most popular songs in the United States in 1844.";

            _riskCommands.Update(risk);
        }

        [Test]
        public void Can_Activate_Or_Deactivate_RiskLevel()
        {
            Guid id = Guid.NewGuid();
            CreateRiskLevel(id);

            // deactivate
            var expectedStatus = Status.Inactive;
            var expectedRemarks = "deactivate remarks";
            UpdateStatusTest(id, expectedStatus, expectedRemarks);

            // activate
            expectedStatus = Status.Active;
            expectedRemarks = "activate remarks";
            UpdateStatusTest(id, expectedStatus, expectedRemarks);
        }
    }
}
