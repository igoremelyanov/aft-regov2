using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Http;
using AFT.RegoV2.Bonus.Api.Interface;
using AFT.RegoV2.Bonus.Api.Interface.Requests;
using AFT.RegoV2.Bonus.Api.Interface.Responses;
using AFT.RegoV2.Bonus.Core.ApplicationServices;
using AFT.RegoV2.Bonus.Core.Models.Commands;
using AFT.RegoV2.Core.Common.EventStore;
using AFT.RegoV2.Core.Common.EventStore.Data;
using AFT.RegoV2.Shared.ApiDataFiltering;
using BonusRedemption = AFT.RegoV2.Bonus.Core.Models.Data.BonusRedemption;

namespace AFT.RegoV2.Bonus.Api.Controllers
{
    public class BonusHistoryController : BaseApiController
    {
        private readonly BonusCommands _bonusCommands;
        private readonly BonusQueries _bonusQueries;
        private readonly IEventRepository _eventRepository;

        public BonusHistoryController(
            BonusCommands bonusCommands,
            BonusQueries bonusQueries,
            IEventRepository eventRepository)
        {
            _bonusCommands = bonusCommands;
            _bonusQueries = bonusQueries;
            _eventRepository = eventRepository;
        }

        [HttpPost]
        [Route(Routes.ListBonusRedemptions)]
        public FilteredDataResponse<BonusRedemption> Data(PlayerFilteredDataRequest request)
        {
            var bonusRedemptions = _bonusQueries.GetBonusRedemptions(request.PlayerId);
            return new FilteredDataBuilder<BonusRedemption>(request.DataRequest, bonusRedemptions)
                .GetPageData();
        }

        [HttpGet]
        [Route(Routes.GetBonusRedemption)]
        public BonusRedemption Get(Guid playerId, Guid redemptionId)
        {
            return _bonusQueries.GetBonusRedemption(playerId, redemptionId);
        }

        [HttpPost]
        [Route(Routes.ListRedemptionEvents)]
        public FilteredDataResponse<Event> GetRedemptionEvents(RedemptionFilteredDataRequest request)
        {
            var redemptionEvents = _eventRepository.Events.Where(e => e.AggregateId == request.BonusRedemptionId).AsNoTracking();

            return new FilteredDataBuilder<Event>(request.DataRequest, redemptionEvents)
                .GetPageData();
        }

        [HttpPost]
        [Route(Routes.CancelBonusRedemption)]
        public CancelBonusResponse Cancel(CancelBonusRedemption model)
        {
            var validationResult = _bonusQueries.GetValidationResult(model);
            if (!validationResult.IsValid)
                return ValidationErrorResponse<CancelBonusResponse>(validationResult);

            _bonusCommands.CancelBonusRedemption(model);
            return new CancelBonusResponse { Success = true };
        }
    }
}