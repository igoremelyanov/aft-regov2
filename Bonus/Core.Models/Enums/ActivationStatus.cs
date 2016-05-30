namespace AFT.RegoV2.Bonus.Core.Models.Enums
{
    public enum ActivationStatus
    {
        /// <summary>
        /// Bonus is redeemed, but player yet has to perform activation actions (finish deposit process etc.)
        /// </summary>
        Pending,
        /// <summary>
        /// All activation actions are performed, but player has to manually claim the reward
        /// </summary>
        Claimable,
        /// <summary>
        /// Bonus reward is credited to player. Rollover is applied
        /// </summary>
        Activated,
        /// <summary>
        /// Player became not qualified for the bonus between Pending and Activated states
        /// </summary>
        Negated,
        /// <summary>
        /// Player canceled bonus
        /// </summary>
        Canceled
    }
}