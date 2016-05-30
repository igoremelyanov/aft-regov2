using System;

namespace FakeUGS.Core.Data
{
    public class AssignLobbiesToBrandData
    {
        public Guid BrandId { get; set; }

        public Guid[] LobbiesIds { get; set; }
    }
}
