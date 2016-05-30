using System;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Brand.Interface.Data;
using AFT.RegoV2.Core.Common.Data.Brand;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Helpers;
using AFT.RegoV2.Tests.Common.Pages.BackEnd;
using Microsoft.Practices.Unity;
using NUnit.Framework;
using OpenQA.Selenium;
using System.Drawing;

namespace AFT.RegoV2.Tests.Selenium
{
    class VipLevelManagerTests : SeleniumBaseForAdminWebsite
    {
        private const string DefaultLicensee = "Flycow";
        private const string DefaultBrand = "138";
        private DashboardPage _dashboardPage;
        VipLevelManagerPage _vipLevelsPage;
        private Guid _brandId;
        private Licensee _defaultLicensee;
        private Brand _brand;
        private BrandTestHelper _brandTestHelper;
        private const string CurrencyCode = "CAD";

        public override void BeforeAll()
        {
            base.BeforeAll();
            _brandTestHelper = _container.Resolve<BrandTestHelper>();
            _defaultLicensee = _brandTestHelper.GetDefaultLicensee();
        }

        public override void BeforeEach()
        {
            base.BeforeEach();
            
            //create a brand for a default licensee
            _brandId = _brandTestHelper.CreateBrand(_defaultLicensee, PlayerActivationMethod.Automatic);

            _brandTestHelper.AssignCurrency(_brandId, "CAD");
            var brandQueries = _container.Resolve<BrandQueries>();
            _brand = brandQueries.GetBrandOrNull(_brandId);

            _driver.Logout();
            _dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            _dashboardPage.BrandFilter.SelectAll();
        }

        [Test, Ignore("Until New Product Form fixes - handle Products by UGS - 18/Jan/2016")]
        public void Can_create_vip_level_with_product_limit()
        {
            //create a product
            var productName = "product" + TestDataGenerator.GetRandomString(5);
            var productManagerPage = _dashboardPage.Menu.ClickProductManagerMenuItem();
            var newProductForm = productManagerPage.OpenNewProductForm();
            //TODO  Can't submit New Product form - 18-Jan-2016
            var submittedProductForm = newProductForm.Submit(productName, "Casino", "Casino_Code", "Token");
            
            //add a product to a licensee
            var licenseeManagerPage = submittedProductForm.Menu.ClickLicenseeManagerItem();
            var editLicenseeForm = licenseeManagerPage.OpenEditLicenseeForm(_defaultLicensee.Name);
            var viewLicenseeForm = editLicenseeForm.EditAssignedProducts(productName);
            viewLicenseeForm.CloseTab("View Licensee");
            
            //assign a product to the default brand
            var supportedProductsPage = licenseeManagerPage.Menu.ClickSupportedProductsMenuItem();
            var manageProductsPage = supportedProductsPage.OpenManageProductsPage();
            var editedLicenseeForm = manageProductsPage.AssignProducts(_defaultLicensee.Name, _brand.Name, new[] { productName });
            
            //add a bet limit to the product
            var betLimitName = TestDataGenerator.GetRandomString(5);
            var betLimitCode = TestDataGenerator.GetRandomString(4);
            var betLimitNameCode = string.Format(betLimitCode + " " + "-" + " " + betLimitName);
            
            var betLevelsPage = editedLicenseeForm.Menu.ClickBetLevelsMenuItem();
            var newBetLevelForm = betLevelsPage.OpenNewBetLevelForm();

            newBetLevelForm.SelectLicensee(_defaultLicensee.Name);
            newBetLevelForm.SelectBrand(_brand.Name);
            newBetLevelForm.SelectProduct(productName);
            newBetLevelForm.AddBetLevelDetails(betLimitName, betLimitCode);
            var submittedBetLevelForm = newBetLevelForm.Submit();
            
            // create a default vip level
            var vipLevelData = TestDataGenerator.CreateValidVipLevelData(_defaultLicensee.Name, _brand.Name);
            _vipLevelsPage = submittedBetLevelForm.Menu.ClickVipLevelManagerMenuItem();
            var newVipLevelForm = _vipLevelsPage.OpenNewVipLevelForm();
            newVipLevelForm.EnterVipLevelDetails(vipLevelData);
            //TODO  New VIP Level - AddProductLimits - Product bet Limits - Bet Limits - Currency  fixes 10-12-2015
            newVipLevelForm.AddProductLimit(productName, betLimitNameCode, CurrencyCode);
            var submittedVipLevelForm = newVipLevelForm.Submit();

            Assert.AreEqual("VIP Level has been created successfully.", submittedVipLevelForm.ConfirmationMessage);
            Assert.AreEqual(vipLevelData.Licensee, submittedVipLevelForm.Licensee);
            Assert.AreEqual(vipLevelData.Brand, submittedVipLevelForm.Brand);
            Assert.AreEqual(vipLevelData.Code, submittedVipLevelForm.Code);
            Assert.AreEqual(vipLevelData.Name, submittedVipLevelForm.Name);
        }

