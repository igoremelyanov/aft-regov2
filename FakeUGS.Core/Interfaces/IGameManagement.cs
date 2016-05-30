using System;

using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Interfaces;

using FakeUGS.Core.Data;

namespace FakeUGS.Core.Interfaces
{
    public interface IGameManagement : IApplicationService
    {
        void CreateGame(GameDTO game);
        void UpdateGame(GameDTO game);
        void DeleteGame(Guid id);

        void UpdateProductSettings(BrandProductSettingsData viewModel);

        void CreateGameProvider(GameProvider gameProvider);
        void UpdateGameProvider(GameProvider gameProvider);

        void CreateGameProviderCurrency(GameProviderCurrency gameProviderCurrency);

        Guid CreateLobby(Lobby lobby);
        void UpdateLobby(Lobby lobby);
        Guid CreateGameGroup(GameGroup gameGroup);
        void UpdateGameGroup(GameGroup gameGroup);
        void AssignGamesToGameGroups(AssignGamesToGameGroupData data);
        void AssignLobbiesToBrand(AssignLobbiesToBrandData data);
        void AssignBrandCredentials(BrandCredentialsData data);
    }
}
