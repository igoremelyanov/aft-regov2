using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web;
using AFT.RegoV2.AdminApi.Interface.Player;
using AFT.RegoV2.Core.Common.Data.Payment;
using AFT.RegoV2.Core.Common.Data.Player;
using AFT.RegoV2.Core.Common.Data.Player.Enums;
using AFT.RegoV2.Core.Payment.Interface.Events;
using AFT.RegoV2.Core.Player.ApplicationServices;
using AFT.RegoV2.Core.Player.Interface.Data;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Helpers;
using AutoMapper;
using FluentAssertions;
using Microsoft.Practices.Unity;
using NUnit.Framework;
using TechTalk.SpecFlow;
using SendNewPasswordData = AFT.RegoV2.Core.Common.Data.Player.SendNewPasswordData;

namespace AFT.RegoV2.AdminApi.Tests.Integration.Steps
{
    public class PlayerSteps : BaseSteps
    {
        private BrandTestHelper BrandHelper { get; set; }
        private PlayerTestHelper PlayerHelper { get; set; }
        private PaymentTestHelper PaymentHelper { get; set; }
        private SecurityTestHelper SecurityTestHelper { get; set; }
        private PlayerQueries PlayerQueries { get; set; }

        protected readonly Guid DefaultBrandId = new Guid("00000000-0000-0000-0000-000000000138");

        public PlayerSteps()
        {
            SecurityTestHelper = Container.Resolve<SecurityTestHelper>();
            SecurityTestHelper.SignInClaimsSuperAdmin();
            BrandHelper = Container.Resolve<BrandTestHelper>();
            PlayerHelper = Container.Resolve<PlayerTestHelper>();
            PaymentHelper = Container.Resolve<PaymentTestHelper>();
            PlayerQueries = Container.Resolve<PlayerQueries>();
        }

        [When(@"New licensee is created")]
        public void WhenNewLicenseeIsCreated()
        {
            var licensee = BrandHelper.CreateLicensee();
            ScenarioContext.Current.Add("licenseeId", licensee.Id);
        }

        [When(@"New brand is created")]
        public void WhenNewBrandIsCreated()
        {
            var licensee = BrandHelper.CreateLicensee();
            var brand = BrandHelper.CreateBrand(licensee, isActive: true);
            ScenarioContext.Current.Add("licenseeId", licensee.Id);
            ScenarioContext.Current.Add("brandId", brand.Id);
            ScenarioContext.Current.Add("currencyCode", brand.DefaultCurrency);
        }

        [When(@"New player is created")]
        public void WhenNewPlayerIsCreated()
        {
            var player = CreatePlayer();
            ScenarioContext.Current.Add("playerId", player.Id);
        }

        [When(@"New player (.*) is created")]
        public void WhenNewPlayerIsCreated(string name)
        {
            var player = CreatePlayer();
            ScenarioContext.Current.Add("playerId"+name, player.Id);
        }

        private Player CreatePlayer()
        {
            ScenarioContext.Current.Should().ContainKey("brandId");
            var brandId = ScenarioContext.Current.Get<Guid>("brandId");
            return PlayerHelper.CreatePlayer(true, brandId);
        }

        [When(@"New vip level is created")]
        public void WhenNewVipLevelIsCreated()
        {
            ScenarioContext.Current.Should().ContainKey("brandId");
            var brandId = ScenarioContext.Current.Get<Guid>("brandId");
            var vipLevel = BrandHelper.CreateVipLevel(brandId, 0, false);
            ScenarioContext.Current.Add("vipLevelId", vipLevel.Id);
        }

        [When(@"New payment level is created")]
        public void WhenNewPaymentLevelIsCreated()
        {
            ScenarioContext.Current.Should().ContainKey("brandId");
            var brandId = ScenarioContext.Current.Get<Guid>("brandId");
            var paymentLevel = PaymentHelper.CreatePaymentLevel(brandId, "CAD", true);
            ScenarioContext.Current.Add("paymentLevelId", paymentLevel.Id);
        }

        [When(@"New bank account is created")]
        public void WhenNewBankAccountIsCreated()
        {
            var licensee = BrandHelper.CreateLicensee();
            var brand = BrandHelper.CreateBrand(licensee, isActive: true);
            var bankAccount = PaymentHelper.CreateBankAccount(brand.Id, brand.DefaultCurrency);
            ScenarioContext.Current.Add("bankAccountId", bankAccount.Id);
        }

        [When(@"New player bank account is created")]
        public void WhenNewPlayerBankAccountIsCreated()
        {
            ScenarioContext.Current.Should().ContainKey("playerId");
            var playerId = ScenarioContext.Current.Get<Guid>("playerId");

            var playerBankAccount = PaymentHelper.CreatePlayerBankAccount(playerId, DefaultBrandId, true);
            ScenarioContext.Current.Add("playerBankAccountId", playerBankAccount.Id);
        }

