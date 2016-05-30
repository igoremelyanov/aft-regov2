using System;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using AFT.RegoV2.AdminApi.Interface.Payment;
using AFT.RegoV2.AdminWebsite.Common;
using AFT.RegoV2.AdminWebsite.Common.jqGrid;
using AFT.RegoV2.AdminWebsite.ViewModels;
using AFT.RegoV2.Bonus.Api.Interface;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Payment.Interface.ApplicationServices;
using AFT.RegoV2.Core.Player.ApplicationServices;
using AFT.RegoV2.Core.Security.Interfaces;
using Newtonsoft.Json;
using DepositMethod = AFT.RegoV2.Core.Payment.Interface.Data.DepositMethod;
using OfflineDeposit = AFT.RegoV2.Core.Payment.Interface.Data.OfflineDeposit;
using OfflineDepositStatus = AFT.RegoV2.Core.Common.Data.Payment.OfflineDepositStatus;
using TransferType = AFT.RegoV2.Core.Payment.Interface.Data.TransferType;
using UnverifyReasons = AFT.RegoV2.Core.Common.Data.UnverifyReasons;

namespace AFT.RegoV2.AdminWebsite.Controllers
{
    [Authorize]
    public class OfflineDepositController : BaseController
    {
        private readonly PlayerQueries _playerQueries;
        private readonly IPaymentQueries _paymentQueries;
        private readonly IAdminQueries _adminQueries;
        private readonly IBonusApiProxy _bonusApiProxy;

        public OfflineDepositController(
            PlayerQueries playerQueries,
            IPaymentQueries paymentQueries,
            IAdminQueries adminQueries,
            IBonusApiProxy bonusApiProxy)
        {
            _playerQueries = playerQueries;
            _paymentQueries = paymentQueries;

            _adminQueries = adminQueries;
            _bonusApiProxy = bonusApiProxy;
        }

