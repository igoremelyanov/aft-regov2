using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFT.RegoV2.Core.Game.Interface.Data
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
