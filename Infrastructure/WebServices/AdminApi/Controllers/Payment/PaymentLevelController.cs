using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using AFT.RegoV2.AdminApi.Controllers.Base;
using AFT.RegoV2.AdminApi.Interface.Common;
using AFT.RegoV2.AdminApi.Interface.Payment;
using AFT.RegoV2.Core.Auth.Interface.ApplicationServices;
using AFT.RegoV2.Core.Payment.Interface.ApplicationServices;
using AFT.RegoV2.Core.Payment.Interface.Data;
using AFT.RegoV2.Core.Security.Interfaces;
using AutoMapper;
using PaymentLevel = AFT.RegoV2.AdminApi.Interface.Payment.PaymentLevel;
namespace AFT.RegoV2.AdminApi.Controllers.Payment
{
    [Authorize]
    public class PaymentLevelController : BaseApiController
    {       
        private readonly IPaymentQueries _paymentQueries;
        private readonly IAdminQueries _adminQueries;
        public PaymentLevelController(
            IPaymentQueries paymentQueries,
            IAuthQueries authQueries,
            IAdminQueries adminQueries)
            : base(authQueries, adminQueries)
        {
            _paymentQueries = paymentQueries;
            _adminQueries = adminQueries;
        }

        [HttpGet]
        [Route(AdminApiRoutes.GetPaymentLevels)]
        public GetPaymentLevelsResponse GetPaymentLevels()
        {
            var brandFilterSelections = _adminQueries.GetBrandFilterSelections();

            var query = _paymentQueries.GetPaymentLevelsAsQueryable()
                .Where(x => brandFilterSelections.Contains(x.BrandId) && x.Status == PaymentLevelStatus.Active);                           

            var requestResultMapped = Mapper.Map<IEnumerable<PaymentLevel>>(query);

            return new GetPaymentLevelsResponse
            {
                PaymentLevels = requestResultMapped
            };
        }
        #region Methods

        #endregion
    }
}