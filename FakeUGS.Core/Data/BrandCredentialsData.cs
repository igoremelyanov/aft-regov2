using System;

namespace FakeUGS.Core.Data
{
    public class BrandCredentialsData
    {
        public BrandCredentialsData(Guid brandId, string clientId, string clientSecret)
        {
            BrandId = brandId;
            ClientId = clientId;
            ClientSecret = clientSecret;
        }

        public Guid BrandId { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }
}
