namespace AFT.RegoV2.Core.Common.Data.Payment
{
    public enum BankValidationError
    {
        Required,
        MaxLength20,
        MaxLength50,
        MaxLength200,
        UnknownId,
        UnknownBrand,
        UnknownCountry,
        Alphanumeric,
        AlphanumericDashUnderscoreSpace,
        BankIdInUse
    }
}