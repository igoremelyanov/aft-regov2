using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Core.Payment.Interface.ApplicationServices;
using AFT.RegoV2.Core.Payment.Interface.Data;
using AFT.RegoV2.Core.Payment.Interface.Data.Commands;
using AFT.RegoV2.Core.Payment;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;
using AFT.RegoV2.Tests.Common.TestDoubles;
using Microsoft.Practices.Unity;
using NUnit.Framework;
using BrandCurrency = AFT.RegoV2.Core.Brand.Interface.Data.BrandCurrency;

namespace AFT.RegoV2.Tests.Unit.Payment
{
    internal class PaymentLevelTests : AdminWebsiteUnitTestsBase
    {
        private static readonly Guid BrandId = new Guid("00000000-0000-0000-0000-000000000138");
        private IPaymentRepository _paymentRepository;
        private IPaymentLevelCommands _paymentLevelCommands;
        private FakeBrandRepository _brandRepository;

        public override void BeforeEach()
        {
            base.BeforeEach();

            _paymentRepository = Container.Resolve<FakePaymentRepository>();

            _paymentRepository.Brands.Add(new Core.Payment.Data.Brand
            {
                Id = BrandId,
                Name = BrandId.ToString(),
                Code = "138",
                TimezoneId = "Pacific Standard Time"
            });

            _brandRepository = Container.Resolve<FakeBrandRepository>();

            _brandRepository.Brands.Add(new Core.Brand.Interface.Data.Brand
            {
                Id = BrandId,
                Name = BrandId.ToString(),
                Code = "138",
                TimezoneId = "Pacific Standard Time",
                BrandCurrencies = new List<BrandCurrency>()
                {
                    new BrandCurrency { BrandId = BrandId, CurrencyCode = "CAD" }
                }
            });
            
            _paymentLevelCommands = Container.Resolve<IPaymentLevelCommands>();

            var securityTestHelper = Container.Resolve<SecurityTestHelper>();
            securityTestHelper.PopulatePermissions();
            securityTestHelper.CreateAndSignInSuperAdmin();
        }

        [Test]
        public void Can_not_create_two_default_vip_levels()
        {
            var paymentLevel = CreatePaymentLevelData();
            _paymentLevelCommands.Save(paymentLevel);

            paymentLevel = CreatePaymentLevelData();
            try
            {
                _paymentLevelCommands.Save(paymentLevel);
            }
            catch (RegoException ex)
            {
                Assert.True(ex.Message == "Default payment level for the brand and currency combination already exists.");
                return;
            }

            Assert.Fail();
        }

        [Test]
        public void Can_activate_payment_level()
        {
            var createPaymentLevelData = CreatePaymentLevelData();

            var id = _paymentLevelCommands.Save(createPaymentLevelData).PaymentLevelId;

            var paymentLevel = _paymentRepository.PaymentLevels.Single(x => x.Id == id);

            paymentLevel.Status = PaymentLevelStatus.Inactive;
            var activatePaymentLevelCommand = new ActivatePaymentLevelCommand {Id = id};
            var canActivate = _paymentLevelCommands.ValidatePaymentLevelCanBeActivated(activatePaymentLevelCommand);
            
            Assert.True(canActivate.IsValid);
            
            _paymentLevelCommands.Activate(activatePaymentLevelCommand);            
            var newStatus = _paymentRepository.PaymentLevels.Single(x => x.Id == id).Status;
            
            Assert.That(newStatus, Is.EqualTo(PaymentLevelStatus.Active));
        }

        [Test]
        public void Cannot_activate_invalid_payment_level()
        {
            var activatePaymentLevelCommand = new ActivatePaymentLevelCommand {Id = Guid.NewGuid()};
            var canActivate = _paymentLevelCommands.ValidatePaymentLevelCanBeActivated(activatePaymentLevelCommand);

            Assert.That(canActivate.IsValid, Is.False);

            var id = _paymentLevelCommands.Save(CreatePaymentLevelData()).PaymentLevelId;

            activatePaymentLevelCommand = new ActivatePaymentLevelCommand {Id = id};
            canActivate = _paymentLevelCommands.ValidatePaymentLevelCanBeActivated(activatePaymentLevelCommand);

            Assert.That(canActivate.IsValid, Is.False);
        }

        [Test]
        public void Can_deactivate_payment_level()
        {
            var createPaymentLevelData = CreatePaymentLevelData();
            var id = _paymentLevelCommands.Save(createPaymentLevelData).PaymentLevelId;

            var deactivatePaymentLevelCommand = new DeactivatePaymentLevelCommand {Id = id};

            var canDeactivate = _paymentLevelCommands.ValidatePaymentLevelCanBeDeactivated(deactivatePaymentLevelCommand);

            Assert.That(canDeactivate.IsValid, Is.True);

            _paymentLevelCommands.Deactivate(deactivatePaymentLevelCommand);

            var newStatus = _paymentRepository.PaymentLevels.Single(x => x.Id == id).Status;

            Assert.That(newStatus, Is.EqualTo(PaymentLevelStatus.Inactive));

        }

        [Test]
        public void Cannot_deactivate_invalid_payment_level()
        {
            var createPaymentLevelData = CreatePaymentLevelData();
            var id = _paymentLevelCommands.Save(createPaymentLevelData).PaymentLevelId;
            _paymentRepository.PaymentLevels.Single(x => x.Id == id).Status = PaymentLevelStatus.Inactive;
                
            var deactivatePaymentLevelCommand = new DeactivatePaymentLevelCommand { Id = id };

            var canDeactivate = _paymentLevelCommands.ValidatePaymentLevelCanBeDeactivated(deactivatePaymentLevelCommand);

            Assert.That(canDeactivate.IsValid, Is.False);
        }

        private EditPaymentLevel CreatePaymentLevelData()
        {
            var data = new EditPaymentLevel
            {             
                Id = Guid.NewGuid(),
                Brand = BrandId,
                Name = TestDataGenerator.GetRandomString(6, "0123456789ABCDEF"),
                Code = TestDataGenerator.GetRandomString(6, "0123456789ABCDEF"),
                Currency = "CAD",
                EnableOfflineDeposit = true,
                EnableOnlineDeposit = true,
                IsDefault = true
            };

            return data;
        }
    }
}
