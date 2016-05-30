using System;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Fraud.Data
{
    public class DuplicateMechanismConfiguration : IExactScoreConfiguration
    {
        public Guid Id { get; set; }

        public Interface.Data.Brand Brand { get; set; }
        public Guid BrandId { get; set; }

        public string CreatedBy { get; set; }
        public DateTimeOffset CreatedDate { get; set; }

        public string UpdatedBy { get; set; }
        public DateTimeOffset? UpdatedDate { get; set; }

        public int DeviceIdExactScore { get; set; }
        public int FirstNameExactScore { get; set; }
        public int LastNameExactScore { get; set; }
        public int FullNameExactScore { get; set; }
        public int UsernameExactScore { get; set; }
        public int AddressExactScore { get; set; }
        public int SignUpIpExactScore { get; set; }
        public int MobilePhoneExactScore { get; set; }
        public int DateOfBirthExactScore { get; set; }
        public int EmailAddressExactScore { get; set; }
        public int ZipCodeExactScore { get; set; }

        public int DeviceIdFuzzyScore { get; set; }
        public int FirstNameFuzzyScore { get; set; }
        public int LastNameFuzzyScore { get; set; }
        public int FullNameFuzzyScore { get; set; }
        public int UsernameFuzzyScore { get; set; }
        public int AddressFuzzyScore { get; set; }
        public int SignUpIpFuzzyScore { get; set; }
        public int MobilePhoneFuzzyScore { get; set; }
        public int DateOfBirthFuzzyScore { get; set; }
        public int EmailAddressFuzzyScore { get; set; }
        public int ZipCodeFuzzyScore { get; set; }

        public int NoHandlingScoreMin { get; set; }
        public int NoHandlingScoreMax { get; set; }
        public SystemAction NoHandlingSystemAction { get; set; }
        public string NoHandlingDescr { get; set; }

        public int RecheckScoreMin { get; set; }
        public int RecheckScoreMax { get; set; }
        public SystemAction RecheckSystemAction { get; set; }
        public string RecheckDescr { get; set; }

        public int FraudulentScoreMin { get; set; }
        public int FraudulentScoreMax { get; set; }
        public SystemAction FraudulentSystemAction { get; set; }
        public string FraudulentDescr { get; set; }
    }
}
