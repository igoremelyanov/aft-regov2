using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using AFT.RegoV2.AdminApi.Controllers.Base;
using AFT.RegoV2.AdminApi.Interface.Common;
using AFT.RegoV2.AdminApi.Interface.Player;
using AFT.RegoV2.AdminWebsite.Common;
using AFT.RegoV2.AdminWebsite.Common.jqGrid;
using AFT.RegoV2.Core.Auth.Interface.ApplicationServices;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Common.Data.Brand;
using AFT.RegoV2.Core.Common.Data.Payment;
using AFT.RegoV2.Core.Common.Data.Player;
using AFT.RegoV2.Core.Payment.Interface.Data;
using AFT.RegoV2.Core.Payment.Interface.ApplicationServices;
using AFT.RegoV2.Core.Player.ApplicationServices;
using AFT.RegoV2.Core.Player.Data;
using AFT.RegoV2.Core.Player.Interface.Data;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Core.Security.Interfaces;
using AutoMapper;
using SendNewPasswordData = AFT.RegoV2.Core.Common.Data.Player.SendNewPasswordData;

namespace AFT.RegoV2.AdminApi.Controllers.Player
{
    [Authorize]
    public class PlayerManagerController : BaseApiController
    {
        private readonly PlayerCommands _commands;
        private readonly PlayerQueries _queries;
        private readonly BrandQueries _brandQueries;
        private readonly IPaymentQueries _paymentQueries;
        private readonly IPlayerBankAccountCommands _playerBankAccountCommands;
        private readonly IPaymentLevelCommands _paymentLevelCommands;
        private readonly IPaymentLevelQueries _paymentLevelQueries;
        private readonly IAdminQueries _adminQueries;

        public PlayerManagerController(
            PlayerCommands commands,
            PlayerQueries queries,
            BrandQueries brandQueries,
            IPaymentQueries paymentQueries,
            IPlayerBankAccountCommands playerBankAccountCommands,
            IPaymentLevelCommands paymentLevelCommands,
            IPaymentLevelQueries paymentLevelQueries,
            IAuthQueries authQueries,
            IAdminQueries adminQueries)
            : base(authQueries, adminQueries)
        {
            _commands = commands;
            _queries = queries;
            _brandQueries = brandQueries;
            _paymentQueries = paymentQueries;
            _playerBankAccountCommands = playerBankAccountCommands;
            _paymentLevelCommands = paymentLevelCommands;
            _paymentLevelQueries = paymentLevelQueries;
            _adminQueries = adminQueries;
        }

        static PlayerManagerController()
        {
            Mapper.CreateMap<AddPlayerData, RegistrationData>()
                .ForMember(x => x.BrandId, y => y.MapFrom(z => z.Brand))
                .ForMember(x => x.CountryCode, y => y.MapFrom(z => z.Country))
                .ForMember(x => x.CultureCode, y => y.MapFrom(z => z.Culture))
                .ForMember(x => x.CurrencyCode, y => y.MapFrom(z => z.Currency));
        }

        [HttpGet]
        [Filters.SearchPackageFilter("searchPackage")]
        [Route(AdminApiRoutes.ListPlayers)]
        public IHttpActionResult Data([FromUri] SearchPackage searchPackage)
        {
            VerifyPermission(Permissions.View, Modules.PlayerManager);
            return Ok(SearchData(searchPackage));
        }

        [HttpGet]
        [Route(AdminApiRoutes.GetAddPlayerDataInPlayerManager)]
        public IHttpActionResult GetAddPlayerData()
        {
            var licenseeFilterSelections = _adminQueries.GetLicenseeFilterSelections();

            var licensees = _brandQueries.GetLicensees()
                .Where(x => 
                    x.Brands.Any(y => y.Status == BrandStatus.Active) && 
                    licenseeFilterSelections.Contains(x.Id))
                .OrderBy(x => x.Name)
                .Select(x => new {x.Id, x.Name});

            var response = new
            {
                Licensees = licensees,
                Genders = Enum.GetNames(typeof (Gender)),
                Titles = Enum.GetNames(typeof (Title)),
                IdStatuses = Enum.GetNames(typeof (IdStatus)),
                ContactMethods = Enum.GetNames(typeof (ContactMethod)).OrderBy(x => x),
                SecurityQuestions = _queries.GetSecurityQuestions()
            };

            return Ok(new {result = "success", data = response});
        }

