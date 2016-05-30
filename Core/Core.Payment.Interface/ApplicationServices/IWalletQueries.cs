using System;
using System.Threading.Tasks;
using AFT.RegoV2.Core.Payment.Interface.Data;

namespace AFT.RegoV2.Core.Payment.Interface.ApplicationServices
{
    public interface IWalletQueries
    {
        Task<PlayerBalance> GetPlayerBalance(Guid playId, Guid? walletTemplateId = null);
    }
}
