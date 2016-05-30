namespace AFT.RegoV2.Core.Fraud.Interface.Data
{
    public enum ComparisonEnum
    {
        GreaterOrEqual = 2,
        LessOrEqual = 3,
        Greater = 0,
        Less = 1,
        
        //Comparison operators needed for the full description of criteria rule. E.g. : Has payment level Of CadLevel, BobLevel.
        Of = 4,
        Is = 5,
        For = 6
    }
}