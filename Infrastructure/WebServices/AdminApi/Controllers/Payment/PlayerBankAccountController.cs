using System.Linq;
using System.Web.Http;
using AFT.RegoV2.AdminApi.Controllers.Base;
using AFT.RegoV2.AdminApi.Interface.Common;
using AFT.RegoV2.AdminApi.Interface.Payment;
using AFT.RegoV2.AdminWebsite.Common.jqGrid;
using AFT.RegoV2.Core.Auth.Interface.ApplicationServices;
using AFT.RegoV2.Core.Payment.Interface.ApplicationServices;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Core.Security.Interfaces;

namespace AFT.RegoV2.AdminApi.Controllers.Payment
{
    [Authorize]
    public class PlayerBankAccountController : BaseApiController
    {
        private readonly IPlayerBankAccountQueries _playerBankAccountQueries;
        private readonly IPlayerBankAccountCommands _playerBankAccountCommands;
        
        public PlayerBankAccountController(
            IPlayerBankAccountQueries playerBankAccountQueries,
            IPlayerBankAccountCommands playerBankAccountCommands,
            IAuthQueries authQueries,
            IAdminQueries adminQueries)
            : base(authQueries, adminQueries)
        {
            _playerBankAccountQueries = playerBankAccountQueries;
            _playerBankAccountCommands = playerBankAccountCommands;          
        }

        [HttpPost]
        [SearchPackageFilter("searchPackage")]
        public object List(SearchPackage searchPackage)
        {
            VerifyPermission(Permissions.View, Modules.PlayerBankAccount);
            return new object();
        }
       
        [HttpPost]
        [Route(AdminApiRoutes.VerifyPlayerBankAccount)]
        public VerifyPlayerBankAccountResponse Verify(VerifyPlayerBankAccountRequest request)
        {
            VerifyPermission(Permissions.Verify, Modules.PlayerBankAccount);
                       
            var playerBankAccount = _playerBankAccountQueries.GetPlayerBankAccounts().FirstOrDefault(x=>x.Id==request.Id);

            CheckBrand(playerBankAccount.Bank.BrandId);

            _playerBankAccountCommands.Verify(request.Id, request.Remarks);
            return new VerifyPlayerBankAccountResponse
            {
                Success = true
            };
        }

        [HttpPost]
        [Route(AdminApiRoutes.RejectPlayerBankAccount)]
        public RejectPlayerBankAccountResponse Reject(RejectPlayerBankAccountRequest request)
        {
            VerifyPermission(Permissions.Reject, Modules.PlayerBankAccount);

            var playerBankAccount = _playerBankAccountQueries.GetPlayerBankAccounts().FirstOrDefault(x => x.Id == request.Id);

            CheckBrand(playerBankAccount.Bank.BrandId);

            _playerBankAccountCommands.Reject(request.Id, request.Remarks);
            return new RejectPlayerBankAccountResponse
            {
                Success = true
            };
        }
        #region Methods

        #endregion
    }
}