using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using AFT.RegoV2.Bonus.Api.Interface;
using AFT.RegoV2.Bonus.Core.ApplicationServices;
using AFT.RegoV2.Bonus.Core.Models.Data;
using AFT.RegoV2.Shared.Helpers;

namespace AFT.RegoV2.Bonus.Api.Controllers
{
    public class AdminController : BaseApiController
    {
        private readonly BonusQueries _bonusQueries;

        public AdminController(BonusQueries bonusQueries)
        {
            _bonusQueries = bonusQueries;
        }

        [HttpGet]
        [Route(Routes.GetDepositQualifiedBonusesByAdmin)]
        public List<DepositQualifiedBonus> GetDepositQualifiedBonuses(Guid playerId)
        {
            return _bonusQueries.GetDepositQualifiedBonuses(playerId).ToList();
        }

        [HttpGet]
        [Route(Routes.EnsureOrWaitUserRegistered)]
        public async Task EnsureOrWaitUserRegistered(Guid playerId, int timeout)
        {
            await WaitHelper.WaitResultAsync(() => _bonusQueries.GetPlayerOrNull(playerId), timeout);
        }
    }
}