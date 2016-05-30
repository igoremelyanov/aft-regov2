using System;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web.Http;
using AFT.RegoV2.AdminApi.Controllers.Base;
using AFT.RegoV2.AdminApi.Interface.Common;
using AFT.RegoV2.AdminWebsite.Common.jqGrid;
using AFT.RegoV2.Core.Auth.Interface.ApplicationServices;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Brand.Interface.Data;
using AFT.RegoV2.Core.Common.Data.Player;
using AFT.RegoV2.Core.Player.ApplicationServices;
using AFT.RegoV2.Core.Player.Data;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Core.Security.Interfaces;
using AFT.RegoV2.Infrastructure.DataAccess.Security;
using AFT.RegoV2.Shared.Constants;

namespace AFT.RegoV2.AdminApi.Controllers.Admin
{
    public class IdentificationDocumentSettingsController : BaseApiController
    {
        private readonly BrandQueries _brandQueries;
        private readonly PlayerQueries _playerQueries;
        private readonly IdentificationDocumentSettingsService _service;
        private readonly SecurityRepository _securityRepository;

        public IdentificationDocumentSettingsController(
            BrandQueries brandQueries,
            PlayerQueries playerQueries,
            IdentificationDocumentSettingsService service,
            SecurityRepository securityRepository,
            IAuthQueries authQueries,
            IAdminQueries adminQueries)
            : base(authQueries, adminQueries)
        {
            _brandQueries = brandQueries;
            _playerQueries = playerQueries;
            _service = service;
            _securityRepository = securityRepository;
        }

        [HttpGet]
        [Filters.SearchPackageFilter("searchPackage")]
        [Route(AdminApiRoutes.ListIdentificationDocumentSettings)]
        public IHttpActionResult Data([FromUri] SearchPackage searchPackage)
        {
            VerifyPermission(Permissions.View, Modules.IdentificationDocumentSettings);

            return Ok(SearchData(searchPackage));
        }

        [HttpPost]
        [Route(AdminApiRoutes.CreateSettingInIdentificationDocumentSettings)]
        public IHttpActionResult CreateSetting(IdentificationDocumentSettingsData data)
        {
            VerifyPermission(Permissions.Create, Modules.IdentificationDocumentSettings);

            try
            {
                _service.CreateSetting(data);
            }
            catch (Exception ex)
            {
                return Ok(new { result = "fail", data = ex.Message });
            }

            return Ok(new { result = "success" });
        }

        [HttpGet]
        [Route(AdminApiRoutes.GetEditDataInIdentificationDocumentSettings)]
        public IHttpActionResult GetEditData(Guid? id = null)
        {
            if (!id.HasValue)
            {
                return Ok(new
                {
                    Licensees = GetLicensees(),
                    TransactionTypes = Enum.GetNames(typeof(TransactionType)),
                });
            }

            var setting = _service.GetSettingById(id.Value);

            return Ok(new
            {
                Licensees = GetLicensees(),
                TransactionTypes = Enum.GetNames(typeof(TransactionType)),
                Setting = new
                {
                    LicenseeId = setting.LicenseeId.ToString(),
                    BrandId = setting.BrandId.ToString(),
                    TransactionType = setting.TransactionType.HasValue ? setting.TransactionType.Value.ToString() : null,
                    PaymentGatewayBankAccountId = setting.PaymentGatewayBankAccountId,
                    setting.IdBack,
                    setting.IdFront,
                    setting.CreditCardFront,
                    setting.CreditCardBack,
                    setting.POA,
                    setting.DCF,
                    setting.Remark
                }
            });
        }

        [HttpPost]
        [Route(AdminApiRoutes.UpdateSettingInIdentificationDocumentSettings)]
        public IHttpActionResult UpdateSetting(IdentificationDocumentSettingsData data)
        {
            VerifyPermission(Permissions.Update, Modules.IdentificationDocumentSettings);
            try
            {
                _service.UpdateSetting(data);
            }
            catch (Exception ex)
            {
                return Ok(new { result = "fail", data = ex.Message });
            }

            return Ok(new
            {
                result = "success",
                data = new
                {
                    Setting = data
                }
            });
        }

        [HttpGet]
        [Route(AdminApiRoutes.GetLicenseeBrandsInIdentificationDocumentSettings)]
        public IHttpActionResult GetLicenseeBrands([FromUri] Guid licenseeId)
        {
            var brands = _brandQueries.GetBrands().Where(b => b.Licensee.Id == licenseeId);

            return Ok(new
            {
                Brands = brands
                    .OrderBy(l => l.Name)
                    .Select(l => new { l.Name, l.Id })
            });
        }

        private IQueryable GetLicensees()
        {
            var licensees = _brandQueries.GetLicensees();

            var user = _securityRepository.Admins
                .Include(x => x.Role)
                .Include(x => x.Licensees)
                .Single(u => u.Id == UserId);

            if (user.Role.Id == RoleIds.LicenseeId || user.Role.Id == RoleIds.SingleBrandManagerId)
            {
                licensees = licensees
                    .Where(l => user.Licensees.Any(x => l.Id == x.Id) && l.Status == LicenseeStatus.Active)
                    .AsQueryable();
            }

            return licensees
                .OrderBy(l => l.Name)
                .Select(l => new { l.Name, l.Id });
        }

        [HttpGet]
        [Route(AdminApiRoutes.GetPaymentMethodsInIdentificationDocumentSettings)]
        public IHttpActionResult GetPaymentMethods([FromUri] Guid brandId)
        {
            return Ok(new
                {
                    PaymentMethods = _playerQueries.GetBankAccounts(brandId)
                        .Select(x => new
                        {
                            x.Id,
                            Name = string.Format("Offline - {0}", x.AccountId)
                        })
                });
        }

        private object SearchData(SearchPackage searchPackage)
        {
            var settings = _service.GetSettings();
            var dataBuilder = new SearchPackageDataBuilder<IdentificationDocumentSettings>(searchPackage, settings);

            return dataBuilder
                .Map(setting => setting.Id, setting => GetRoleCell(setting))
                .GetPageData(setting => setting.CreatedBy);
        }

        private object GetRoleCell(IdentificationDocumentSettings setting)
        {
            var licensee = _brandQueries.GetLicensee(setting.LicenseeId);
            var brand = _brandQueries.GetBrand(setting.BrandId);
            return new[]
            {
                licensee.Name,
                brand.Name,
                setting.TransactionType.HasValue ? setting.TransactionType.Value.ToString() : "-",
                "Offline-" + setting.PaymentGatewayBankAccount.AccountId,
                GetRequiredDocumentData(setting),
                setting.Remark,
                setting.CreatedBy,
                setting.CreatedOn.ToString("yyyy/MM/dd")
            };
        }

        private static string GetRequiredDocumentData(IdentificationDocumentSettings setting)
        {
            var builder = new StringBuilder();

            if (setting.IdFront)
                builder.AppendLine("ID (front)");

            if (setting.IdBack)
                builder.AppendLine("ID (back)");

            if (setting.CreditCardFront)
                builder.AppendLine("Credir Card (front)");

            if (setting.CreditCardBack)
                builder.AppendLine("Credir Card (back)");

            if (setting.POA)
                builder.AppendLine("POA");

            if (setting.DCF)
                builder.AppendLine("DCF");

            var result = builder.ToString();

            return string.IsNullOrEmpty(result)
                ? "-"
                : result;
        }
    }
}