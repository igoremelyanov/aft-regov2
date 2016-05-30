using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;
using System.Drawing;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd.Brand
{
    public class ViewVipLevelForm : BackendPageBase
    {
        public ViewVipLevelForm(IWebDriver driver) : base(driver){}

        public const string PageXPath = "//div[@data-view='vip-manager/view']";        

        public string ConfirmationMessage
        {
            get { return _driver.FindElementValue(By.XPath(PageXPath + "//div[contains(@data-bind, 'text: message')]")); }
        }

        public string Licensee
        {
            get
            {
                return _driver.FindElementValue(By.XPath(PageXPath + "//p[@data-bind='text: licensee']"));
            }
        }

        public string Brand
        {
            get
            {
                return _driver.FindElementValue(By.XPath(PageXPath + "//p[@data-bind='text: brand']"));
            }
        }

        public bool DefaultForNewPlayers
        {
            get
            {
                return _driver.FindElement(By.XPath("//div[contains(@data-bind,'activeItem')][@style='']/div[2]//input[@data-bind='checked: defaultForNewPlayers']")).Selected;               
            }
        }

        public string Code
        {
            get
            {
                return _driver.FindElementValue(By.XPath(PageXPath + "//p[@data-bind='text: code']"));
            }
        }

        public string Name
        {
            get
            {
                return _driver.FindElementValue(By.XPath(PageXPath + "//p[@data-bind='text: name']"));
            }
        }

        public string Rank
        {
            get
            {
                return _driver.FindElementValue(By.XPath(PageXPath + "//p[@data-bind='text: rank']"));
            }
        }

        public string Description
        {
            get
            {
                return _driver.FindElementValue(By.XPath(PageXPath + "//p[@data-bind='text: description']"));
            }
        }

        public bool IsColorDisplayed(Color color)
        {            
            return _driver.FindElementWait(By.XPath(string.Format("//span[@style='background-color: rgb({0}, {1}, {2});']", color.R, color.G, color.B))).Displayed;
        }            
        
        //first bet limit
        public string GameProvider1
        {
            get
            {
                return _driver.FindElementValue(By.XPath("(//span[contains(@data-bind, 'text: gameProvider')])[1]"));
            } 
        }       

        public string Currency1
        {
            get
            {
                return _driver.FindElementValue(By.XPath("(//span[contains(@data-bind, 'text: currency')])[1]"));
            }
        }

        public object BetLevel1
        {
            get
            {
                return _driver.FindElementValue(By.XPath("(//span[contains(@data-bind, 'text: betLimit')])[1]"));
            }
        }

        //second bet limit
        public string GameProvider2
        {
            get
            {
                return _driver.FindElementValue(By.XPath("(//span[contains(@data-bind, 'text: gameProvider')])[2]"));
            }
        }

        public string Currency2
        {
            get
            {
                return _driver.FindElementValue(By.XPath("(//span[contains(@data-bind, 'text: currency')])[2]"));
            }
        }

        public string BetLevel2
        {
            get
            {
                return _driver.FindElementValue(By.XPath("(//span[contains(@data-bind, 'text: betLimit')])[2]"));
            }
        }

    }
}