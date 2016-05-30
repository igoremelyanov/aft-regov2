using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using AFT.RegoV2.AdminWebsite.Common;
using AFT.RegoV2.AdminWebsite.Common.jqGrid;
using AFT.RegoV2.Core.Auth.Interface;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Common.Data.Brand;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Fraud.ApplicationServices;
using AFT.RegoV2.Core.Fraud.ApplicationServices.Data;
using AFT.RegoV2.Core.Fraud.Data;
using AFT.RegoV2.Core.Security.Interfaces;
using Newtonsoft.Json;

namespace AFT.RegoV2.AdminWebsite.Controllers
{
    public class DuplicateMechanismController : BaseController
    {
        private readonly DuplicateMechanismQueries _duplicateMechanismQueries;
        private readonly DuplicateMechanismCommands _duplicateMechanismCommands;
        private readonly BrandQueries _brandQueries;
        private readonly FraudPlayerQueries _playerQueries;
        private readonly IDuplicateScoreService _scoreService;
        private readonly IAdminQueries _adminQueries;
        private readonly IActorInfoProvider _actorInfoProvider;

        public DuplicateMechanismController(
            DuplicateMechanismQueries duplicateMechanismQueries,
            DuplicateMechanismCommands duplicateMechanismCommands,
            BrandQueries brandQueries,
            IAdminQueries adminQueries,
            FraudPlayerQueries playerQueries,
            IDuplicateScoreService scoreService,
            IActorInfoProvider actorInfoProvider)
        {
            _duplicateMechanismQueries = duplicateMechanismQueries;
            _duplicateMechanismCommands = duplicateMechanismCommands;
            _brandQueries = brandQueries;
            _playerQueries = playerQueries;
            _scoreService = scoreService;
            _adminQueries = adminQueries;
            _actorInfoProvider = actorInfoProvider;
        }

        [SearchPackageFilter("searchPackage")]
        [HttpGet]
        public object PlayerList(SearchPackage searchPackage, Guid playerId, string formData)
        {
            var setting = JsonConvert.DeserializeObject<ExactScoreConfiguration>(formData);

            var matchingResults = _duplicateMechanismQueries.GetMatchingResults(playerId)
                .ToList()
                .AsQueryable();

            var dataBuilder = new SearchPackageDataBuilder<MatchingResult>(
                searchPackage,
                matchingResults);

            dataBuilder
                .Map(o => o.SecondPlayerId,
                    obj => new[]
                    {
                        obj.SecondPlayerId.ToString(),
                        JsonConvert.SerializeObject(obj.MatchingCriterias.Select(o=>o.Code).ToArray()),
                        _brandQueries.GetBrand(obj.SecondPlayer.BrandId).Name,
                        obj.SecondPlayer.Username,
                        obj.SecondPlayer.DateRegistered.ToString("yyyy/MM/dd"),
                        obj.SecondPlayer.FirstName,
                        obj.SecondPlayer.LastName,
                        obj.SecondPlayer.DateOfBirth.ToString("yyyy/MM/dd"),
                        obj.SecondPlayer.Email,
                        obj.SecondPlayer.Phone,
                        obj.SecondPlayer.Address,
                        obj.SecondPlayer.ZipCode,
                        obj.SecondPlayer.IPAddress,
                        string.Empty,
                        _scoreService.ScorePlayer(playerId, obj.SecondPlayerId, setting).ToString()
                    });
            var data = dataBuilder.GetPageData(configuration => configuration.SecondPlayer.Username);

            return new JsonResult { Data = data, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpGet]
        public string GetConfiguration(Guid playerId)
        {
            var player = _playerQueries.GetPlayers()
                .Single(o => o.Id == playerId);

            var configuration = _duplicateMechanismQueries
                .GetConfigurations()
                .FirstOrDefault(x => x.BrandId == player.BrandId);

            return SerializeJson(new
            {
                Configuration = configuration
            });
        }

        [SearchPackageFilter("searchPackage")]
        public object List(SearchPackage searchPackage)
        {
            var dataBuilder = new SearchPackageDataBuilder<DuplicateMechanismConfiguration>(
                searchPackage,
                _duplicateMechanismQueries.GetConfigurations().Include(o => o.Brand));

            dataBuilder.SetFilterRule(x => x.Brand, value => p => p.Brand.Id == Guid.Parse(value))
                .Map(configuration => configuration.Id,
                    obj => new[]
                    {
                        obj.Brand.LicenseeName,
                        obj.Brand.Name,
                        obj.CreatedBy,
                        obj.CreatedDate.ToString(),
                        obj.UpdatedBy ?? "-",
                        obj.UpdatedDate!=null ? obj.UpdatedDate.ToString() : "-"
                    });
            var data = dataBuilder.GetPageData(configuration => configuration.Brand.Name);
            return new JsonResult { Data = data, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpPost]
        public ActionResult AddOrUpdate(DuplicateMechanismDTO data)
        {
            try
            {
                if (data.Id == Guid.Empty)
                    _duplicateMechanismCommands.Create(data);
                else
                    _duplicateMechanismCommands.Update(data);
            }
            catch (Exception e)
            {
                return this.Failed(e);
            }
            return this.Success(new
            {
                Code = data.Id == Guid.Empty
                    ? "successfullyCreated"
                    : "successfullyUpdated"
            });
        }

        public string GetBrands(Guid licensee, Guid? configId)
        {
            var filteredBrandsForAdmin = _adminQueries.GetAdminById(_actorInfoProvider.Actor.Id)
                .BrandFilterSelections
                .Select(fbr => fbr.BrandId);

            var brands = _brandQueries
                .GetFilteredBrands(_brandQueries.GetBrandsByLicensee(licensee), CurrentUser.Id)
                .Where(o => o.Status == BrandStatus.Active && filteredBrandsForAdmin.Any(fbr => fbr == o.Id));

            return SerializeJson(new
            {
                Brands = brands
                    .OrderBy(b => b.Name)
                    .Select(b => new { name = b.Name, id = b.Id })
            });
        }

        public string GetById(Guid id)
        {
            var configuration = _duplicateMechanismQueries.GetConfiguration(id);
            return SerializeJson(configuration);
        }
    }

    public class ExactScoreConfiguration : IExactScoreConfiguration
    {
        public int DeviceIdExactScore { get; set; }
        public int FirstNameExactScore { get; set; }
        public int LastNameExactScore { get; set; }
        public int FullNameExactScore { get; set; }
        public int UsernameExactScore { get; set; }
        public int AddressExactScore { get; set; }
        public int SignUpIpExactScore { get; set; }
        public int MobilePhoneExactScore { get; set; }
        public int DateOfBirthExactScore { get; set; }
        public int EmailAddressExactScore { get; set; }
        public int ZipCodeExactScore { get; set; }
    }
}