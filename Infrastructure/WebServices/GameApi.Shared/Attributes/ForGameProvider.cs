using System;

namespace AFT.RegoV2.GameApi.Shared.Attributes
{
    public class ForGameProviderAttribute : Attribute
    {
        public Guid GameProviderId { get; set; }
        public ForGameProviderAttribute(string gpId)
        {
            GameProviderId = Guid.Parse(gpId);
        }
    }
}
