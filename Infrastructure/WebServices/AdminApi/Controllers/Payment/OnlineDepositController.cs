using System;
using System.Web;
using System.Web.Http;
using AFT.RegoV2.AdminApi.Controllers.Base;
using AFT.RegoV2.AdminApi.Interface.Common;
using AFT.RegoV2.AdminApi.Interface.Payment;
using AFT.RegoV2.Core.Auth.Interface.ApplicationServices;
using AFT.RegoV2.Core.Payment.Interface.ApplicationServices;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Core.Security.Interfaces;
using AutoMapper;

namespace AFT.RegoV2.AdminApi.Controllers.Payment
{
    [Authorize]
    public class OnlineDepositController : BaseApiController
    {
        private readonly IOnlineDepositCommands _onlineDepositCommands;
        private readonly IOnlineDepositQueries _onlineDepositQueries;
        private readonly IAuthQueries _authQueries;

        public OnlineDepositController(
            IOnlineDepositCommands onlineDepositCommands,
            IOnlineDepositQueries onlineDepositQueries,
            IAuthQueries authQueries,
            IAdminQueries adminQueries)
            : base(authQueries, adminQueries)
        {
            _onlineDepositCommands = onlineDepositCommands;
            _onlineDepositQueries = onlineDepositQueries;
            _authQueries = authQueries;
        }

        [HttpGet]
        [Route(AdminApiRoutes.GetOnlineDepositById)]
        public OnlineDepositViewDataResponse GetById(Guid id)
        {
            var deposit = _onlineDepositQueries.GetOnlineDepositById(id);

            var response = Mapper.Map<OnlineDepositViewDataResponse>(deposit);

            CheckBrand(response.BrandId);

            return response;
        }

        [HttpPost]
        [Route(AdminApiRoutes.VerifyOnlineDeposit)]
        public VerifyOnlineDepositResponse Verify(VerifyOnlineDepositRequest request)
        {
            VerifyPermission(Permissions.Verify, Modules.DepositVerification);

            var data = _onlineDepositQueries.GetOnlineDepositById(request.Id);

            CheckBrand(data.BrandId);

            var model = Mapper.Map<Core.Payment.Interface.Data.VerifyOnlineDepositRequest>(request);

            _onlineDepositCommands.Verify(model);

            return new VerifyOnlineDepositResponse
            {
                Success = true
            };
        }

        [HttpPost]
        [Route(AdminApiRoutes.UnverifyOnlineDeposit)]
        public VerifyOnlineDepositResponse Unverify(UnverifyOnlineDepositRequest request)
        {
            VerifyPermission(Permissions.Unverify, Modules.DepositVerification);

            var data = _onlineDepositQueries.GetOnlineDepositById(request.Id);

            CheckBrand(data.BrandId);

            var model = Mapper.Map<Core.Payment.Interface.Data.UnverifyOnlineDepositRequest>(request);

            _onlineDepositCommands.Unverify(model);

            return new VerifyOnlineDepositResponse
            {
                Success = true
            };
        }

        [HttpPost]
        [Route(AdminApiRoutes.ApproveOnlineDeposit)]
        public ApproveOnlineDepositResponse Approve(ApproveOnlineDepositRequest request)
        {
            VerifyPermission(Permissions.Approve, Modules.DepositApproval);

            var data = _onlineDepositQueries.GetOnlineDepositById(request.Id);

            CheckBrand(data.BrandId);

            var model = Mapper.Map<Core.Payment.Interface.Data.ApproveOnlineDepositRequest>(request);

            _onlineDepositCommands.Approve(model);

            return new ApproveOnlineDepositResponse
            {
                Success = true
            };
        }      

        [HttpPost]
        [Route(AdminApiRoutes.RejectOnlineDeposit)]
        public RejectOnlineDepositResponse Reject(RejectOnlineDepositRequest request)
        {
            if (false ==
                (_authQueries.VerifyPermission(UserId, Permissions.Reject, Modules.DepositVerification)
                || _authQueries.VerifyPermission(UserId, Permissions.Reject, Modules.DepositApproval)))
                throw new HttpException(403, "Access forbidden");

            var data = _onlineDepositQueries.GetOnlineDepositById(request.Id);

            CheckBrand(data.BrandId);

            var model = Mapper.Map<Core.Payment.Interface.Data.RejectOnlineDepositRequest>(request);

            _onlineDepositCommands.Reject(model);

            return new RejectOnlineDepositResponse
            {
                Success = true
            };
        }
    }
}