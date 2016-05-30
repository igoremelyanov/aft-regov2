namespace AFT.RegoV2.Bonus.Core.Models.Enums
{
    public enum DurationType
    {
        /// <summary>
        /// Matches bonus activity date range
        /// </summary>
        None,
        /// <summary>
        /// Duration calculated based on bonus ActiveFrom date
        /// </summary>
        StartDateBased,
        /// <summary>
        /// Date time range inside bonus activity date range
        /// </summary>
        Custom
    }
}