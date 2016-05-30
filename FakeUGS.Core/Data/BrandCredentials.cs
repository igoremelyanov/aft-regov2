using System;

namespace FakeUGS.Core.Data
{
    public class BrandCredentials
    {
        public BrandCredentials(Guid brandId, string clientId, string clientSecret, string timezoneId)
        {
            BrandId = brandId;
            ClientId = clientId;
            ClientSecret = clientSecret;
            TimezoneId = timezoneId;
        }

        public Guid BrandId { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string TimezoneId { get; set; }
    }
}
