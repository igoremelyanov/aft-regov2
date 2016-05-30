using System;
using System.Linq;
using AFT.RegoV2.Core.Auth.Interface;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Payment;
using AFT.RegoV2.Core.Payment.Interface.ApplicationServices;
using AFT.RegoV2.Core.Payment.Interface.Data.Commands;
using AFT.RegoV2.Core.Security.Data.Users;
using AFT.RegoV2.Core.Payment;
using AFT.RegoV2.Core.Payment.Data;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;
using FluentAssertions;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Payment
{
    class PaymentSettingsTests : AdminWebsiteUnitTestsBase
    {
        private IPaymentSettingsCommands _commands;
        private IPaymentRepository _paymentRepository;
        private IActorInfoProvider _actorInfoProvider;

        public override void BeforeEach()
        {
            base.BeforeEach();
            _paymentRepository = Container.Resolve<IPaymentRepository>();
            _actorInfoProvider = Container.Resolve<IActorInfoProvider>();
            var securityTestHelper = Container.Resolve<SecurityTestHelper>();
            securityTestHelper.PopulatePermissions();
            var admin = securityTestHelper.CreateSuperAdmin();
            admin.AllowedBrands.Add(new BrandId());
            securityTestHelper.SignInAdmin(admin);
            _commands = Container.Resolve<IPaymentSettingsCommands>();
        }

        [Test]
        public void Can_add_payment_settings()
        {
            // Arrange
            var brand = Container.Resolve<BrandTestHelper>().CreateBrand(isActive: true);

            // Act
            var settings = Container.Resolve<PaymentTestHelper>().CreatePaymentSettings(brand, PaymentType.Deposit);

            //Assert
            settings.Should().NotBeNull();
            settings.BrandId.Should().NotBeEmpty();
            settings.PaymentType.ShouldBeEquivalentTo(PaymentType.Deposit);
            settings.VipLevel.ShouldBeEquivalentTo(brand.DefaultVipLevelId.ToString());
            settings.CurrencyCode.ShouldBeEquivalentTo(brand.BrandCurrencies.First().CurrencyCode);
            settings.PaymentMethod.Should().NotBeNull();
            settings.MinAmountPerTransaction.ShouldBeEquivalentTo(10);
            settings.MaxAmountPerTransaction.ShouldBeEquivalentTo(200);
            settings.MaxAmountPerDay.ShouldBeEquivalentTo(0);
            settings.MaxTransactionPerDay.ShouldBeEquivalentTo(10);
            settings.MaxTransactionPerWeek.ShouldBeEquivalentTo(20);
            settings.MaxTransactionPerMonth.ShouldBeEquivalentTo(30);
            settings.Enabled.Should().Be(Status.Active);
            settings.CreatedBy.ShouldBeEquivalentTo(_actorInfoProvider.Actor.UserName);
            settings.CreatedDate.Should().BeCloseTo(DateTime.Now, 60000);
            settings.UpdatedBy.Should().BeNull();
            settings.UpdatedDate.Should().NotHaveValue();
            settings.EnabledBy.ShouldBeEquivalentTo(_actorInfoProvider.Actor.UserName);
            settings.EnabledDate.Should().BeCloseTo(DateTime.Now, 60000);
            settings.DisabledBy.Should().BeNull();
            settings.DisabledDate.Should().NotHaveValue();
        }

        [Test]
        public void Should_throw_exception_if_no_paymentMethod()
        {
            // Arrange
            var savePaymentSettingsCommand = new SavePaymentSettingsCommand();
            //Add to pass Validator
            savePaymentSettingsCommand.MaxAmountPerTransaction = 1;
            savePaymentSettingsCommand.PaymentGatewayMethod = PaymentMethod.OfflineBank;
            // Act
            Action action = () => _commands.AddSettings(savePaymentSettingsCommand);

            //Assert
            action.ShouldThrow<Exception>().WithMessage("PaymentMethodIsRequired");
        }

        [Test]
        public void Enable_payment_settings_test()
        {
            // Arrange
            var paymentSettings = new PaymentSettings
            {
                Id = new Guid("50797305-5320-4086-BA1C-9D062DDF53D9"),
                Enabled = Status.Inactive
            };
            _paymentRepository.PaymentSettings.Add(paymentSettings);

            // Act
            _commands.Enable(paymentSettings.Id, "remark");

            //Assert
            var settings = _paymentRepository.PaymentSettings.Single(x => x.Id == paymentSettings.Id);
            settings.Enabled.Should().Be(Status.Active);
            settings.EnabledBy.ShouldBeEquivalentTo(_actorInfoProvider.Actor.UserName);
            settings.EnabledDate.Should().BeCloseTo(DateTime.Now, 5000);
        }

        [Test]
        public void Disable_payment_settings_test()
        {
            // Arrange
            var paymentSettings = new PaymentSettings
            {
                Id = new Guid("50797305-5320-4086-BA1C-9D062DDF53D9"),
                Enabled = Status.Active
            };
            _paymentRepository.PaymentSettings.Add(paymentSettings);

            // Act
            _commands.Disable(paymentSettings.Id, "remark");

            //Assert
            var settings = _paymentRepository.PaymentSettings.Single(x => x.Id == paymentSettings.Id);
            settings.Enabled.Should().Be(Status.Inactive);
            settings.DisabledBy.ShouldBeEquivalentTo(_actorInfoProvider.Actor.UserName);
            settings.DisabledDate.Should().BeCloseTo(DateTime.Now, 5000);
        }

        [Test]
        public void Update_payment_settings_limits_test()
        {
            // Arrange
            var paymentSettings = new PaymentSettings {Id = new Guid("50797305-5320-4086-BA1C-9D062DDF53D9")};
            _paymentRepository.PaymentSettings.Add(paymentSettings);

            var savePaymentSettingsCommand = new SavePaymentSettingsCommand();
            savePaymentSettingsCommand.Id = paymentSettings.Id;
            savePaymentSettingsCommand.MinAmountPerTransaction = 10;
            savePaymentSettingsCommand.MaxAmountPerTransaction = 20;
            savePaymentSettingsCommand.MaxAmountPerDay = 30;
            savePaymentSettingsCommand.MaxTransactionPerDay = 40;
            savePaymentSettingsCommand.MaxTransactionPerWeek = 50;
            savePaymentSettingsCommand.MaxTransactionPerMonth = 60;
            savePaymentSettingsCommand.PaymentMethod = "XPAY";
            savePaymentSettingsCommand.PaymentGatewayMethod = PaymentMethod.Online;
            // Act
            _commands.UpdateSettings(savePaymentSettingsCommand);

            //Assert
            var settings = _paymentRepository.PaymentSettings.Single(x => x.Id == paymentSettings.Id);
            settings.MinAmountPerTransaction.ShouldBeEquivalentTo(10);
            settings.MaxAmountPerTransaction.ShouldBeEquivalentTo(20);
            settings.MaxAmountPerDay.ShouldBeEquivalentTo(30);
            settings.MaxTransactionPerDay.ShouldBeEquivalentTo(40);
            settings.MaxTransactionPerWeek.ShouldBeEquivalentTo(50);
            settings.MaxTransactionPerMonth.ShouldBeEquivalentTo(60);
            settings.UpdatedBy.ShouldBeEquivalentTo(_actorInfoProvider.Actor.UserName);
            settings.UpdatedDate.Should().BeCloseTo(DateTime.Now, 5000);
        }

        [Test]
        public void Can_not_enable_the_same_online_settings()
        {
            // Arrange
            var activatedPaymentSettings = new PaymentSettings
            {
                Id = new Guid("50797305-5320-4086-BA1C-9D062DDF53B9"),
                PaymentType = PaymentType.Deposit,
                PaymentGatewayMethod = PaymentMethod.Online,
                PaymentMethod ="XPay",
                CurrencyCode = "RMB",
                Enabled = Status.Active
            };
            var paymentSettings = new PaymentSettings
            {
                Id = new Guid("50797305-5320-4086-BA1C-9D062DDF53A9"),
                PaymentType = PaymentType.Deposit,
                PaymentGatewayMethod = PaymentMethod.Online,
                PaymentMethod = "XPay",
                CurrencyCode = "RMB",
                Enabled = Status.Inactive
            };
            _paymentRepository.PaymentSettings.Add(activatedPaymentSettings);
            _paymentRepository.PaymentSettings.Add(paymentSettings);

            // Act
            Action action = () => _commands.Enable(paymentSettings.Id, "remark");

            //Assert
            action.ShouldThrow<Exception>().WithMessage("TheSameSettingsActivated");
        }

        [Test]
        public void Can_not_enable_the_same_offline_settings()
        {
            // Arrange
            var activatedPaymentSettings = new PaymentSettings
            {
                Id = new Guid("50797305-5320-4086-BA1C-9D062DDF53E9"),
                PaymentType = PaymentType.Deposit,
                PaymentGatewayMethod = PaymentMethod.OfflineBank,
                CurrencyCode = "RMB",
                Enabled = Status.Active
            };
            var paymentSettings = new PaymentSettings
            {
                Id = new Guid("50797305-5320-4086-BA1C-9D062DDF53F9"),
                PaymentType = PaymentType.Deposit,
                PaymentGatewayMethod = PaymentMethod.OfflineBank,
                CurrencyCode = "RMB",
                Enabled = Status.Inactive
            };
            _paymentRepository.PaymentSettings.Add(activatedPaymentSettings);
            _paymentRepository.PaymentSettings.Add(paymentSettings);

            // Act
            Action action = () => _commands.Enable(paymentSettings.Id, "remark");

            //Assert
            action.ShouldThrow<Exception>().WithMessage("TheSameSettingsActivated");
        }
    }
}
