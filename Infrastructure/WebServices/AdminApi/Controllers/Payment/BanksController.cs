using System;
using System.Web.Http;
using AFT.RegoV2.AdminApi.Controllers.Base;
using AFT.RegoV2.AdminApi.Interface.Common;
using AFT.RegoV2.AdminApi.Interface.Payment;
using AFT.RegoV2.AdminWebsite.Common.jqGrid;
using AFT.RegoV2.Core.Auth.Interface.ApplicationServices;
using AFT.RegoV2.Core.Payment.Interface.ApplicationServices;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Core.Security.Interfaces;
using AutoMapper;

namespace AFT.RegoV2.AdminApi.Controllers.Payment
{
    [Authorize]
    public class BanksController : BaseApiController
    {
        private readonly IPaymentQueries _paymentQueries;
        private readonly IBankCommands _bankCommands;
        private readonly IBankQueries _bankQueries;        

        public BanksController(
            IPaymentQueries paymentQueries,
            IBankCommands bankCommands,
            IBankQueries bankQueries,
            IAuthQueries authQueries,
            IAdminQueries adminQueries)
            : base(authQueries, adminQueries)
        {
            _paymentQueries = paymentQueries;            
            _bankCommands = bankCommands;
            _bankQueries = bankQueries;            
        }

        [HttpPost]
        [Route(AdminApiRoutes.ListBank)]
        [Filters.SearchPackageFilter("searchPackage")]
        public SearchPackageResult List([FromUri] SearchPackage searchPackage)
        {
            VerifyPermission(Permissions.View, Modules.Banks);
            //TODO:AFTREGO-4143
            return new SearchPackageResult();
        }

        [HttpPost]
        [Route(AdminApiRoutes.AddBank)]
        public SaveBankResponse Add(AddBankRequest request)
        {
            VerifyPermission(Permissions.Create, Modules.Banks);

            CheckBrand(request.BrandId);

            var data = Mapper.Map<Core.Payment.Interface.Data.Commands.AddBankData>(request);
            var validationResult = _bankQueries.ValidateCanAdd(data);

            if (!validationResult.IsValid)
                return ValidationErrorResponse<SaveBankResponse>(validationResult);

            var bankId = _bankCommands.Add(data);

            return new SaveBankResponse
            {
                Success = true,
                Id = bankId
            };
        }

        [HttpPost]
        [Route(AdminApiRoutes.EditBank)]
        public SaveBankResponse Edit(EditBankRequest request)
        {
            VerifyPermission(Permissions.Update, Modules.Banks);

            CheckBrand(request.BrandId);

            var data = Mapper.Map<Core.Payment.Interface.Data.Commands.EditBankData>(request);
            var validationResult = _bankQueries.ValidateCanEdit(data);

            if (!validationResult.IsValid)
                return ValidationErrorResponse<SaveBankResponse>(validationResult);

            _bankCommands.Edit(data);

            return new SaveBankResponse
            {
                Success = true
            };
        }

        [HttpGet]
        [Route(AdminApiRoutes.GetBankById)]
        public GetBankByIdResponse GetById(Guid id)
        {
            var bank = _paymentQueries.GetBank(id);

            var mappedBank = Mapper.Map<BankDto>(bank);

            CheckBrand(mappedBank.BrandId);

            return new GetBankByIdResponse
            {
                Bank = mappedBank
            };
        }

        #region Methods

        #endregion
    }
}