        [Test]
        [Ignore("Until VladS fixesfor New Form - AFTREGO-4595, 14/April/2016, Igor")]
        public void Cannot_create_more_than_one_default_vip_level_per_brand()
        {
            var vipLevelData = TestDataGenerator.CreateValidVipLevelData(_defaultLicensee.Name, _brand.Name);
            
            _vipLevelsPage = _dashboardPage.Menu.ClickVipLevelManagerMenuItem();
            var newVipLevelForm = _vipLevelsPage.OpenNewVipLevelForm();
            var viewVipLevelForm = newVipLevelForm.Submit(vipLevelData);
            viewVipLevelForm.CloseTab("View VIP Level");

            var newVipLevelForm2 = _vipLevelsPage.OpenNewVipLevelForm();
            newVipLevelForm2.Submit(vipLevelData);

            Assert.AreEqual("Default vip level for this brand already exists.", newVipLevelForm2.ValidationMessage);
        }

        [Test]
        public void Can_view_vip_level()
        {
            var vipLevelData = TestDataGenerator.CreateValidVipLevelData(DefaultLicensee, "831", false); 

            //create a vip level
            _vipLevelsPage = _dashboardPage.Menu.ClickVipLevelManagerMenuItem();
            var newForm = _vipLevelsPage.OpenNewVipLevelForm();
            var submittedForm = newForm.Submit(vipLevelData);
            Assert.AreEqual("VIP Level has been created successfully.", submittedForm.ConfirmationMessage);
            submittedForm.CloseTab("View VIP Level");

            var searchNameXPath = _vipLevelsPage.Grid.FilterGrid(vipLevelData.Name);
            _driver.FindElementClick(searchNameXPath);
            var viewVipLevelForm = _vipLevelsPage.OpenViewVipLevelForm();
            Assert.AreEqual(vipLevelData.Licensee, viewVipLevelForm.Licensee);
            Assert.AreEqual(vipLevelData.Brand, viewVipLevelForm.Brand);
            Assert.AreEqual(vipLevelData.DefaultForNewPlayers, viewVipLevelForm.DefaultForNewPlayers);
            Assert.AreEqual(vipLevelData.Code, viewVipLevelForm.Code);
            Assert.AreEqual(vipLevelData.Name, viewVipLevelForm.Name);
            Assert.AreEqual(vipLevelData.Rank.ToString(), viewVipLevelForm.Rank);
            Assert.AreEqual(vipLevelData.Description, viewVipLevelForm.Description);            
        }

