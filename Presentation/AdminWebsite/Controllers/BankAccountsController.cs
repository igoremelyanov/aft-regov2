using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AFT.RegoV2.AdminApi.Interface.Payment;
using AFT.RegoV2.AdminWebsite.Common;
using AFT.RegoV2.AdminWebsite.Common.jqGrid;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Payment.Interface.ApplicationServices;
using AFT.RegoV2.Core.Payment.Interface.Data;
using AFT.RegoV2.Core.Security.Interfaces;

using Newtonsoft.Json;

namespace AFT.RegoV2.AdminWebsite.Controllers
{
    public class BankAccountsController : BaseController
    {
        private readonly IBankAccountQueries _bankAccountQueries;
        private readonly IAdminQueries _adminQueries;

        private const string DefaultImagePreviewSrc = "data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHdpZHRoPSIzMDAiIGhlaWdodD0iMjAwIj48cmVjdCB3aWR0aD0iMzAwIiBoZWlnaHQ9IjIwMCIgZmlsbD0iI2VlZSIvPjx0ZXh0IHRleHQtYW5jaG9yPSJtaWRkbGUiIHg9IjE1MCIgeT0iMTAwIiBzdHlsZT0iZmlsbDojYWFhO2ZvbnQtd2VpZ2h0OmJvbGQ7Zm9udC1zaXplOjE5cHg7Zm9udC1mYW1pbHk6QXJpYWwsSGVsdmV0aWNhLHNhbnMtc2VyaWY7ZG9taW5hbnQtYmFzZWxpbmU6Y2VudHJhbCI+MzAweDIwMDwvdGV4dD48L3N2Zz4=";

        public BankAccountsController(
            IBankAccountQueries bankAccountQueries,
            IAdminQueries adminQueries)
        {
            _bankAccountQueries = bankAccountQueries;
            _adminQueries = adminQueries;
        }
      
        [SearchPackageFilter("searchPackage")]
        public object BankAccounts(SearchPackage searchPackage, string currencyCode)
        {
            var brandFilterSelections = _adminQueries.GetBrandFilterSelections();
            
            var bankAccounts = _bankAccountQueries.GetFilteredBankAccounts(CurrentUser.Id)
                .Where(x => brandFilterSelections.Contains(x.Bank.BrandId));

            if (!string.IsNullOrEmpty(currencyCode))
                bankAccounts = bankAccounts.Where(x => x.CurrencyCode == currencyCode);

            var dataBuilder = new SearchPackageDataBuilder<BankAccount>(searchPackage, bankAccounts);

            dataBuilder.SetFilterRule(x => x.Bank.Brand, value => p => p.Bank.BrandId == new Guid(value))
                .Map(obj => obj.Id, obj =>
                    new[]
                    {
                        obj.CurrencyCode,
                        obj.AccountId,
                        obj.AccountName,
                        obj.AccountNumber,
                        obj.AccountType.Name,
                        obj.Bank.BankName,
                        obj.Province,
                        obj.Branch,
                        obj.Status.ToString(),
                        obj.CreatedBy,
                        obj.Created.ToString("yyyy/MM/dd HH:mm:ss zzz"),
                        obj.UpdatedBy,
                        obj.Updated.HasValue ? obj.Updated.Value.ToString("yyyy/MM/dd HH:mm:ss zzz") : "",
                        obj.PaymentLevels.Any().ToString()
                    }
                );

            var data = dataBuilder.GetPageData(obj => obj.AccountName);

