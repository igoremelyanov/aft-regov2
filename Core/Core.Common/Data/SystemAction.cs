using System.ComponentModel;

namespace AFT.RegoV2.Core.Common.Data
{
    public enum SystemAction
    {
        [Description("No Action")]
        NoAction,
        [Description("Freeze Account")]
        FreezeAccount,
        [Description("Disable Bonus")]
        DisableBonus,
        [Description("Deactivate")]
        Deactivate
    }
}