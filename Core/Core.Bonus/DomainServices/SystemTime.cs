using System;

namespace AFT.RegoV2.Bonus.Core.DomainServices
{
    public static class SystemTime
    {
        public static Func<DateTimeOffset> Factory = () => DateTimeOffset.Now;

        public static DateTimeOffset Now => Factory();
    }
}
