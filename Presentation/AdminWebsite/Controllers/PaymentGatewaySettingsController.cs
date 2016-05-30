using System;
using System.Linq;
using System.Web.Mvc;
using AFT.RegoV2.AdminApi.Interface.Payment;
using AFT.RegoV2.AdminWebsite.Common;
using AFT.RegoV2.AdminWebsite.Common.jqGrid;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Payment.Interface.ApplicationServices;
using AFT.RegoV2.Core.Security.Interfaces;


namespace AFT.RegoV2.AdminWebsite.Controllers
{
    [Authorize]
    public class PaymentGatewaySettingsController : BaseController
    {
        private readonly IPaymentQueries _paymentQueries;
        private readonly IAdminQueries _adminQueries;
        
        public PaymentGatewaySettingsController(
            IPaymentQueries paymentQueries,
            IAdminQueries adminQueries
            )
        {            
            _paymentQueries = paymentQueries;
            _adminQueries = adminQueries;
        }
     
        public ActionResult GetById(Guid id)
        {
            var response = GetAdminApiProxy(Request).GetPaymentGatewaySettingsById(id);
            return this.Success(response);
        }

        [SearchPackageFilter("searchPackage")]
        public object List(SearchPackage searchPackage)
        {
            var brandFilterSelections = _adminQueries.GetBrandFilterSelections();

            var paymentGatewaySettings = _paymentQueries.GetPaymentGatewaySettings()
                .Where(x => brandFilterSelections.Contains(x.BrandId));

            var dataBuilder = new SearchPackageDataBuilder<AFT.RegoV2.Core.Payment.Interface.Data.PaymentGatewaySettings>(searchPackage, paymentGatewaySettings);

            dataBuilder
                .SetFilterRule(x => x.Brand, (value) => p => p.Brand.Id == new Guid(value))
                .Map(settings => settings.Id,
                    settings =>
                        new object[]
                        {
                            settings.Brand.LicenseeName,
                            settings.Brand.Name,
                            settings.OnlinePaymentMethodName,
                            settings.PaymentGatewayName,
                            settings.Channel,
                            Enum.GetName(typeof(Status), settings.Status),
                            settings.CreatedBy,
                            Format.FormatDate(settings.DateCreated, true),
                            settings.UpdatedBy,
                            Format.FormatDate(settings.DateUpdated, true),
                            settings.ActivatedBy,
                            Format.FormatDate(settings.DateActivated, true),
                            settings.DeactivatedBy,
                            Format.FormatDate(settings.DateDeactivated, true)
                        }
                );
            var data = dataBuilder.GetPageData(setting => setting.Brand.Name);
            return new JsonResult { Data = data, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpPost]
        public ActionResult Save(SavePaymentGatewaySettingsRequest model)
        {            
            var isAdd = (model.Id == Guid.Empty);
            var response = isAdd
                ? GetAdminApiProxy(Request).AddPaymentGatewaySettings(model)
                : GetAdminApiProxy(Request).EditPaymentGatewaySettings(model);

            return response.Success ? this.Success(response.Id) : this.Failed(response.Errors);
        }

        [HttpPost]
        public ActionResult Activate(ActivatePaymentGatewaySettingsRequest request)
        {
            var response = GetAdminApiProxy(Request).ActivatePaymentGatewaySettings(request);

            return response.Success ? this.Success() : this.Failed(response.Errors);
        }

        [HttpPost]
        public ActionResult Deactivate(DeactivatePaymentGatewaySettingsRequest request)
        {
            var response = GetAdminApiProxy(Request).DeactivatePaymentGatewaySettings(request);
            return response.Success ? this.Success() : this.Failed(response.Errors);
        }

        public ActionResult GetPaymentGateways(Guid? brandId)
        {
            var response = GetAdminApiProxy(Request).GetPaymentGateways(new GetPaymentGatewaysRequest
            {
                BrandId = brandId
            });

            return this.Success(new { response.PaymentGateways });
        }
    }
}