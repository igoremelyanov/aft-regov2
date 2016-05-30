using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using AFT.RegoV2.AdminApi.Controllers.Base;
using AFT.RegoV2.AdminApi.Interface.Common;
using AFT.RegoV2.AdminApi.Interface.Payment;
using AFT.RegoV2.AdminWebsite.Common;
using AFT.RegoV2.AdminWebsite.Common.jqGrid;
using AFT.RegoV2.Core.Auth.Interface.ApplicationServices;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Payment.Interface.Data;
using AFT.RegoV2.Core.Payment.Interface.ApplicationServices;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Core.Security.Interfaces;
using AutoMapper;
using PaymentGateway = AFT.RegoV2.AdminApi.Interface.Payment.PaymentGateway;
namespace AFT.RegoV2.AdminApi.Controllers.Payment
{
    [Authorize]
    public class PaymentGatewaySettingsController : BaseApiController
    {
        private readonly IPaymentQueries _paymentQueries;
        private readonly IPaymentGatewaySettingsQueries _paymentGatewaySettingsQueries;
        private readonly IPaymentGatewaySettingsCommands _paymentGatewaySettingsCommands;
        private readonly IAdminQueries _adminQueries;

        public PaymentGatewaySettingsController(
            IPaymentQueries paymentQueries,
            IPaymentGatewaySettingsQueries paymentGatewaySettingsQueries,
            IPaymentGatewaySettingsCommands paymentGatewaySettingsCommands,
            IAuthQueries authQueries,
            IAdminQueries adminQueries)
            : base(authQueries, adminQueries)
        {
            _paymentQueries = paymentQueries;
            _adminQueries = adminQueries;
            _paymentGatewaySettingsQueries = paymentGatewaySettingsQueries;
            _paymentGatewaySettingsCommands = paymentGatewaySettingsCommands;

            Mapper
              .CreateMap
              <Core.Payment.Interface.Data.PaymentGatewaySettings, PaymentGatewaySettingsViewDataResponse>();
        }

        [HttpPost]
        [Route(AdminApiRoutes.ListPaymentGatewaySettings)]
        [Filters.SearchPackageFilter("searchPackage")]
        public SearchPackageResult List([FromUri] SearchPackage searchPackage)
        {
            VerifyPermission(Permissions.View, Modules.PaymentGatewaySettings);

            return SearchData(searchPackage);
        }

        [HttpPost]
        [Route(AdminApiRoutes.AddPaymentGatewaySettings)]
        public SavePaymentGatewaySettingsResponse Add(SavePaymentGatewaySettingsRequest request)
        {
            VerifyPermission(Permissions.Create, Modules.PaymentGatewaySettings);

            CheckBrand(request.Brand);

            var model = Mapper.DynamicMap<Core.Payment.Interface.Data.Commands.SavePaymentGatewaysSettingsData>(request);

            var validatResult = _paymentGatewaySettingsCommands.ValidateThatPaymentGatewaySettingsCanBeAdded(model);
            if (false == validatResult.IsValid)
            {
                return ValidationErrorResponse<SavePaymentGatewaySettingsResponse>(validatResult);
            }

            var result = _paymentGatewaySettingsCommands.Add(model);

            return new SavePaymentGatewaySettingsResponse
            {
                Success = true,
                Id = result.PaymentGatewaySettingsId
            };
        }

        [HttpPost]
        [Route(AdminApiRoutes.EditPaymentGatewaySettings)]
        public SavePaymentGatewaySettingsResponse Edit(SavePaymentGatewaySettingsRequest request)
        {
            VerifyPermission(Permissions.Update, Modules.PaymentGatewaySettings);

            CheckBrand(request.Brand);

            var model = Mapper.DynamicMap<Core.Payment.Interface.Data.Commands.SavePaymentGatewaysSettingsData>(request);

            var validatResult = _paymentGatewaySettingsCommands.ValidateThatPaymentGatewaySettingsCanBeEdited(model);
            if (false == validatResult.IsValid)
            {
                return ValidationErrorResponse<SavePaymentGatewaySettingsResponse>(validatResult);
            }

            var result = _paymentGatewaySettingsCommands.Edit(model);

            return new SavePaymentGatewaySettingsResponse
            {
                Success = true,
                Id = result.PaymentGatewaySettingsId
            };
        }

