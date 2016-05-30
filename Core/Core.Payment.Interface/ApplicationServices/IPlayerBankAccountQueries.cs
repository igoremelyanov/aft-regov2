using System.Linq;
using AFT.RegoV2.Core.Payment.Interface.Data;

namespace AFT.RegoV2.Core.Payment.Interface.ApplicationServices
{
    public interface IPlayerBankAccountQueries
    {
        IQueryable<PlayerBankAccount> GetPlayerBankAccounts();
        
        IQueryable<PlayerBankAccount> GetPlayerBankAccounts(PlayerId playerId);

        IQueryable<PlayerBankAccount> GetPendingPlayerBankAccounts();
    }
}
