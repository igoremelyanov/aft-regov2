using System.Web.Http;
using AFT.RegoV2.AdminApi.Controllers.Base;
using AFT.RegoV2.AdminApi.Interface.Common;
using AFT.RegoV2.AdminWebsite.Common;
using AFT.RegoV2.AdminWebsite.Common.jqGrid;
using AFT.RegoV2.Core.Auth.Interface.ApplicationServices;
using AFT.RegoV2.Core.Common.Data.Admin;
using AFT.RegoV2.Core.Payment.Interface.ApplicationServices;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Core.Security.Interfaces;
using Currency = AFT.RegoV2.Core.Payment.Interface.Data.Currency;
namespace AFT.RegoV2.AdminApi.Controllers.Admin
{
    [Authorize]
    public class CurrencyController : BaseApiController
    {
        private readonly ICurrencyCommands _currencyCommands;

        private readonly IPaymentQueries _paymentQueries;
        private readonly IPaymentCommands _paymentCommands;

        public CurrencyController(
            ICurrencyCommands currencyCommands,
            IPaymentQueries paymentQueries,
            IPaymentCommands paymentCommands,
            IAuthQueries authQueries,
            IAdminQueries adminQueries)
            : base(authQueries, adminQueries)
        {
            _currencyCommands = currencyCommands;
            _paymentQueries = paymentQueries;
            _paymentCommands = paymentCommands;
        }

        [HttpGet]
        [Filters.SearchPackageFilter("searchPackage")]
        [Route(AdminApiRoutes.ListCurrencies)]
        public IHttpActionResult List([FromUri] SearchPackage searchPackage)
        {
            var dataBuilder = new SearchPackageDataBuilder<Currency>(searchPackage,
                _paymentQueries.GetCurrencies());

            dataBuilder.Map(
                c => c.Code,
                c => new object[]
                {
                    c.Code,
                    c.Name,
                    c.Status.ToString(),
                    c.CreatedBy,
                    Format.FormatDate(c.DateCreated, false),
                    c.UpdatedBy,
                    Format.FormatDate(c.DateUpdated, false),
                    c.ActivatedBy,
                    Format.FormatDate(c.DateActivated, false),
                    c.DeactivatedBy,
                    Format.FormatDate(c.DateDeactivated,false),
                    c.Remarks
                });

            return Ok(dataBuilder.GetPageData(c => c.Code));
        }

        [HttpGet]
        [Route(AdminApiRoutes.GetCurrencyByCode)]
        public IHttpActionResult GetByCode(string code)
        {
            var currency = _paymentQueries.GetCurrency(code);

            return Ok(new
            {
                currency.Code,
                currency.Name,
                currency.Remarks,
                OldName = currency.Name
            });
        }

        [HttpPost]
        [Route(AdminApiRoutes.ActivateCurrency)]
        public IHttpActionResult Activate(ActivateCurrencyData data)
        {
            VerifyPermission(Permissions.Activate, Modules.CurrencyManager);
            _paymentCommands.ActivateCurrency(data.Code, data.Remarks);
            return Ok(new { result = "success" });
        }

        [HttpPost]
        [Route(AdminApiRoutes.DeactivateCurrency)]
        public IHttpActionResult Deactivate(DeactivateCurrencyData data)
        {
            VerifyPermission(Permissions.Deactivate, Modules.CurrencyManager);
            _paymentCommands.DeactivateCurrency(data.Code, data.Remarks);
            return Ok(new { result = "success" });
        }

        [HttpPost]
        [Route(AdminApiRoutes.SaveCurrency)]
        public IHttpActionResult Save(EditCurrencyData data)
        {
            var newCurrency = string.IsNullOrEmpty(data.OldCode);
            VerifyPermission(newCurrency ? Permissions.Create : Permissions.Update, Modules.CurrencyManager);

            var result = newCurrency
                    ? _currencyCommands.Add(data)
                    : _currencyCommands.Save(data);

            if (!result.IsSuccess)
                return Ok(new { Result = "false", Data = "app:common." + result.Message });

            return Ok(new { Result = "success", Data = "app:currencies." + result.Message });
        }
    }
}
