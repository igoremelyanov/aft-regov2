using System;
using System.Globalization;

namespace AFT.RegoV2.Core.Common.Utils
{
    public static class DateTimeExtensions
    {
        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek = DayOfWeek.Monday)
        {
            int diff = dt.DayOfWeek - startOfWeek;
            if (diff < 0)
            {
                diff += 7;
            }

            return dt.AddDays(-1 * diff).Date;
        }

        public static string GetNormalizedDate(this DateTime dateTime)
        {
            return dateTime.ToString("yyyy/MM/dd");
        }

        public static DateTime EndOfTheDay(this DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day)
                .AddDays(1)
                .AddTicks(-1);
        }

        public static DateTime StartOfTheDay(this DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0);
        }

        public static DateTimeOffset ToBrandDateTimeOffset(this DateTime dt, string brandTimezoneId)
        {
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(brandTimezoneId);

            return new DateTimeOffset(dt, timeZoneInfo.GetUtcOffset(dt));
        }
    }

    public static class DateTimeOffsetExtensions
    {
        public static DateTimeOffset ToBrandOffset(this DateTimeOffset dateTimeOffset, string brandTimezoneId)
        {
            return TimeZoneInfo.ConvertTimeBySystemTimeZoneId(dateTimeOffset, brandTimezoneId);
        }

        public static string GetNormalizedDateTime(this DateTimeOffset dateTimeOffset, bool includeTime = true)
        {
            var dateFormat = includeTime ? "yyyy/MM/dd HH:mm:ss" : "yyyy/MM/dd";
            return dateTimeOffset.ToString(dateFormat, CultureInfo.InvariantCulture);
        }

        public static string GetNormalizedDateTime(this DateTimeOffset? dateTimeOffset, bool includeTime = true)
        {
            return dateTimeOffset.HasValue ? dateTimeOffset.Value.GetNormalizedDateTime(includeTime) : string.Empty;
        }
    }
}