using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using AFT.RegoV2.AdminWebsite.Common;
using AFT.RegoV2.AdminWebsite.Common.jqGrid;
using AFT.RegoV2.AdminWebsite.ViewModels;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Fraud.Interface.ApplicationServices;
using AFT.RegoV2.Core.Fraud.Interface.Data;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Core.Payment.Interface.ApplicationServices;
using AFT.RegoV2.Core.Security.Interfaces;
using FluentValidation;

namespace AFT.RegoV2.AdminWebsite.Controllers
{
    public class AVCList
    {
        public Guid Id { get; set; }
        public Guid BrandId { get; set; }
        public string Status { get; set; }
        public string LicenseeName { get; set; }
        public string BrandName { get; set; }
        public string Currency { get; set; }
        public string VipLevel { get; set; }
        public string Criterias { get; set; }
        public string Username { get; set; }
        public DateTimeOffset DateCreated { get; set; }
    }

    public class AutoVerificationController : BaseController
    {
        private readonly IAVCConfigurationCommands _avcConfigurationCommands;
        private readonly IAVCConfigurationQueries _avcConfigurationQueries;
        private readonly IRiskLevelQueries _riskLevelQueries;
        private readonly BrandQueries _brandQueries;
        private readonly ISecurityRepository _securityRepository;
        private readonly IGameQueries _gameQueries;
        private readonly IPaymentLevelQueries _paymentQueries;
        private readonly IAdminQueries _adminQueries;

        public AutoVerificationController(
            IAVCConfigurationCommands avcConfigurationCommands,
            IAVCConfigurationQueries avcConfigurationQueries,
            IRiskLevelQueries riskLevelQueries,
            BrandQueries brandQueries,
            IGameQueries gameQueries,
            ISecurityRepository securityRepository,
            IPaymentLevelQueries paymentQueries,
            IAdminQueries adminQueries)
        {
            _avcConfigurationCommands = avcConfigurationCommands;
            _avcConfigurationQueries = avcConfigurationQueries;
            _riskLevelQueries = riskLevelQueries;
            _brandQueries = brandQueries;
            _gameQueries = gameQueries;
            _securityRepository = securityRepository;
            _paymentQueries = paymentQueries;
            _adminQueries = adminQueries;
        }

        [SearchPackageFilter("searchPackage")]
        public object List(SearchPackage searchPackage)
        {
            var brandFilterSelections = _adminQueries.GetBrandFilterSelections();

            var configs = _avcConfigurationQueries
                .GetAutoVerificationCheckConfigurations()
                .AsQueryable()
                .Where(x => brandFilterSelections.Contains(x.BrandId))
                .ToList()
                .Select(x => new AVCList
                {
                    Id = x.Id,
                    BrandId = x.BrandId,
                    Status = x.Status.ToString(),
                    LicenseeName =
                    x.Brand.LicenseeName,
                    BrandName = x.Brand.Name,
                    Currency = x.Currency,
                    VipLevel = string.Join("\r\n", x.VipLevels.Select(o => o.Name)),
                    Criterias = GetCriteriasString(x),
                    Username = _securityRepository.Admins.Single(y => y.Id == x.CreatedBy).Username,
                    DateCreated = x.DateCreated
                })
                .AsQueryable();
            var dataBuilder = new SearchPackageDataBuilder<AVCList>(
                searchPackage,
                configs);

