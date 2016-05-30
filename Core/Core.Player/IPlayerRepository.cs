using System.Data.Entity;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Player;
using AFT.RegoV2.Core.Player.Data;
using IdentificationDocumentSettings = AFT.RegoV2.Core.Player.Data.IdentificationDocumentSettings;

namespace AFT.RegoV2.Core.Player
{
    public interface IPlayerRepository
    {
        IDbSet<Common.Data.Player.Player>     Players { get; }
        IDbSet<PlayerBetStatistics>         PlayerBetStatistics { get; }
        IDbSet<VipLevel>                    VipLevels { get; }
        IDbSet<SecurityQuestion>            SecurityQuestions { get; }
        IDbSet<PlayerActivityLog>           PlayerActivityLog { get; }
        IDbSet<PlayerInfoLog>               PlayerInfoLog { get; }
        IDbSet<Common.Data.Player.Brand>                  Brands { get; }
        IDbSet<IdentificationDocumentSettings> IdentificationDocumentSettings { get; }
        IDbSet<BankAccount>                 BankAccounts { get; }
        IDbSet<Bank>                        Banks { get; }
        IDbSet<OnSiteMessage> OnSiteMessages { get; }

        int SaveChanges();
    }
}