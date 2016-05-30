using System;
using System.Collections.Generic;

namespace AFT.RegoV2.MemberApi.Interface.Player
{
    
    public class LanguagesRequest 
    {
        public Guid BrandId { get; set; }
    }

    public class LanguagesResponse 
    {
        public List<Language> Languages { get; set; }
    }

    public class Language
    {
        public string NativeName { get; set; }
        public string Culture { get; set; }
    }
}
