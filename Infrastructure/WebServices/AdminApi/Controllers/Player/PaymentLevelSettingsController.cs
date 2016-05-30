using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using AFT.RegoV2.AdminApi.Controllers.Base;
using AFT.RegoV2.AdminApi.Interface.Common;
using AFT.RegoV2.AdminWebsite.Common.jqGrid;
using AFT.RegoV2.Core.Auth.Interface.ApplicationServices;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Payment.Interface.ApplicationServices;
using AFT.RegoV2.Core.Payment.Interface.Data;
using AFT.RegoV2.Core.Payment.Interface.Data.Commands;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Core.Security.Interfaces;


namespace AFT.RegoV2.AdminApi.Controllers.Player
{
    [Authorize]
    public class PaymentLevelSettingsController : BaseApiController
    {
        private readonly IPaymentQueries _paymentQueries;
        private readonly IAdminQueries _adminQueries;

        public PaymentLevelSettingsController(
            IPaymentQueries paymentQueries,
            IAuthQueries authQueries,
            IAdminQueries adminQueries)
            : base(authQueries, adminQueries)
        {
            _paymentQueries = paymentQueries;
            _adminQueries = adminQueries;
        }

        [HttpGet]
        [Filters.SearchPackageFilter("searchPackage")]
        [Route(AdminApiRoutes.ListPlayersInPaymentLevelSettings)]
        public IHttpActionResult List([FromUri] SearchPackage searchPackage)
        {
            VerifyPermission(Permissions.View, Modules.PaymentLevelSettings);
            return Ok(SearchData(searchPackage));
        }

        #region Methods
        protected object SearchData(SearchPackage searchPackage)
        {
            var brandFilterSelections = _adminQueries.GetBrandFilterSelections();

            var queryFilter = HandleSearchPackage(searchPackage);

            IQueryable<PlayerPaymentAmount> query = null;

            if (queryFilter.IsQueryDeposit)
                query = _paymentQueries.GetPlayerDepositAmount(queryFilter).Where(p => brandFilterSelections.Contains(p.BrandId));
            else
                query = _paymentQueries.GetPlayerWithdrawalAmount(queryFilter).Where(p => brandFilterSelections.Contains(p.BrandId));

            var dataBuilder = new SearchPackageDataBuilder<PlayerPaymentAmount>(searchPackage, query);

            dataBuilder
                .SetFilterRule(x => x.BrandId, (value) => p => p.BrandId == new Guid(value))
                .Map(player => player.PlayerId,
                    player => new[]
                    {
                        player.LicenseeName,
                        player.BrandName,
                        player.Username,
                        player.FullName,
                        player.PaymentLevelName,
                        player.Amount.Format(),
                        player.PaymentLevelId.ToString(),
                        player.BrandId.ToString(),
                        player.Currency
                    }
                );
            return dataBuilder.GetPageData(player => player.Username);
        }

        private GetPlayerPaymentAmountRequest HandleSearchPackage(SearchPackage searchPackage)
        {
            var queryFilter = new GetPlayerPaymentAmountRequest();
            
            foreach (var rule in searchPackage.AdvancedFilter.Rules)
            {
                switch (rule.Field)
                {
                    case "TransactionType":
                        ParseTransactionTypeRule(searchPackage, rule, queryFilter);
                        break;
                    case "ApproveDate":
                        ParseApproveDateRule(searchPackage, rule, queryFilter);
                        break;
                    case "PaymentMethod":
                        ParsePaymentMethodRule(searchPackage, rule, queryFilter);
                        break;
                    case "PlayerStatus":
                        ParsePlayerStatusRule(searchPackage, rule, queryFilter);
                        break;
                }
            }
            return queryFilter;
        }

        private void ParsePlayerStatusRule(SearchPackage searchPackage, SingleFilter rule,
           GetPlayerPaymentAmountRequest queryFilter)
        {
            if (!string.IsNullOrEmpty(rule.Data))
            {
                if (rule.Comparison == ComparisonOperator.@in || rule.Comparison == ComparisonOperator.eq)
                {
                    var options = ParseOptionsData(rule.Data);

                    queryFilter.IsActive = options.Contains("Active");
                    queryFilter.IsInactive = options.Contains("Inactive");
                    queryFilter.IsTimeOut = options.Contains("Time-out");
                    queryFilter.IsSelfExcluded = options.Contains("Self-Excluded");
                }
            }
            searchPackage.AdvancedFilter.Remove(rule);
        }

        private void ParseApproveDateRule(SearchPackage searchPackage, SingleFilter rule,
          GetPlayerPaymentAmountRequest queryFilter)
        {
            if (!string.IsNullOrEmpty(rule.Data))
            {
                DateTime date;
                DateTime.TryParse(rule.Data, out date);
                if (rule.Comparison == ComparisonOperator.ge)
                {
                    queryFilter.DateApprovedStart = date;
                }
                else if (rule.Comparison == ComparisonOperator.lt)
                {
                    queryFilter.DateApprovedEnd = date;
                }
            }
            searchPackage.AdvancedFilter.Remove(rule);
        }

        private void ParsePaymentMethodRule(SearchPackage searchPackage, SingleFilter rule,
            GetPlayerPaymentAmountRequest queryFilter)
        {
            if (!string.IsNullOrEmpty(rule.Data))
            {
                if (rule.Comparison == ComparisonOperator.@in || rule.Comparison == ComparisonOperator.eq)
                {
                    queryFilter.PaymentMethods= ParseOptionsData(rule.Data);
                }
            }
            searchPackage.AdvancedFilter.Remove(rule);
        }

        private void ParseTransactionTypeRule(SearchPackage searchPackage, SingleFilter rule,
            GetPlayerPaymentAmountRequest queryFilter)
        {
            if (rule.Data == "Withdraw")
            {
                queryFilter.IsQueryDeposit = false;
            }
            searchPackage.AdvancedFilter.Remove(rule);
        }

        private List<string> ParseOptionsData(string data)
        {
            var options = new List<string>();
            options.AddRange(data.TrimStart('[').TrimEnd(']').Replace("\"", string.Empty).Split(','));
            return options;
        }
        #endregion
    }
}