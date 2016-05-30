namespace AFT.RegoV2.Bonus.Core.Data
{
    public class RolloverContribution: Identity
    {
        public virtual Transaction Transaction { get; set; }
        public decimal Contribution { get; set; }
        public ContributionType Type { get; set; }
    }

    public enum ContributionType
    {
        Bet,
        Threshold,
        Cancellation
    }
}