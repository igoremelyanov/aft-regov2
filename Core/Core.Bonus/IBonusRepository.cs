using System;
using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.Bonus.Core.Data;
using BonusRedemption = AFT.RegoV2.Bonus.Core.Entities.BonusRedemption;

namespace AFT.RegoV2.Bonus.Core
{
    public interface IBonusRepository
    {
        IDbSet<Template>                Templates { get; }
        IDbSet<Data.Bonus>              Bonuses { get; }
        IDbSet<Player>                  Players { get; set; }
        IDbSet<Brand>                   Brands { get; set; }
        IDbSet<Game>                    Games { get; set; } 

        Entities.Player                 GetLockedPlayer(Guid playerId);
        Entities.Bonus                  GetLockedBonus(Guid bonusId);
        Entities.Bonus                  GetLockedBonusOrNull(Guid bonusId);
        Entities.Bonus                  GetLockedBonus(string bonusCode);
        Entities.Bonus                  GetLockedBonusOrNull(string bonusCode);
        BonusRedemption                 GetBonusRedemption(Guid playerId, Guid redemptionId);
        IQueryable<Data.Bonus>          GetCurrentVersionBonuses();
        void                            RemoveGameContributionsForGame(Guid gameId);
        Entities.Wallet                 GetLockedWallet(Guid playerId, Guid? walletTemplateId = null);

        int SaveChanges();
    }
}