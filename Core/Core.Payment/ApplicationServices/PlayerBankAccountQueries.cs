using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Payment.Interface.ApplicationServices;
using AFT.RegoV2.Core.Payment.Interface.Data;
using PlayerBankAccount =AFT.RegoV2.Core.Payment.Interface.Data.PlayerBankAccount;
using AutoMapper.QueryableExtensions;
namespace AFT.RegoV2.Core.Payment.ApplicationServices
{
    public class PlayerBankAccountQueries :IPlayerBankAccountQueries
    {
        private readonly IPaymentRepository _repository;

        static PlayerBankAccountQueries()
        {
            MapperConfig.CreateMap();
        }

        public PlayerBankAccountQueries(IPaymentRepository repository)
        {
            _repository = repository;
        }

        public IQueryable<PlayerBankAccount> GetPlayerBankAccounts()
        {
            var query = _repository.PlayerBankAccounts
                .Include(x => x.Player)
                .Include(x => x.Bank)
                .Project().To<PlayerBankAccount>();
            return query;
        }

        public IQueryable<PlayerBankAccount> GetPlayerBankAccounts(PlayerId playerId)
        {
            return GetPlayerBankAccounts()
                .Where(x => x.Player.Id == playerId)
                .AsQueryable();
        }

        public IQueryable<PlayerBankAccount> GetPendingPlayerBankAccounts()
        {
            return GetPlayerBankAccounts()
                .Where(x => x.Status == BankAccountStatus.Pending)
                .AsQueryable();
        }
    }
}