        [HttpGet]
        [Route(AdminApiRoutes.GetAddPlayerBrandsInPlayerManager)]
        public IHttpActionResult GetAddPlayerBrands(Guid licenseeId)
        {
            var brandFilterSelections = _adminQueries.GetBrandFilterSelections();

            var brands = _brandQueries.GetBrands()
                .Where(x =>
                    x.Status == BrandStatus.Active &&
                    x.Licensee.Id == licenseeId &&
                    brandFilterSelections.Contains(x.Id))
                .OrderBy(x => x.Name)
                .Select(x => new {x.Id, x.Name, x.PlayerPrefix});

            return Ok(new { result = "success", data = new { brands } });
        }

        [HttpGet]
        [Route(AdminApiRoutes.GetAddPlayerBrandDataInPlayerManager)]
        public IHttpActionResult GetAddPlayerBrandData(Guid brandId)
        {
            var brand = _brandQueries.GetBrandOrNull(brandId);
            var paymentLevels = _paymentQueries.GetPaymentLevels(brandId);

            var currencies = brand.BrandCurrencies
                .Where(x => paymentLevels.Any(y => y.CurrencyCode == x.CurrencyCode && y.IsDefault))
                .OrderBy(x => x.CurrencyCode)
                .Select(x => x.CurrencyCode);

            var countries = brand.BrandCountries
                .OrderBy(x => x.CountryCode)
                .Select(x => x.CountryCode);

            var cultures = brand.BrandCultures
                .OrderBy(x => x.CultureCode)
                .Select(x => x.CultureCode);

            var response = new
            {
                Countries = countries,
                Cultures = cultures,
                Currencies = currencies
            };

            return Ok(new { result = "success", data = response });
        }

        [HttpGet]
        [Route(AdminApiRoutes.GetPaymentLevelsInPlayerManager)]
        public IHttpActionResult GetPaymentLevels(Guid brandId, string currency)
        {
            var paymentLevels = _paymentLevelQueries.GetPaymentLevelsByBrandAndCurrency(brandId, currency)
                .Select(l => new {l.Id, l.Name});
            
            return Ok(new
            {
                PaymentLevels = paymentLevels
            });
        }

        [HttpGet]
        [Route(AdminApiRoutes.GetVipLevelsInPlayerManager)]
        public IHttpActionResult GetVipLevels(Guid brandId)
        {
            return Ok(new
            {
                VipLevels = _queries.VipLevels.Where(x => x.BrandId == brandId).OrderBy(x => x.Code)
            });
        }

        [HttpPost]
        [Route(AdminApiRoutes.ChangeVipLevelInPlayerManager)]
        public IHttpActionResult ChangeVipLevel(ChangeVipLevelData command)
        {
            VerifyPermission(Permissions.AssignVipLevel, Modules.PlayerManager);

            if (ModelState.IsValid == false)
                return Ok(ErrorResponse());

            _commands.ChangeVipLevel(command.PlayerId, command.NewVipLevel, command.Remarks);

            return Ok(new { Result = "success" });
        }

        [HttpPost]
        [Route(AdminApiRoutes.ChangePaymentLevelInPlayerManager)]
        public IHttpActionResult ChangePaymentLevel(ChangePaymentLevelData command)
        {
            VerifyPermission(Permissions.AssignPaymentLevel, Modules.PlayerManager);

            if (ModelState.IsValid == false)
                return Ok(ErrorResponse());

            var request = new ChangePaymentLevelData
            {
                PlayerId = command.PlayerId,
                PaymentLevelId = command.PaymentLevelId,
                Remarks = command.Remarks
            };
            var validationResult = _commands.ValidatePlayerPaymentLevelCanBeChanged(request);
            if (validationResult.IsValid == false)
            {
                return Ok(ValidationExceptionResponse(validationResult.Errors));
            }

            _commands.ChangePaymentLevel(request);

            return Ok(new { Result = "success" });
        }

        [HttpPost]
        [Route(AdminApiRoutes.ChangePlayersPaymentLevelInPlayerManager)]
        public IHttpActionResult ChangePlayersPaymentLevel(ChangePlayersPaymentLevelData command)
        {
            VerifyPermission(Permissions.Update, Modules.PaymentLevelSettings);

            var playerIds = command.PlayerIds;
            var allRequests = new List<ChangePaymentLevelData>();
            //verify all players
            for (int i = 0; i < playerIds.Length; i++)
            {
                var request = new ChangePaymentLevelData
                {
                    PlayerId = playerIds[i],
                    PaymentLevelId = command.PaymentLevelId,
                    Remarks = command.Remarks
                };


                var validationResult = _commands.ValidatePlayerPaymentLevelCanBeChanged(request);
                if (validationResult.IsValid == false)
                {
                    return Ok(ValidationExceptionResponse(validationResult.Errors));
                }
                allRequests.Add(request);
            }

            foreach (var request in allRequests)
            {
                _commands.ChangePaymentLevel(request);

            }
            return Ok(new { Result = "success" });
        }