        [Then(@"Required data to add new player is visible to me")]
        public void ThenRequiredDataToAddNewPlayerIsVisibleToMe()
        {
            var result = AdminApiProxy.GetAddPlayerDataInPlayerManager();

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        [Then(@"Required data to add player brands is visible to me")]
        public void ThenRequiredDataToAddPlayerBrandsIsVisibleToMe()
        {
            ScenarioContext.Current.Should().ContainKey("licenseeId");
            var licenseeId = ScenarioContext.Current.Get<Guid>("licenseeId");

            var result = AdminApiProxy.GetAddPlayerBrandsInPlayerManager(licenseeId);

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        [Then(@"Required data to add player brand data is visible to me")]
        public void ThenRequiredDataToAddPlayerBrandDataIsVisibleToMe()
        {
            ScenarioContext.Current.Should().ContainKey("brandId");
            var brandId = ScenarioContext.Current.Get<Guid>("brandId");

            var result = AdminApiProxy.GetAddPlayerBrandDataInPlayerManager(brandId);

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        [Then(@"Payment levels are visible to me")]
        public void ThenPaymentLevelsAreVisibleToMe()
        {
            ScenarioContext.Current.Should().ContainKey("brandId");
            ScenarioContext.Current.Should().ContainKey("currencyCode");

            var brandId = ScenarioContext.Current.Get<Guid>("brandId");
            var currencyCode = ScenarioContext.Current.Get<string>("currencyCode");

            var result = AdminApiProxy.GetPaymentLevelsInPlayerManager(brandId, currencyCode);

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        [Then(@"Vip levels are visible to me")]
        public void ThenVipLevelsAreVisibleToMe()
        {
            ScenarioContext.Current.Should().ContainKey("brandId");
            var brandId = ScenarioContext.Current.Get<Guid>("brandId");

            var result = AdminApiProxy.GetVipLevelsInPlayerManager(brandId);

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        [Then(@"Vip level is successfully changed")]
        public void ThenVipLevelIsSuccessfullyChanged()
        {
            ScenarioContext.Current.Should().ContainKey("playerId");
            ScenarioContext.Current.Should().ContainKey("vipLevelId");
            var playerId = ScenarioContext.Current.Get<Guid>("playerId");
            var vipLevelId = ScenarioContext.Current.Get<Guid>("vipLevelId");

            var data = new ChangeVipLevelData
            {
                NewVipLevel = vipLevelId,
                PlayerId = playerId,
                Remarks = TestDataGenerator.GetRandomString()
            };

            var result = AdminApiProxy.ChangeVipLevelInPlayerManager(data);

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        [Then(@"Payment level is successfully changed")]
        public void ThenPaymentLevelIsSuccessfullyChanged()
        {
            ScenarioContext.Current.Should().ContainKey("playerId");
            ScenarioContext.Current.Should().ContainKey("paymentLevelId");
            var playerId = ScenarioContext.Current.Get<Guid>("playerId");
            var paymentLevelId = ScenarioContext.Current.Get<Guid>("paymentLevelId");

            var data = new ChangePaymentLevelData
            {
                PaymentLevelId = paymentLevelId,
                PlayerId = playerId,
                Remarks = TestDataGenerator.GetRandomString()
            };

            var result = AdminApiProxy.ChangePaymnetLevelInPlayerManager(data);

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        [Then(@"Players Payment level is successfully changed")]
        public void ThenPlayersPaymentLevelIsSuccessfullyChanged()
        {
            ScenarioContext.Current.Should().ContainKey("playerId");
            ScenarioContext.Current.Should().ContainKey("paymentLevelId");
            var playerId = ScenarioContext.Current.Get<Guid>("playerId");
            var playerId2 = ScenarioContext.Current.Get<Guid>("playerId2");
            var playerId3 = ScenarioContext.Current.Get<Guid>("playerId3");
            var paymentLevelId = ScenarioContext.Current.Get<Guid>("paymentLevelId");
            Guid[] playerIds = new Guid[3] {playerId,playerId2,playerId3};
            var data = new ChangePlayersPaymentLevelData
            {
                PaymentLevelId = paymentLevelId,
                PlayerIds = playerIds,
                Remarks = TestDataGenerator.GetRandomString()
            };

            var result = AdminApiProxy.ChangePlayersPaymentLevelInPlayerManager(data);

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        [Then(@"New password is sent to player")]
        public void ThenNewPasswordIsSentToPlayer()
        {
            ScenarioContext.Current.Should().ContainKey("playerId");
            var playerId = ScenarioContext.Current.Get<Guid>("playerId");

            var data = new SendNewPasswordData
            {
                PlayerId = playerId,
                NewPassword = TestDataGenerator.GetRandomString(),
                SendBy = SendBy.Email
            };

            var result = AdminApiProxy.SendNewPasswordInPlayerManager(data);

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        [Then(@"New player is created")]
        public void ThenNewPlayerIsCreated()
        {
            var data = TestDataGenerator.CreateRandomRegistrationData();
            var addPlayerData = Mapper.DynamicMap<AddPlayerData>(data);
            addPlayerData.Brand = data.BrandId;
            addPlayerData.Country = data.CountryCode;
            addPlayerData.Culture = data.CultureCode;
            addPlayerData.Currency = "CAD";

            var result = AdminApiProxy.AddPlayerInPlayerManager(addPlayerData);

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        [Then(@"Required player data is visible to me")]
        public void ThenRequiredPlayerDataIsVisibleToMe()
        {
            ScenarioContext.Current.Should().ContainKey("playerId");
            var playerId = ScenarioContext.Current.Get<Guid>("playerId");

            var result = AdminApiProxy.GetPlayerForBankAccountInPlayerManager(playerId);

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        [Then(@"Required brank account data is visible to me")]
        public void ThenRequiredBrankAccountDataIsVisibleToMe()
        {
            ScenarioContext.Current.Should().ContainKey("bankAccountId");
            var bankAccountId = ScenarioContext.Current.Get<Guid>("bankAccountId");

            var result = AdminApiProxy.GetBankAccountInPlayerManager(bankAccountId);

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        [Then(@"Current bank account for player is visible to me")]
        public void ThenCurrentBankAccountForPlayerIsVisibleToMe()
        {
            ScenarioContext.Current.Should().ContainKey("playerId");
            var playerId = ScenarioContext.Current.Get<Guid>("playerId");

            var result = AdminApiProxy.GetCurrentBankAccountInPlayerManager(playerId);

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        [Then(@"Player bank account is successfully added")]
        public void ThenPlayerBankAccountIsSuccessfullyAdded()
        {
            ScenarioContext.Current.Should().ContainKey("playerId");
            ScenarioContext.Current.Should().ContainKey("bankAccountId");
            var playerId = ScenarioContext.Current.Get<Guid>("playerId");
            var bankAccountId = ScenarioContext.Current.Get<Guid>("bankAccountId");

            var data = new EditPlayerBankAccountData
            {
                Id = Guid.NewGuid(),
                PlayerId = playerId,
                Bank = bankAccountId,
                AccountName = TestDataGenerator.GetRandomString(),
                AccountNumber = TestDataGenerator.GetRandomString(10, "1234567890"),
                Province = TestDataGenerator.GetRandomString(),
                City = TestDataGenerator.GetRandomString(),
                Branch = TestDataGenerator.GetRandomString(),
                SwiftCode = TestDataGenerator.GetRandomString(),
                Address = TestDataGenerator.GetRandomString()
            };

            var result = AdminApiProxy.SaveBankAccountInPlayerManager(data);

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        [Then(@"Current bank account is successfully set")]
        public void ThenCurrentBankAccountIsSuccessfullySet()
        {
            ScenarioContext.Current.Should().ContainKey("bankAccountId");
            var bankAccountId = ScenarioContext.Current.Get<Guid>("bankAccountId");

            var data = new SetCurrentPlayerBankAccountData
            {
                PlayerBankAccountId = bankAccountId
            };

            var result = AdminApiProxy.SetCurrentBankAccountInPlayerManager(data);

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        [When(@"New activity log is created")]
        public void WhenNewActivityLogIsCreated()
        {
            ScenarioContext.Current.Should().ContainKey("playerId");
            var playerId = ScenarioContext.Current.Get<Guid>("playerId");

            var @event = new DepositSubmitted
            {
                PlayerId = playerId,
                Amount = 1
            };

            PlayerHelper.AddActivityLog("Deposit submitted", @event, playerId, string.Empty, string.Empty);
        }

        [Then(@"Activity log remark is successfully edited")]
        public void ThenActivityLogRemarkIsSuccessfullyEdited()
        {
            var record = PlayerQueries.GetPlayerActivityLog();

            var data = new EditLogRemarkData
            {
                LogId = record.First().Id,
                Remarks = TestDataGenerator.GetRandomString()
            };

            var result = AdminApiProxy.EditLogRemarkInPlayerInfo(data);

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        [Then(@"Transaction types are visible to me")]
        public void ThenTransactionTypesAreVisibleToMe()
        {
            var result = AdminApiProxy.GetTransactionTypesInPlayerInfo();

            result.Should().NotBeNull();
        }

        [Then(@"Identification document edit data is visible to me")]
        public void ThenIdentificationDocumentEditDataIsVisibleToMe()
        {
            ScenarioContext.Current.Should().ContainKey("playerId");
            var playerId = ScenarioContext.Current.Get<Guid>("playerId");

            var result = AdminApiProxy.GetIdentificationDocumentEditDataInPlayerInfo(playerId);

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        [Then(@"Player data is successfully edited")]
        public void ThenPlayerDataIsSuccessfullyEdited()
        {
            ScenarioContext.Current.Should().ContainKey("playerId");
            var playerId = ScenarioContext.Current.Get<Guid>("playerId");

            var player = PlayerQueries.GetPlayer(playerId);

            var data = new EditPlayerData
            {
                PlayerId = player.Id,
                FirstName = player.FirstName,
                LastName = player.LastName,
                MailingAddressCity = TestDataGenerator.GetRandomString(),
                MailingAddressPostalCode = TestDataGenerator.GetRandomString(),
                CountryCode = player.CountryCode,
                Email = TestDataGenerator.GetRandomEmail(),
                DateOfBirth = "1980/01/01",
                MailingAddressLine1 = TestDataGenerator.GetRandomString(),
                PhoneNumber = player.PhoneNumber,
                PaymentLevelId = new Guid(),
                Title = (PlayerEnums.Title?) Title.Mr,
                Gender = (PlayerEnums.Gender?) Gender.Male,
                AccountAlertEmail = true,
                AccountAlertSms = true,
                MarketingAlertEmail = true,
                MarketingAlertPhone = true,
                MarketingAlertSms = true
            };

            var result = AdminApiProxy.EditPlayerInfo(data);

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        [Then(@"Exemption is successfully submitted")]
        public void ThenExemptionIsSuccessfullySubmitted()
        {
            ScenarioContext.Current.Should().ContainKey("playerId");
            var playerId = ScenarioContext.Current.Get<Guid>("playerId");

            var exemption = new Exemption
            {
                PlayerId = playerId,
                Exempt = true,
                ExemptFrom = DateTime.Now.ToString(CultureInfo.InvariantCulture),
                ExemptTo = DateTime.Now.ToString(CultureInfo.InvariantCulture),
                ExemptLimit = 1
            };

            var result = AdminApiProxy.SubmitExemptionInPlayerInfo(exemption);

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        [Then(@"Player status is successfully set")]
        public void ThenPlayerStatusIsSuccessfullySet()
        {
            ScenarioContext.Current.Should().ContainKey("playerId");
            var playerId = ScenarioContext.Current.Get<Guid>("playerId");

            var data = new SetStatusData
            {
                Id = playerId,
                Active = false
            };

            var result = AdminApiProxy.SetStatusInPlayerInfo(data);

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        [Then(@"Activation email is successfully resent")]
        public void ThenActivationEmailIsSuccessfullyResent()
        {
            ScenarioContext.Current.Should().ContainKey("playerId");
            var playerId = ScenarioContext.Current.Get<Guid>("playerId");

            var data = new ResendActivationEmailData
            {
                Id = playerId
            };

            var result = AdminApiProxy.ResendActivationEmailInPlayerInfo(data);

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        [Then(@"I can not execute protected player methods with insufficient permissions")]
        public void ThenICanNotExecuteProtectedPlayerMethodsWithInsufficientPermissions()
        {
            LogWithNewUser(Modules.AdminActivityLog, Permissions.View);

            const int statusCode = (int)HttpStatusCode.Forbidden;

            Assert.That(Assert.Throws<HttpException>(() => AdminApiProxy.ChangeVipLevelInPlayerManager(new ChangeVipLevelData())).GetHttpCode(), Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<HttpException>(() => AdminApiProxy.ChangePaymnetLevelInPlayerManager(new ChangePaymentLevelData())).GetHttpCode(), Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<HttpException>(() => AdminApiProxy.ChangePlayersPaymentLevelInPlayerManager(new ChangePlayersPaymentLevelData())).GetHttpCode(), Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<HttpException>(() => AdminApiProxy.AddPlayerInPlayerManager(new AddPlayerData())).GetHttpCode(), Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<HttpException>(() => AdminApiProxy.GetBankAccountInPlayerManager(new Guid())).GetHttpCode(), Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<HttpException>(() => AdminApiProxy.SaveBankAccountInPlayerManager(new EditPlayerBankAccountData())).GetHttpCode(), Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<HttpException>(() => AdminApiProxy.SetCurrentBankAccountInPlayerManager(new SetCurrentPlayerBankAccountData())).GetHttpCode(), Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<HttpException>(() => AdminApiProxy.EditPlayerInfo(new EditPlayerData())).GetHttpCode(), Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<HttpException>(() => AdminApiProxy.SubmitExemptionInPlayerInfo(new Exemption())).GetHttpCode(), Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<HttpException>(() => AdminApiProxy.SetStatusInPlayerInfo(new SetStatusData())).GetHttpCode(), Is.EqualTo(statusCode));
        }
    }
}