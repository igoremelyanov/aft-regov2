using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using AFT.RegoV2.Bonus.Api.Interface;
using AFT.RegoV2.Bonus.Api.Interface.Requests;
using AFT.RegoV2.Bonus.Api.Interface.Responses;
using AFT.RegoV2.Bonus.Core.ApplicationServices;
using AFT.RegoV2.Bonus.Core.Models.Commands;
using AFT.RegoV2.Bonus.Core.Models.Data;
using AFT.RegoV2.Shared.ApiDataFiltering;

namespace AFT.RegoV2.Bonus.Api.Controllers
{
    public class IssueBonusController : BaseApiController
    {
        private readonly BonusCommands _bonusCommands;
        private readonly BonusQueries _bonusQueries;

        public IssueBonusController(
            BonusCommands bonusCommands,
            BonusQueries bonusQueries)
        {
            _bonusCommands = bonusCommands;
            _bonusQueries = bonusQueries;
        }

        [HttpPost]
        [Route(Routes.ListIssueBonuses)]
        public FilteredDataResponse<ManualByCsQualifiedBonus> Data(PlayerFilteredDataRequest request)
        {
            var qualifiedBonuses = _bonusQueries.GetManualByCsQualifiedBonuses(request.PlayerId).AsQueryable();
            return new FilteredDataBuilder<ManualByCsQualifiedBonus>(request.DataRequest, qualifiedBonuses)
                .GetPageData();
        }

        [HttpGet]
        [Route(Routes.GetIssueBonusTransactions)]
        public List<QualifiedTransaction> Transactions(Guid playerId, Guid bonusId)
        {
            return _bonusQueries.GetManualByCsQualifiedTransactions(playerId, bonusId);
        }

        [HttpPost]
        [Route(Routes.IssueBonus)]
        public IssueBonusResponse IssueBonus(IssueBonusByCs model)
        {
            var validationResult = _bonusQueries.GetValidationResult(model);
            if (!validationResult.IsValid)
                return ValidationErrorResponse<IssueBonusResponse>(validationResult);

            _bonusCommands.IssueBonusByCs(model);
            return new IssueBonusResponse { Success = true };
        }
    }
}