            dataBuilder
                .Map(configuration => configuration.Id,
                    obj => new[]
                    {
                        obj.Status.ToString(),
                        obj.LicenseeName,
                        obj.BrandName,
                        obj.Currency,
                        obj.VipLevel,
                        obj.Criterias,
                        obj.Username,
                        Format.FormatDate(obj.DateCreated,true)
                    });
            var data = dataBuilder.GetPageData(configuration => configuration.Status);
            return new JsonResult { Data = data, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        private string GetCriteriasString(AutoVerificationCheckConfiguration autoVerificationCheckConfiguration)
        {
            //TODO: I think next query has to be refactored. It doesn't seem to be consistent. Plus, it sets a constraint over the namings
            //TODO: and this is not something well known by all the developers.

            var type = autoVerificationCheckConfiguration.GetType();
            var criteriasCount =
                    type
                    .GetProperties()
                    .Where(x => x.Name.StartsWith("has", StringComparison.InvariantCultureIgnoreCase))
                    .Count(x => (bool)x.GetValue(autoVerificationCheckConfiguration));

            return string.Format("Criterias count: {0}", criteriasCount);
        }

        [HttpPost]
        public ActionResult Verification(AVCConfigurationDTO data)
        {
            var callMadeForCreation = data.Id == Guid.Empty;

            try
            {
                if (callMadeForCreation)
                    _avcConfigurationCommands.Create(data);
                else
                    _avcConfigurationCommands.Update(data);
            }
            catch (ValidationException e)
            {
                return this.Failed(e);
            }
            catch (Exception e)
            {
                return this.Failed(e);
            }

            return this.Success(new
            {
                requestMadeForCreation = callMadeForCreation
            });
        }

        public string GetById(Guid id)
        {
            var configuration = _avcConfigurationQueries.GetAutoVerificationCheckConfiguration(id);
            return SerializeJson(configuration);
        }

        public object GetCurrencies(Guid brandId)
        {
            var brandCurrencies = _brandQueries.GetCurrenciesByBrand(brandId)
                .OrderBy(x => x.Code)
                .Select(x => x.Code);

            var userCurrencyCodes = _securityRepository.Admins
                .Include(o => o.Currencies)
                .Single(o => o.Id == CurrentUser.Id)
                .Currencies
                .Select(o => o.Currency)
                .ToList();

            return SerializeJson(new
            {
                Currencies = brandCurrencies.Where(userCurrencyCodes.Contains)
            });
        }

        public string GetFraudRiskLevels(Guid brandId)
        {
            var riskLevels = _riskLevelQueries.GetByBrand(brandId).ToList();
            return SerializeJson(new
            {
                RiskLevels = riskLevels.Select(x => new { id = x.Id, name = x.Name })
            });
        }

        [HttpPost]
        public ActionResult Activate(AvcChangeStatusCommand model)
        {
            var validationResult = _avcConfigurationQueries
                .GetValidationResult(model, AutoVerificationCheckStatus.Inactive);

            if (validationResult.IsValid == false)
                return ValidationErrorResponse(validationResult);

            _avcConfigurationCommands.Activate(model);
            return Json(new { Success = true });
        }

        [HttpPost]
        public ActionResult Deactivate(AvcChangeStatusCommand model)
        {
            var validationResult = _avcConfigurationQueries
                .GetValidationResult(model, AutoVerificationCheckStatus.Active);

            if (validationResult.IsValid == false)
                return ValidationErrorResponse(validationResult);

            _avcConfigurationCommands.Deactivate(model);

            return Json(new { Success = true });
        }

        /// <summary>
        /// The method returns all the payment levels that have brandId and currencyCode as the passed ones
        /// </summary>
        /// <param name="brandId"></param>
        /// <param name="currencyCode"></param>
        /// <returns>Pairs of payment level ID and payment level Name</returns>
        public string GetPaymentLevels(Guid brandId, string currencyCode)
        {
            var paymentLevels = _paymentQueries.GetPaymentLevelsByBrandAndCurrency(brandId, currencyCode);

            return SerializeJson(new
            {
                PaymentLevels = paymentLevels.Select(x => new { id = x.Id, name = x.Name })
            });
        }

        public JsonResult GetAllowedBrandProducts(Guid brandId)
        {
            var brand = _brandQueries.GetBrand(brandId);
            var productIds = brand.Products.Select(x => x.ProductId);
            var products = ProductViewModel.BuildFromIds(_gameQueries, productIds);
            return Json(products.ToArray(), JsonRequestBehavior.AllowGet);
        }
    }
}
