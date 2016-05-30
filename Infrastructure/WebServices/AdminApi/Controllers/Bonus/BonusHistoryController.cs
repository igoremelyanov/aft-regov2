using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using AFT.RegoV2.AdminApi.Attributes;
using AFT.RegoV2.AdminApi.Controllers.Base;
using AFT.RegoV2.AdminApi.Interface.Common;
using AFT.RegoV2.AdminWebsite.Common;
using AFT.RegoV2.AdminWebsite.Common.jqGrid;
using AFT.RegoV2.Bonus.Api.Interface;
using AFT.RegoV2.Bonus.Api.Interface.Requests;
using AFT.RegoV2.Bonus.Api.Interface.Responses;
using AFT.RegoV2.Bonus.Core.Models.Commands;
using AFT.RegoV2.Bonus.Core.Models.Enums;
using AFT.RegoV2.Bonus.Core.Models.Events.Redemption;
using AFT.RegoV2.Core.Auth.Interface.ApplicationServices;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Common.EventStore.Data;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Player.Interface.ApplicationServices;
using AFT.RegoV2.Core.Security.Interfaces;
using AFT.RegoV2.Shared.ApiDataFiltering;
using AutoMapper;
using Microsoft.Practices.ObjectBuilder2;
using Newtonsoft.Json;
using SearchPackageResult = AFT.RegoV2.AdminWebsite.Common.jqGrid.SearchPackageResult;

namespace AFT.RegoV2.AdminApi.Controllers.Bonus
{
    [Authorize]
    [DisableCamelCaseSerialization]
    public class BonusHistoryController : BaseApiController
    {
        private readonly IPlayerQueries _playerQueries;
        private readonly BrandQueries _brandQueries;
        private readonly IBonusApiProxy _bonusApiProxy;

        public BonusHistoryController(
            IPlayerQueries playerQueries,
            BrandQueries brandQueries,
            IAuthQueries authQueries,
            IAdminQueries adminQueries,
            IBonusApiProxy bonusApiProxy)
            : base(authQueries, adminQueries)
        {
            _playerQueries = playerQueries;
            _brandQueries = brandQueries;
            _bonusApiProxy = bonusApiProxy;
        }

        [HttpGet]
        [Filters.SearchPackageFilter("searchPackage")]
        [Route(AdminApiRoutes.ListBonusRedemptions)]
        public async Task<SearchPackageResult> Data([FromUri] SearchPackage searchPackage, [FromUri] Guid playerId)
        {
            var player = _playerQueries.GetPlayer(playerId);
            CheckBrand(player.BrandId);

            var brand = _brandQueries.GetBrand(player.BrandId);
            var rules = searchPackage.AdvancedFilter.Rules.Where(r => r.Field == "CreatedOn");
            foreach (var rule in rules)
            {
                var dateInBrandsTimezone = DateTime.Parse(rule.Data).ToBrandDateTimeOffset(brand.TimezoneId).ToString("O");
                rule.Data = dateInBrandsTimezone;
            }

            var request = Mapper.Map<FilteredDataRequest>(searchPackage);
            request.Filters.Where(f => f.Field == "BonusType").ForEach(f => f.Field = "Bonus.Type");
            var filteredBonuses = await _bonusApiProxy.GetFilteredBonusRedemptionAsync(new PlayerFilteredDataRequest
            {
                DataRequest = request,
                PlayerId = playerId
            });
            var searchPackageResult = new SearchPackageResult
            {
                page = filteredBonuses.Page,
                total = filteredBonuses.Total,
                records = filteredBonuses.Records,
                rows = filteredBonuses.Rows.Select(redemption => new SearchPackageResultRow
                {
                    id = redemption.Id,
                    cell = new object[]
                    {
                        redemption.Bonus.Name,
                        redemption.Bonus.Type.ToString(),
                        Format.FormatDate(redemption.CreatedOn),
                        redemption.ActivationState.ToString(),
                        redemption.Amount.Format(),
                        redemption.Rollover.Format(),
                        (redemption.Rollover - redemption.RolloverLeft).Format(),
                        redemption.RolloverLeft.Format(),
                        redemption.RolloverState == RolloverStatus.Active //can be canceled
                    }
                }).ToList()
            };

            return searchPackageResult;
        }

        [HttpGet]
        [Route(AdminApiRoutes.GetBonusRedemption)]
        public async Task<Interface.Bonus.BonusRedemptionResponse> Get(Guid playerId, Guid redemptionId)
        {
            var player = _playerQueries.GetPlayer(playerId);
            CheckBrand(player.BrandId);

            var redemptionTask = _bonusApiProxy.GetBonusRedemptionAsync(playerId, redemptionId);
            var licenseeName = _brandQueries.GetLicensee(player.Brand.LicenseeId).Name;
            var redemption = await redemptionTask;

            return new Interface.Bonus.BonusRedemptionResponse
            {
                LicenseeName = licenseeName,
                BrandName = player.Brand.Name,
                Username = player.Username,
                BonusName = redemption.Bonus.Name,
                ActivationState = redemption.ActivationState.ToString(),
                RolloverState = redemption.RolloverState.ToString(),
                Amount = redemption.Amount.Format(),
                LockedAmount = redemption.LockedAmount.Format(),
                Rollover = redemption.Rollover.Format()
            };
        }

