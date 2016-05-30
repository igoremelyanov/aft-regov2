using System;
using System.Globalization;
using System.Linq;

namespace AFT.RegoV2.Core.Common.Utils
{
    public static class CurrencyExtensions
    {
        public static string Format(this decimal amount)
        {
            return FormatMoney(amount);
        }

        public static string Format(this decimal amount, string currency)
        {
            return FormatMoney(amount, currency);
        }

        public static string Format(this decimal amount, string currency, bool useCurrencySymbol)
        {
            return FormatMoney(amount, currency, useCurrencySymbol);
        }

        public static string Format(this decimal amount, string currency, bool useCurrencySymbol, DecimalDisplay decimalDisplay)
        {
            return FormatMoney(amount, currency, useCurrencySymbol, decimalDisplay);
        }

        private static string FormatMoney(
            decimal amount,
            string currency = null,
            bool useCurrencySymbol = true,
            DecimalDisplay decimalDisplay = DecimalDisplay.AlwaysShow)
        {
            if (currency == null)
                return GetDefaultFormattedString(amount);

            IFormatProvider formatProvider = CurrencyHelper.GetCurrencyCulture(currency);

            if (formatProvider == null)
                return GetDefaultFormattedString(amount);

            if (!useCurrencySymbol)
            {
                var numberFormatInfo = (NumberFormatInfo)((CultureInfo)formatProvider).NumberFormat.Clone();
                numberFormatInfo.CurrencySymbol = string.Empty;
                formatProvider = numberFormatInfo;
            }

            var amountHasDecimals = amount % 1 != 0;

            var format =
                decimalDisplay == DecimalDisplay.AlwaysShow ||
                (decimalDisplay == DecimalDisplay.ShowNonZeroOnly && amountHasDecimals)
                    ? "C"
                    : "C0";

            return amount.ToString(format, formatProvider).Trim();
        }

        private static string GetDefaultFormattedString(decimal amount)
        {
            return amount.ToString("N", new CultureInfo("en-US"));
        }
    }

    public static class CurrencyHelper
    {
        public static string GetCurrencySymbol(string currencyCode)
        {
            var isoCurrencyCode = GetIso4217CurrencyCode(currencyCode);
            var culture = GetCurrencyCulture(isoCurrencyCode);
            return culture?.NumberFormat.CurrencySymbol;
        }

        internal static CultureInfo GetCurrencyCulture(string currency)
        {
            var isoCurrencyCode = GetIso4217CurrencyCode(currency);

            var culture = CultureInfo.GetCultures(CultureTypes.SpecificCultures)
                .Select(c => new { c, r = new RegionInfo(c.LCID) })
                .Where(@t => @t.r != null
                             &&
                             string.Equals(@t.r.ISOCurrencySymbol, isoCurrencyCode, StringComparison.OrdinalIgnoreCase))
                .Select(@t => @t.c)
                .FirstOrDefault();

            return culture;
        }

        private static string GetIso4217CurrencyCode(string currencyCode)
        {
            return string.Equals(currencyCode, "RMB", StringComparison.OrdinalIgnoreCase) 
                ? "CNY" 
                : currencyCode;
        }
    }

    public enum DecimalDisplay
    {
        AlwaysShow,
        ShowNonZeroOnly
    }
}