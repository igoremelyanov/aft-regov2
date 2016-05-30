using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using AFT.RegoV2.AdminApi.Controllers.Base;
using AFT.RegoV2.AdminApi.Interface.Common;
using AFT.RegoV2.AdminApi.Interface.Payment;
using AFT.RegoV2.Core.Auth.Interface.ApplicationServices;
using AFT.RegoV2.Core.Common.Data.Base;
using AFT.RegoV2.Core.Payment.Interface.ApplicationServices;
using AFT.RegoV2.Core.Payment.Interface.Data;
using AFT.RegoV2.Core.Payment.Interface.Data.Commands;
using AFT.RegoV2.Core.Payment.Interface.Exceptions;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Core.Security.Interfaces;
using AFT.RegoV2.Shared;
using AutoMapper;
using FluentValidation.Results;

namespace AFT.RegoV2.AdminApi.Controllers.Payment
{
    [Authorize]
    public class OfflineDepositController : BaseApiController
    {
        private readonly IPaymentQueries _paymentQueries;
        private readonly IOfflineDepositCommands _offlineDepositCommands;
        
        public OfflineDepositController(
            IPaymentQueries paymentQueries,
            IOfflineDepositCommands offlineDepositCommands,
            IAuthQueries authQueries,
            IAdminQueries adminQueries)
            : base(authQueries, adminQueries)
        {
            _paymentQueries = paymentQueries;
            _offlineDepositCommands = offlineDepositCommands;                        
        }

        [HttpGet]
        [Route(AdminApiRoutes.GetOfflineDepositById)]
        public GetOfflineDepositByIdResponse GetById(Guid id)
        {
            var deposit = _paymentQueries.GetDepositByIdForViewRequest(id);            

            CheckBrand(deposit.BrandId);

            return new GetOfflineDepositByIdResponse
            {
                OfflineDeposit = Mapper.Map<OfflineDepositDto>(deposit)
            };
        }

        [HttpPost]
        [Route(AdminApiRoutes.CreateOfflineDeposit)]
        public async Task<CreateOfflineDepositResponse> Create(CreateOfflineDepositRequest request)
        {
            VerifyPermission(Permissions.Create, Modules.OfflineDepositRequests);

            var player = _paymentQueries.GetPlayer(request.PlayerId);            

            CheckBrand(player.BrandId);

            var command = Mapper.Map<OfflineDepositRequest>(request);

            //TODO: This is a temporary solution. We need to have unified way of returning validation errors
            //TODO: The problem specifically for 'PaymentSettingsViolatedException' is that it doesn't use FluentValidation.
            //TODO: PaymentSettingsValidator.cs has to be replaced with FluentValidation in order to fall under the common pattern that we use.
            OfflineDeposit response;
            try
            {
                response = await _offlineDepositCommands.Submit(command);
            }
            catch (PaymentSettingsViolatedException ex)
            {

                var errors = new List<ValidationError> {
                    new ValidationError
                    {
                        ErrorMessage = ex.Message
                    }
                };

                return new CreateOfflineDepositResponse()
                {
                    Success = false,
                    Errors = errors
                };
            }

            return new CreateOfflineDepositResponse
            {
                Success = true,
                Id = response.Id
            };
        }

        [HttpPost]
        [Route(AdminApiRoutes.ConfirmOfflineDeposit)]
        public ConfirmOfflineDepositResponse Confirm(ConfirmOfflineDepositRequest request)
        {
            VerifyPermission(Permissions.Confirm, Modules.OfflineDepositConfirmation);

            var data = _paymentQueries.GetDepositByIdForViewRequest(request.Id);

            CheckBrand(data.BrandId);

            var model = Mapper.Map<OfflineDepositConfirm>(request);

            var deposit = _offlineDepositCommands.Confirm(model,request.CurrentUser, request.IdFrontImageFile, request.IdBackImageFile, request.ReceiptImageFile);

            return new ConfirmOfflineDepositResponse
            {
                Success = true,
                PlayerId = deposit.PlayerId,
                IdFrontImageId = deposit.IdFrontImage,
                IdBackImageId = deposit.IdBackImage,
                ReceiptImageId = deposit.ReceiptImage,
            };
        }

        [HttpPost]
        [Route(AdminApiRoutes.VerifyOfflineDeposit)]
        public VerifyOfflineDepositResponse Verify(VerifyOfflineDepositRequest request)
        {
            VerifyPermission(Permissions.Verify, Modules.DepositVerification);

            var data = _paymentQueries.GetDepositByIdForViewRequest(request.Id);

            CheckBrand(data.BrandId);

            _offlineDepositCommands.Verify(request.Id,request.BankAccountId,request.Remarks);

            return new VerifyOfflineDepositResponse
            {
                Success = true
            };
        }

        [HttpPost]
        [Route(AdminApiRoutes.UnverifyOfflineDeposit)]
        public UnverifyOfflineDepositResponse Unverify(UnverifyOfflineDepositRequest request)
        {
            VerifyPermission(Permissions.Unverify, Modules.DepositVerification);

            var data = _paymentQueries.GetDepositByIdForViewRequest(request.Id);

            CheckBrand(data.BrandId);

            _offlineDepositCommands.Unverify(request.Id, request.Remarks, request.UnverifyReason);

            return new UnverifyOfflineDepositResponse
            {
                Success = true
            };
        }

        [HttpPost]
        [Route(AdminApiRoutes.ApproveOfflineDeposit)]
        public ApproveOfflineDepositResponse Approve(ApproveOfflineDepositRequest request)
        {
            VerifyPermission(Permissions.Approve, Modules.DepositApproval);

            var data = _paymentQueries.GetDepositByIdForViewRequest(request.Id);

            CheckBrand(data.BrandId);

            var command = Mapper.Map<OfflineDepositApprove>(request);
            
            try
            {
                _offlineDepositCommands.Approve(command);
            }
            catch (RegoException ex)
            {
                var errors = new List<ValidationError> { new ValidationError {ErrorMessage = ex.Message}};

                return new ApproveOfflineDepositResponse
                {
                    Success = false,
                    Errors = errors
                };
            }
            catch (PaymentSettingsViolatedException ex)
            {

                var errors = new List<ValidationError> {
                    new ValidationError
                    {
                        //PropertyName = ex.Message,
                        ErrorMessage = ex.Message
                    }
                };

                return new ApproveOfflineDepositResponse
                {
                    Success = false,
                    Errors = errors
                };
            }

            return new ApproveOfflineDepositResponse
            {
                Success = true
            };
        }

        [HttpPost]
        [Route(AdminApiRoutes.RejectOfflineDeposit)]
        public RejectOfflineDepositResponse Reject(RejectOfflineDepositRequest request)
        {
            VerifyPermission(Permissions.Reject, Modules.DepositApproval);

            var data = _paymentQueries.GetDepositByIdForViewRequest(request.Id);

            CheckBrand(data.BrandId);

            _offlineDepositCommands.Reject(request.Id, request.Remarks);

            return new RejectOfflineDepositResponse
            {
                Success = true
            };
        }
        #region Methods

        #endregion
    }
}