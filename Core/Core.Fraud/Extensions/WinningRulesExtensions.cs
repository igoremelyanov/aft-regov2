using AFT.RegoV2.Core.Fraud.Interface.Data;

namespace AFT.RegoV2.Core.Fraud.Extensions
{
    public static class WinningRulesExtensions
    {
        public static string WinningRuleDescription(this WinningRule rule, string productName)
        {
            var comparisonInterpreted = ComparisonOperatorInterpreted(rule.Comparison);
            var periodInterpreted = PeriodInterpereted(rule.Period);

            return string.Format("product \"{0}\" with winnings amount {1} {2} in the period of {3}.", 
                productName, 
                comparisonInterpreted, 
                rule.Amount, 
                periodInterpreted);
        }

        private static string ComparisonOperatorInterpreted(ComparisonEnum op)
        {
            switch (op)
            {
                case ComparisonEnum.Greater:
                    return ">";
                case ComparisonEnum.Less:
                    return "<";
                case ComparisonEnum.GreaterOrEqual:
                    return ">=";
                case ComparisonEnum.LessOrEqual:
                    return "<=";
                default:
                    return "";
            }
        }

        private static string PeriodInterpereted(PeriodEnum per)
        {
            switch (per)
            {
                case PeriodEnum.CurrentYear:
                    return "current year";
                case PeriodEnum.FromSignUp:
                    return "sign up";
                case PeriodEnum.Last14Days:
                    return "last 14 days";
                case PeriodEnum.Last7Days:
                    return "last 7 days";
                case PeriodEnum.CustomDate:
                    return "a custom date";
                default:
                    return "";

            }
        }
    }
}
