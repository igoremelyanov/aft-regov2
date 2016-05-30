using System;
using System.Linq;
using AFT.RegoV2.Core.Auth.Interface;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Payment.Data;
using AFT.RegoV2.Core.Payment.Interface.ApplicationServices;
using AFT.RegoV2.Core.Payment.Interface.Data.Commands;
using AFT.RegoV2.Core.Payment.Validators;
using AFT.RegoV2.Core.Security.Data.Users;
using AFT.RegoV2.Core.Payment;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;
using FluentAssertions;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Payment
{
    class PaymentGatewaySettingsTests : AdminWebsiteUnitTestsBase
    {
        private IPaymentGatewaySettingsCommands _commands;
        private IPaymentGatewaySettingsQueries _queries;
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
            _commands = Container.Resolve<IPaymentGatewaySettingsCommands>();
            _queries = Container.Resolve<IPaymentGatewaySettingsQueries>();
        }

        [Test]
        public void Can_add_paymentGatewaySettings()
        {
            // Arrange
            var brand = Container.Resolve<BrandTestHelper>().CreateBrand(isActive: true);

            // Act         
            var addData = new SavePaymentGatewaysSettingsData
            {                
                Brand = brand.Id,
                OnlinePaymentMethodName = "TEST PAYMENT NAME1",
                PaymentGatewayName = "XXPAY",
                Channel = 99,
                EntryPoint = "http://test.com/payment/issue",
                Remarks = "ADD NEW SETTING REMARK"
            };
            // Act
            var response = _commands.Add(addData);

            //Assert            
            response.PaymentGatewaySettingsId.Should().NotBeEmpty();

            var newSettingId = response.PaymentGatewaySettingsId;
            var settings = _paymentRepository.PaymentGatewaySettings.Single(x => x.Id == newSettingId);

            settings.Should().NotBeNull();
            settings.BrandId.Should().NotBeEmpty();
            settings.OnlinePaymentMethodName.ShouldBeEquivalentTo("TEST PAYMENT NAME1");
            settings.PaymentGatewayName.ShouldBeEquivalentTo("XXPAY");
            settings.Channel.ShouldBeEquivalentTo(99);            
            settings.EntryPoint.ShouldBeEquivalentTo("http://test.com/payment/issue");
            settings.Remarks.ShouldBeEquivalentTo("ADD NEW SETTING REMARK");
            settings.Status.Should().Be(Status.Inactive);
            settings.CreatedBy.ShouldBeEquivalentTo(_actorInfoProvider.Actor.UserName);            
            settings.DateCreated.Should().BeCloseTo(DateTime.Now, 60000);
            settings.UpdatedBy.Should().BeNull();
            settings.DateUpdated.Should().NotHaveValue();
            settings.ActivatedBy.Should().BeNull();
            settings.DateActivated.Should().NotHaveValue();            
            settings.DeactivatedBy.Should().BeNull();
            settings.DateDeactivated.Should().NotHaveValue();
        }

        [Test]
        public void Can_edit_paymentGatewaySettings()
        {            
            // Arrange
            var brand = Container.Resolve<BrandTestHelper>().CreateBrand(isActive: true);
          
            var paymentGatewaySettings = Container.Resolve<PaymentTestHelper>().CreatePaymentGatewaySettings(                
                brand.Id,
                enable: false,
                onlinePaymentMethodName: "TEST PAYMENT NAME1",
                paymentGatewayName: "XXPAY",
                channel: 99,
                entryPoint: "http://test.com/payment/issue",
                remarks: "ADD SETTING REMARK");


            var editData = new SavePaymentGatewaysSettingsData
            {
                Id = paymentGatewaySettings.Id,
                Brand = paymentGatewaySettings.BrandId,
                OnlinePaymentMethodName = "Modified Name",
                PaymentGatewayName = "YYPAY",
                Channel = 100,
                EntryPoint = "http://changedDomain.com/payment/issue",
                Remarks = "Edit SETTING REMARK"
            };
            // Act
            var response = _commands.Edit(editData);

            //Assert            
            response.PaymentGatewaySettingsId.Should().Be(paymentGatewaySettings.Id);

            var settings = _paymentRepository.PaymentGatewaySettings.Single(x => x.Id == paymentGatewaySettings.Id);
            settings.Should().NotBeNull();
            settings.BrandId.Should().NotBeEmpty();
            settings.OnlinePaymentMethodName.ShouldBeEquivalentTo("Modified Name");
            settings.PaymentGatewayName.ShouldBeEquivalentTo("YYPAY");
            settings.Channel.ShouldBeEquivalentTo(100);
            settings.EntryPoint.ShouldBeEquivalentTo("http://changedDomain.com/payment/issue");
            settings.Remarks.ShouldBeEquivalentTo("Edit SETTING REMARK");         
            settings.Status.Should().Be(Status.Inactive);            
            settings.UpdatedBy.ShouldBeEquivalentTo(_actorInfoProvider.Actor.UserName);                      
            settings.DateUpdated.Should().BeCloseTo(DateTime.Now, 60000);
            settings.ActivatedBy.Should().BeNull();
            settings.DateActivated.Should().NotHaveValue();
            settings.DeactivatedBy.Should().BeNull();
            settings.DateDeactivated.Should().NotHaveValue();
        }

        [Test]
        public void Can_activate_paymentGatewaySettings()
        {
            var brand = Container.Resolve<BrandTestHelper>().CreateBrand(isActive: true);

            // Arrange
            var repositorySettings = new PaymentGatewaySettings 
            { 
                Id = new Guid("50797305-5320-4086-BA1C-9D062DDF53D9"),
                BrandId =brand.Id
            };
            _paymentRepository.PaymentGatewaySettings.Add(repositorySettings);

            // Act
            _commands.Activate(new ActivatePaymentGatewaySettingsData
            {
                Id=repositorySettings.Id,
                Remarks = "remark"
            });

            //Assert
            var settings = _paymentRepository.PaymentGatewaySettings.Single(x => x.Id == repositorySettings.Id);
            settings.Status.Should().Be(Status.Active);
            settings.ActivatedBy.ShouldBeEquivalentTo(_actorInfoProvider.Actor.UserName);
            settings.DateActivated.Should().BeCloseTo(DateTime.Now, 5000);
        }

        [Test]
        public void Can_deactivate_paymentGatewaySettings()
        {
            // Arrange
            var brand = Container.Resolve<BrandTestHelper>().CreateBrand(isActive: true);
            
            var repositorySettings = new PaymentGatewaySettings
            {
                Id = new Guid("50797305-5320-4086-BA1C-9D062DDF53D9"),
                BrandId = brand.Id,
                Status = Status.Active
            };
            _paymentRepository.PaymentGatewaySettings.Add(repositorySettings);

            // Act
            _commands.Deactivate(new DeactivatePaymentGatewaySettingsData
            {
                Id=repositorySettings.Id,
                Remarks = "remark"
            }); 

            //Assert
            var settings = _paymentRepository.PaymentGatewaySettings.Single(x => x.Id == repositorySettings.Id);
            settings.Status.Should().Be(Status.Inactive);
            settings.DeactivatedBy.ShouldBeEquivalentTo(_actorInfoProvider.Actor.UserName);
            settings.DateDeactivated.Should().BeCloseTo(DateTime.Now, 5000);
        }

        [Test]
        public void Can_get_paymentGatewaySettings()
        {
            // Arrange
            var brand = new AFT.RegoV2.Core.Payment.Data.Brand
            {
                Id=new Guid("00000000-0000-0000-0000-000000000138"),
                LicenseeId = new Guid("4A557EA9-E6B7-4F1F-AEE5-49E170ADB7E0")

            };
            var repositorySettings = new PaymentGatewaySettings
            {
                Id = new Guid("50797305-5320-4086-BA1C-9D062DDF53D9"),
                Brand=brand,                
                OnlinePaymentMethodName = "Name",
                PaymentGatewayName = "XXPAY",
                Channel = 199,
                Remarks = "Remark",
                EntryPoint = "http://test.domain.com",
                Status = Status.Active,
                CreatedBy = "CreatedBy",
                DateCreated = DateTime.Now,
                UpdatedBy = "UpdatedBy",
                DateUpdated = DateTime.Now,
                ActivatedBy = "ActivatedBy",
                DateActivated = DateTime.Now,
                DeactivatedBy = "DeactivatedBy",
                DateDeactivated = DateTime.Now
            };
            _paymentRepository.PaymentGatewaySettings.Add(repositorySettings);

            // Act
            var data =_queries.GetPaymentGatewaySettingsById(repositorySettings.Id);

            //Assert       
            data.Id.Should().Be(repositorySettings.Id);
            data.BrandId.Should().Be(repositorySettings.Brand.Id);
            data.LicenseeId.Should().Be(repositorySettings.Brand.LicenseeId);
            data.OnlinePaymentMethodName.Should().Be(repositorySettings.OnlinePaymentMethodName);
            data.PaymentGatewayName.Should().Be(repositorySettings.PaymentGatewayName);
            data.Channel.Should().Be(repositorySettings.Channel);
            data.Remarks.Should().Be(repositorySettings.Remarks);
            data.EntryPoint.Should().Be(repositorySettings.EntryPoint);
            data.Status.Should().Be(repositorySettings.Status);
            data.CreatedBy.Should().Be(repositorySettings.CreatedBy);
            data.DateCreated.Should().Be(repositorySettings.DateCreated);
            data.UpdatedBy.Should().Be(repositorySettings.UpdatedBy);
            data.DateUpdated.Should().Be(repositorySettings.DateUpdated);
            data.ActivatedBy.Should().Be(repositorySettings.ActivatedBy);
            data.DateActivated.Should().Be(repositorySettings.DateActivated);
            data.DeactivatedBy.Should().Be(repositorySettings.DeactivatedBy);
            data.DateDeactivated.Should().Be(repositorySettings.DateDeactivated);          
        }

        [Test]
        public void Can_get_payment_gatways()
        {         
            // Act
            var result = _queries.GetPaymentGateways();

            //Assert       
            result.Should().NotBeNull();
        }

        [Test]
        public void Should_return_false_if_onlinePaymentMethodName_existed()
        {
            // Arrange      
            var brand = Container.Resolve<BrandTestHelper>().CreateBrand(isActive: true);

            var repositorySettings = new PaymentGatewaySettings
            {
                Id = new Guid("50797305-5320-4086-BA1C-9D062DDF53D9"),
                BrandId = brand.Id,
                OnlinePaymentMethodName = "OnlinePaymentMethodName",
                Status = Status.Active
            };
            _paymentRepository.PaymentGatewaySettings.Add(repositorySettings);

            var savePaymentSettingsCommand = new SavePaymentGatewaysSettingsData
            {
                Brand = brand.Id,
                Channel = 1,
                PaymentGatewayName = "PG-NAME",
                OnlinePaymentMethodName = "OnlinePaymentMethodName",
                EntryPoint = "http://domain.com"
            };            

            // Act
            var response=_commands.ValidateThatPaymentGatewaySettingsCanBeAdded(savePaymentSettingsCommand);

            //Assert
            response.IsValid.Should().Be(false);
            response.Errors.First().ErrorMessage.Should().Be("OnlinePaymentMethodNameAlreadyExists");
        }

        [Test]
        public void Should_return_false_if_the_same_setting_existed()
        {
            // Arrange      
            var brand = Container.Resolve<BrandTestHelper>().CreateBrand(isActive: true);

            var repositorySettings = new PaymentGatewaySettings
            {
                Id = new Guid("50797305-5320-4086-BA1C-9D062DDF53D9"),
                BrandId = brand.Id,
                OnlinePaymentMethodName = "OnlinePaymentMethodName",
                PaymentGatewayName = "XPAY",
                Channel = 1,
                Status = Status.Active
            };
            _paymentRepository.PaymentGatewaySettings.Add(repositorySettings);

            var savePaymentSettingsCommand = new SavePaymentGatewaysSettingsData
            {
                Brand = brand.Id,                             
                OnlinePaymentMethodName = "OnlinePaymentMethodName2",
                PaymentGatewayName = "XPAY",
                Channel = 1,
                EntryPoint = "http://domain.com"
            };

            // Act
            var response = _commands.ValidateThatPaymentGatewaySettingsCanBeAdded(savePaymentSettingsCommand);

            //Assert
            response.IsValid.Should().Be(false);
            response.Errors.First().ErrorMessage.Should().Be("PaymentGatewaySettingAlreadyExists");
        }

        [Test]
        public void Should_return_false_if_onlinePaymentMethodName_invalid()
        {
            // Arrange      
            var brand = Container.Resolve<BrandTestHelper>().CreateBrand(isActive: true);
         
            var savePaymentSettingsCommand = new SavePaymentGatewaysSettingsData
            {
                Brand = brand.Id,
                PaymentGatewayName = "XPAY",
                Channel = 1,
                OnlinePaymentMethodName = "test%%",
                EntryPoint = "http://domain.com"                
            };

            // Act
            var response = _commands.ValidateThatPaymentGatewaySettingsCanBeAdded(savePaymentSettingsCommand);

            //Assert
            response.IsValid.Should().Be(false);
            response.Errors.First().ErrorMessage.Should().Be(PaymentGatewaySettingsErrors.AlphanumericSpaces.ToString());
        }

        [Test]
        public void Should_return_false_if_onlinePaymentMethodName_isnull()
        {
            // Arrange      
            var brand = Container.Resolve<BrandTestHelper>().CreateBrand(isActive: true);

            var savePaymentSettingsCommand = new SavePaymentGatewaysSettingsData
            {
                Brand = brand.Id,
                PaymentGatewayName = "XPAY",
                Channel = 1,
                EntryPoint = "http://domain.com"
            };

            // Act
            var response = _commands.ValidateThatPaymentGatewaySettingsCanBeAdded(savePaymentSettingsCommand);
            
            //Assert
            response.IsValid.Should().Be(false);
            response.Errors.First().ErrorMessage.Should().Be(PaymentGatewaySettingsErrors.RequiredField.ToString());
        }

        [Test]
        public void Can_add_the_same_onlinePaymentMethodName_in_different_brands()
        {
            // Arrange      
            var brand = Container.Resolve<BrandTestHelper>().CreateBrand(isActive: true);

            var repositorySettings = new PaymentGatewaySettings
            {
                Id = new Guid("50797305-5320-4086-BA1C-9D062DDF53D9"),
                BrandId = brand.Id,
                OnlinePaymentMethodName = "OnlinePaymentMethodName",
                Status = Status.Active
            };
            _paymentRepository.PaymentGatewaySettings.Add(repositorySettings);

            var brand2 = Container.Resolve<BrandTestHelper>().CreateBrand(isActive: true);
            var savePaymentSettingsCommand = new SavePaymentGatewaysSettingsData
            {
                Brand = brand2.Id,
                Channel = 1,
                PaymentGatewayName = "PG-NAME",
                OnlinePaymentMethodName = "OnlinePaymentMethodName",
                EntryPoint = "http://domain.com"
            };

            // Act
            var response = _commands.ValidateThatPaymentGatewaySettingsCanBeAdded(savePaymentSettingsCommand);

            //Assert
            response.IsValid.Should().Be(true); 
        }

        [Test]
        public void Can_not_activate_activated_paymentGatewaySettings()
        {
            // Arrange
            var repositorySettings = new PaymentGatewaySettings { Id = new Guid("50797305-5320-4086-BA1C-9D062DDF53D9") ,Status = Status.Active};
            _paymentRepository.PaymentGatewaySettings.Add(repositorySettings);

            // Act
            var response =_commands.ValidateThatPaymentGatewaySettingsCanBeActivated(new ActivatePaymentGatewaySettingsData
            {
                Id = repositorySettings.Id,
                Remarks = "remark"
            });

            //Assert
            response.IsValid.Should().BeFalse();
            response.Errors.FirstOrDefault().ErrorMessage.Should().Be(PaymentGatewaySettingsErrors.AlreadyActive.ToString());

        }

        [Test]
        public void Can_not_deactivate_inactivate_paymentGatewaySettings()
        {
            // Arrange
            var repositorySettings = new PaymentGatewaySettings
            {
                Id = new Guid("50797305-5320-4086-BA1C-9D062DDF53D9"),
                Status = Status.Inactive
            };
            _paymentRepository.PaymentGatewaySettings.Add(repositorySettings);

            // Act
            var response =_commands.ValidateThatPaymentGatewaySettingsCanBeDeactivated(new DeactivatePaymentGatewaySettingsData
            {
                Id = repositorySettings.Id,
                Remarks = "remark"
            });

            //Assert
            response.IsValid.Should().BeFalse();
            response.Errors.FirstOrDefault().ErrorMessage.Should().Be(PaymentGatewaySettingsErrors.NotActive.ToString());
        }

        [Test]
        public void Can_get_payment_gatway_settings_by_playerId()
        {
            // Arrange
            var playerId = GeneratePaymentGatewaySettinigsData();
            
            // Act
            var response = _queries.GetPaymentGatewaySettingsByPlayerId(playerId);

            //Assert
            response.Should().NotBeNull();
            var settings = response.FirstOrDefault();
            settings.Should().NotBeNull();
            settings.EntryPoint.Should().NotBeNullOrEmpty();
            settings.PaymentGatewayName.Should().NotBeNullOrEmpty();
            settings.Channel.Should().BeGreaterOrEqualTo(0);
        }

        private Guid GeneratePaymentGatewaySettinigsData()
        {            
            var brandTestHelper = Container.Resolve<BrandTestHelper>();
            var playerTestHelper = Container.Resolve<PlayerTestHelper>();
            var licensee = brandTestHelper.CreateLicensee();
            var brand = brandTestHelper.CreateBrand(licensee, isActive: true);
            var player = playerTestHelper.CreatePlayer(true, brand.Id);

            return player.Id;
        }                
    }
}
