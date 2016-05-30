using System;
using System.Collections.Generic;

namespace AFT.RegoV2.Core.Messaging.Interface.Data
{
    public class Brand
    {
        public Brand()
        {
            Languages = new List<Language>();
            Players = new List<Player>();
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string SmsNumber { get; set; }
        public string WebsiteUrl { get; set; }
        public string DefaultLanguageCode { get; set; }
        public string TimezoneId { get; set; }

        public ICollection<Language> Languages { get; set; }
        public ICollection<Player> Players { get; set; }
    }
}