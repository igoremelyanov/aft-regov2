using System;
using System.Linq;
using System.Web.Mvc;
using AFT.RegoV2.AdminApi.Interface.Payment;
using AFT.RegoV2.AdminWebsite.Common;
using AFT.RegoV2.AdminWebsite.Common.jqGrid;
using AFT.RegoV2.Core.Payment.Interface.ApplicationServices;
using AFT.RegoV2.Core.Security.Interfaces;
using BankAccountStatus = AFT.RegoV2.Core.Common.Data.BankAccountStatus;
using PlayerBankAccount = AFT.RegoV2.Core.Payment.Interface.Data.PlayerBankAccount;
namespace AFT.RegoV2.AdminWebsite.Controllers
{
    public class PlayerBankAccountController : BaseController
    {        
        private readonly IPlayerBankAccountQueries _queries;
        private readonly IAdminQueries _adminQueries;

        public PlayerBankAccountController(IPlayerBankAccountQueries queries, IAdminQueries adminQueries)
        {
            _queries = queries;
            _adminQueries = adminQueries;
        }

        [HttpGet]
        [SearchPackageFilter("searchPackage")]
        public object PlayerList(SearchPackage searchPackage)
        {
            var playerBankAccounts = _queries.GetPlayerBankAccounts();

            var dataBuilder = new SearchPackageDataBuilder<PlayerBankAccount>(searchPackage, playerBankAccounts);

            dataBuilder.SetFilterRule(x => x.Player.Id, (value) => p => p.Player.Id == new Guid(value))
                .Map(obj => obj.Id,
                    obj => new[]
                    {
                        obj.AccountName,
                        obj.AccountNumber,
                        obj.Bank.BankName,
                        obj.Province,
                        obj.City,
                        obj.Branch,
                        obj.SwiftCode,
                        obj.Address,
                        obj.IsCurrent ? "Yes" : "No",
                        Enum.GetName(typeof(BankAccountStatus), obj.Status),
                        obj.CreatedBy,
                        Format.FormatDate(obj.Created, false),
                        obj.UpdatedBy,
                        Format.FormatDate(obj.Updated, false),
                        obj.VerifiedBy,
                        Format.FormatDate(obj.Verified, false),
                        obj.RejectedBy,
                        Format.FormatDate(obj.Rejected, false)
                    }
                );

            var data = dataBuilder.GetPageData(obj => obj.AccountName);

            var result = new JsonResult
            {
                Data = data,
                MaxJsonLength = int.MaxValue,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

            return result;
        }

        [HttpGet]
        [SearchPackageFilter("searchPackage")]
        public object PendingList(SearchPackage searchPackage)
        {
            var brandFilterSelections = _adminQueries.GetBrandFilterSelections();
            var pendingPlayerBankAccounts = _queries.GetPendingPlayerBankAccounts()
                .Where(x => brandFilterSelections.Contains(x.Player.BrandId));

            var dataBuilder = new SearchPackageDataBuilder<PlayerBankAccount>(searchPackage, pendingPlayerBankAccounts);

            dataBuilder.SetFilterRule(x => x.Player.BrandId, (value) => y => y.Player.BrandId == new Guid(value))
                .Map(obj => obj.Id,
                    obj => new[]
                    {
                        obj.AccountName,
                        obj.AccountNumber,
                        obj.Bank.BankName,
                        obj.Province,
                        obj.City,
                        obj.Branch,
                        obj.SwiftCode,
                        obj.Address,
                        obj.IsCurrent ? "Yes" : "No",
                        Enum.GetName(typeof(BankAccountStatus), obj.Status),
                        obj.CreatedBy,
                        Format.FormatDate(obj.Created, false),
                        obj.UpdatedBy,
                        Format.FormatDate(obj.Updated, false)
                    }
                );

            var data = dataBuilder.GetPageData(obj => obj.AccountName);

            var result = new JsonResult
            {
                Data = data,
                MaxJsonLength = int.MaxValue,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

            return result;
        }

        [HttpPost]
        public ActionResult Verify(VerifyPlayerBankAccountRequest request)
        {
            var response = GetAdminApiProxy(Request).VerifyPlayerBankAccount(request);

            return response.Success ? this.Success() : this.Failed(response.Errors);
        }

        [HttpPost]
        public ActionResult Reject(RejectPlayerBankAccountRequest request)
        {
            var response = GetAdminApiProxy(Request).RejectPlayerBankAccount(request);

            return response.Success ? this.Success() : this.Failed(response.Errors);
        }
    }
}