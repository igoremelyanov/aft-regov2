using System.Linq;

namespace AFT.RegoV2.Core.Fraud.ApplicationServices
{
    public class FraudPlayerQueries : IFraudPlayerQueries
    {
        private readonly IFraudRepository _repository;

        public FraudPlayerQueries(IFraudRepository repository)
        {
            _repository = repository;
        }

        public IQueryable<Fraud.Data.Player> GetPlayers()
        {
            return _repository.Players;
        }
    }
}
