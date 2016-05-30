using System.Web.Http;
using AFT.RegoV2.AdminApi.Controllers.Base;
using AFT.RegoV2.Core.Auth.Interface.ApplicationServices;
using AFT.RegoV2.Core.Payment.Interface.ApplicationServices;
using AFT.RegoV2.Core.Security.Interfaces;

namespace AFT.RegoV2.AdminApi.Controllers.Payment
{
    [Authorize]
    public class PaymentController : BaseApiController
    {       
        private readonly IBankAccountQueries _bankAccountQueries;

        public PaymentController(            
            IBankAccountQueries bankAccountQueries,
            IAuthQueries authQueries,
            IAdminQueries adminQueries)
            : base(authQueries, adminQueries)
        {
            _bankAccountQueries = bankAccountQueries;
        }
     

        #region Methods

        #endregion
    }
}