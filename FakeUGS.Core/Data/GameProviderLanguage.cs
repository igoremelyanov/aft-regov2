using System;

namespace FakeUGS.Core.Data
{
    public class GameProviderLanguage
    {
        public Guid Id { get; set; }

        public virtual GameProvider GameProvider { get; set; }
        public Guid GameProviderId { get; set; }

        public string CultureCode { get; set; }
        public string GameProviderCultureCode { get; set; }
    }
}
