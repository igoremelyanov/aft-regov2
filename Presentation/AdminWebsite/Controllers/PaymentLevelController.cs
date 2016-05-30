using System;
using System.Linq;
using System.Web.Mvc;
using AFT.RegoV2.AdminWebsite.Common;
using AFT.RegoV2.AdminWebsite.Common.jqGrid;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Payment.Interface.ApplicationServices;
using AFT.RegoV2.Core.Payment.Interface.Data;
using AFT.RegoV2.Core.Payment.Interface.Data.Commands;
using AFT.RegoV2.Core.Security.Interfaces;
using AFT.RegoV2.Shared;
using AutoMapper;
using ServiceStack.Validation;
using BankAccount = AFT.RegoV2.Core.Payment.Interface.Data.BankAccount;
using EditPaymentLevelModel = AFT.RegoV2.AdminWebsite.ViewModels.EditPaymentLevelModel;
using PaymentLevel = AFT.RegoV2.Core.Payment.Interface.Data.PaymentLevel;
namespace AFT.RegoV2.AdminWebsite.Controllers
{
    [Authorize]
    public class PaymentLevelController : BaseController
    {
        private readonly BrandQueries _brandQueries;
        private readonly IPaymentQueries _paymentQueries;
        private readonly IPaymentLevelQueries _paymentLevelQueries;
        private readonly IPaymentLevelCommands _paymentLevelCommands;
        private readonly IAdminQueries _adminQueries;

        public PaymentLevelController(
            BrandQueries brandQueries,
            IPaymentQueries paymentQueries,
            IPaymentLevelQueries paymentLevelQueries,
            IPaymentLevelCommands paymentLevelCommands,
            IAdminQueries adminQueries)
        {
            _brandQueries = brandQueries;
            _paymentQueries = paymentQueries;
            _paymentLevelQueries = paymentLevelQueries;
            _paymentLevelCommands = paymentLevelCommands;
            _adminQueries = adminQueries;
        }

        // TODO Permissions
        public string GetById(Guid id)
        {
            var level = _paymentLevelQueries
                .GetPaymentLevelById(id);
            return SerializeJson(level);
        }

        [SearchPackageFilter("searchPackage")]
        public object List(SearchPackage searchPackage)
        {
            var brandFilterSelections = _adminQueries.GetBrandFilterSelections();

            var paymentLevels = _paymentQueries.GetPaymentLevelsAsQueryable()
                .Where(x => brandFilterSelections.Contains(x.BrandId));

            var dataBuilder = new SearchPackageDataBuilder<PaymentLevel>(searchPackage, paymentLevels);

