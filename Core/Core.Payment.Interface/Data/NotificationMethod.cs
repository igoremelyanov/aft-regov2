using System;

namespace AFT.RegoV2.Core.Payment.Interface.Data
{
    [Flags]
    public enum NotificationMethod
    {
        None = 0,
        Email = 1,
        SMS = 2
    }
}