        [HttpPost]
        [Route(AdminApiRoutes.SendNewPasswordInPlayerManager)]
        public IHttpActionResult SendNewPassword(SendNewPasswordData command)
        {
            var validationResult = _commands.ValidateThatNewPasswordCanBeSent(command);
            
            if (!validationResult.IsValid)
                return Ok(ValidationExceptionResponse(validationResult.Errors));

            _commands.SendNewPassword(command);

            return Ok(new { Result = "success" });
        }

        [HttpPost]
        [Route(AdminApiRoutes.AddPlayerInPlayerManager)]
        public IHttpActionResult Add(AddPlayerData command)
        {
            VerifyPermission(Permissions.Create, Modules.PlayerManager);

            var playerData = Mapper.DynamicMap<RegistrationData>(command);
            playerData.IsRegisteredFromAdminSite = true;
            playerData.IpAddress = "127.0.0.1";

            var validationResult = _commands.ValidateThatPlayerCanBeRegistered(playerData);
            if (!validationResult.IsValid)
                return Ok(ValidationExceptionResponse(validationResult.Errors));

            _commands.Register(playerData);
            return Ok(new { Result = "success" });
        }

        [HttpGet]
        [Route(AdminApiRoutes.GetPlayerForBankAccountInPlayerManager)]
        public IHttpActionResult GetPlayerForBankAccount(Guid id)
        {
            var player = _paymentQueries.GetPlayer(id);
            if (player == null)
                return Ok(new {Result = "failed"});

            return Ok(new
            {
                Result = "success",
                Player = SelectPlayerDataForBankAccount(player)
            });
        }

        [HttpGet]
        [Route(AdminApiRoutes.GetBankAccountInPlayerManager)]
        public IHttpActionResult GetBankAccount(Guid id)
        {
            VerifyPermission(Permissions.View, Modules.PlayerBankAccount);

            var bankAccount =
                _paymentQueries.GetPlayerBankAccounts()
                    .SingleOrDefault(x => x.Id == id);

            return bankAccount == null
                ? Ok(new {Result = "failed"})
                : SerializePlayerAndBankAccount(bankAccount.Player, bankAccount);
        }

        [HttpGet]
        [Route(AdminApiRoutes.GetCurrentBankAccountInPlayerManager)]
        public IHttpActionResult GetCurrentBankAccount(Guid playerId)
        {
            var player = _paymentQueries.GetPlayerWithBank(playerId);

            return player == null
                ? Ok(new {Result = "failed"})
                : SerializePlayerAndBankAccount(player, player.CurrentBankAccount);
        }

        [HttpPost]
        [Route(AdminApiRoutes.SaveBankAccountInPlayerManager)]
        public IHttpActionResult SaveBankAccount(EditPlayerBankAccountData model)
        {
            var isExistingBankAccount = model.Id.HasValue;
            VerifyPermission(isExistingBankAccount ? Permissions.Update : Permissions.Create, Modules.PlayerBankAccount);

            if (isExistingBankAccount)
            {
                var editValidationResult = _playerBankAccountCommands.ValidateThatPlayerBankAccountCanBeEdited(model);
                if (!editValidationResult.IsValid)
                    return Ok(ValidationExceptionResponse(editValidationResult.Errors));

                _playerBankAccountCommands.Edit(model);
                return Ok(new { Result = "success" });
            }

            var validationResult = _playerBankAccountCommands.ValidateThatPlayerBankAccountCanBeAdded(model);
            if (!validationResult.IsValid)
                return Ok(ValidationExceptionResponse(validationResult.Errors));

            _playerBankAccountCommands.Add(model);
            return Ok(new { Result = "success" });
        }

        [HttpPost]
        [Route(AdminApiRoutes.SetCurrentBankAccountInPlayerManager)]
        public IHttpActionResult SetCurrentBankAccount(SetCurrentPlayerBankAccountData data)
        {
            VerifyPermission(Permissions.Update, Modules.PlayerBankAccount);

            var validationResult = _playerBankAccountCommands.ValidateThatPlayerBankAccountCanBeSet(data);
            if (!validationResult.IsValid)
                return Ok(ValidationExceptionResponse(validationResult.Errors));

            _playerBankAccountCommands.SetCurrent(data.PlayerBankAccountId);
            return Ok(new { Result = "success" });
        }

