using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using AFT.RegoV2.Bonus.Api.Interface;
using AFT.RegoV2.Bonus.Api.Interface.Requests;
using AFT.RegoV2.Bonus.Api.Interface.Responses;
using AFT.RegoV2.Bonus.Core.ApplicationServices;
using AFT.RegoV2.Bonus.Core.Models.Commands;
using AFT.RegoV2.Shared.ApiDataFiltering;

namespace AFT.RegoV2.Bonus.Api.Controllers
{
    public class BonusController : BaseApiController
    {
        private readonly BonusManagementCommands _bonusCommands;
        private readonly BonusQueries _bonusQueries;

        public BonusController(
            BonusManagementCommands bonusCommands,
            BonusQueries bonusQueries)
        {
            _bonusCommands = bonusCommands;
            _bonusQueries = bonusQueries;
        }

        [HttpPost]
        [Route(Routes.ListBonuses)]
        public FilteredDataResponse<Core.Models.Data.Bonus> Data(BrandFilteredDataRequest request)
        {
            var bonuses = _bonusQueries.GetBonuses().Where(b => request.BrandFilters.Contains(b.BrandId));

            return new FilteredDataBuilder<Core.Models.Data.Bonus>(request.DataRequest, bonuses)
                .GetPageData();
        }

        [HttpGet]
        [Route(Routes.GetBonusOrNull)]
        public Core.Models.Data.Bonus GetBonusOrNull(Guid bonusId)
        {
            return _bonusQueries.GetBonusOrNull(bonusId);
        }

        [HttpGet]
        [Route(Routes.GetBonuses)]
        public List<BonusData> GetBonuses()
        {
            var bonuses =
                _bonusQueries.GetBonuses()
                    .Select(bonus => new BonusData { Id = bonus.Id, Name = bonus.Name, BrandId = bonus.BrandId })
                    .ToList();

            return bonuses;
        }

        [HttpPost]
        [Route(Routes.ChangeBonusStatus)]
        public ToggleBonusStatusResponse ChangeStatus(ToggleBonusStatus model)
        {
            var validationResult = _bonusQueries.GetValidationResult(model);
            if (!validationResult.IsValid)
                return ValidationErrorResponse<ToggleBonusStatusResponse>(validationResult);

            _bonusCommands.ChangeBonusStatus(model);
            return new ToggleBonusStatusResponse { Success = true };
        }

        [HttpPost]
        [Route(Routes.CreateUpdateBonus)]
        public AddEditBonusResponse CreateUpdate(CreateUpdateBonus model)
        {
            var validationResult = _bonusQueries.GetValidationResult(model);
            if (!validationResult.IsValid)
                return ValidationErrorResponse<AddEditBonusResponse>(validationResult);

            var bonusId = _bonusCommands.AddUpdateBonus(model);

            return new AddEditBonusResponse { Success = true, BonusId = bonusId };
        }
    }
}