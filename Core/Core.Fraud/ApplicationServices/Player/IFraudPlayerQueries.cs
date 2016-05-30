using System.Linq;

namespace AFT.RegoV2.Core.Fraud.ApplicationServices
{
    public interface IFraudPlayerQueries
    {
        IQueryable<Fraud.Data.Player> GetPlayers();
    }
}