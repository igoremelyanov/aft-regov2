using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Pages.BackEnd;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Selenium
{
    class BrandIpRegulationsTests : SeleniumBaseForAdminWebsite
    {
        private DashboardPage _dashboardPage;
        private BrandIpRegulationsPage _brandIpRegulationManagerPage;

        public override void BeforeEach()
        { 
            base.BeforeEach();
            _driver.Logout();
            _dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            _dashboardPage.BrandFilter.SelectAll();
            _brandIpRegulationManagerPage = _dashboardPage.Menu.ClickBrandIpRegulationsMenuItem();
        }
        
        [Test]
        public void Can_create_brand_ip_regulation()
        {
            var form = _brandIpRegulationManagerPage.OpenNewBrandIpRegulationForm();
            var data = new BrandIpRegulationData
            {
                Licensee = "Flycow",
                Brand = "138",
                IpAddress = TestDataGenerator.GetRandomIpAddress(),
                AdvancedSettings = false,
                Description = "test ip regulation",
                Restriction = "Block",
                BlockingType = "Redirection",
                RedirectUrl = "http://test.com"
            };
            var submittedForm = form.Submit(data);

            Assert.AreEqual("IP Regulation has been successfully created", submittedForm.ConfirmationMessage);
            Assert.AreEqual(data.IpAddress, submittedForm.IpAddress);
        }

        [Test]
        [Ignore("Failing unstable on RC-1.0 - Igor, 25-Aiprl-2016")]
        public void Cannot_create_invalid_brand_ip_regulation()
        {
            var form = _brandIpRegulationManagerPage.OpenNewBrandIpRegulationForm();
            var data = new BrandIpRegulationData
            {
                Licensee = "Flycow",
                Brand = "138",
                IpAddress = TestDataGenerator.GetRandomIpAddress() + TestDataGenerator.GetRandomString(5),
                AdvancedSettings = false,
                Description = "test ip regulation",
                Restriction = "Block",
                BlockingType = "Redirection",
                RedirectUrl = "http://test.com"
            };
            var submittedForm = form.Submit(data);

            Assert.AreEqual("IP address is invalid", submittedForm.IpAddressValidation);
        }

        [Test]
        [Ignore("Till fix of bug https://jira.afusion.com/browse/AFTREGO-4656, Volodymyr 24/04/2016")]
        public void Can_edit_brand_ip_regulation()
        {
            var form = _brandIpRegulationManagerPage.OpenNewBrandIpRegulationForm();

            var data = new BrandIpRegulationData
            {
                Licensee = "Flycow",
                Brand = "138",
                IpAddress = TestDataGenerator.GetRandomIpAddress(),
                AdvancedSettings = false,
                Description = "test ip regulation",
                Restriction = "Block",
                BlockingType = "Redirection",
                RedirectUrl = "http://test.com"
            };
            var submittedForm = form.Submit(data);

            Assert.AreEqual("IP Regulation has been successfully created", submittedForm.ConfirmationMessage);
            Assert.AreEqual(data.IpAddress, submittedForm.IpAddress);

            submittedForm.Close();

            var editForm = _brandIpRegulationManagerPage.OpenEditBrandIpRegulationForm(data.IpAddress);

            var editData = new BrandIpRegulationData
            {
                Licensee = "Flycow",
                Brand = "138",
                IpAddress = TestDataGenerator.GetRandomIpAddress(),
                AdvancedSettings = false,
                Description = "test edit ip regulation",
                Restriction = "Block",
                BlockingType = "Redirection",
                RedirectUrl = "http://test.com"
            };
            editForm.ClearFieldsOnForm();
            var submittedEditForm = editForm.Submit(editData);

            Assert.AreEqual("IP Regulation has been successfully updated", submittedEditForm.ConfirmationMessage);
            Assert.AreEqual(editData.IpAddress, submittedEditForm.IpAddress);
        }

        [Test]
        [Ignore("Till fix of bug https://jira.afusion.com/browse/AFTREGO-4656, Volodymyr 24/04/2016")]
        public void Can_delete_brand_ip_regulation()
        {
            var form = _brandIpRegulationManagerPage.OpenNewBrandIpRegulationForm();
            var data = new BrandIpRegulationData
            {
                Licensee = "Flycow",
                Brand = "138",
                IpAddress = TestDataGenerator.GetRandomIpAddress(),
                AdvancedSettings = false,
                Description = "test ip regulation",
                Restriction = "Block",
                BlockingType = "Redirection",
                RedirectUrl = "http://test.com"
            };
            var submittedForm = form.Submit(data);

            Assert.AreEqual("IP Regulation has been successfully created", submittedForm.ConfirmationMessage);
            Assert.AreEqual(data.IpAddress, submittedForm.IpAddress);

            submittedForm.Close();

            _brandIpRegulationManagerPage.DeleteIpRegulation(data.IpAddress);

            Assert.True(_brandIpRegulationManagerPage.IsIpRegulationExists(data.IpAddress));
        }
    }
}
