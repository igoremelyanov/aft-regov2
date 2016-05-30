using System;
using System.Linq;
using System.Web.Mvc;
using AFT.RegoV2.AdminWebsite.Common;
using AFT.RegoV2.AdminWebsite.Common.jqGrid;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Common.Data.Brand;
using AFT.RegoV2.Core.Fraud.ApplicationServices;
using AFT.RegoV2.Core.Fraud.ApplicationServices.Data;
using AFT.RegoV2.Core.Fraud.Interface.ApplicationServices;
using AFT.RegoV2.Core.Fraud.Interface.Data;
using AFT.RegoV2.Core.Security.Interfaces;
using ServiceStack.Validation;

namespace AFT.RegoV2.AdminWebsite.Controllers
{
    [Authorize]
    public class WageringController : BaseController
    {
        private readonly IWagerConfigurationQueries _wagerConfigurationQueries;
        private readonly IWagerConfigurationCommands _wagerConfigurationCommands;
        private readonly BrandQueries _brandQueries;
        private readonly IAdminQueries _adminQueries;

        public WageringController(
            IWagerConfigurationQueries wagerConfigurationQueries,
            BrandQueries brandQueries,
            IWagerConfigurationCommands wagerConfigurationCommands,
            IAdminQueries adminQueries)
        {
            _wagerConfigurationQueries = wagerConfigurationQueries;
            _wagerConfigurationCommands = wagerConfigurationCommands;
            _adminQueries = adminQueries;
            _brandQueries = brandQueries;
        }

        [SearchPackageFilter("searchPackage")]
        public ActionResult List(SearchPackage searchPackage)
        {
            var data = SearchWageringConfigurations(searchPackage);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        private object SearchWageringConfigurations(SearchPackage searchPackage)
        {
            var brandFilterSelections = _adminQueries.GetBrandFilterSelections();
            var wagerConfigurations = _wagerConfigurationQueries.GetWagerConfigurations()
                .Where(x => brandFilterSelections.Contains(x.BrandId));

            var dataBuilder = new SearchPackageDataBuilder<WagerConfigurationDTO>(searchPackage, wagerConfigurations);

            dataBuilder
                .Map(wager => wager.Id.ToString(),
                    wager => new[]
                    {
                        _brandQueries.GetBrandOrNull(wager.BrandId).Licensee.Name,
                        _brandQueries.GetBrandOrNull(wager.BrandId).Name,
                        wager.Currency,
                        wager.Status,
                        wager.IsActive,
                        wager.ActivatedBy.ToString(),
                        wager.DateActivated == null ? null : Format.FormatDate(wager.DateActivated, false),
                        wager.DeactivatedBy.ToString(),
                        wager.DateDeactivated == null ? null : Format.FormatDate(wager.DateDeactivated, false),
                        wager.CreatedBy.ToString(),
                        Format.FormatDate(wager.DateCreated, false),
                        wager.UpdatedBy.ToString(),
                        wager.DateUpdated == null ? null : Format.FormatDate(wager.DateUpdated, false)
                    }
                );

            return dataBuilder.GetPageData(walletTemplate => walletTemplate.DateCreated);
        }

        [HttpPost]
        public ActionResult WagerConfiguration(WagerConfigurationDTO wagerConfigurationDTO)
        {
            try
            {
                string message;
                if (wagerConfigurationDTO.Id == Guid.Empty)
                {
                    _wagerConfigurationCommands.CreateWagerConfiguration(wagerConfigurationDTO, CurrentUser.Id);
                    message = "Wagering configuration has been created successfully";
                }
                else
                {
                    _wagerConfigurationCommands.UpdateWagerConfiguration(wagerConfigurationDTO, CurrentUser.Id);
                    message = "Wagering configuration has been updated successfully";
                }
                return this.Success(message); 
            }
            catch (ValidationError e)
            {
                return this.Failed(e);
            }
            catch(Exception e)
            {
                return this.Failed(e);
            }
        }

        [HttpPost]
        public ActionResult Activate(Guid id)
        {
            try
            {
                _wagerConfigurationCommands.ActivateWagerConfiguration(id, CurrentUser.Id);
                return this.Success("ok");
            }
            catch (ValidationError e)
            {
                return this.Failed(e);
            }
            catch (Exception e)
            {
                return this.Failed(e);
            }
        }

        [HttpPost]
        public ActionResult Deactivate(Guid id)
        {
            try
            {
                _wagerConfigurationCommands.DeactivateWagerConfiguration(id, CurrentUser.Id);
                return this.Success("ok");
            }
            catch (ValidationError e)
            {
                return this.Failed(e);
            }
            catch (Exception e)
            {
                return this.Failed(e);
            }
        }

        public ActionResult Currencies(Guid brandId, bool isEdit)
        {
            var currencies = _brandQueries.GetBrandOrNull(brandId).BrandCurrencies.Select(x => x.CurrencyCode);

            var existingCurrencies = _wagerConfigurationQueries
                .GetWagerConfigurations()
                .Where(x => x.BrandId == brandId)
                .Select(x => x.Currency);

            if (!isEdit)
                currencies = currencies.Except(existingCurrencies);

            var returnList = currencies.ToList();
            if (existingCurrencies.Any() && (!existingCurrencies.Any(x => x.Equals("all", StringComparison.InvariantCultureIgnoreCase)) || isEdit))
                returnList.Add("All");

            return Json(returnList,JsonRequestBehavior.AllowGet);
        }

        public string Brands(Guid licensee)
        {
            var brands = _brandQueries
                .GetFilteredBrands(_brandQueries.GetBrandsByLicensee(licensee), CurrentUser.Id)
                .Where(b => b.Status == BrandStatus.Active)
                .ToList();

            return SerializeJson(new
            {
                Brands = brands
                    .OrderBy(b => b.Name)
                    .Select(b => new { name = b.Name, id = b.Id })
            });
        }

        public ActionResult GetConfiguration(Guid id)
        {
            var configuration = _wagerConfigurationQueries.GetWagerConfiguration(id);
            return Json(configuration, JsonRequestBehavior.AllowGet);
        }
    }
}