        [Test]
        public void Can_deactivate_vip_level()
        {
            var vipLevelData = TestDataGenerator.CreateValidVipLevelData(DefaultLicensee, "831", false);

            //create a vip level for brand '831'
            _vipLevelsPage = _dashboardPage.Menu.ClickVipLevelManagerMenuItem();
            var newForm = _vipLevelsPage.OpenNewVipLevelForm();
            var submittedForm = newForm.Submit(vipLevelData);
            submittedForm.CloseTab("View VIP Level");

            //deactivate the vip level
            var deactivateDialog = _vipLevelsPage.OpenDeactivateDialog(vipLevelData.Name);
            deactivateDialog.Deactivate();

            Assert.IsTrue(_vipLevelsPage.CheckDeactivatedVipLevelStatus(vipLevelData.Name));
        }

        [Test]
        public void Can_deactivate_active_default_vip_level()
        {
            //create a default vip level
            var vipLevelData = TestDataGenerator.CreateValidVipLevelData(_defaultLicensee.Name, _brand.Name);            
            _vipLevelsPage = _dashboardPage.Menu.ClickVipLevelManagerMenuItem();
            var newForm = _vipLevelsPage.OpenNewVipLevelForm();
            var submittedForm = newForm.Submit(vipLevelData);
            Assert.AreEqual("VIP Level has been created successfully.", submittedForm.ConfirmationMessage);
            submittedForm.CloseTab("View VIP Level");

            //create a not default vip level
            var secondVipLevelData = TestDataGenerator.CreateValidVipLevelData(_defaultLicensee.Name, _brand.Name, false);
            _vipLevelsPage = _dashboardPage.Menu.ClickVipLevelManagerMenuItem();
            newForm = _vipLevelsPage.OpenNewVipLevelForm();
            submittedForm = newForm.Submit(secondVipLevelData);
            Assert.AreEqual("VIP Level has been created successfully.", submittedForm.ConfirmationMessage);
            submittedForm.CloseTab("View VIP Level");

            //deactivate active default vip level
            var deactivateDialog = _vipLevelsPage.OpenDeactivateDialog(vipLevelData.Name);
            deactivateDialog.Deactivate(secondVipLevelData.Name);
            Assert.AreEqual("The VIP level has been successfully deactivated", _vipLevelsPage.ConfirmationMessage);
            _driver.FindElementWait(By.XPath("//button[text()='Close']")).Click();

            Assert.IsTrue(_vipLevelsPage.CheckDeactivatedVipLevelStatus(vipLevelData.Name));            
            Assert.IsTrue(!_vipLevelsPage.CheckDeactivatedVipLevelStatus(secondVipLevelData.Name));
        }

        [Test, Ignore("AFTREGO-4306 Rostyslav 03/28/2016")]
        public void Can_edit_inactive_vip_level()
        {
            //create vip level
            var vipLevelData = TestDataGenerator.CreateValidVipLevelData(_defaultLicensee.Name, _brand.Name, false);
            _vipLevelsPage = _dashboardPage.Menu.ClickVipLevelManagerMenuItem();
            var newForm = _vipLevelsPage.OpenNewVipLevelForm();
            var submittedForm = newForm.Submit(vipLevelData);
            Assert.AreEqual("VIP Level has been created successfully.", submittedForm.ConfirmationMessage);
            submittedForm.CloseTab("View VIP Level");
            //deactivate vip level
            var deactivateDialog = _vipLevelsPage.OpenDeactivateDialog(vipLevelData.Name).Deactivate();
            Assert.IsTrue(_vipLevelsPage.CheckDeactivatedVipLevelStatus(vipLevelData.Name));
            
            //edit vip level
            var secondVipLevelData = TestDataGenerator.CreateValidVipLevelData(_defaultLicensee.Name, _brand.Name, false);
            _vipLevelsPage.Grid.SelectRecord(vipLevelData.Name);            
            var editForm = _vipLevelsPage.OpenEditVipLevelForm();
            submittedForm = editForm.Submit(secondVipLevelData);
            Assert.AreEqual("VIP Level has been edited successfully.", submittedForm.ConfirmationMessage);
            submittedForm.CloseTab("View VIP Level");

            //check changes
            _vipLevelsPage.Grid.SelectRecord(secondVipLevelData.Name);
            var viewVipLevelForm = _vipLevelsPage.OpenViewVipLevelForm();
            Assert.AreEqual(secondVipLevelData.Licensee, viewVipLevelForm.Licensee);
            Assert.AreEqual(secondVipLevelData.Brand, viewVipLevelForm.Brand);
            Assert.AreEqual(secondVipLevelData.DefaultForNewPlayers, viewVipLevelForm.DefaultForNewPlayers);
            Assert.AreEqual(secondVipLevelData.Code, viewVipLevelForm.Code);
            Assert.AreEqual(secondVipLevelData.Name, viewVipLevelForm.Name);
            Assert.AreEqual(secondVipLevelData.Rank.ToString(), viewVipLevelForm.Rank);
            Assert.AreEqual(secondVipLevelData.Description, viewVipLevelForm.Description);
        }