        protected object SearchData(SearchPackage searchPackage)
        {
            var vipLevels = _queries.VipLevels.ToArray();
            
            var playerPaymentLevels = _paymentQueries.GetPlayerPaymentLevels();
            
            var brands = CurrentBrands;

            var brandFilterSelections = _adminQueries.GetBrandFilterSelections();
            
            var query = _queries.GetPlayers().Where(p => brands.Contains(p.BrandId) && brandFilterSelections.Contains(p.BrandId));
            
            var dataBuilder = new SearchPackageDataBuilder<Core.Common.Data.Player.Player>(searchPackage, query);

            dataBuilder
                .SetFilterRule(x => x.BrandId, (value) => p => p.BrandId == new Guid(value))
                .Map(player => player.Id,
                    player => new[]
                    {
                        player.Username,
                        player.FirstName,
                        player.LastName,
                        string.Empty, //affilaite code
                        player.Gender.ToString(),
                        player.Email,
                        player.PhoneNumber,
                        Format.FormatDate(player.DateRegistered, true),
                        player.IpAddress,
                        (Format.FormatDate(player.DateOfBirth, false) == "0001-01-01"
                            ? null
                            : Format.FormatDate(player.DateOfBirth, false)),
                        string.Empty, //language
                        player.CurrencyCode ?? string.Empty,
                        player.CountryCode ?? string.Empty,
                        (!player.IsInactive).ToString(),
                        _brandQueries.GetBrandOrNull(player.BrandId).Name,
                        string.Empty, //licensee
                        player.VipLevel != null && vipLevels.SingleOrDefault(x => x.Id == player.VipLevel.Id) != null
                            ? vipLevels.Single(x => x.Id == player.VipLevel.Id).Name
                            : string.Empty,
                        playerPaymentLevels.FirstOrDefault(x => x.PlayerId == player.Id) == null
                            ? string.Empty
                            : playerPaymentLevels.FirstOrDefault(x => x.PlayerId == player.Id).PaymentLevel.Name,
                        string.Empty, //fraud risk level
                        player.MailingAddressLine1,
                        player.MailingAddressCity,
                        player.MailingAddressPostalCode,
                        string.Empty, //province
                        player.VipLevel != null && vipLevels.SingleOrDefault(x => x.Id == player.VipLevel.Id) != null
                            ? vipLevels.Single(x => x.Id == player.VipLevel.Id).ColorCode
                            : string.Empty,
                    }
                );
            return dataBuilder.GetPageData(player => player.Username);
        }

        private IHttpActionResult SerializePlayerAndBankAccount(Core.Payment.Interface.Data.Player player, PlayerBankAccount bankAccount)
        {
            object bankAccountSent = null;
            if (bankAccount != null)
            {
                var bankAcccountTime = bankAccount.Updated ?? bankAccount.Created;
                var bankTime = bankAccount.Bank.Updated ?? bankAccount.Bank.Created;

                bankAccountSent = new
                {
                    bankAccount.Id,
                    Bank = bankAccount.Bank.Id,
                    bankAccount.Bank.BankName,
                    bankAccount.Province,
                    bankAccount.City,
                    bankAccount.Branch,
                    bankAccount.SwiftCode,
                    bankAccount.Address,
                    bankAccount.AccountName,
                    bankAccount.AccountNumber,
                    bankAccount.Status,
                    bankAccount.EditLock,
                    Time = bankAcccountTime.ToString("o"),
                    BankTime = bankTime.ToString("o")
                };
            }
            return Ok(new
            {
                Result = "success",
                Player = SelectPlayerDataForBankAccount(player),
                BankAccount = bankAccountSent
            });
        }

        private static object SelectPlayerDataForBankAccount(Core.Payment.Interface.Data.Player player)
        {
            return new
            {
                player.Id,
                player.Username,
                Brand = new
                {
                    Id = player.BrandId
                }
            };
        }

        private IEnumerable<Guid> CurrentBrands
        {
            get
            {
                if (string.IsNullOrEmpty(Username))
                {
                    return null;
                }

                var admin = _adminQueries.GetAdminById(UserId);

                return admin.AllowedBrands.Select(b => b.Id);
            }
        }
    }
}