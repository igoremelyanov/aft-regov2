using System;
using AFT.RegoV2.Core.Common.Data.Player;

namespace AFT.RegoV2.Core.Common
{
    public static class ExclusionDateHelper
    {
        public static DateTimeOffset GetTimeOutEndDate(TimeOut timeOutType, DateTimeOffset startDate)
        {
            switch (timeOutType)
            {
                case TimeOut._24Hrs:
                    return startDate.AddDays(1);
                case TimeOut.Week:
                    return startDate.AddDays(7);
                case TimeOut.Month:
                    return startDate.AddMonths(1);
                case TimeOut._6Weeks:
                    return startDate.AddDays(42);
                default:
                    return DateTimeOffset.MinValue;
            }
        }

        public static DateTimeOffset GetSelfExcusionEndDate(SelfExclusion selfEclusionType, DateTimeOffset startDate)
        {
            switch (selfEclusionType)
            {
                case SelfExclusion.Permanent:
                    return DateTimeOffset.MaxValue;
                case SelfExclusion._6months:
                    return startDate.AddMonths(6);
                case SelfExclusion._1Year:
                    return startDate.AddYears(1);
                case SelfExclusion._5Years:
                    return startDate.AddYears(5);
                default:
                    return DateTimeOffset.MinValue;
            }
        }
    }
}
