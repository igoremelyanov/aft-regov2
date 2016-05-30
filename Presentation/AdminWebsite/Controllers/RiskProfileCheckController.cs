using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using AFT.RegoV2.AdminWebsite.Common;
using AFT.RegoV2.AdminWebsite.Common.jqGrid;
using AFT.RegoV2.Core.Fraud;
using AFT.RegoV2.Core.Fraud.Interface.ApplicationServices;
using AFT.RegoV2.Core.Fraud.Interface.Data;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Core.Security.Interfaces;

namespace AFT.RegoV2.AdminWebsite.Controllers
{
    public class RiskProfileCheckController : BaseController
    {
        private readonly IRiskProfileCheckCommands _riskProfileCheckCommands;
        private readonly IRiskProfileCheckQueries _riskProfileCheckQueries;
        private readonly ISecurityRepository _securityRepository;
        private readonly IRiskLevelQueries _riskLevelQueries;
        private readonly IFraudRepository _repository;
        private readonly IGameRepository _gameRepository;

        public RiskProfileCheckController(
            IRiskProfileCheckCommands riskProfileCheckCommands,
            IRiskProfileCheckQueries riskProfileCheckQueries,
            ISecurityRepository securityRepository,
            IRiskLevelQueries riskLevelQueries,
            IFraudRepository repository,
            IGameRepository gameRepository)
        {
            _riskProfileCheckCommands = riskProfileCheckCommands;
            _riskProfileCheckQueries = riskProfileCheckQueries;
            _securityRepository = securityRepository;
            _riskLevelQueries = riskLevelQueries;
            _repository = repository;
            _gameRepository = gameRepository;
        }

        [SearchPackageFilter("searchPackage")]
        public object List(SearchPackage searchPackage)
        {
            var dataBuilder = new SearchPackageDataBuilder<RiskProfileConfiguration>(
                searchPackage,
                _riskProfileCheckQueries.GetConfigurations()
                .Include(o => o.Brand)
                .Include(o => o.VipLevels));

            dataBuilder.SetFilterRule(x => x.Brand, value => p => p.Brand.Id == Guid.Parse(value))
                .Map(configuration => configuration.Id,
                    obj => new[]
                    {
                        obj.Brand.LicenseeName,
                        obj.Brand.Name,
                        obj.Currency,
                        string.Join("\r\n", obj.VipLevels.Select(o=>o.Name)),
                        GetCriteriasString(obj),
                        _securityRepository.Admins.Single(x => x.Id == obj.CreatedBy).Username,
                        Format.FormatDate(obj.DateCreated, false)
                    });
            var data = dataBuilder.GetPageData(configuration => configuration.Brand.Name);
            return new JsonResult { Data = data, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        private string GetCriteriasString(RiskProfileConfiguration configuration)
        {
            var type = configuration.GetType();
            var criteriasCount =
                    type
                    .GetProperties()
                    .Where(x => x.Name.StartsWith("has", StringComparison.InvariantCultureIgnoreCase))
                    .Count(x => (bool)x.GetValue(configuration));

            return string.Format("Criterias count: {0}", criteriasCount);
        }

        public string GetVipLevels(Guid? brandId)
        {
            var vipLevels = _riskProfileCheckQueries
                .GetVipLevels(brandId);

            return SerializeJson(new { vipLevels });
        }

        [HttpPost]
        public ActionResult AddOrUpdate(RiskProfileCheckDTO data)
        {
            try
            {
                if (data.Id == Guid.Empty)
                    _riskProfileCheckCommands.Create(data);
                else
                    _riskProfileCheckCommands.Update(data);
            }
            catch (Exception e)
            {
                return this.Failed(e);
            }
            return this.Success(new
            {
                Code = data.Id == Guid.Empty
                    ? "successfullyCreated"
                    : "successfullyUpdated"
            });
        }

        public string GetById(Guid id)
        {
            var configuration = _riskProfileCheckQueries.GetConfiguration(id);
            return SerializeJson(configuration);
        }

        public string GetFraudRiskLevels(Guid brandId)
        {
            var riskLevels = _riskLevelQueries.GetByBrand(brandId).ToList();
            return SerializeJson(new
            {
                RiskLevels = riskLevels.Select(x => new { id = x.Id, name = x.Name })
            });
        }

        public string GetBonuses(Guid brandId)
        {
            var bonuses = _repository.Bonuses
                .Where(o => o.BrandId == brandId)
                .Where(o => o.BonusType == BonusType.FirstDeposit || o.BonusType == BonusType.ReloadDeposit)
                .Where(o => o.IsActive);

            return SerializeJson(new
            {
                Bonuses = bonuses.Select(x => new { id = x.Id, name = x.Name })
            });
        }

        public string GetPaymentMethods()
        {
            var bonuses = _repository.PaymentMethods;
            return SerializeJson(new
            {
                PaymentMethods = bonuses.Select(x => new { id = x.Id, name = x.Code })
            });
        }

        public string GetWallets(Guid brandId)
        {
            return SerializeJson(new
            {
                Wallets = _gameRepository.Wallets
                    .Where(o => o.Brand.Id == brandId)
                    .Select(x => new { id = x.Id, name = "" })
            });
        }
    }
}