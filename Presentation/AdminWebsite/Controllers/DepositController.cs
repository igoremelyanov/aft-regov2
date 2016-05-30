using System;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using AFT.RegoV2.AdminWebsite.Common;
using AFT.RegoV2.AdminWebsite.Common.jqGrid;
using AFT.RegoV2.Core.Payment.Interface.ApplicationServices;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Payment.Interface.Data;
using AFT.RegoV2.Core.Security.Interfaces;



namespace AFT.RegoV2.AdminWebsite.Controllers
{
    [Authorize]
    public class DepositController : BaseController
    {        
        private readonly IPaymentQueries _paymentQueries;        
        private readonly IAdminQueries _adminQueries;

        public DepositController(IPaymentQueries paymentQueries,            
            IAdminQueries adminQueries)
        {
            _paymentQueries = paymentQueries;     
            _adminQueries = adminQueries;
        }
      
        [SearchPackageFilter("searchPackage")]
        public ActionResult VerifyList(SearchPackage searchPackage)
        {
            var data = SearchDeposits(
              searchPackage,
              "Processing",
              obj =>
                  new[] 
                  {
                      obj.Licensee,
                      obj.BrandName,
                      obj.Username,
                      obj.PlayerName,
                      obj.ReferenceCode,
                      obj.PaymentMethod,
                      obj.CurrencyCode,
                      obj.Amount.Format(),
                      obj.UniqueDepositAmount.Format(),
                      obj.Status,
                      obj.DateSubmitted.ToString("yyyy/MM/dd HH:mm:ss zzz"),
                      obj.DepositType.ToString(),
                      obj.BankAccountId,
                      obj.BankName,
                      obj.BankProvince,
                      obj.BankBranch,
                      obj.BankAccountNumber,
                      obj.BankAccountName
                  },
              _paymentQueries.GetDeposits());

            return new JsonResult { Data = data, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [SearchPackageFilter("searchPackage")]
        public ActionResult ApproveList(SearchPackage searchPackage)
        {
            var data = SearchDeposits(
              searchPackage,
              "Verified",
              obj =>
                  new[] 
                  {
                      obj.Licensee,
                      obj.BrandName,
                      obj.Username,                      
                      obj.ReferenceCode,
                      obj.PaymentMethod,
                      obj.CurrencyCode,
                      obj.Amount.ToString("0.00"),
                      obj.UniqueDepositAmount.ToString("0.00"),
                      obj.Status,
                      obj.DateVerified.HasValue ? obj.DateVerified.Value.ToString("yyyy/MM/dd HH:mm:ss zzz") : "",
                      obj.VerifiedBy,
                      obj.DepositType.ToString(),
                      obj.BankAccountId,
                      obj.BankName,
                      obj.BankProvince,
                      obj.BankBranch,
                      obj.BankAccountNumber,
                      obj.BankAccountName
                  },
              _paymentQueries.GetDeposits());

            return new JsonResult { Data = data, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        private SearchPackageResult SearchDeposits(SearchPackage searchPackage, string status,
            Expression<Func<DepositDto, object>> cellExpression, IQueryable<DepositDto> queryable)
        {
            var brandFilterSelections = _adminQueries.GetBrandFilterSelections();

            var query = queryable.AsQueryable()
                .Where(p => p.Status == status && brandFilterSelections.Contains(p.BrandId));                

            var dataBuilder = new SearchPackageDataBuilder<DepositDto>(searchPackage, query);

            dataBuilder.SetFilterRule(x => x.BrandId, value => d => d.BrandId == new Guid(value))
                .Map(obj => obj.Id, cellExpression);

            var data = dataBuilder.GetPageData(obj => obj.DateSubmitted);

            return data;
        }
    }
}