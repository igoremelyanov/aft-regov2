namespace AFT.RegoV2.Bonus.Core.Models.Enums
{
    public enum RolloverStatus
    {
        /// <summary>
        /// Rollover is not applicable (bonus redemption is not in activated state)
        /// </summary>
        None,
        /// <summary>
        /// Rollover is in progress
        /// </summary>
        Active,
        /// <summary>
        /// Rollover is completed or activated bonus with no rollover specified
        /// </summary>
        Completed,
        /// <summary>
        /// Rollover is zeroed out due to wagering threshold
        /// </summary>
        ZeroedOut
    }
}