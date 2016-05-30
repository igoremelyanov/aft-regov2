using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using AFT.RegoV2.AdminWebsite.Common;
using AFT.RegoV2.AdminWebsite.Common.jqGrid;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Fraud.ApplicationServices;
using AFT.RegoV2.Core.Fraud.ApplicationServices.Data;
using AFT.RegoV2.Core.Fraud.ApplicationServices.Duplicate_mechanism;
using AFT.RegoV2.Core.Fraud.Data;
using Newtonsoft.Json;

namespace AFT.RegoV2.AdminWebsite.Controllers.Fraud
{
    public class SignupController : BaseController
    {
        private readonly IFraudPlayerQueries _fraudPlayerQueries;
        private readonly SignUpFraudTypeQueries _fraudTypeQueries;
        private readonly IFraudTypeCommands _fraudTypeCommands;

        public SignupController(
            IFraudPlayerQueries fraudPlayerQueries, 
            SignUpFraudTypeQueries fraudTypeQueries,
            IFraudTypeCommands fraudTypeCommands)
        {
            _fraudPlayerQueries = fraudPlayerQueries;
            _fraudTypeQueries = fraudTypeQueries;
            _fraudTypeCommands = fraudTypeCommands;
        }

        [SearchPackageFilter("searchPackage")]
        [HttpGet]
        public object List(SearchPackage searchPackage, string filter)
        {
            var signUpFilter = JsonConvert.DeserializeObject<SignUpFilter>(filter);
            var matchingResults = _fraudPlayerQueries
                .GetPlayers();

            matchingResults = MatchingResults(signUpFilter, matchingResults);

            var signUpPlayerResults = matchingResults.ToList().Select(x => new SignUpPlayer()
            {
                Id = x.Id,
                Username = x.Username,
                SignDate = Format.FormatDate(x.DateRegistered, true),
                DmcDate = Format.FormatDate(x.DuplicateCheckDate, true),
                Folder = x.Tag.ToString(),
                Status = x.AccountStatus.ToString(),
                Description = x.SignUpRemark,
                HandledBy = "",
                HandledDate = x.HandledDate.HasValue ? Format.FormatDate(x.HandledDate, true) : "",
                CompletedDate = x.CompletedDate.HasValue ? Format.FormatDate(x.HandledDate, true) : "",
                UpdatedBy = "",
                DateUpdated = "",
                FraudType = x.FraudType
            }).AsQueryable();

            var dataBuilder = new SearchPackageDataBuilder<SignUpPlayer>(
                searchPackage,
                signUpPlayerResults);

            dataBuilder
                .Map(o => o.Id,
                    obj => new[]
                    {
                       obj.Id.ToString(),
                       obj.Username,
                       obj.SignDate,
                       obj.DmcDate,
                       obj.Folder,
                       obj.Description,
                       obj.Status,
                       obj.HandledBy,
                       obj.HandledDate,
                       obj.CompletedDate,
                       obj.UpdatedBy,
                       obj.DateUpdated,
                       obj.FraudType
                    });
            var data = dataBuilder.GetPageData(player => player.SignDate);

            return new JsonResult { Data = data, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public string FraudTypes()
        {
            return SerializeJson(_fraudTypeQueries.GetSignUpFraudTypes());
        }

        public ActionResult Update(SignupUpdateData data)
        {
            _fraudTypeCommands.UpdatePlayer(data);
            return this.Success();
        }

        private IQueryable<Player> MatchingResults(SignUpFilter signUpFilter, IQueryable<Player> matchingResults)
        {
            if (signUpFilter.Period == Period.Custom)
            {
                if (signUpFilter.StartDate != null && signUpFilter.StartDate != default(DateTime))
                {
                    if (signUpFilter.EndDate != null && signUpFilter.EndDate != default(DateTime))

                    {
                        var start = (DateTimeOffset) signUpFilter.StartDate;
                        var end = (DateTimeOffset) signUpFilter.EndDate;
                        matchingResults =
                            matchingResults.Where(
                                x => x.DateRegistered > start && x.DateRegistered < end);
                    }
                }
            }

            if (signUpFilter.Username != null)
            {
                matchingResults = matchingResults
                    .Where(x => x.Username.Contains(signUpFilter.Username));
            }

            if (signUpFilter.Tags != null && signUpFilter.Tags.Any())
            {
                matchingResults = matchingResults.Where(x => signUpFilter.Tags.Contains(x.Tag));
            }

            if (signUpFilter.Period == Period.Today)
            {
                var date = DateTimeOffset.UtcNow.Date;
                matchingResults =
                    matchingResults.Where(
                        x =>
                            x.DateRegistered.Year == date.Year && x.DateRegistered.Month == date.Month &&
                            x.DateRegistered.Day == date.Day);
            }

            if (signUpFilter.Period == Period.CurrentMonth)
            {
                var startOfTthisMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                var firstDay = startOfTthisMonth;
                var lastDay = startOfTthisMonth.AddMonths(1);

                matchingResults = matchingResults.Where(x => x.DateRegistered <= lastDay && x.DateRegistered >= firstDay);
            }

            if (signUpFilter.Period == Period.PreviousMonth)
            {
                var startOfTthisMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                var firstDay = startOfTthisMonth.AddMonths(-1);
                var lastDay = firstDay.AddMonths(1);

                matchingResults = matchingResults.Where(x => x.DateRegistered <= lastDay && x.DateRegistered >= firstDay);
            }
            return matchingResults;
        }
    }

    public class SignUpFilter
    {
        public string Username { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Period Period { get; set; }
        public IEnumerable<QueueFolderTag> Tags { get; set; }
    }

    public enum Period
    {
        Today,
        CurrentMonth,
        PreviousMonth,
        All,
        Custom
    }

    public class SignUpPlayer
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string SignDate { get; set; }
        public string DmcDate { get; set; }
        public string Folder { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string HandledBy { get; set; }
        public string HandledDate { get; set; }
        public string CompletedDate { get; set; }
        public string UpdatedBy { get; set; }
        public string DateUpdated { get; set; }
        public string FraudType { get; set; }
    }
}
