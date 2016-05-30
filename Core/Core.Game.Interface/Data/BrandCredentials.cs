using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFT.RegoV2.Core.Game.Interface.Data
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
