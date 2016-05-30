using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Payment.ApplicationServices
{
    public class CurrencyManagerQueries : IApplicationService
    {
        private readonly BrandQueries _brandQueries;

        public CurrencyManagerQueries(BrandQueries brandQueries)
        {
            _brandQueries = brandQueries;
        }

        public IEnumerable<string> GetCurrencyCodes(Guid brandId)
        {
            var brand = _brandQueries.GetBrandOrNull(brandId);
            var currencyCodes = brand.BrandCurrencies.Select(c => c.CurrencyCode);

            return currencyCodes;
        }
    }
}
