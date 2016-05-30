using System;
using System.Linq;
using System.Web;
using System.Web.Http;
using AFT.RegoV2.AdminApi.Controllers.Base;
using AFT.RegoV2.AdminApi.Interface.Common;
using AFT.RegoV2.AdminWebsite.Common;
using AFT.RegoV2.AdminWebsite.Common.jqGrid;
using AFT.RegoV2.Core.Auth.Interface.ApplicationServices;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Brand;
using AFT.RegoV2.Core.Messaging.Interface.ApplicationServices;
using AFT.RegoV2.Core.Messaging.Interface.Data.MassMessageCommands;
using AFT.RegoV2.Core.Payment.Interface.ApplicationServices;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Core.Security.Interfaces;

namespace AFT.RegoV2.AdminApi.Controllers.Messaging
{
    [Authorize]
    public class MassMessageController : BaseApiController
    {
        private readonly IMassMessageQueries _massMessageQueries;
        private readonly IMassMessageCommands _massMessageCommands;
        private readonly BrandQueries _brandQueries;
        private readonly IPaymentQueries _paymentQueries;
        private readonly IAdminQueries _adminQueries;

        public MassMessageController(
            IMassMessageQueries massMessageQueries,
            IMassMessageCommands massMessageCommands,
            BrandQueries brandQueries,
            IPaymentQueries paymentQueries,
            IAuthQueries authQueries,
            IAdminQueries adminQueries)
            : base(authQueries, adminQueries)
        {
            _massMessageQueries = massMessageQueries;
            _massMessageCommands = massMessageCommands;
            _brandQueries = brandQueries;
            _paymentQueries = paymentQueries;
            _adminQueries = adminQueries;
        }

        [HttpGet]
        [Route(AdminApiRoutes.GetNewDataInMassMessage)]
        public NewMassMessageDataResponse GetNewData()
        {
            VerifyPermission(Permissions.Send, Modules.MassMessageTool);

            var licenseeFilterSelections = _adminQueries.GetLicenseeFilterSelections();
            var brandFilterSelections = _adminQueries.GetBrandFilterSelections();

            var licensees = _brandQueries.GetLicensees()
                .Where(licensee =>
                    licensee.Brands.Any(y => y.Status == BrandStatus.Active) &&
                    licenseeFilterSelections.Contains(licensee.Id))
                .OrderBy(licensee => licensee.Name)
                .ToArray();

            var licenseeIds = licensees.Select(licensee => licensee.Id).ToArray();

            var brands = _brandQueries.GetBrands()
                .Where(brand =>
                    brand.Status == BrandStatus.Active &&
                    brandFilterSelections.Contains(brand.Id) &&
                    licenseeIds.Contains(brand.LicenseeId))
                .OrderBy(brand => brand.Name)
                .ToArray();

            var brandIds = brands.Select(brand => brand.Id).ToArray();

            var paymentLevels = _paymentQueries.GetPaymentLevelsAsQueryable()
                .Where(paymentLevel => brandIds.Contains(paymentLevel.BrandId))
                .OrderBy(paymentLevel => paymentLevel.Name)
                .ToArray();

            var vipLevels = _brandQueries.GetVipLevels()
                .Where(vipLevel => brandIds.Contains(vipLevel.BrandId))
                .OrderBy(vipLevel => vipLevel.Name)
                .ToArray();

            var response = new NewMassMessageDataResponse
            {
                Licensees = licensees.Select(licensee => new NewMassMessageLicensee
                {
                    Id = licensee.Id,
                    Name = licensee.Name,
                    Brands = brands.Where(brand => brand.LicenseeId == licensee.Id)
                        .Select(brand => new NewMassMessageBrand
                        {
                            Id = brand.Id,
                            Name = brand.Name,
                            PaymentLevels = paymentLevels.Where(paymentLevel => paymentLevel.BrandId == brand.Id)
                                .Select(paymentLevel => new NewMassMessagePaymentLevel
                                {
                                    Id = paymentLevel.Id,
                                    Name = paymentLevel.Name
                                }).ToArray(),
                            VipLevels = vipLevels.Where(vipLevel => vipLevel.BrandId == brand.Id)
                                .Select(vipLevel => new NewMassMessageVipLevel
                                {
                                    Id = vipLevel.Id,
                                    Name = vipLevel.Name
                                }).ToArray()
                        }).ToArray()
                }).ToArray()
            };

            return response;
        }

