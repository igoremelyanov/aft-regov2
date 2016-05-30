using System;
using System.Collections.Generic;

namespace AFT.RegoV2.MemberApi.Interface.GameProvider
{
    public class GamesRequest
    {
        public string PlayerUsername { get; set; }
        public bool IsForMobile { get; set; }
        public string PlayerIpAddress { get; set; }
        public string UserAgent { get; set; }
        public string LobbyUrl { get; set; }
    }

    public class GameListResponse
    {
        public List<GameProviderData> GameProviders { get; set; }
    }

    public class GameProviderData
    {
        public string Code { get; set; }
        public List<GameData> Games { get; set; }
    }

    public class GameData
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
    }

    public class GamesResponse
    {
        public string CdnRoot { get; set; }
        public string[] Iconres { get; set; }
        public GameGroupDto[] GameGroups { get; set; }
    }

    public class GameProviderDto
    {
        public string Name { get; set; }
        public string Code { get; set; }
    }

    public class GameGroupDto
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public int Order { get; set; }
        public GameDto[] Games { get; set; }
    }

    public class GameDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Url { get; set; }
        public int Order { get; set; }
        public string ProviderCode { get; set; }
        public bool IsActive { get; set; }
        public int? IeCompatibilityLevel { get; set; }
        public string IconPath { get; set; }
    }
}
