using System;

namespace FakeUGS.Attributes
{
    public class ForGameProviderAttribute : Attribute
    {
        public string GameProviderCode { get; set; }
        public ForGameProviderAttribute(string gpId)
        {
            GameProviderCode = gpId;
        }
    }
}
