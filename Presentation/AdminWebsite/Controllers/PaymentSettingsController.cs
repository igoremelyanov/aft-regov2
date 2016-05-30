using System;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using AFT.RegoV2.AdminWebsite.Common;
using AFT.RegoV2.AdminWebsite.Common.jqGrid;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Payment.Interface.ApplicationServices;
using AFT.RegoV2.Core.Payment.Interface.Data;
using AFT.RegoV2.Core.Payment.Interface.Data.Commands;
using AFT.RegoV2.Core.Player.Interface.ApplicationServices;
using AFT.RegoV2.Core.Security.Interfaces;
using AFT.RegoV2.Shared;
using ServiceStack.Validation;
namespace AFT.RegoV2.AdminWebsite.Controllers
{
    [Authorize]
    public class PaymentSettingsController : BaseController
    {
        private readonly IPaymentQueries _paymentQueries;
        private readonly IPaymentSettingsCommands _settingsCommands;
        private readonly IPaymentSettingsQueries _paymentSettingsQueries;
        private readonly IPlayerQueries _playerQueries;
        private readonly IAdminQueries _adminQueries;
        private readonly BrandQueries _brandQueries;

        public PaymentSettingsController(IPaymentQueries paymentQueries,
            IPaymentSettingsCommands settingsCommands,
            IPaymentSettingsQueries paymentSettingsQueries,
            IPlayerQueries playerQueries, IAdminQueries adminQueries,
            BrandQueries brandQueries
            )
        {
            _paymentQueries = paymentQueries;
            _settingsCommands = settingsCommands;
            _paymentSettingsQueries = paymentSettingsQueries;
            _playerQueries = playerQueries;
            _adminQueries = adminQueries;
            _brandQueries = brandQueries;
        }

        // TODO Permissions
        public string GetById(Guid id)
        {
            var setting = _paymentQueries.GetPaymentSettingById(id);
            return SerializeJson(setting);
        }

        [SearchPackageFilter("searchPackage")]
        public object List(SearchPackage searchPackage)
        {
            var brandFilterSelections = _adminQueries.GetBrandFilterSelections();

            var paymentSettings = _paymentQueries.GetPaymentSettings()
                .Where(x => brandFilterSelections.Contains(x.BrandId));

            var dataBuilder = new SearchPackageDataBuilder<PaymentSettings>(searchPackage, paymentSettings);

            dataBuilder
                .SetFilterRule(x => x.Brand, value => p => p.Brand.Id == new Guid(value))
                .Map(level => level.Id,
                    obj =>
                        new[]
                        {
                            obj.Brand.LicenseeName,
                            obj.Brand.Name,
                            obj.CurrencyCode,
                            obj.PaymentType.ToString(),
                            _playerQueries.VipLevels.Single(x => x.Id == new Guid(obj.VipLevel)).Name,
                            obj.PaymentMethod,
                            obj.Enabled == Status.Inactive ? "Inactive":"Active",
                            obj.MinAmountPerTransaction.ToString(CultureInfo.InvariantCulture),
                            obj.MaxAmountPerTransaction.ToString(CultureInfo.InvariantCulture),
                            obj.MaxAmountPerDay.ToString(CultureInfo.InvariantCulture),
                            obj.MaxTransactionPerDay.ToString(CultureInfo.InvariantCulture),
                            obj.MaxTransactionPerWeek.ToString(CultureInfo.InvariantCulture),
                            obj.MaxTransactionPerMonth.ToString(CultureInfo.InvariantCulture),
                            obj.CreatedBy,
                            Format.FormatDate(obj.CreatedDate, false),
                            obj.UpdatedBy,
                            Format.FormatDate(obj.UpdatedDate, false),
                            obj.EnabledBy,
                            Format.FormatDate(obj.EnabledDate, false),
                            obj.DisabledBy,
                            Format.FormatDate(obj.DisabledDate, false)
                        }
                );
            var data = dataBuilder.GetPageData(level => level.Brand.Name);
            return new JsonResult { Data = data, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public string GetVipLevels(Guid? brandId)
        {
            var vipLevels = _paymentSettingsQueries
                .GetVipLevels(brandId);

            return SerializeJson(new { vipLevels });
        }

        public string GetPaymentMethods(Guid? brandId)
        {
            var paymentMethods = _paymentSettingsQueries
                 .GetPaymentMethods(brandId);
            
            return SerializeJson(paymentMethods);
        }

        [HttpPost]
        public string Save(SavePaymentSettingsCommand model)
        {
            try
            {
                var result = _paymentSettingsQueries
                    .SaveSetting(model);

                return SerializeJson(new
                {
                    Result = "success",
                    Data = result.Message,
                    Id = result.PaymentSettingsId
                });
            }
            catch (RegoException regoEx)
            {
                return SerializeJson(new { Result = "failed", Data = regoEx.Message });
            }
            catch (ValidationError e)
            {
                return SerializeJson(new { Result = "failed", Data = e.ErrorMessage });
            }
        }

        [HttpPost]
        public ActionResult Enable(Guid id, string remarks)
        {
            try
            {
                _settingsCommands.Enable(id, remarks);
                return this.Success();
            }
            catch (Exception ex)
            {
                return this.Failed(ex);
            }
        }

        [HttpPost]
        public ActionResult Disable(Guid id, string remarks)
        {
            try
            {
                _settingsCommands.Disable(id, remarks);
                return this.Success();
            }
            catch (Exception ex)
            {
                return this.Failed(ex);
            }
        }

        public JsonResult LicenseesList()
        {
            var licensees = _brandQueries.GetLicensees().Select(x => x.Name).ToList();
            return Json(licensees, JsonRequestBehavior.AllowGet);
        }

        public JsonResult VipLevelList()
        {
            var vipLevels = _playerQueries.VipLevels;
            return Json(vipLevels.Select(v => new { v.Id, v.Name }), JsonRequestBehavior.AllowGet);
        }

        public JsonResult BrandsList()
        {
            var brandFilterSelections = _adminQueries.GetBrandFilterSelections();
            var brands = _brandQueries.GetBrands().Where(x => brandFilterSelections.Contains(x.Id)).Select(x => x.Name).ToList();
            return Json(brands, JsonRequestBehavior.AllowGet);
        }

        public JsonResult PaymentTypesList()
        {
            var paymentTypes = Enum.GetNames(typeof(PaymentType));
            return Json(paymentTypes, JsonRequestBehavior.AllowGet);
        }

        public JsonResult PaymenMethodsList()
        {
            var paymentMethods = _paymentSettingsQueries.GetPaymentMethods().Select(x => x.Id);
            return Json(paymentMethods, JsonRequestBehavior.AllowGet);
        }

        public JsonResult CurrencyList()
        {
            var currencies = _brandQueries.GetBrands().Select(brand => brand.Id).ToList()
                .SelectMany(brandId => _brandQueries.GetCurrenciesByBrand(brandId)).Distinct()
                .OrderBy(currency => currency.Code)
                .Select(currency => currency.Code);
            return Json(currencies, JsonRequestBehavior.AllowGet);
        }

        public JsonResult StatusesList()
        {
            var statuses = Enum.GetNames(typeof(Status));
            return Json(statuses, JsonRequestBehavior.AllowGet);
        }
    }
}