            dataBuilder
                .SetFilterRule(x => x.Brand, (value) => p => p.Brand.Id == new Guid(value))
                .Map(level => level.Id,
                    level =>
                        new object[]
                        {
                            level.Name,
                            level.Code,
                            level.Brand.LicenseeName,
                            level.Brand.Name,
                            level.CurrencyCode,
                            Enum.GetName(typeof(PaymentLevelStatus), level.Status),
                            level.CreatedBy,
                            Format.FormatDate(level.DateCreated, false),
                            level.UpdatedBy,
                            Format.FormatDate(level.DateUpdated, false),
                            level.ActivatedBy,
                            Format.FormatDate(level.DateActivated, false),
                            level.DeactivatedBy,
                            Format.FormatDate(level.DateDeactivated, false)
                        }
                );
            var data = dataBuilder.GetPageData(level => level.Name);
            return new JsonResult { Data = data, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        // TODO Move to a higher level
        [HttpGet]
        public string GetBrandCurrencies(Guid brandId)
        {
            var filterCurrencies = _brandQueries.GetFilteredCurrencies(brandId);
            return SerializeJson(new
            {
                Currencies = filterCurrencies
            });
        }

        [HttpPost]
        public dynamic Save(EditPaymentLevelModel model)
        {
            try
            {
                var editData = Mapper.DynamicMap<EditPaymentLevel>(model);
                var result = editData.Id.HasValue
                    ? _paymentLevelCommands.Edit(editData)
                    : _paymentLevelCommands.Save(editData);

                return SerializeJson(new { Result = "success", Data = result.Message, result.PaymentLevelId });
            }
            catch (ValidationError ex)
            {
                return ValidationErrorResponseActionResult(ex);
            }
            catch (RegoException ex)
            {
                return SerializeJson(new { Result = "failed", Data = ex.Message });
            }
            catch (Exception ex)
            {
                return this.Failed(ex);
            }
        }

        [SearchPackageFilter("searchPackage")]
        public object GetBankAccounts(SearchPackage searchPackage)
        {
            var dataBuilder = new SearchPackageDataBuilder<BankAccount>(searchPackage,
                _paymentQueries.GetBankAccounts()
                    .Where(x => x.Status == BankAccountStatus.Active)
                    );
            dataBuilder
                .SetFilterRule(x => x.PaymentLevels, (value) => ba => (value == null) || ba.PaymentLevels.Any(x => x.Id == new Guid(value)))
                .SetFilterRule(x => x.Bank.Brand, (value) => x => x.Bank.Brand.Id == new Guid(value))
                .SetFilterRule(x => x.CurrencyCode, (value) => c => c.CurrencyCode == value)
                .Map(account => account.Id,
                    account =>
                        new[]
                        {
                            account.AccountId,
                            account.Bank.BankName,
                            account.Branch,
                            account.AccountName,
                            account.AccountNumber,
                            account.InternetSameBank.ToString(),
                            account.AtmSameBank.ToString(),
                            account.CounterDepositSameBank.ToString(),
                            account.InternetDifferentBank.ToString(),
                            account.AtmDifferentBank.ToString(),
                            account.CounterDepositDifferentBank.ToString()
                        }
                );
            var data = dataBuilder.GetPageData(account => account.AccountId);
            return new JsonResult { Data = data, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpPost]
        public ActionResult Activate(ActivatePaymentLevelCommand data)
        {
            var validationResult = _paymentLevelCommands.ValidatePaymentLevelCanBeActivated(data);

            if (!validationResult.IsValid)
                return ValidationErrorResponseActionResult(validationResult.Errors);

            _paymentLevelCommands.Activate(data);

            return this.Success();
        }

        [HttpGet]
        public string Deactivate(Guid id)
        {
            var status = _paymentLevelQueries.GetDeactivatePaymentLevelStatus(id);
            var statusName = Enum.GetName(typeof(DeactivatePaymentLevelStatus), status);

            var newLevelsRequired = status == DeactivatePaymentLevelStatus.CanDeactivateIsAssigned ||
                                    status == DeactivatePaymentLevelStatus.CanDeactivateIsDefault;

            var paymentLevels = newLevelsRequired
                ? _paymentLevelQueries.GetReplacementPaymentLevels(id).Select(x => new { x.Id, x.Name })
                : null;

            return SerializeJson(new
            {
                status = statusName,
                paymentLevels
            });
        }

        [HttpPost]
        public ActionResult Deactivate(DeactivatePaymentLevelCommand data)
        {
            var validationResult = _paymentLevelCommands.ValidatePaymentLevelCanBeDeactivated(data);

            if (!validationResult.IsValid)
                return ValidationErrorResponseActionResult(validationResult.Errors);

            _paymentLevelCommands.Deactivate(data);

            return this.Success();
        }

        [SearchPackageFilter("searchPackage")]
        public object GetPaymentGatewaySettings(SearchPackage searchPackage)
        {
            var dataBuilder = new SearchPackageDataBuilder<PaymentGatewaySettings>(searchPackage,
               _paymentQueries.GetPaymentGatewaySettings());
            dataBuilder
                .SetFilterRule(x => x.Brand, (value) => x => x.Brand.Id == new Guid(value))
                .SetFilterRule(x => x.PaymentLevels, (value) => ba => (value == null) || ba.PaymentLevels.Any(x => x.Id == new Guid(value)))
                .Map(settings => settings.Id,
                    settings =>
                        new[]
                        {
                            settings.OnlinePaymentMethodName
                        }
                );
            var data = dataBuilder.GetPageData(account => account.OnlinePaymentMethodName);
            return new JsonResult { Data = data, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public ActionResult GetPaymentLevels()
        {
            var response = GetAdminApiProxy(Request).GetPaymentLevels();

            return this.Success(new { response.PaymentLevels });
        }
    }
}