using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd.Brand
{
    public class EditVipLevelForm : BackendPageBase
    {
        public By LoadingCompleteBy { get { return By.XPath("//label[contains(@for, 'vip-level-licensee')]"); } }
        public By LicenseeSelectBy { get { return By.XPath("//select[contains(@id, 'vip-level-licensee')]"); } }
        public By BrandSelectBy { get { return By.XPath("//select[contains(@id, 'vip-level-brand')]"); } }
        public By DefaultForNewPlayersCheckboxBy { get { return By.XPath("//input[contains(@id, 'vip-level-is-default')]"); } }
        public By CodeFieldBy { get { return By.XPath("//input[contains(@id, 'vip-level-code')]"); } }
        public By NameFieldBy { get { return By.XPath("//input[contains(@id, 'vip-level-name')]"); } }
        public By RankFieldBy { get { return By.XPath("//input[contains(@id, 'vip-level-rank')]"); } }
        public By DescriptionFieldBy { get { return By.XPath("//textarea[contains(@id, 'vip-level-description')]"); } }
        public By ColorFieldBy { get { return By.XPath("//input[contains(@id, 'vip-level-color')]"); } }
        public By RemarkFieldBy { get { return By.XPath("//textarea[contains(@id, 'vip-level-remark')]"); } }
        public By ValidationMessageBy { get { return By.XPath("//span[@data-bind='validationMessage: fields.isDefault']"); } }

        public By GameLimitsBy { get { return By.XPath("//div[@class='col-sm-2']/button[@class='btn btn-success btn-round btn-xs']"); } }              
        public By CurrencySelectBy { get { return By.XPath("//select[contains(@data-bind, 'options: currencies')]"); } }
        public By AddGameLimitButtonBy { get { return By.XPath("//select[contains(@data-bind, 'options: betLimits')]"); } }
        public By ProductListBy { get { return By.XPath("//select[contains(@data-bind, 'options: gameProviders')]"); } }
        public By BetLevelListBy { get { return By.XPath("//select[contains(@data-bind, 'options: betLimits')]"); } }
        public By CurrencyListBy { get { return By.XPath("//select[contains(@data-bind, 'options: currencies')]"); } }

        public EditVipLevelForm(IWebDriver driver) : base(driver) { }

        public ViewVipLevelForm EditColor(string color)
        {
            var colorPicker = _driver.FindElementWait(By.XPath("//span[contains(@class,'btn-colorpicker')]"));
            colorPicker.Click();

            var Element = _driver.FindElementWait(By.XPath(string.Format("//a[@data-color='{0}']", color)));
            Element.Click();

            var remarkField = _driver.FindElementWait(RemarkFieldBy);
            remarkField.SendKeys(TestDataGenerator.GetRandomAlphabeticString(5));

            return Submit();
        }

        public ViewVipLevelForm Submit(VipLevelData data)
        {
            EnterVipLevelDetails(data);

            return Submit();
        }


        public void EnterVipLevelDetails(VipLevelData data)
        {
            SelectLicenseeBrand(LoadingCompleteBy, LicenseeSelectBy, data.Licensee, BrandSelectBy, data.Brand);

            if (!data.DefaultForNewPlayers)
            {
                var defaultForNewPlayerCheckbox = _driver.FindElementWait(DefaultForNewPlayersCheckboxBy);
                defaultForNewPlayerCheckbox.Click();
            }

            var codeField = _driver.FindElementWait(CodeFieldBy);
            codeField.Clear();
            codeField.SendKeys(data.Code);

            var nameField = _driver.FindElementWait(NameFieldBy);
            nameField.Clear();
            nameField.SendKeys(data.Name);

            var rankField = _driver.FindElementWait(RankFieldBy);
            rankField.Clear();
            rankField.SendKeys(data.Rank.ToString());

            var descriptionField = _driver.FindElementWait(DescriptionFieldBy);
            descriptionField.Clear();
            descriptionField.SendKeys(data.Description);

            var colorPicker = _driver.FindElementWait(By.XPath("//span[contains(@class,'btn-colorpicker')]"));
            colorPicker.Click();

            var color = _driver.FindElementWait(By.XPath(string.Format("//a[@data-color='{0}']", data.DisplayColor)));
            color.Click();

            var remarkField = _driver.FindElementWait(RemarkFieldBy);
            remarkField.SendKeys(data.Description);
        }
        

        public ViewVipLevelForm Submit()
        {
            _driver.FindElementWait(By.XPath("//button[text()='Save']")).Click();
            return new ViewVipLevelForm(_driver);
        }

        public string ValidationMessage
        {
            get { return _driver.FindElementValue(ValidationMessageBy); }
        }
    }
}
