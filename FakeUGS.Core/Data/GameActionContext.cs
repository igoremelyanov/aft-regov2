using System;

namespace FakeUGS.Core.Data
{
    public class GameActionContext
    {
        public string GameProviderCode { get; set; }
        public string PlayerToken { get; set; }
        public bool OptionalTxRefId { get; set; }
    }
}
