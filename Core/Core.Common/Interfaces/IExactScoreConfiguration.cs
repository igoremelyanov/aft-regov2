namespace AFT.RegoV2.Core.Common.Interfaces
{
    public interface IExactScoreConfiguration
    {
        int DeviceIdExactScore { get; set; }
        int FirstNameExactScore { get; set; }
        int LastNameExactScore { get; set; }
        int FullNameExactScore { get; set; }
        int UsernameExactScore { get; set; }
        int AddressExactScore { get; set; }
        int SignUpIpExactScore { get; set; }
        int MobilePhoneExactScore { get; set; }
        int DateOfBirthExactScore { get; set; }
        int EmailAddressExactScore { get; set; }
        int ZipCodeExactScore { get; set; }
    }
}