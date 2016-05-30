using System;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Pages.BackEnd;
using AFT.RegoV2.Tests.Common.Pages.BackEnd.Fraud.FraudManager;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Selenium.Fraud
{
    /// <summary>
    /// Represents tests related to Fraud -> Fraud Manager and Fraud Risk Level
    /// </summary>
    [Ignore("Svitlana: 02/15/2016; Ignored until  Fraud subdomain will be back; AFTREGO-4260 Hide Fraud / Risk UI components in order to isolate functionality for R1.0")]
    public class FraudRiskLevelTests : SeleniumBaseForAdminWebsite
    {
        private FraudRiskLevelPage _fraudManagerPage;
        private DashboardPage _dashboardPage;
        private FraudRiskLevelData _frlData;
        private const string DefaultLicensee = "Flycow";
        private const string DefaultBrand = "138";
        private TimeSpan _ts = DateTime.Now.TimeOfDay;
        private string FRLCode;
        private string FRLName;
        private string Remarks = "Initial Remarks. ";

        public override void BeforeAll()
        {
            base.BeforeAll();

            _dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();

            FRLCode = (_ts.Milliseconds + 1000 * (_ts.Seconds + 60 * (_ts.Minutes + 60 * _ts.Hours))).ToString();
            FRLName = Guid.NewGuid().ToString();

            //generate fraud risk level form data
            _frlData = TestDataGenerator.CreateFraudRiskLevelData(
                DefaultLicensee,
                DefaultBrand,
                FRLCode,
                FRLName,
                Remarks
                );

            _fraudManagerPage = _dashboardPage.Menu.OpenFraudManager();
            var _newFRLform = _fraudManagerPage.OpenNewFraudRiskLevelForm();
            _newFRLform.SetFraudRiskLevelFields(_frlData);
       
            var viewFRLForm = _newFRLform.SubmitFraudRiskLevel();
            Assert.AreEqual("The Fraud Risk Level has been successfully created", viewFRLForm.SuccessAlert.Text);
            viewFRLForm.CloseTab("View Fraud Risk Level");
          
            //Activate FRL
            var _confirmFRLModal = _fraudManagerPage.ActivateFRL(_frlData);
            //Close modal
            _confirmFRLModal.CloseConfirmationModal();

            viewFRLForm.CloseTab("Fraud Manager");

        }

        public override void BeforeEach()
        {
            base.BeforeEach();
            _driver.Logout();
            _dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            _fraudManagerPage = _dashboardPage.Menu.OpenFraudManager();
           
        }

        [Test]
        public void Can_edit_FRL()
        {
            //generate fraud risk level edited form data
            var frlDataEdited = TestDataGenerator.CreateFraudRiskLevelData(
                 DefaultLicensee,
                 DefaultBrand,
                 _frlData.FRLCode,
                 "Edited_Name Several words",
                 "Edited_Remark"
               );

            //edit FRL
            var editFRLForm = _fraudManagerPage.OpenEditFraudRiskLevelForm(_frlData);
            editFRLForm.EditFraudRiskLevelFields(frlDataEdited);
            var viewFRLForm = editFRLForm.SubmitFraudRiskLevel();

            Assert.AreEqual("The Fraud Risk Level has been successfully updated", viewFRLForm.SuccessAlert.Text);
            Assert.AreEqual(frlDataEdited.Licensee, viewFRLForm.Licensee.Text);
            Assert.AreEqual(frlDataEdited.Brand, viewFRLForm.Brand.Text);
            Assert.AreEqual(frlDataEdited.FRLName, viewFRLForm.FRLName.Text);
            Assert.AreEqual(frlDataEdited.FRLCode, viewFRLForm.FRLCode.Text);
            Assert.AreEqual(frlDataEdited.Remarks, viewFRLForm.Remarks.Text);

            viewFRLForm.CloseTab("View Fraud Risk Level");

            //move to the initial state
            editFRLForm = _fraudManagerPage.OpenEditFraudRiskLevelForm(frlDataEdited);
            editFRLForm.EditFraudRiskLevelFields(_frlData);
            viewFRLForm = editFRLForm.SubmitFraudRiskLevel();

            Assert.AreEqual("The Fraud Risk Level has been successfully updated", viewFRLForm.SuccessAlert.Text);
        }

        [Test]
        public void Can_not_create_duplicate_FRl_by_name()
        {
            var frlDataEdited = TestDataGenerator.CreateFraudRiskLevelData(
           _frlData.Licensee,
           _frlData.Brand,
            (_ts.Milliseconds + 1000 * (_ts.Seconds + 60 * (_ts.Minutes + 60 * _ts.Hours))).ToString(),
           _frlData.FRLName,
            "Remarks where duplicate by name."
            );

            var _newFRLForm = _fraudManagerPage.OpenNewFraudRiskLevelForm();

            _newFRLForm.SetFraudRiskLevelFields(frlDataEdited);
            _newFRLForm.SubmitFraudRiskLevel();

            Assert.AreEqual("Fraud Risk Level name should be unique per brand.", _newFRLForm.ValidationMessage.Text);

        }

        [Test]
        public void Can_not_create_duplicate_FRl_by_code()
        {
            var frlDataEdited = TestDataGenerator.CreateFraudRiskLevelData(
           _frlData.Licensee,
           _frlData.Brand,
           _frlData.FRLCode,
            Guid.NewGuid().ToString(),
            "Remarks where duplicate by code."
          );
           
            var _newFRLForm = _fraudManagerPage.OpenNewFraudRiskLevelForm();

            _newFRLForm.SetFraudRiskLevelFields(frlDataEdited);
            _newFRLForm.SubmitFraudRiskLevel();

            Assert.AreEqual("Fraud Risk Level should be unique per brand.", _newFRLForm.ValidationMessage.Text);

        }

        [Test]
          public void Can_not_edit_FRL_wrong_name()
        {
            //generate fraud risk level edited form data
            var frlDataEdited = TestDataGenerator.CreateFraudRiskLevelData(
                _frlData.Licensee,
                _frlData.Brand,
                _frlData.FRLCode,
                 "Name#with***wro$ng",
                 "Edited_Wrong_Name_Remark"
               );

            //edit FRL
            var editFRLForm = _fraudManagerPage.OpenEditFraudRiskLevelForm(_frlData);
            editFRLForm.EditFraudRiskLevelFields(frlDataEdited);
            var viewFRLForm = editFRLForm.SubmitFraudRiskLevel();

            Assert.AreEqual("Name can only start from alphanumeric character.", editFRLForm.SpecValidationMessage.Text);

            viewFRLForm.CloseTab("Edit Fraud Risk Level");
        }

        [Test]
        public void Can_activate_deactivate_FRL()
        {
  
            //Deactivate FRL
            var _confirmFRLModal = _fraudManagerPage.DeactivateFRL(_frlData);
            Assert.AreEqual("Fraud Risk Level has been successfully deactivated.", _confirmFRLModal.SuccessAlert.Text);
            //Close modal
            _confirmFRLModal.CloseConfirmationModal();
            
            Assert.AreEqual("Inactive", _fraudManagerPage.GetFRLStatus(_frlData));

            _fraudManagerPage.CancelActivationFRL(_frlData);
            Assert.AreEqual("Inactive", _fraudManagerPage.GetFRLStatus(_frlData));

            //Activate FRL
            _confirmFRLModal = _fraudManagerPage.ActivateFRL(_frlData);
            Assert.AreEqual("Fraud Risk Level has been successfully activated.", _confirmFRLModal.SuccessAlert.Text);

            //Close modal
            _confirmFRLModal.CloseConfirmationModal();
            Assert.AreEqual("Active", _fraudManagerPage.GetFRLStatus(_frlData));

        }

    }
}
