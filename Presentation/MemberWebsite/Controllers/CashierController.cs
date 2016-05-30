using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.UI.WebControls;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.MemberApi.Interface.Payment;
using AFT.RegoV2.MemberApi.Interface.Proxy;
using AFT.RegoV2.MemberWebsite.Common;

namespace AFT.RegoV2.MemberWebsite.Controllers
{
    public class CashierController : ControllerBase
    {
        [Authorize]
        public ActionResult Home()
        {
            return View("Cashier");
        }

        [Authorize]
        public async Task<ActionResult> Withdrawal()
        {
            var playerProfile = await GetMemberApiProxy(Request).ProfileAsync();
            if (playerProfile.IsFrozen)
                return RedirectToActionLocalized("FrozenAccount");

            var withdrawalData = await GetMemberApiProxy(Request).GetWithdrawalFormDataAsync(new AppSettings().BrandId);

            if (withdrawalData.BankAccount == null)
                return RedirectToActionLocalized("CreateBankAccount");
            if (withdrawalData.BankAccount.Status == BankAccountStatus.Pending)
                return View("PlayerBankAcountPending");
            if (withdrawalData.BankAccount.Status == BankAccountStatus.Rejected)
                return RedirectToActionLocalized("RejectBankAccount");

            return View(withdrawalData);
        }

        [Authorize]
        public ActionResult FrozenAccount()
        {
            return View();
        }

        [Authorize]
        public ActionResult WithdrawalSuccess()
        {
            return View();
        }

        [Authorize]
        public ActionResult CreateBankAccount()
        {
            var model = new CreateBankAccountModel()
            {
                IsRejected = false
            };

            return View(model);
        }

        [Authorize]
        public async Task<ActionResult> RejectBankAccount()
        {
            var withdrawalData = await GetMemberApiProxy(Request).GetWithdrawalFormDataAsync(new AppSettings().BrandId);
            var model = new CreateBankAccountModel()
            {
                IsRejected = true,
                Remark = withdrawalData.BankAccount.Remark
            };

            return View("CreateBankAccount", model);
        }

        private MemberApiProxy GetMemberApiProxy(HttpRequestBase request)
        {
            var appSettings = new AppSettings();
            return new MemberApiProxy(appSettings.MemberApiUrl.ToString(), request.AccessToken());
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            if (filterContext.Exception is MemberApiProxyException)
            {
                var mapException = filterContext.Exception as MemberApiProxyException;

                if (mapException.StatusCode == HttpStatusCode.Unauthorized)
                {
                    filterContext.ExceptionHandled = true;
                    filterContext.Result = RedirectToActionLocalized("PlayerProfile");

                    FormsAuthentication.SignOut();
                }
            }
        }

        [Authorize]
        public ActionResult BalanceDetails()
        {
            return View();
        }

        [Authorize]
        public ActionResult TransactionHistory()
        {
            return View();
        }

        [Authorize]
        public ActionResult OnlineDepositConfirmation()
        {
            return View();
        }

        [Authorize]
        public ActionResult OnlineDepositOn()
        {
            return View();
        }

        [Authorize]
        public ActionResult OnlineDepositOff()
        {
            return View();
        }

        [Authorize]
        public ActionResult PendingDepositConfirmation()
        {
            return View();
        }

        [Authorize]
        public ActionResult PendingDeposit()
        {
            return View();
        }

    }
}