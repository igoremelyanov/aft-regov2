using System;
using System.Collections.Generic;

namespace AFT.RegoV2.MemberApi.Interface.Player
{
    public class RegistrationFormDataRequest 
    {
        public Guid BrandId { get; set; }
    }

    public class RegistrationFormDataResponse 
    {
        public string[] CountryCodes { get; set; }
        public string[] CurrencyCodes { get; set; }
        public string[] Genders { get; set; }
        public string[] Titles { get; set; }
        public string[] ContactMethods { get; set; }
        public List<SecurityQuestion> SecurityQuestions { get; set; }
    }
}