        [HttpGet]
        [Filters.SearchPackageFilter("searchPackage")]
        [Route(AdminApiRoutes.SearchPlayersListInMassMessage)]
        public SearchPackageResult SearchPlayersList([FromUri] SearchPackage searchPackage, [FromUri] SearchPlayersRequest request, [FromUri] Guid? id)
        {
            VerifyPermission(Permissions.Send, Modules.MassMessageTool);

            var dataBuilder = new SearchPackageDataBuilder<Core.Messaging.Interface.Data.Player>(
                searchPackage,
                _massMessageQueries.CreateMassMessagePlayerQuery(request));

            var recipients = id.HasValue
                ? _massMessageQueries.GetRecipients(id.Value).Select(x => x.Id).ToArray()
                : new Guid[]{};

            dataBuilder.SetFilterRule(x => x.BrandId, (value) => y => y.BrandId == new Guid(value))
                .Map(player => player.Id,
                    player => new object[]
                    {
                        player.Username,
                        player.FirstName,
                        player.LastName,
                        player.Email,
                        player.PhoneNumber,
                        player.PaymentLevelName,
                        player.VipLevelName,
                        Enum.GetName(typeof(Status), player.IsActive ? Status.Active : Status.Inactive),
                        Format.FormatDate(player.DateRegistered, false),
                        recipients.Contains(player.Id)
                    });

            return dataBuilder.GetPageData(player => player.Username);
        }

        [HttpPost]
        [Route(AdminApiRoutes.UpdateRecipientsInMassMessage)]
        public UpdateRecipientsResponse UpdateRecipients(UpdateRecipientsRequest request)
        {
            VerifyPermission(Permissions.Send, Modules.MassMessageTool);
            request.IpAddress = HttpContext.Current.Request.UserHostAddress;
            var validationResult = _massMessageQueries.GetValidationResult(request);

            if (!validationResult.IsValid)
                return ValidationErrorResponse<UpdateRecipientsResponse>(validationResult);

            var response = _massMessageCommands.UpdateRecipients(request);
            
            return response;
        }

        [HttpGet]
        [Filters.SearchPackageFilter("searchPackage")]
        [Route(AdminApiRoutes.RecipientsListInMassMessage)]
        public SearchPackageResult RecipientsList([FromUri] SearchPackage searchPackage, [FromUri] Guid? id)
        {
            VerifyPermission(Permissions.Send, Modules.MassMessageTool);

            var query = id.HasValue
                ? _massMessageQueries.GetRecipients(id.Value)
                : Enumerable.Empty<Core.Messaging.Interface.Data.Player>().AsQueryable();

            var dataBuilder = new SearchPackageDataBuilder<Core.Messaging.Interface.Data.Player>(
                searchPackage,
                query);

            dataBuilder.SetFilterRule(x => x.BrandId, (value) => y => y.BrandId == new Guid(value))
                .Map(player => player.Id,
                    player => new[]
                    {
                        player.Username,
                        player.FirstName,
                        player.LastName,
                        player.Email,
                        player.PhoneNumber,
                        player.PaymentLevelName,
                        player.VipLevelName,
                        Enum.GetName(typeof(Status), player.IsActive ? Status.Active : Status.Inactive),
                        Format.FormatDate(player.DateRegistered, false)
                    });

            return dataBuilder.GetPageData(player => player.Username);
        }

        [HttpPost]
        [Route(AdminApiRoutes.SendMassMessage)]
        public SendMassMessageResponse Send(SendMassMessageRequest request)
        {
            VerifyPermission(Permissions.Send, Modules.MassMessageTool);

            var validationResult = _massMessageQueries.GetValidationResult(request);

            if (!validationResult.IsValid)
                return ValidationErrorResponse<SendMassMessageResponse>(validationResult);

            var response = _massMessageCommands.Send(request);

            return response;
        }
    }
}