        [Test]
        [Ignore("Until VladS fixesfor New Form - AFTREGO-4595, 14-April/-016,Igor")]
        public void Can_not_create_duplicate_vip_levels()
        {
            //create vip level
            var vipLevelData = TestDataGenerator.CreateValidVipLevelData(_defaultLicensee.Name, _brand.Name, false);
            _vipLevelsPage = _dashboardPage.Menu.ClickVipLevelManagerMenuItem();
            var newForm = _vipLevelsPage.OpenNewVipLevelForm();
            var submittedForm = newForm.Submit(vipLevelData);
            Assert.AreEqual("VIP Level has been created successfully.", submittedForm.ConfirmationMessage);
            submittedForm.CloseTab("View VIP Level");

            //check validation messages
            _vipLevelsPage.OpenNewVipLevelForm();
            submittedForm = newForm.Submit(vipLevelData);
            Assert.AreEqual("This code has been used.", newForm.CodeValidationMessage);
            Assert.AreEqual("This name has been used.", newForm.NameValidationMessage);
            Assert.AreEqual("Rank should be unique for brand", newForm.RankValidationMessage);
        }

        [Test, Ignore("AFTREGO-4309 Rostyslav 03/28/2016")]
        public void Can_edit_color_of_vip_Level()
        {
            //create vip level
            var vipLevelData = TestDataGenerator.CreateValidVipLevelData(_defaultLicensee.Name, _brand.Name, false);
            _vipLevelsPage = _dashboardPage.Menu.ClickVipLevelManagerMenuItem();
            var newForm = _vipLevelsPage.OpenNewVipLevelForm();
            var submittedForm = newForm.Submit(vipLevelData);
            Assert.AreEqual("VIP Level has been created successfully.", submittedForm.ConfirmationMessage);
            submittedForm.CloseTab("View VIP Level");
            //deactivate vip level
            var deactivateDialog = _vipLevelsPage.OpenDeactivateDialog(vipLevelData.Name).Deactivate();
            Assert.IsTrue(_vipLevelsPage.CheckDeactivatedVipLevelStatus(vipLevelData.Name));

            //edit color of vip level
            var color = TestDataGenerator.GetRandomColor();                     
            _vipLevelsPage.Grid.SelectRecord(vipLevelData.Name);
            var editForm = _vipLevelsPage.OpenEditVipLevelForm();
            submittedForm = editForm.EditColor(color);
            Assert.AreEqual("VIP Level has been edited successfully.", submittedForm.ConfirmationMessage);
            submittedForm.CloseTab("View VIP Level");

            //check color's change
            _vipLevelsPage.Grid.SelectRecord(vipLevelData.Name);
            var viewVipLevelForm = _vipLevelsPage.OpenViewVipLevelForm();
            Assert.IsTrue(viewVipLevelForm.IsColorDisplayed(ColorTranslator.FromHtml(color)));          
        }
    }
}
