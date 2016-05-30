using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Core.Common.Data.Player;
using AFT.RegoV2.Core.Common.Events;
using AFT.RegoV2.Core.Common.EventStore;
using AFT.RegoV2.Core.Payment;
using AFT.RegoV2.Core.Player;
using AFT.RegoV2.Core.Player.ApplicationServices;
using AFT.RegoV2.Core.Player.Validators;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;
using FluentAssertions;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Player
{
    public class ChangePaymentLevelTests : AdminWebsiteUnitTestsBase
    {
        private IPlayerRepository _playerRepository;
        private IPaymentRepository _paymentRepository;
        private SecurityTestHelper _securityTestHelper;
        private PaymentTestHelper _paymentTestHelper;
        private BrandTestHelper _brandTestHelper;
        private PlayerTestHelper _playerTestHelper;
        private PlayerCommands _playerCommands;
        public override void BeforeEach()
        {
            base.BeforeEach();
            _playerRepository = Container.Resolve<IPlayerRepository>();
            _paymentRepository = Container.Resolve<IPaymentRepository>();
            _securityTestHelper = Container.Resolve<SecurityTestHelper>();
            _securityTestHelper.PopulatePermissions();
            _securityTestHelper.CreateAndSignInSuperAdmin();

            _paymentTestHelper = Container.Resolve<PaymentTestHelper>();
            _brandTestHelper = Container.Resolve<BrandTestHelper>();
            _playerTestHelper = Container.Resolve<PlayerTestHelper>();
            _playerCommands = Container.Resolve<PlayerCommands>();
        }

        [Test]
        public void Can_change_payment_level()
        {
            //setup                             
            var brand = _brandTestHelper.CreateBrand();                       
            var player = _playerTestHelper.CreatePlayer();
            var paymentLevel = _paymentTestHelper.CreatePaymentLevel(brand.Id, player.CurrencyCode);

            //act
            _playerCommands.ChangePaymentLevel(new ChangePaymentLevelData
                {
                    PlayerId = player.Id,
                    PaymentLevelId = paymentLevel.Id, 
                    Remarks = "test"
                });

            //assert
            var playerData = _playerRepository.Players.FirstOrDefault(x => x.Id == player.Id);
            playerData.PaymentLevelId.Should().Be(paymentLevel.Id);

            var playerPaymentLevel = _paymentRepository.PlayerPaymentLevels.FirstOrDefault(x => x.PlayerId == player.Id);
            playerPaymentLevel.PaymentLevel.Id.Should().Be(paymentLevel.Id);

            var eventRepository = Container.Resolve<IEventRepository>();

            eventRepository.Events.Where(x => x.DataType == typeof (PlayerPaymentLevelChanged).Name).Should().NotBeEmpty();            
        }

        [Test]
        public void Can_not_change_inactivate_payment_level()
        {
            //setup
            var brand = _brandTestHelper.CreateBrand();
            var player = _playerTestHelper.CreatePlayer();
            var paymentLevel = _paymentTestHelper.CreatePaymentLevel(brand.Id, player.CurrencyCode,false);

            //act            
            var reseponse =_playerCommands.ValidatePlayerPaymentLevelCanBeChanged(new ChangePaymentLevelData
            {
                PlayerId = player.Id,
                PaymentLevelId = paymentLevel.Id,
                Remarks = "test"
            });

            //assert
            reseponse.IsValid.Should().BeFalse();
            reseponse.Errors[0].ErrorMessage.Should().Be(PaymentLevelErrors.PaymentLevelInactivate.ToString());
        }

        [Test]
        public void Can_not_change_payment_level_with_different_currencyCode()
        {
            //setup
            var currencies = new List<Core.Brand.Interface.Data.Currency>
            {
                _brandTestHelper.CreateCurrency("EUR", "EUR Dollar"),
                _brandTestHelper.CreateCurrency("CAD", "Canadian Dollar"),             
            };
            var license = _brandTestHelper.CreateLicensee(true, currencies: currencies);
            var brand = _brandTestHelper.CreateBrand(license);            
            var player = _playerTestHelper.CreatePlayer();//Player's currency is EUR
            _brandTestHelper.AssignLicenseeCurrency(brand.LicenseeId, "CAD");
            _brandTestHelper.AssignCurrency(brand.Id, "CAD");
            var paymentLevel = _paymentTestHelper.CreatePaymentLevel(brand.Id, "CAD");//Payment Level's currency is CAD

            //act            
            var reseponse = _playerCommands.ValidatePlayerPaymentLevelCanBeChanged(new ChangePaymentLevelData
            {
                PlayerId = player.Id,
                PaymentLevelId = paymentLevel.Id,
                Remarks = "test"
            });

            //assert
            reseponse.IsValid.Should().BeFalse();
            reseponse.Errors[0].ErrorMessage.Should().Be(PaymentLevelErrors.PaymentLevelAndPlayerNotMatch.ToString());
        }

        [Test]
        public void Can_not_change_payment_level_with_wrong_player_id()
        {
            //setup
            var brand = _brandTestHelper.CreateBrand();
            var player = _playerTestHelper.CreatePlayer();
            var paymentLevel = _paymentTestHelper.CreatePaymentLevel(brand.Id, player.CurrencyCode, false);

            //act            
            var reseponse = _playerCommands.ValidatePlayerPaymentLevelCanBeChanged(new ChangePaymentLevelData
            {
                PlayerId = new Guid(),
                PaymentLevelId = paymentLevel.Id,
                Remarks = "test"
            });

            //assert
            reseponse.IsValid.Should().BeFalse();
            reseponse.Errors[0].ErrorMessage.Should().Be(PaymentLevelErrors.PlayerNotFound.ToString());
        }

        [Test]
        public void Can_not_change_payment_level_with_wrong_id()
        {
            //setup
            var brand = _brandTestHelper.CreateBrand();
            var player = _playerTestHelper.CreatePlayer();
            var paymentLevel = _paymentTestHelper.CreatePaymentLevel(brand.Id, player.CurrencyCode, false);

            //act            
            var reseponse = _playerCommands.ValidatePlayerPaymentLevelCanBeChanged(new ChangePaymentLevelData
            {
                PlayerId = player.Id,
                PaymentLevelId = new Guid(),
                Remarks = "test"
            });

            //assert
            reseponse.IsValid.Should().BeFalse();
            reseponse.Errors[0].ErrorMessage.Should().Be(PaymentLevelErrors.PaymentLevelNotFound.ToString());
        }
    }
}