            return new JsonResult
            {
                Data = data, 
                MaxJsonLength = int.MaxValue, 
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        [HttpGet]
        public string View(Guid id)
        {
            var response = GetAdminApiProxy(Request).GetBankAccountById(id);
            var bankAccount = response.BankAccount;
            return SerializeJson(new
            {
                licenseeName = bankAccount.Bank.LicenseeName,
                brandName = bankAccount.Bank.BrandName,
                currencyCode = bankAccount.CurrencyCode,
                bankAccountId = bankAccount.AccountId,
                bankName = bankAccount.Bank.BankName,
                bankAccountNumber = bankAccount.AccountNumber,
                bankAccountProvince = bankAccount.Province,
                bankAccountBranch = bankAccount.Branch,
                bankAccountAccountName = bankAccount.AccountName,
                
                bankAccountAccountType = bankAccount.AccountType,
                bankAccountAccountTypeName = bankAccount.AccountType.Name,

                supplierName = bankAccount.SupplierName,
                contactNumber = bankAccount.ContactNumber,
                usbCode = bankAccount.USBCode,

                purchasedDate = bankAccount.PurchasedDate.HasValue ? bankAccount.PurchasedDate.Value.ToString("yyyy/MM/dd") : "",
                utilizationDate = bankAccount.UtilizationDate.HasValue ? bankAccount.UtilizationDate.Value.ToString("yyyy/MM/dd") : "",
                expirationDate = bankAccount.ExpirationDate.HasValue ? bankAccount.ExpirationDate.Value.ToString("yyyy/MM/dd") : "",

                uploadId1Src = bankAccount.IdFrontImage.HasValue ?
                    "image/Show?fileId=" + bankAccount.IdFrontImage.Value + "&playerId=" + bankAccount.Id :
                    DefaultImagePreviewSrc,

                uploadId2Src = bankAccount.IdBackImage.HasValue ?
                     "image/Show?fileId=" + bankAccount.IdBackImage.Value + "&playerId=" + bankAccount.Id :
                     DefaultImagePreviewSrc,

                uploadId3Src = bankAccount.ATMCardImage.HasValue ?
                     "image/Show?fileId=" + bankAccount.ATMCardImage.Value + "&playerId=" + bankAccount.Id :
                     DefaultImagePreviewSrc,

                remarks = bankAccount.Remarks
            });
        }

        [HttpPost]
        public ActionResult Add(FormCollection form)
        {
            var bankAccount = form["bankAccount"];
            var accountData = JsonConvert.DeserializeObject<AddBankAccountRequest>(bankAccount);
            var uploadId1 = base.Request.Files["uploadId1"];
            var uploadId2 = base.Request.Files["uploadId2"];
            var uploadId3 = base.Request.Files["uploadId3"];

            accountData.IdFrontImage = uploadId1.GetFileName();
            accountData.IdBackImage = uploadId2.GetFileName();
            accountData.AtmCardImage = uploadId3.GetFileName();
            
            return AddWithFiles(accountData, uploadId1, uploadId2, uploadId3);
        }

        private ActionResult AddWithFiles(AddBankAccountRequest bankAccountData, HttpPostedFileBase idFrontImage, HttpPostedFileBase idBackImage, HttpPostedFileBase atmCardImage)
        {
            if (ModelState.IsValid == false)
            {
                return this.Failed();
            }
           
            bankAccountData.IdFrontImageFile = idFrontImage.GetBytes();
            bankAccountData.IdBackImageFile = idBackImage.GetBytes();
            bankAccountData.AtmCardImageFile = atmCardImage.GetBytes();
            var response = GetAdminApiProxy(Request).AddBankAccount(bankAccountData);

            return response.Success?
                this.Success(
                    new
                    {
                        Message = "added",
                        Id = response.Id
                    })
                    : this.Failed(response.Errors);
        }

        [HttpGet]
        public string Edit(Guid id)
        {
            var response = GetAdminApiProxy(Request).GetBankAccountById(id);
            var bankAccount = response.BankAccount;

            return SerializeJson(new
            {
                id = bankAccount.Id,
                isActive = bankAccount.Status == BankAccountStatus.Active,
                licenseeId = bankAccount.Bank.LicenseeId,
                licenseeName = bankAccount.Bank.LicenseeName,
                brandId = bankAccount.Bank.BrandId,
                brandName = bankAccount.Bank.BrandName,
                bankAccountId = bankAccount.AccountId,
                bankId = bankAccount.Bank.Id,
                bankName = bankAccount.Bank.BankName,
                bankAccountNumber = bankAccount.AccountNumber,
                bankAccountName = bankAccount.AccountName,
                bankAccountProvince = bankAccount.Province,
                bankAccountBranch = bankAccount.Branch,
                bankAccountAccountTypeId = bankAccount.AccountType.Id,
                currencyCode = bankAccount.CurrencyCode,
                supplierName = bankAccount.SupplierName,
                contactNumber = bankAccount.ContactNumber,
                usbCode = bankAccount.USBCode,

                //purchasedDate = bankAccount.PurchasedDate.HasValue ? bankAccount.PurchasedDate.Value.ToShortDateString() : "",
                purchasedDate = bankAccount.PurchasedDate.HasValue ? bankAccount.PurchasedDate.Value.ToString("yyyy/MM/dd") : "",
                utilizationDate = bankAccount.UtilizationDate.HasValue ? bankAccount.UtilizationDate.Value.ToString("yyyy/MM/dd") : "",
                expirationDate = bankAccount.ExpirationDate.HasValue ? bankAccount.ExpirationDate.Value.ToString("yyyy/MM/dd") : "",

                idFrontImage = bankAccount.IdFrontImage,
                idBackImage = bankAccount.IdBackImage,
                atmCardImage = bankAccount.ATMCardImage,

                remarks = bankAccount.Remarks
            });
        }

        [HttpPost]
        public ActionResult SaveChanges(FormCollection form)
        {
            var bankAccount = form["bankAccount"];

            var accountData = JsonConvert.DeserializeObject<EditBankAccountRequest>(bankAccount);
            var uploadId1 = base.Request.Files["uploadId1"];
            var uploadId2 = base.Request.Files["uploadId2"];
            var uploadId3 = base.Request.Files["uploadId3"];

            accountData.IdFrontImage = uploadId1.GetFileName();
            accountData.IdBackImage = uploadId2.GetFileName();
            accountData.AtmCardImage = uploadId3.GetFileName();
            
            return SaveWithFiles(accountData, uploadId1, uploadId2, uploadId3);
        }

        private ActionResult SaveWithFiles(EditBankAccountRequest bankAccountData, HttpPostedFileBase idFrontImage, HttpPostedFileBase idBackImage, HttpPostedFileBase atmCardImage)
        {
            if (ModelState.IsValid == false)
            {
                return this.Failed();
            }
            bankAccountData.IdFrontImageFile = idFrontImage.GetBytes();
            bankAccountData.IdBackImageFile = idBackImage.GetBytes();
            bankAccountData.AtmCardImageFile = atmCardImage.GetBytes();

            var response = GetAdminApiProxy(Request).EditBankAccount(bankAccountData);

            var bankAccount = bankAccountData;
            return response.Success?
                this.Success(new { Message = "updated", bankAccount.IdFrontImage, bankAccount.IdBackImage, bankAccount.AtmCardImage, bankAccountData.Id })
                : this.Failed(response.Errors);
        }

        [HttpPost]
        public ActionResult Activate(ActivateBankAccountRequest request)
        {
            var response = GetAdminApiProxy(Request).ActivateBankAccount(request);
            return response.Success ? this.Success(
                new
                {
                    messageKey = "app:bankAccounts.bankAccountActivated"
                }) : this.Failed(response.Errors);
        }

        [HttpPost]
        public ActionResult Deactivate(DeactivateBankAccountRequest request)
        {
            var response = GetAdminApiProxy(Request).DeactivateBankAccount(request);
            return response.Success ? this.Success(new
            {
                messageKey = "app:bankAccounts.bankAccountDeactivated"
            }) : this.Failed(response.Errors);
        }

        [HttpGet]
        public string GetCurrencies(Guid brandId)
        {
            var currencies = _bankAccountQueries.GetCurrencies(brandId);
            return SerializeJson(new
            {
                Currencies = currencies
            });
        }

        [HttpGet]
        public string GetBanks(Guid brandId)
        {
            var response = GetAdminApiProxy(Request).GetBankListByBrandId(brandId);
            var banks = response.Banks.Select(x =>
                new
                {
                    Id = x.Id,
                    Name = x.BankName
                });
            return SerializeJson(new
            {
                Banks = banks
            });
        }

        [HttpGet]
        public string AccountCurrencies()
        {
            var response = GetAdminApiProxy(Request).GetBankAccountCurrencyByUserId(CurrentUser.Id);

            return SerializeJson(new
            {
                currencies = response.Currency
            });
        }

        [HttpGet]
        public string GetBankAccountTypes()
        {
            var response = GetAdminApiProxy(Request).GetBankAccountTypes();

            return SerializeJson(new
            {
                BankAccountTypes = response.BankAccountTypes
            });
        }
    }
}