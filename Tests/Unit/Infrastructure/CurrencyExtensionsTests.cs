using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Tests.Common.Base;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Infrastructure
{
    public class CurrencyExtensionsTests : AdminWebsiteUnitTestsBase
    {
        [Test]
        public void CanFormatCurrency()
        {
            const decimal number = 1234.56m;
            const string currency = "CAD";

            Assert.That(number.Format(currency, true, DecimalDisplay.AlwaysShow), Is.EqualTo("$1,234.56"));
            Assert.That(number.Format(currency, true, DecimalDisplay.ShowNonZeroOnly), Is.EqualTo("$1,234.56"));
            Assert.That(1234m.Format(currency, true, DecimalDisplay.ShowNonZeroOnly), Is.EqualTo("$1,234"));
            Assert.That(1234.00m.Format(currency, true, DecimalDisplay.ShowNonZeroOnly), Is.EqualTo("$1,234"));
            Assert.That(number.Format(currency, true), Is.EqualTo("$1,234.56"));
            Assert.That(number.Format(currency, false), Is.EqualTo("1,234.56"));
            Assert.That(number.Format(currency), Is.EqualTo("$1,234.56"));
            Assert.That(number.Format("xx-XX"), Is.EqualTo("1,234.56"));
            Assert.That(number.Format(), Is.EqualTo("1,234.56"));
        }
    }
}