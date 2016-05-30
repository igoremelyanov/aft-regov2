using System;
using System.Linq;
using System.Web.Mvc;
using AFT.RegoV2.AdminApi.Interface.Payment;
using AFT.RegoV2.AdminWebsite.Common;
using AFT.RegoV2.AdminWebsite.Common.jqGrid;
using AFT.RegoV2.AdminWebsite.ViewModels;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Payment.Interface.ApplicationServices;
using AFT.RegoV2.Core.Payment.Interface.Data.Commands;
using AFT.RegoV2.Core.Security.Interfaces;
using AutoMapper;
using Bank = AFT.RegoV2.Core.Payment.Interface.Data.Bank;

namespace AFT.RegoV2.AdminWebsite.Controllers
{
    public class BanksController : BaseController
    {
        private readonly IPaymentQueries _paymentQueries;
        private readonly IAdminQueries _adminQueries;

        static BanksController()
        {
            Mapper.CreateMap<AddBankModel, AddBankData>();
            Mapper.CreateMap<EditBankModel, EditBankData>();
        }

        public BanksController(
            IPaymentQueries paymentQueries,
            IAdminQueries adminQueries)
        {
            _paymentQueries = paymentQueries;
            _adminQueries = adminQueries;
        }

        [HttpGet]
        public ActionResult List()
        {
            return Json(_paymentQueries.GetBankAccounts(), JsonRequestBehavior.AllowGet);
        }

        [SearchPackageFilter("searchPackage")]
        public object GetBanks(SearchPackage searchPackage)
        {
            var brandFilterSelecttions = _adminQueries.GetBrandFilterSelections();
            
            var banks = _paymentQueries.GetBanks()
                .Where(x => brandFilterSelecttions.Contains(x.BrandId));

            var dataBuilder = new SearchPackageDataBuilder<Bank>(searchPackage, banks);

            dataBuilder
                .SetFilterRule(obj => obj.Brand, (value) => x => x.Brand.Id == new Guid(value))
                .Map(obj => obj.Id,
                    obj =>
                        new[]
                        {
                            obj.BankId,
                            obj.BankName,
                            obj.Country.Name,
                            obj.Brand.Name,
                            obj.Brand.LicenseeName,
                            obj.Remarks,
                            obj.Created.GetNormalizedDateTime(true),
                            obj.CreatedBy,
                            obj.Updated.GetNormalizedDateTime(true),
                            obj.UpdatedBy
                        }
                );

            var data = dataBuilder.GetPageData(obj => obj.BankName);
            
            return new JsonResult
            {
                Data = data,
                MaxJsonLength = int.MaxValue,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        [HttpPost]
        public ActionResult Add(AddBankRequest model)
        {
            var response = GetAdminApiProxy(Request).AddBank(model);
            return response.Success ? this.Success(response.Id) : this.Failed(response.Errors);
        }

        [HttpPost]
        public ActionResult Edit(EditBankRequest model)
        {
            var response = GetAdminApiProxy(Request).EditBank(model);
            return response.Success ? this.Success() : this.Failed(response.Errors);
        }

        [HttpGet]
        public ActionResult View(Guid id)
        {
            var response = GetAdminApiProxy(Request).GetBankById(id);
            return this.Success(response.Bank);
        }
    }
}