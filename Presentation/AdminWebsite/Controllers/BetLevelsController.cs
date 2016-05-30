using System;
using System.Linq;
using System.Web.Mvc;
using AFT.RegoV2.AdminWebsite.Common;
using AFT.RegoV2.AdminWebsite.Common.jqGrid;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Core.Security.Interfaces;

namespace AFT.RegoV2.AdminWebsite.Controllers
{
    public class BetLevelsController : BaseController
    {
        private readonly BrandQueries _brandQueries;
        private readonly IGameQueries _gameQueries;
        private readonly IAdminQueries _adminQueries;

        public BetLevelsController(
            BrandQueries brandQueries, 
            IGameQueries gameQueries,
            IAdminQueries adminQueries)
        {
            _brandQueries = brandQueries;
            _gameQueries = gameQueries;
            _adminQueries = adminQueries;
        }

        [SearchPackageFilter("searchPackage")]
        public ActionResult List(SearchPackage searchPackage)
        {
            return new JsonResult
            {
                Data = SearchData(searchPackage),
                MaxJsonLength = int.MaxValue,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        protected object SearchData(SearchPackage searchPackage)
        {
            var brandFilterSelections = _adminQueries.GetBrandFilterSelections();
            var products = _gameQueries.GetBrandProducts().Where(x => brandFilterSelections.Contains(x.BrandId));
            var dataBuilder = new SearchPackageDataBuilder<BrandProductData>(searchPackage, products.AsQueryable());

            dataBuilder
                .Map(record => record.BrandId + "," + record.GameProviderId + ','+record.LicenseeId,
                    record =>
                        new object[]
                        {
                            record.BrandName,
                            record.GameProviderName,
                            record.CreatedBy,
                            Format.FormatDate(record.DateCreated, false),
                            record.UpdatedBy,
                            Format.FormatDate(record.DateUpdated, false),
                        }
                );
            return dataBuilder.GetPageData(record => record.BrandName);
        }

        public string Brands(Guid? licenseeId)
        {
            if (!licenseeId.HasValue)
                return null;

            var brands = _brandQueries
                .GetBrandsByLicensee(licenseeId.Value)
                .Select(x => new { x.Id, x.Name });

            return SerializeJson(brands);
        }

        public string AssignedProducts(Guid? brandId)
        {
            if (!brandId.HasValue)
                return null;

            var brand = _brandQueries.GetBrandOrNull(brandId.Value);
            var assignedProductsIds = brand.Products.Select(x => x.ProductId).ToList();
            return SerializeJson(_gameQueries.GetAssignedProductsData(assignedProductsIds));
        }
    }
}