        [HttpGet]
        [Filters.SearchPackageFilter("searchPackage")]
        [Route(AdminApiRoutes.ListRedemptionEvents)]
        public async Task<SearchPackageResult> GetRedemptionEvents([FromUri] SearchPackage searchPackage, [FromUri] Guid playerId, [FromUri] Guid redemptionId)
        {
            var player = _playerQueries.GetPlayer(playerId);
            CheckBrand(player.BrandId);

            var request = Mapper.Map<FilteredDataRequest>(searchPackage);
            var filteredBonuses = await _bonusApiProxy.GetFilteredBonusRedemptionEventsAsync(new RedemptionFilteredDataRequest
            {
                DataRequest = request,
                BonusRedemptionId = redemptionId
            });
            var searchPackageResult = new SearchPackageResult
            {
                page = filteredBonuses.Page,
                total = filteredBonuses.Total,
                records = filteredBonuses.Records,
                rows = filteredBonuses.Rows.Select(@event => new SearchPackageResultRow
                {
                    id = @event.Id,
                    cell = new object[]
                    {
                        Format.FormatDate(@event.Created),
                        @event.DataType,
                        GetData(@event)
                    }
                }).ToList()
            };

            return searchPackageResult;
        }

        private dynamic GetData(Event eventObj)
        {
            if (eventObj.DataType == typeof(BonusRedeemed).Name)
            {
                var deserializedEvent = JsonConvert.DeserializeObject<BonusRedeemed>(eventObj.Data);
                return new
                {
                    Amount = deserializedEvent.Amount.Format()
                };
            }
            if (eventObj.DataType == typeof(RedemptionCanceled).Name)
            {
                var deserializedEvent = JsonConvert.DeserializeObject<RedemptionCanceled>(eventObj.Data);
                return new
                {
                    AdminUsername = deserializedEvent.EventCreatedBy
                };
            }
            if (eventObj.DataType == typeof(RedemptionClaimed).Name)
            {
                var deserializedEvent = JsonConvert.DeserializeObject<RedemptionClaimed>(eventObj.Data);
                return new
                {
                    Amount = deserializedEvent.Amount.Format()
                };
            }
            if (eventObj.DataType == typeof(RedemptionNegated).Name)
            {
                var deserializedEvent = JsonConvert.DeserializeObject<RedemptionNegated>(eventObj.Data);
                return new
                {
                    Reasons = string.Join(Environment.NewLine, deserializedEvent.Reasons)
                };
            }
            if (eventObj.DataType == typeof(RedemptionRolloverDecreased).Name)
            {
                var deserializedEvent = JsonConvert.DeserializeObject<RedemptionRolloverDecreased>(eventObj.Data);
                return new
                {
                    Decreasement = deserializedEvent.Decreasement.Format(),
                    WageringLeft = deserializedEvent.RemainingRollover.Format()
                };
            }
            if (eventObj.DataType == typeof(RedemptionRolloverIssued).Name)
            {
                var deserializedEvent = JsonConvert.DeserializeObject<RedemptionRolloverIssued>(eventObj.Data);
                return new
                {
                    WageringAmount = deserializedEvent.WageringRequrement.Format(),
                    LockedAmount = deserializedEvent.LockedAmount.Format()
                };
            }
            if (eventObj.DataType == typeof(RedemptionRolloverCompleted).Name)
            {
                var deserializedEvent = JsonConvert.DeserializeObject<RedemptionRolloverCompleted>(eventObj.Data);
                return new
                {
                    Amount = deserializedEvent.MainBalanceAdjustment.Format()
                };
            }
            if (eventObj.DataType == typeof(RedemptionRolloverZeroedOut).Name)
            {
                var deserializedEvent = JsonConvert.DeserializeObject<RedemptionRolloverZeroedOut>(eventObj.Data);
                return new
                {
                    Amount = deserializedEvent.MainBalanceAdjustment.Format()
                };
            }

            return null;
        }

        [HttpPost]
        [Route(AdminApiRoutes.CancelBonusRedemption)]
        public Task<CancelBonusResponse> Cancel(CancelBonusRedemption model)
        {
            var player = _playerQueries.GetPlayer(model.PlayerId);
            CheckBrand(player.BrandId);

            return _bonusApiProxy.CancelBonusRedemptionAsync(model);
        }
    }
}