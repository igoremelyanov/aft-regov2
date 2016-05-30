using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFT.RegoV2.Core.Payment.Interface.Data
{
    public class OnlineDepositPayNotifyRequest
    {
        public string OrderIdOfMerchant { get; set; }

        public string OrderIdOfRouter { get; set; }

        public string OrderIdOfGateway { get; set; }

        public string Language { get; set; }

        public string PayMethod { get; set; }

        public string Signature { get; set; }        
    }
}