        [SearchPackageFilter("searchPackage")]
        public ActionResult RequestedList(SearchPackage searchPackage)
        {
            var data = SearchOfflineDeposits(
               searchPackage,
               new[] { OfflineDepositStatus.New, OfflineDepositStatus.Unverified },
               obj =>
                   new object[]
                    {
                        obj.Brand.LicenseeName,
                        obj.Brand.Name,
                        obj.Player.Username,
                        obj.TransactionNumber,
                        obj.BankAccount.AccountName,
                        obj.BankAccount.AccountNumber,
                        obj.BankReferenceNumber,
                        obj.Amount.Format(),
                        obj.BankAccount.AccountId,
                        Enum.GetName(typeof(TransferType), obj.TransferType),
                        Enum.GetName(typeof(DepositMethod), obj.DepositMethod),
                        obj.CreatedBy,
                        obj.Created.ToString("yyyy/MM/dd HH:mm:ss zzz")
                    },
               _paymentQueries.GetOfflineDeposits());

            return new JsonResult { Data = data, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [SearchPackageFilter("searchPackage")]
        public ActionResult ApproveList(SearchPackage searchPackage)
        {
            var data = SearchOfflineDeposits(
                searchPackage,
                new[] { OfflineDepositStatus.Verified },
                obj =>
                    new[]
                    {
                        obj.Player.Username,
                        obj.TransactionNumber,
                        LabelHelper.LabelPaymentMethod(obj.PaymentMethod),
                        obj.CurrencyCode,
                        obj.Amount.ToString(CultureInfo.InvariantCulture),
                        obj.Status.ToString(),
                        obj.Brand.Name,
                        obj.Created.ToString("yyyy/MM/dd HH:mm:ss zzz"),
                        obj.VerifiedBy,
                        obj.DepositType.ToString(),
                        obj.BankAccount.AccountId,
                        obj.BankAccount.Bank.BankName,
                        obj.BankAccount.Province,
                        obj.BankAccount.Branch,
                        obj.BankAccount.AccountNumber,
                        obj.BankAccount.AccountName
                    },
                _paymentQueries.GetOfflineDeposits());
            return new JsonResult { Data = data, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [SearchPackageFilter("searchPackage")]
        public ActionResult VerifyList(SearchPackage searchPackage)
        {
            var data = SearchOfflineDeposits(
              searchPackage,
              new[] { OfflineDepositStatus.Processing },
              obj =>
                  new[] 
                  {
                      obj.Player.Username,
                      obj.Player.FirstName,
                      obj.Player.LastName,
                      obj.TransactionNumber,
                      LabelHelper.LabelPaymentMethod(obj.PaymentMethod),
                      obj.CurrencyCode,
                      obj.Amount.ToString(CultureInfo.InvariantCulture),
                      obj.Status.ToString(),
                      obj.Brand.Name,
                      obj.Created.ToString("yyyy/MM/dd HH:mm:ss zzz"),
                      obj.DepositType.ToString(),
                      obj.BankAccount.AccountId,
                      obj.BankAccount.Bank.BankName,
                      obj.BankAccount.Province,
                      obj.BankAccount.Branch,
                      obj.BankAccount.AccountNumber,
                      obj.BankAccount.AccountName
                  },
              _paymentQueries.GetOfflineDeposits());

            return new JsonResult { Data = data, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpGet]
        public ActionResult GetBankAccounts(Guid depositId)
        {
            var deposit = _paymentQueries.GetDepositById(depositId);
            var bankAccounts = _paymentQueries.GetBankAccountsForAdminOfflineDepositRequest(deposit.PlayerId);

            return this.Success(new
            {
                BankAccounts = bankAccounts.Select(o => new
                {
                    Id = o.Id,
                    AccountId = o.AccountId,
                    AccountName = o.AccountName,
                    AccountNumber = o.AccountNumber,
                    BankName = o.Bank.BankName,
                    Province = o.Province,
                    Branch = o.Branch
                })
            });
        }

        [HttpGet]
        public ActionResult Get(Guid id)
        {
            var offlineDeposit =GetAdminApiProxy(Request).GetOfflineDepositById(id).OfflineDeposit;
            return this.Success(new OfflineDepositViewModel(offlineDeposit));
        }

        [HttpGet]
        public ActionResult CalculateFeeForDeposit(Guid id, decimal actualAmount)
        {
            try
            {
                var fee = _paymentQueries.CalculateFeeForDeposit(id, actualAmount);
                return this.Success(new
                {
                    Fee = fee
                });
            }
            catch (InvalidOperationException exception)
            {
                return this.Failed(exception);
            }
        }

        [HttpGet]
        public async Task<ActionResult> GetForView(Guid id)
        {
            var response = GetAdminApiProxy(Request).GetOfflineDepositById(id);
            var offlineDeposit = response.OfflineDeposit;
            
            var viewModel = new OfflineDepositViewModel(offlineDeposit);
            if (offlineDeposit.BonusRedemptionId.HasValue)
            {
                var player = _playerQueries.GetPlayer(offlineDeposit.PlayerId);
                if(!_adminQueries.IsBrandAllowed(CurrentUser.Id, player.BrandId))
                    throw new HttpException(403, "Access forbidden");

                var redemption = await _bonusApiProxy.GetBonusRedemptionAsync(offlineDeposit.PlayerId, offlineDeposit.BonusRedemptionId.Value);
                viewModel.BonusName = redemption.Bonus.Name;
            }

            return this.Success(viewModel);
        }

        // TODO Edit permission perhaps?
        [HttpGet]
        public async Task<ActionResult> GetInfoForCreate(Guid playerId)
        {
            var player = _playerQueries.GetPlayer(playerId);
            if (player == null)
            {
                throw new ArgumentException(@"Player was not found", nameof(playerId));
            }
            if (!_adminQueries.IsBrandAllowed(CurrentUser.Id, player.BrandId))
                throw new HttpException(403, "Access forbidden");

            var ensureOrWaitUserRegisteredTask = _bonusApiProxy.EnsureOrWaitPlayerRegistered(playerId);
            var depositBonusesTask = _bonusApiProxy.GetDepositQualifiedBonusesByAdminAsync(playerId);
            var bankAccounts = GetAdminApiProxy(Request).GetBankAccountsByPlayerId(playerId).BankAccounts;

            var bankData = bankAccounts
                .Select(bankAccount => new
                    {
                        bankAccount.Id,
                        Name = $"{bankAccount.Bank.BankName} / {bankAccount.AccountName}"
                    });

            await ensureOrWaitUserRegisteredTask;
            return this.Success(new
            {
                Banks = bankData,
                player.Username,
                Bonuses = await depositBonusesTask
            });
        }

        [HttpPost]
        public ActionResult Create(CreateOfflineDepositRequest request)
        {
            var response = GetAdminApiProxy(Request).CreateOfflineDeposit(request);

            return response.Success
                ? this.Success(response.Id)
                : this.Failed(response.Errors);
        }

        [HttpGet]
        public ActionResult ViewRequestForConfirm(Guid id)
        {
            var offlineDeposit = GetAdminApiProxy(Request).GetOfflineDepositById(id).OfflineDeposit;

            return this.Success(new OfflineDepositViewModel(offlineDeposit));         
        }

        [HttpGet]
        public ActionResult Confirm(Guid id)
        {
            var offlineDeposit = GetAdminApiProxy(Request).GetOfflineDepositById(id).OfflineDeposit;
             
            return this.Success(new OfflineDepositViewModel(offlineDeposit));
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult ConfirmDeposit(FormCollection form)
        {
            var depositConfirm = form["depositConfirm"];

            var confirm = JsonConvert.DeserializeObject<ConfirmOfflineDepositRequest>(depositConfirm);
            var uploadId1 = Request.Files["uploadId1"];
            var uploadId2 = Request.Files["uploadId2"];
            var receiptUpLoad = Request.Files["receiptUpLoad"];
            confirm.IdFrontImage = uploadId1.GetFileName();
            confirm.IdBackImage = uploadId2.GetFileName();
            confirm.ReceiptImage = receiptUpLoad.GetFileName();
         
            return Confirm(confirm,uploadId1,uploadId2,receiptUpLoad);
        }

        private ActionResult Confirm(ConfirmOfflineDepositRequest depositConfirm, HttpPostedFileBase idFrontImage,
            HttpPostedFileBase idBackImage, HttpPostedFileBase receiptImage)
        {
            if (ModelState.IsValid == false)
            {
                return this.Failed();
            }

            depositConfirm.CurrentUser = CurrentUser.UserName;
            depositConfirm.IdFrontImageFile = idFrontImage.GetBytes();
            depositConfirm.IdBackImageFile = idBackImage.GetBytes();
            depositConfirm.ReceiptImageFile = receiptImage.GetBytes();

            var response = GetAdminApiProxy(Request).ConfirmOfflineDeposit(depositConfirm);
            return response.Success ?
                this.Success(
                    new
                    {
                        Message = "app:payment.deposit.successfullyConfirmed",
                        idFrontImage = response.IdFrontImageId,
                        idBackImage = response.IdBackImageId,
                        receiptImage = response.ReceiptImageId,
                        response.PlayerId
                    })
                : this.Failed(response.Errors);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Verify(Guid id, string remark, Guid bankAccountId)
        {
            var response = GetAdminApiProxy(Request).VerifyOfflineDeposit(new VerifyOfflineDepositRequest
            {
                Id = id,
                BankAccountId = bankAccountId,
                Remarks = remark
            });

            return response.Success
                ? this.Success("app:payment.deposit.successfullyVerified")
                : this.Failed(response.Errors);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Approve(ApproveOfflineDepositRequest approveCommand)
        {
            if (ModelState.IsValid == false)
            {
                return this.Failed();
            }

            var response = GetAdminApiProxy(Request).ApproveOfflineDeposit(approveCommand);

            return response.Success
                ? this.Success("app:payment.deposit.successfullyApproved")
                : this.Failed(response.Errors);
        }

        [HttpPost]
        public ActionResult Reject(Guid id, string remark)
        {
            var response = GetAdminApiProxy(Request).RejectOfflineDeposit(new RejectOfflineDepositRequest
            {
                Id=id,
                Remarks = remark
            });

            return response.Success
                ? this.Success("app:payment.deposit.rejected")
                : this.Failed(response.Errors);
        }

        [HttpPost]
        public ActionResult Unverify(Guid id, string remark, UnverifyReasons unverifyReason)
        {
            var response = GetAdminApiProxy(Request).UnverifyOfflineDeposit(new UnverifyOfflineDepositRequest
            {
                Id = id,
                Remarks = remark,
                UnverifyReason = unverifyReason
            });

            return response.Success
               ? this.Success("app:payment.deposit.successfullyUnverified")
               : this.Failed(response.Errors);
        }

        private SearchPackageResult SearchOfflineDeposits(SearchPackage searchPackage, OfflineDepositStatus[] offlineDepositStatuses,
            Expression<Func<OfflineDeposit, object>> cellExpression, IQueryable<OfflineDeposit> queryable)
        {
            var brandFilterSelections = _adminQueries.GetBrandFilterSelections();

            var query = queryable
                .Where(p => offlineDepositStatuses.Contains(p.Status) && brandFilterSelections.Contains(p.BrandId));

            var dataBuilder = new SearchPackageDataBuilder<OfflineDeposit>(searchPackage, query);

            dataBuilder.SetFilterRule(x => x.BrandId, value => d => d.BrandId == new Guid(value))
                .Map(obj => obj.Id, cellExpression);

            var data = dataBuilder.GetPageData(obj => obj.Created);

            return data;
        }
    }
}