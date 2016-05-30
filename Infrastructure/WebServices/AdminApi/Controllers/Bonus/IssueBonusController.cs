using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using AFT.RegoV2.AdminApi.Attributes;
using AFT.RegoV2.AdminApi.Controllers.Base;
using AFT.RegoV2.AdminApi.Interface.Bonus;
using AFT.RegoV2.AdminApi.Interface.Common;
using AFT.RegoV2.AdminWebsite.Common;
using AFT.RegoV2.AdminWebsite.Common.jqGrid;
using AFT.RegoV2.Bonus.Api.Interface;
using AFT.RegoV2.Bonus.Api.Interface.Requests;
using AFT.RegoV2.Bonus.Api.Interface.Responses;
using AFT.RegoV2.Bonus.Core.Models.Commands;
using AFT.RegoV2.Core.Auth.Interface.ApplicationServices;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Player.Interface.ApplicationServices;
using AFT.RegoV2.Core.Security.Interfaces;
using AFT.RegoV2.Shared.ApiDataFiltering;
using AutoMapper;
using SearchPackageResult = AFT.RegoV2.AdminWebsite.Common.jqGrid.SearchPackageResult;

namespace AFT.RegoV2.AdminApi.Controllers.Bonus
{
    [Authorize]
    [DisableCamelCaseSerialization]
    public class IssueBonusController : BaseApiController
    {
        private readonly IBonusApiProxy _bonusApiProxy;
        private readonly IPlayerQueries _playerQueries;

        public IssueBonusController(
            IAuthQueries authQueries,
            IAdminQueries adminQueries,
            IBonusApiProxy bonusApiProxy,
            IPlayerQueries playerQueries)
            : base(authQueries, adminQueries)
        {
            _bonusApiProxy = bonusApiProxy;
            _playerQueries = playerQueries;
        }

        [HttpGet]
        [Filters.SearchPackageFilter("searchPackage")]
        [Route(AdminApiRoutes.ListIssueBonuses)]
        public async Task<SearchPackageResult> Data([FromUri] SearchPackage searchPackage, [FromUri] Guid playerId)
        {
            var player = _playerQueries.GetPlayer(playerId);
            CheckBrand(player.BrandId);

            var request = Mapper.Map<FilteredDataRequest>(searchPackage);
            var filteredBonuses = await _bonusApiProxy.GetFilteredIssueBonusesAsync(new PlayerFilteredDataRequest
            {
                DataRequest = request,
                PlayerId = playerId
            });
            var searchPackageResult = new SearchPackageResult
            {
                page = filteredBonuses.Page,
                total = filteredBonuses.Total,
                records = filteredBonuses.Records,
                rows = filteredBonuses.Rows.Select(b => new SearchPackageResultRow
                {
                    id = b.Id,
                    cell = new object[]
                    {
                        b.Name,
                        b.Code,
                        b.Type,
                        b.Status,
                        b.Description
                    }
                }).ToList()
            };

            return searchPackageResult;
        }

        [HttpGet]
        [Route(AdminApiRoutes.GetIssueBonusTransactions)]
        public async Task<IssueTransactionsResponse> Transactions(Guid playerId, Guid bonusId)
        {
            var player = _playerQueries.GetPlayer(playerId);
            CheckBrand(player.BrandId);

            var qualifiedTransactions = await _bonusApiProxy.GetIssueBonusTransactionsAsync(playerId, bonusId);

            return new IssueTransactionsResponse
            {
                Transactions = qualifiedTransactions
                    .OrderByDescending(qt => qt.Date)
                    .Select(qt => new IssueTransaction
                    {
                        Id = qt.Id,
                        Amount = qt.Amount,
                        BonusAmount = qt.BonusAmount,
                        CurrencyCode = qt.CurrencyCode,
                        Date = Format.FormatDate(qt.Date)
                    })
                    .ToArray()
            };
        }

        [HttpPost]
        [Route(AdminApiRoutes.IssueBonus)]
        public Task<IssueBonusResponse> IssueBonus(IssueBonusByCs model)
        {
            var player = _playerQueries.GetPlayer(model.PlayerId);
            CheckBrand(player.BrandId);

            return _bonusApiProxy.IssueBonusAsync(model);
        }
    }
}