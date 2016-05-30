using System;
using System.Globalization;
using System.Linq;
using AFT.RegoV2.Core.Brand;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Player.ApplicationServices;
using AFT.RegoV2.Core.Player;
using AFT.RegoV2.Core.Player.Data;
using AFT.RegoV2.Core.Player.Interface.Data;
using AFT.RegoV2.Tests.Common.Pages.FrontEnd;
using AutoMapper;
using Player = AFT.RegoV2.Core.Common.Data.Player.Player;

namespace AFT.RegoV2.Tests.Common.Helpers
{
    public class PlayerTestHelper
    {
        private readonly IBrandRepository _brandRepository;
        private readonly IPlayerRepository _playerRepository;
        private readonly PlayerCommands _playerCommands;
        private readonly PlayerQueries _playerQueries;
        private readonly BrandQueries _brandQueries;

        public PlayerTestHelper(
            IBrandRepository brandRepository,
            IPlayerRepository playerRepository,
            PlayerCommands playerCommands,
            PlayerQueries playerQueries,
            BrandQueries brandQueries)
        {
            _brandRepository = brandRepository;
            _playerRepository = playerRepository;
            _playerCommands = playerCommands;
            _playerQueries = playerQueries;
            _brandQueries = brandQueries;

            Seed();
        }

        private void Seed()
        {
            foreach (var securityQuestionId in TestDataGenerator.SecurityQuestions)
            {
                if (!_playerRepository.SecurityQuestions.Any(x => x.Id == new Guid(securityQuestionId)))
                    _playerRepository.SecurityQuestions.Add(new SecurityQuestion
                    {
                        Id = new Guid(securityQuestionId),
                        Question = "Security Question " + securityQuestionId
                    });
            }
        }

        public Guid GetPlayerByUsername(string username)
        {
            var player = _playerRepository.Players.SingleOrDefault(pl => pl.Username == username);

            return player != null ? player.Id : Guid.Empty;
        }

        public Player CreatePlayer(bool isActive = true, Guid? brandId = null)
        {
            var playerRegData = TestDataGenerator.CreateRandomRegistrationRequestData();
            var registrationData = Mapper.DynamicMap<RegistrationData>(playerRegData);

            brandId = brandId ?? _brandRepository.Brands.First().Id;
            var brand = _brandQueries.GetBrandOrNull(brandId.Value);
            registrationData.BrandId = brand.Id.ToString();
            registrationData.CountryCode = brand.BrandCountries.First().Country.Code;
            registrationData.CurrencyCode = brand.BrandCurrencies.First().CurrencyCode;
            registrationData.CultureCode = brand.BrandCultures.First().CultureCode;
            registrationData.IsInactive = !isActive;
            registrationData.AccountAlertEmail = true;
            registrationData.AccountAlertSms = true;

            var playerId = _playerCommands.Register(registrationData);

            return _playerQueries.GetPlayer(playerId);
        }

        public RegistrationDataForMemberWebsite CreatePlayerForMemberWebsite(string currencyCode = null, string password = null)
        {
            var playerRegData = TestDataGenerator.CreateValidPlayerDataForMemberWebsite(currencyCode:currencyCode, password:password);
            var registrationData = Mapper.DynamicMap<RegistrationData>(playerRegData);

            registrationData.BrandId = "00000000-0000-0000-0000-000000000138";
            registrationData.CountryCode = playerRegData.Country;
            registrationData.CurrencyCode = playerRegData.Currency;
            registrationData.CultureCode = TestDataGenerator.GetRandomCultureCode();
            registrationData.DateOfBirth =
                new DateTime(playerRegData.Year, playerRegData.Month, playerRegData.Day).ToString("yyyy/MM/dd", CultureInfo.InvariantCulture);
            registrationData.MailingAddressCity = playerRegData.City;
            registrationData.MailingAddressLine1 = playerRegData.Address;
            registrationData.MailingAddressLine2 = playerRegData.AddressLine2;
            registrationData.MailingAddressLine3 = playerRegData.AddressLine3;
            registrationData.MailingAddressLine4 = playerRegData.AddressLine4;
            registrationData.MailingAddressStateProvince = playerRegData.Province;
            registrationData.MailingAddressPostalCode = playerRegData.PostalCode;
            registrationData.PasswordConfirm = playerRegData.Password;
            registrationData.PhysicalAddressCity = playerRegData.City;
            registrationData.PhysicalAddressLine1 = playerRegData.Address;
            registrationData.PhysicalAddressLine2 = playerRegData.AddressLine2;
            registrationData.PhysicalAddressLine3 = playerRegData.AddressLine3;
            registrationData.PhysicalAddressLine4 = playerRegData.AddressLine4;
            registrationData.PhysicalAddressPostalCode = playerRegData.PostalCode;
            registrationData.SecurityQuestionId = TestDataGenerator.GetRandomSecurityQuestion();
            registrationData.SecurityAnswer = TestDataGenerator.GetRandomString();
            registrationData.IpAddress = "127.0.0.1";

            _playerCommands.Register(registrationData);

            return playerRegData;
        }

        public void AddActivityLog(string activityName, IDomainEvent @event, Guid playerId, string performedBy, string remarks)
        {
            _playerRepository.PlayerActivityLog.Add(new PlayerActivityLog
            {
                Id = Guid.NewGuid(),
                PlayerId = playerId,
                Category = string.Empty,
                ActivityDone = activityName,
                DatePerformed = @event.EventCreated,
                PerformedBy = performedBy,
                Remarks = remarks ?? string.Empty
            });
            _playerRepository.SaveChanges();
        }
    }
}