        [HttpPost]
        [Route(AdminApiRoutes.ActivatePaymentGatewaySettings)]
        public ActivatePaymentGatewaySettingsResponse Activate(ActivatePaymentGatewaySettingsRequest request)
        {
            VerifyPermission(Permissions.Activate, Modules.PaymentGatewaySettings);

            var model = Mapper.DynamicMap<Core.Payment.Interface.Data.Commands.ActivatePaymentGatewaySettingsData>(request);

            var validatResult = _paymentGatewaySettingsCommands.ValidateThatPaymentGatewaySettingsCanBeActivated(model);
            if (false == validatResult.IsValid)
            {
                return ValidationErrorResponse<ActivatePaymentGatewaySettingsResponse>(validatResult);
            }

            var setting = _paymentGatewaySettingsQueries.GetPaymentGatewaySettingsById(request.Id);
            CheckBrand(setting.BrandId);

            _paymentGatewaySettingsCommands.Activate(model);
            return new ActivatePaymentGatewaySettingsResponse
            {
                Success = true
            };
        }

        [HttpPost]
        [Route(AdminApiRoutes.DeactivatePaymentGatewaySettings)]
        public DeactivatePaymentGatewaySettingsResponse Deactivate(DeactivatePaymentGatewaySettingsRequest request)
        {
            VerifyPermission(Permissions.Deactivate, Modules.PaymentGatewaySettings);

            var model = Mapper.DynamicMap<Core.Payment.Interface.Data.Commands.DeactivatePaymentGatewaySettingsData>(request);

            var validatResult = _paymentGatewaySettingsCommands.ValidateThatPaymentGatewaySettingsCanBeDeactivated(model);
            if (false == validatResult.IsValid)
            {
                return ValidationErrorResponse<DeactivatePaymentGatewaySettingsResponse>(validatResult);
            }

            var setting = _paymentGatewaySettingsQueries.GetPaymentGatewaySettingsById(request.Id);
            CheckBrand(setting.BrandId);

            _paymentGatewaySettingsCommands.Deactivate(model);
            return new DeactivatePaymentGatewaySettingsResponse
            {
                Success = true
            };
        }

        [HttpPost]
        [Route(AdminApiRoutes.GetPaymentGatewaysInPaymentGatewaySettings)]
        public GetPaymentGatewaysResponse GetPaymentGateways(GetPaymentGatewaysRequest request)
        {
            if (request.BrandId.HasValue)
            {
                CheckBrand(request.BrandId.Value);
            }

            var paymentGateways = _paymentGatewaySettingsQueries
              .GetPaymentGateways(request.BrandId);

            var requestResultMapped = Mapper.Map<IEnumerable<PaymentGateway>>(paymentGateways);
            return new GetPaymentGatewaysResponse{
                PaymentGateways = requestResultMapped
            };
        }

        [HttpGet]
        [Route(AdminApiRoutes.GetPaymentGatewaySettingsById)]
        public PaymentGatewaySettingsViewDataResponse GetById(Guid id)
        {
            var setting = _paymentGatewaySettingsQueries.GetPaymentGatewaySettingsById(id);
            var response = Mapper.Map<PaymentGatewaySettingsViewDataResponse>(setting);

            CheckBrand(response.BrandId);

            return response;
        }

        #region Methods
        private SearchPackageResult SearchData(SearchPackage searchPackage)
        {
            var brandFilterSelections = _adminQueries.GetBrandFilterSelections();

            var paymentGatewaySettings = _paymentQueries.GetPaymentGatewaySettings()
                .Where(x => brandFilterSelections.Contains(x.BrandId));

            var dataBuilder = new SearchPackageDataBuilder<PaymentGatewaySettings>(searchPackage, paymentGatewaySettings);

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
                            Format.FormatDate(settings.DateCreated, false),
                            settings.UpdatedBy,
                            Format.FormatDate(settings.DateUpdated, false),
                            settings.ActivatedBy,
                            Format.FormatDate(settings.DateActivated, false),
                            settings.DeactivatedBy,
                            Format.FormatDate(settings.DateDeactivated, false)
                        }
                );
            var data = dataBuilder.GetPageData(setting => setting.Brand.Name);
            return data;
        }
        #endregion
    }
}