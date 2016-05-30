using System;

namespace FakeUGS.Core.Extensions
{
    public static class DateTimeOffsetExtensions
    {
        public static DateTimeOffset ToBrandOffset(this DateTimeOffset dateTimeOffset, string brandTimezoneId)
        {
            return TimeZoneInfo.ConvertTimeBySystemTimeZoneId(dateTimeOffset, brandTimezoneId);
        }

    }
}