using System;
using System.Collections.Generic;
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
using AutoMapper;

namespace AFT.RegoV2.AdminApi.Controllers.Payment
{
    [Authorize]
    public class BankAccountsController : BaseApiController
    {
        private readonly IPaymentQueries _paymentQueries;
        private readonly IBankAccountQueries _bankAccountQueries;
        private readonly IBankAccountCommands _bankAccountCommands;
        
        public BankAccountsController(
            IPaymentQueries paymentQueries,
            IBankAccountQueries bankAccountQueries,
            IBankAccountCommands bankAccountCommands,
            IAuthQueries authQueries,
            IAdminQueries adminQueries)
            : base(authQueries, adminQueries)
        {
            _paymentQueries = paymentQueries;
            _bankAccountQueries = bankAccountQueries;
            _bankAccountCommands = bankAccountCommands;
        }

        [SearchPackageFilter("searchPackage")]
        public object List(SearchPackage searchPackage)
        {
            VerifyPermission(Permissions.View, Modules.BankAccounts);
            return new object();
        }

        [HttpPost]
        [Route(AdminApiRoutes.AddBankAccount)]
        public SaveBankAccountResponse Add(AddBankAccountRequest request)
        {
            VerifyPermission(Permissions.Create, Modules.BankAccounts);
            CheckBrand(new Guid(request.BrandId));

            var data = Mapper.Map<Core.Payment.Interface.Data.Commands.AddBankAccountData>(request);

            var id = _bankAccountCommands.AddWithFiles(
               data,
               request.IdFrontImageFile,
               request.IdBackImageFile,
               request.AtmCardImageFile);

            return new SaveBankAccountResponse
            {
                Success = true,
                Id = id
            };
        }

        [HttpPost]
        [Route(AdminApiRoutes.EditBankAccount)]
        public SaveBankAccountResponse Edit(EditBankAccountRequest request)
        {
            VerifyPermission(Permissions.Update, Modules.BankAccounts);
           
            CheckBrand(request.BrandId);

            var data = Mapper.Map<Core.Payment.Interface.Data.Commands.EditBankAccountData>(request);

            _bankAccountCommands.SaveChanges(
               data,
               request.IdFrontImageFile,
               request.IdBackImageFile,
               request.AtmCardImageFile);

            return new SaveBankAccountResponse
            {
                Success = true,
                Id = request.Id
            };
        }

        [HttpGet]
        [Route(AdminApiRoutes.GetCurrencyByUserId)]
        public GetBankAccountCurrencyResponse GetCurrencyByUserId(Guid userId)
        {
            var currencyData = _bankAccountQueries.GetCurrencyData(userId);

            return new GetBankAccountCurrencyResponse
            {
                Currency = currencyData
            };
        }

        [HttpGet]
        [Route(AdminApiRoutes.GetBankAccountById)]
        public GetBankAccountByIdResponse GetById(Guid id)
        {
            var bankAccount = _bankAccountQueries.GetBankAccountFull(id);

            CheckBrand(bankAccount.Bank.BrandId);

            return new GetBankAccountByIdResponse
            {
                BankAccount = Mapper.Map<BankAccountDto>(bankAccount)
            };
        }

        [HttpGet]
        [Route(AdminApiRoutes.GetBankAccountTypes)]
        public GetBankAccountTypesResponse GetBankAccountTypes()
        {
            var bankAccountTypes = _bankAccountQueries.GetBankAccountTypes();

            return new GetBankAccountTypesResponse
            {
                BankAccountTypes = Mapper.Map<IEnumerable<BankAccountTypeDto>>(bankAccountTypes)
            };
        }

        [HttpGet]
        [Route(AdminApiRoutes.GetBankListByBrandId)]
        public GetBankListByBrandIdResponse GetBankListByBrandId(Guid brandId)
        {            
            var banks = _bankAccountQueries.GetBanks(brandId)
                .Select(b => new BankDto {
                    Id = b.Id,
                    BankName = b.BankName }
                );

            return new GetBankListByBrandIdResponse
            {
                Banks = banks
            };
        }

        [HttpPost]
        [Route(AdminApiRoutes.ActivateBankAccount)]
        public ActivateBankAccountResponse Activate(ActivateBankAccountRequest request)
        {
            VerifyPermission(Permissions.Activate, Modules.BankAccounts);
                       
            var bankAccount = _bankAccountQueries.GetBankAccount(request.Id);
            CheckBrand(bankAccount.Bank.BrandId);

            _bankAccountCommands.Activate(request.Id, request.Remarks);
            return new ActivateBankAccountResponse
            {
                Success = true
            };
        }

        [HttpPost]
        [Route(AdminApiRoutes.DeactivateBankAccount)]
        public DeactivateBankAccountResponse Deactivate(DeactivateBankAccountRequest request)
        {
            VerifyPermission(Permissions.Deactivate, Modules.BankAccounts);

            var bankAccount = _bankAccountQueries.GetBankAccount(request.Id);
            CheckBrand(bankAccount.Bank.BrandId);

            _bankAccountCommands.Deactivate(request.Id, request.Remarks);
            return new DeactivateBankAccountResponse
            {
                Success = true
            };
        }

        [HttpGet]
        [Route(AdminApiRoutes.GetBankAccountsByPlayerId)]
        public GetBankAccountsByPlayerIdResponse GetBankAccountsByPlayerId(Guid playerId)
        {
            var bankAccount = _paymentQueries.GetBankAccountsForAdminOfflineDepositRequest(playerId);            

            return new GetBankAccountsByPlayerIdResponse
            {                
                BankAccounts = Mapper.Map<IEnumerable<BankAccountDto>>(bankAccount)
            };
        }       
    }
}