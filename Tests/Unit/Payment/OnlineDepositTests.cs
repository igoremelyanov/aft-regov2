using System;
using System.Linq;
using System.Threading.Tasks;
using AFT.RegoV2.Core.Auth.Interface;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Payment;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Payment.Data;
using AFT.RegoV2.Core.Payment.Interface.Data;
using AFT.RegoV2.Core.Payment.Interface.ApplicationServices;
using AFT.RegoV2.Core.Payment.Interface.Exceptions;
using AFT.RegoV2.Core.Payment;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;
using AFT.RegoV2.Tests.Common.TestDoubles;
using AFT.RegoV2.Core.Payment.ApplicationServices;
using FluentAssertions;
using Microsoft.Practices.Unity;
using NUnit.Framework;
using OnlineDepositData=AFT.RegoV2.Core.Payment.Data.OnlineDeposit ;
using PaymentGatewaySettings = AFT.RegoV2.Core.Payment.Data.PaymentGatewaySettings;
using PaymentSettings = AFT.RegoV2.Core.Payment.Data.PaymentSettings;

namespace AFT.RegoV2.Tests.Unit.Payment
{
    class OnlineDepositTests : MemberWebsiteUnitTestsBase
    {
        private IOnlineDepositCommands _depositCommands;
        private IOnlineDepositQueries _depositQueries;
        private OnlineDepositData _processingOnlineDeposit;
        private OnlineDepositData _approvedOnlineDeposit;
        private FakePaymentRepository _paymentRepositoryMock;
        private Core.Payment.Data.Player _testPlayer;
        private PaymentGatewaySettings _testPaymentGatewaySettings;
        private PaymentTestHelper _paymentTestHelper;
        private IActorInfoProvider _actorInfoProvider;
        private Core.Brand.Interface.Data.Brand _testBrand;

        public override void BeforeEach()
        {
            base.BeforeEach();

            _paymentRepositoryMock = Container.Resolve<FakePaymentRepository>();
            _depositCommands = Container.Resolve<IOnlineDepositCommands>();
            _depositQueries = Container.Resolve<IOnlineDepositQueries>();
            _paymentTestHelper = Container.Resolve<PaymentTestHelper>();
            _actorInfoProvider = Container.Resolve<IActorInfoProvider>();
            
            //Add Testing Data
            GenerateTestingData();
        }

        private void GenerateTestingData()
        {
            GeneratePlayer();

            GenerateDepositSettingsData();

            GenerateOnlineDepositsData();
        }

        private void GeneratePlayer()
        {
            var brandTestHelper = Container.Resolve<BrandTestHelper>();
            var playerTestHelper = Container.Resolve<PlayerTestHelper>();
            var securityTestHelper = Container.Resolve<SecurityTestHelper>();
            var admin = securityTestHelper.CreateSuperAdmin();
            securityTestHelper.SignInAdmin(admin);

            var licensee = brandTestHelper.CreateLicensee();
            _testBrand = brandTestHelper.CreateBrand(licensee, isActive: true);
            var player = playerTestHelper.CreatePlayer(true, _testBrand.Id);

            _testPlayer = _paymentRepositoryMock.Players.SingleOrDefault(x => x.Id == player.Id);
        }

        private void GenerateDepositSettingsData()
        {
            //the data will be created when brandTestHelper.CreateBrand
            _testPaymentGatewaySettings = _paymentRepositoryMock.PaymentGatewaySettings.FirstOrDefault();

            _paymentRepositoryMock.PaymentSettings.Add(
            new PaymentSettings
            {
                Id = new Guid("12FE5A6D-7053-5432-12D7-208865D08830"),
                BrandId = _testBrand.Id,
                PaymentType = PaymentType.Deposit,
                VipLevel = _testPlayer.VipLevelId.ToString(),
                CurrencyCode = _testPlayer.CurrencyCode,
                PaymentMethod = _testPaymentGatewaySettings.PaymentGatewayName,
                PaymentGatewayMethod = PaymentMethod.Online,
                MaxAmountPerTransaction = 1000,
                MinAmountPerTransaction = 200,
                MaxAmountPerDay = 2000,
                MaxTransactionPerDay = 4,
                MaxTransactionPerWeek = 4,
                MaxTransactionPerMonth = 4,
                Enabled = Status.Active,
                CreatedBy = "initializer",
                CreatedDate = DateTime.Now
            });
        }

        private void GenerateOnlineDepositsData()
        {
            _processingOnlineDeposit = new OnlineDepositData
            {
                Id = new Guid("CF73876A-F42F-4F3E-8576-FE540FAA9235"),
                PlayerId = _testPlayer.Id,
                BrandId = _testPlayer.BrandId,
                TransactionNumber = "XP-0-20151202154756",
                Currency = "CAD",
                Status = OnlineDepositStatus.Processing,
                Amount = 5000,
                Created = DateTime.Now.AddHours(-1),
                CreatedBy = "TestPlayer"
            };

            _approvedOnlineDeposit = new OnlineDepositData
            {
                Id = new Guid("CF73876A-F42F-4F3E-8576-FE540FAA9212"),
                PlayerId = new Guid("91E44F36-BCD6-4797-9602-63E38488123E"),
                TransactionNumber = "XP-0-20151202154711",
                Status = OnlineDepositStatus.Approved,
                Currency = "CAD",
                Amount = 5000,
                Created = DateTime.Now.AddHours(-1),
                CreatedBy = "TestPlayer"
            };
            _paymentRepositoryMock.OnlineDeposits.Add(_processingOnlineDeposit);
            _paymentRepositoryMock.OnlineDeposits.Add(_approvedOnlineDeposit);

            var deposit = new Deposit
            {
                Id = new Guid("CF73876A-F42F-4F3E-8576-FE540FAA9235"),
                PlayerId = _testPlayer.Id,
                Status = OnlineDepositStatus.Processing.ToString(),
                Amount = 5000,
                DepositType = DepositType.Online
            };
            _paymentRepositoryMock.Deposits.Add(deposit);
        }

        private void SetPaymentSettings(
            int maxAmountPerTrans = 1000,
            int minAmountPerTrans =200,
            int maxAmountPerDay = 2000,
            int maxTransPerDay =4,
            int maxTransPerWeek = 4,
            int maxTransPerMonth = 4
            )
        {
            var setting = _paymentRepositoryMock.PaymentSettings.FirstOrDefault();
            if (setting != null)
            {
                setting.MaxAmountPerTransaction = maxAmountPerTrans;
                setting.MinAmountPerTransaction = minAmountPerTrans;
                setting.MaxAmountPerDay = maxAmountPerDay;
                setting.MaxTransactionPerDay = maxTransPerDay;
                setting.MaxTransactionPerWeek = maxTransPerWeek;
                setting.MaxTransactionPerMonth = maxTransPerMonth;
            }
        }

        private void GenerateOnlineDepositsDataForLimitChecking(bool isTheSameMonth = false)
        {
            var now = DateTime.Now;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);
            var startOfWeek = now.StartOfWeek();
            if (isTheSameMonth)
            {
                if (startOfWeek.Month != startOfMonth.Month)
                    startOfWeek = startOfMonth;
            }
            var startOfDay = now.Date;
            var todayOnlineDeposit = new OnlineDepositData
            {
                PlayerId = _testPlayer.Id,
                Status = OnlineDepositStatus.Approved,
                Approved = startOfDay,
                Amount = 500,
                Method = _testPaymentGatewaySettings.PaymentGatewayName
            };
            var weekOnlineDeposit = new OnlineDepositData
            {
                PlayerId = _testPlayer.Id,
                Status = OnlineDepositStatus.Approved,
                Approved = startOfWeek,
                Amount = 500,
                Method = _testPaymentGatewaySettings.PaymentGatewayName
            };
            var monthOnlineDeposit = new OnlineDepositData
            {
                PlayerId = _testPlayer.Id,
                Status = OnlineDepositStatus.Approved,
                Approved = startOfMonth,
                Amount = 500,
                Method = _testPaymentGatewaySettings.PaymentGatewayName
            };
            _paymentRepositoryMock.OnlineDeposits.Add(todayOnlineDeposit);
            _paymentRepositoryMock.OnlineDeposits.Add(weekOnlineDeposit);
            _paymentRepositoryMock.OnlineDeposits.Add(monthOnlineDeposit);
        }

        [Test]
        public void Can_resolve_online_deposit_commands()
        {
            Assert.DoesNotThrow(() =>
            {
                var commands = Container.Resolve<IOnlineDepositCommands>();
                Assert.That(commands, Is.Not.Null);
                Assert.That(commands is OnlineDepositCommands);
            });
        }

        [Test]
        public void Can_resolve_online_deposit_queryies()
        {
            Assert.DoesNotThrow(() =>
            {
                var commands = Container.Resolve<IOnlineDepositQueries>();
                Assert.That(commands, Is.Not.Null);
                Assert.That(commands is OnlineDepositQueries);
            });
        }

        [Test]
        public void Can_resolve_fake_repository()
        {
            Assert.DoesNotThrow(() =>
            {
                var respository = Container.Resolve<IPaymentRepository>();
                Assert.That(respository, Is.Not.Null);
                Assert.That(respository is FakePaymentRepository);
            });
        }

        [Test]
        public async void Can_submit_online_deposit_request()
        {
            var depositRequest = new OnlineDepositRequest
            {
                Amount = 200,
                PlayerId = _testPlayer.Id,
                RequestedBy = "Pl_QIOZZLVL4",
                BonusCode = string.Empty,
                CultureCode = "en-US",
                NotifyUrl = "http://localhost/notifyurl",
                ReturnUrl = "http://localhost/returnurl",
            };

            var requestResult = await _depositCommands.SubmitOnlineDepositRequest(depositRequest);

            requestResult.RedirectUrl.Should().Be(new Uri(_testPaymentGatewaySettings.EntryPoint));
            requestResult.RedirectParams.Amount.Should().Be(depositRequest.Amount, "Amount");
            requestResult.RedirectParams.OrderId.Should().NotBeNullOrEmpty("OrderId");
            requestResult.RedirectParams.Channel.Should().Be(_testPaymentGatewaySettings.Channel);
            requestResult.RedirectParams.Method.Should().Be(_testPaymentGatewaySettings.PaymentGatewayName);
            requestResult.RedirectParams.Currency.Should().Be(_testPlayer.CurrencyCode, "Currency");
            requestResult.RedirectParams.Language.Should().Be("en-US", "Language");
            requestResult.RedirectParams.NotifyUrl.Should().Be(depositRequest.NotifyUrl);
            requestResult.RedirectParams.ReturnUrl.Should().Be(depositRequest.ReturnUrl);
            //TODO:ONLINEDEPOSIT
            requestResult.RedirectParams.MerchantId.Should().NotBeNullOrEmpty("MerchantId");
            requestResult.RedirectParams.Signature.Should().NotBeNullOrEmpty("Signature");
            var orderId = requestResult.RedirectParams.OrderId;

            var onlineDeposit = _paymentRepositoryMock.OnlineDeposits.OrderByDescending(x => x.Created).First();
            onlineDeposit.Should().NotBeNull();
            if (onlineDeposit != null)
            {
                onlineDeposit.Amount.Should().Be(depositRequest.Amount,"Amount");
                onlineDeposit.Status.Should().Be(OnlineDepositStatus.Processing, "Status");
                onlineDeposit.PlayerId.Should().Be(depositRequest.PlayerId, "PlayerId");
                onlineDeposit.CreatedBy.Should().Be(depositRequest.RequestedBy, "CreatedBy");
                onlineDeposit.TransactionNumber.Should().Be(orderId,"TransactionNumber");
                onlineDeposit.Method.Should().NotBeNullOrEmpty("Method");
                onlineDeposit.MerchantId.Should().NotBeNullOrEmpty("MerchantId");
                onlineDeposit.Language.Should().Be("en-US", "Language");
                onlineDeposit.Currency.Should().Be(_testPlayer.CurrencyCode,"Currency");
                onlineDeposit.ReturnUrl.Should().Be(depositRequest.ReturnUrl);
                onlineDeposit.NotifyUrl.Should().Be(depositRequest.NotifyUrl);
                onlineDeposit.OrderIdOfGateway.Should().BeNull("OrderIdOfGateway");
                onlineDeposit.OrderIdOfRouter.Should().BeNull("OrderIdOfRouter");
                onlineDeposit.BonusCode.Should().Be(depositRequest.BonusCode,"BonusCode");
            }
        }

        [Test]
        public void Can_paynotify_online_deposit()
        {
            var request = new OnlineDepositPayNotifyRequest
            {
                OrderIdOfMerchant = "XP-0-20151202154756",
                OrderIdOfRouter = "ROID20151202154803",
                OrderIdOfGateway = "GOID20151202154803",
                Language = "zh-CN",
                PayMethod = "XPAY",
                Signature = "EB1DA0FA24C29F809885B5AC7A1233F4"
            };
            var response = _depositCommands.PayNotify(request);
            response.Should().Be("SUCCESS", "Response should be 'SUCCESS'");
         
            _processingOnlineDeposit.OrderIdOfGateway.Should().Be(request.OrderIdOfGateway, "OrderIdOfGateway");
            _processingOnlineDeposit.OrderIdOfRouter.Should().Be(request.OrderIdOfRouter, "OrderIdOfRouter");
            _processingOnlineDeposit.Status.Should().Be(OnlineDepositStatus.Approved, "Status");
            _processingOnlineDeposit.ApprovedBy.Should().Be("TestPlayer","ApprovedBy");
            _processingOnlineDeposit.Approved.Should().HaveValue("Approved");
            
            _paymentTestHelper.AssertBalance(_testPlayer.Id, total: 5000, playable: 5000, main: 5000, free: 5000);
        }

        [Test]
        public void Can_query_processing_online_deposit_status()
        {
            var request = new CheckStatusRequest
            {
                TransactionNumber = "XP-0-20151202154756"
            };

            var response = _depositQueries.CheckStatus(request);
            response.IsPaid.Should().Be(false, "IsPaid");
            response.Amount.Should().Be(0, "Amount");
            response.Bonus.Should().Be(0, "Bonus");
            response.TotalAmount.Should().Be(0, "TotalAmount");
        }

        [Test]
        public void Can_query_approved_online_deposit_status()
        {
            var request = new CheckStatusRequest
            {
                TransactionNumber = _approvedOnlineDeposit.TransactionNumber
            };

            var response = _depositQueries.CheckStatus(request);
            response.IsPaid.Should().Be(true, "IsPaid");
            response.Amount.Should().Be(_approvedOnlineDeposit.Amount, "Amount");
            //TODO:ONLINEDEPOSIT wait for Bonus function
            //response.Bonus.Should().Be(0, "Bonus");
            //response.TotalAmount.Should().Be(0, "TotalAmount");
        }

        [Test]
        public void Submit_command_should_throw_exception_if_player_not_found()
        {
            // Arrange

            // Act
            Func<Task> act = () => _depositCommands.SubmitOnlineDepositRequest(new OnlineDepositRequest());

            // Assert
            act.ShouldThrow<RegoValidationException>().WithMessage("{\"text\": \"app:apiResponseCodes.PlayerDoesNotExist\"}");
        }

        [Test]
        public void PayNotify_command_should_throw_exception_if_transactionNumber_not_found()
        {
            // Arrange

            // Act
            Action act = () => _depositCommands.PayNotify(new OnlineDepositPayNotifyRequest());

            // Assert
            act.ShouldThrow<RegoValidationException>().WithMessage("{\"text\": \"app:payment.onlineDeposit.transactionNumberNotExist\"}");
        }

        [Test]
        public void Submit_command_should_throw_exception_if_amount_below_min_amount_per_transaction()
        {
            var depositRequest = new OnlineDepositRequest
            {
                Amount = 10,
                PlayerId = _testPlayer.Id,
                RequestedBy = "Pl_QIOZZLVL4"
            };

            // Act
            Func<Task> act = () => _depositCommands.SubmitOnlineDepositRequest(depositRequest);

            // Assert
            act.ShouldThrow<PaymentSettingsViolatedException>().WithMessage("{\"text\": \"app:payment.settings.amountBelowAllowedValueError\", \"variables\": {\"value\": \"200.00\"}}");        
        }

        [Test]
        public void Submit_command_should_throw_exception_if_amount_exceed_max_amount_per_transaction()
        {
            var depositRequest = new OnlineDepositRequest
            {
                Amount = 999999,
                PlayerId = _testPlayer.Id,
                RequestedBy = "Pl_QIOZZLVL4"
            };

            // Act
            Func<Task> act = () => _depositCommands.SubmitOnlineDepositRequest(depositRequest);

            // Assert
            act.ShouldThrow<PaymentSettingsViolatedException>().WithMessage("{\"text\": \"app:payment.settings.amountExceedsAllowedValueError\", \"variables\": {\"value\": \"1000.00\"}}");
        }

        [Test]
        public void Submit_command_should_throw_exception_if_amount_exceed_max_amount_per_day()
        {
            //Arrange
            GenerateOnlineDepositsDataForLimitChecking();

            SetPaymentSettings(maxAmountPerDay:600);

            var depositRequest = new OnlineDepositRequest
            {
                Amount = 500,
                PlayerId = _testPlayer.Id,
                RequestedBy = "Pl_QIOZZLVL4"
            };

            // Act
            Func<Task> act = () => _depositCommands.SubmitOnlineDepositRequest(depositRequest);

            // Assert
            act.ShouldThrow<PaymentSettingsViolatedException>().WithMessage("{\"text\": \"app:payment.settings.amountExceedsDailyLimitError\", \"variables\": {\"value\": \"600.00\"}}");
        }

        [Test]
        public void Submit_command_should_throw_exception_if_exceed_max_trans_per_day()
        {
            //Arrange
            GenerateOnlineDepositsDataForLimitChecking();

            SetPaymentSettings(maxTransPerDay:1,maxTransPerWeek:5,maxTransPerMonth:10);

            var depositRequest = new OnlineDepositRequest
            {
                Amount = 900,
                PlayerId = _testPlayer.Id,
                RequestedBy = "Pl_QIOZZLVL4"
            };
            //Act
            Func<Task> act = () => _depositCommands.SubmitOnlineDepositRequest(depositRequest);

            //Assert
            act.ShouldThrow<PaymentSettingsViolatedException>().WithMessage("{\"text\": \"app:payment.settings.numberTransactionsExceedsDailyLimitError\", \"variables\": {\"value\": \"1.00\"}}");
        }

        [Test]
        public void Submit_command_should_throw_exception_if_exceed_max_trans_per_week()
        {
            //Arrange
            GenerateOnlineDepositsDataForLimitChecking();

            SetPaymentSettings(maxTransPerDay: 4, maxTransPerWeek: 2, maxTransPerMonth: 10);

            var depositRequest = new OnlineDepositRequest
            {
                Amount = 200,
                PlayerId = _testPlayer.Id,
                RequestedBy = "Pl_QIOZZLVL4"
            };
            //Act
            Func<Task> act = () => _depositCommands.SubmitOnlineDepositRequest(depositRequest);

            // Assert
            act.ShouldThrow<PaymentSettingsViolatedException>().WithMessage("{\"text\": \"app:payment.settings.numberTransactionsExceedsWeeklyLimitError\", \"variables\": {\"value\": \"2.00\"}}");
        }

        [Test]
        public void Submit_command_should_throw_exception_if_exceed_max_trans_per_month()
        {
            //Arrange
            GenerateOnlineDepositsDataForLimitChecking(true);

            SetPaymentSettings(maxTransPerDay: 4, maxTransPerWeek: 4, maxTransPerMonth: 3);

            var depositRequest = new OnlineDepositRequest
            {
                Amount = 200,
                PlayerId = _testPlayer.Id,
                RequestedBy = "Pl_QIOZZLVL4"
            };
            //Act
            Func<Task> act = () => _depositCommands.SubmitOnlineDepositRequest(depositRequest);

            // Assert
            act.ShouldThrow<PaymentSettingsViolatedException>().WithMessage("{\"text\": \"app:payment.settings.numberTransactionsExceedsMonthLimitError\", \"variables\": {\"value\": \"3.00\"}}");
        }

        [Test]
        public void Can_verify_online_deposit()
        {
            //Arrange
            var deposit = _paymentTestHelper.CreateOnlineDeposit(_testPlayer.Id, 300);

            var request = new VerifyOnlineDepositRequest
            {
                Id=deposit.Id,
                Remarks = "verify remark"
            };
            //Act
            _depositCommands.Verify(request);

            //Assert
            var settings = _paymentRepositoryMock.OnlineDeposits.Single(x => x.Id == deposit.Id);
            settings.Status.Should().Be(OnlineDepositStatus.Verified);
            settings.Remarks.Should().Be(request.Remarks);
            settings.VerifiedBy.ShouldBeEquivalentTo(_actorInfoProvider.Actor.UserName);
            settings.Verified.Should().BeCloseTo(DateTimeOffset.Now, 5000);
        }

        [Test]
        public void Can_reject_online_deposit()
        {
            //Arrange
            var deposit = _paymentTestHelper.CreateOnlineDeposit(_testPlayer.Id, 300);

            var request = new RejectOnlineDepositRequest
            {
                Id = deposit.Id,
                Remarks = "verify remark"
            };
            //Act
            _depositCommands.Reject(request);

            //Assert
            var settings = _paymentRepositoryMock.OnlineDeposits.Single(x => x.Id == deposit.Id);
            settings.Status.Should().Be(OnlineDepositStatus.Rejected);
            settings.Remarks.Should().Be(request.Remarks);
            settings.RejectedBy.ShouldBeEquivalentTo(_actorInfoProvider.Actor.UserName);
            settings.Rejected.Should().BeCloseTo(DateTimeOffset.Now, 5000);
        }

        [Test]
        public void Can_approve_online_deposit()
        {
            //Arrange
            var deposit = _paymentTestHelper.CreateOnlineDeposit(_testPlayer.Id, 300);
            _paymentTestHelper.VerifyOnlineDeposit(deposit.Id);

            var request = new ApproveOnlineDepositRequest
            {
                Id = deposit.Id,
                Remarks = "remark"
            };
            //Act
            _depositCommands.Approve(request);

            //Assert
            var settings = _paymentRepositoryMock.OnlineDeposits.Single(x => x.Id == deposit.Id);
            settings.Status.Should().Be(OnlineDepositStatus.Approved);
            settings.Remarks.Should().Be(request.Remarks);
            settings.ApprovedBy.ShouldBeEquivalentTo(_actorInfoProvider.Actor.UserName);
            settings.Approved.Should().BeCloseTo(DateTimeOffset.Now, 5000);

            _paymentTestHelper.AssertBalance(_testPlayer.Id, total: 300, playable: 300, main: 300,free:300);
        }

        [Test]
        public void Can_not_approve_rejected_online_deposit()
        {
            //Arrange
            var deposit = _paymentTestHelper.CreateOnlineDeposit(_testPlayer.Id, 300);
            _paymentTestHelper.RejectOnlineDeposit(deposit.Id);

            var request = new ApproveOnlineDepositRequest
            {
                Id = deposit.Id,
                Remarks = "remark"
            };
            //Act
            //Assert
            Assert.Throws<RegoException>(() => _depositCommands.Approve(request))
                .Message.Should().Be("The deposit has 'Rejected' status, so it can't be Approved");
        }

        [Test]
        public void Can_not_approve_approved_online_deposit()
        {
            //Arrange
            var deposit = _paymentTestHelper.CreateOnlineDeposit(_testPlayer.Id, 300);
            _paymentTestHelper.VerifyOnlineDeposit(deposit.Id);
            _paymentTestHelper.ApproveOnlineDeposit(deposit.Id);

            var request = new ApproveOnlineDepositRequest
            {
                Id = deposit.Id,
                Remarks = "remark"
            };
            //Act
            //Assert
            Assert.Throws<RegoException>(() => _depositCommands.Approve(request))
                .Message.Should().Be("The deposit has 'Approved' status, so it can't be Approved");
        }

        [Test]
        public void Can_not_approve_unverified_online_deposit()
        {
            //Arrange
            var deposit = _paymentTestHelper.CreateOnlineDeposit(_testPlayer.Id, 300);            

            var request = new ApproveOnlineDepositRequest
            {
                Id = deposit.Id,
                Remarks = "remark"
            };
            //Act
            //Assert
            Assert.Throws<RegoException>(() => _depositCommands.Approve(request))
                .Message.Should().Be("The deposit has 'Processing' status, so it can't be Approved");
        }

        [Test]
        public void Can_not_reject_approved_online_deposit()
        {
            //Arrange
            var deposit = _paymentTestHelper.CreateOnlineDeposit(_testPlayer.Id, 300);
            _paymentTestHelper.VerifyOnlineDeposit(deposit.Id);
            _paymentTestHelper.ApproveOnlineDeposit(deposit.Id);

            var request = new RejectOnlineDepositRequest
            {
                Id = deposit.Id,
                Remarks = "remark"
            };
            //Act
            //Assert
            Assert.Throws<RegoException>(() => _depositCommands.Reject(request))
                .Message.Should().Be("The deposit has 'Approved' status, so it can't be Rejected");
        }


        [Test]
        public void Can_not_reject_rejected_online_deposit()
        {
            //Arrange
            var deposit = _paymentTestHelper.CreateOnlineDeposit(_testPlayer.Id, 300);
            _paymentTestHelper.RejectOnlineDeposit(deposit.Id);
            
            var request = new RejectOnlineDepositRequest
            {
                Id = deposit.Id,
                Remarks = "remark"
            };
            //Act
            //Assert
            Assert.Throws<RegoException>(() => _depositCommands.Reject(request))
                .Message.Should().Be("The deposit has 'Rejected' status, so it can't be Rejected");
        }

        [Test]
        public void Can_not_verify_approved_online_deposit()
        {
            //Arrange
            var deposit = _paymentTestHelper.CreateOnlineDeposit(_testPlayer.Id, 300);
            _paymentTestHelper.VerifyOnlineDeposit(deposit.Id);
            _paymentTestHelper.ApproveOnlineDeposit(deposit.Id);

            var request = new VerifyOnlineDepositRequest
            {
                Id = deposit.Id,
                Remarks = "remark"
            };
            //Act
            //Assert
            Assert.Throws<RegoException>(() => _depositCommands.Verify(request))
                .Message.Should().Be("The deposit has 'Approved' status, so it can't be Verified");
        }

        [Test]
        public void Can_not_verify_rejected_online_deposit()
        {
            //Arrange
            var deposit = _paymentTestHelper.CreateOnlineDeposit(_testPlayer.Id, 300);
            _paymentTestHelper.VerifyOnlineDeposit(deposit.Id);
            _paymentTestHelper.RejectOnlineDeposit(deposit.Id);

            var request = new VerifyOnlineDepositRequest
            {
                Id = deposit.Id,
                Remarks = "remark"
            };
            //Act
            //Assert
            Assert.Throws<RegoException>(() => _depositCommands.Verify(request))
                .Message.Should().Be("The deposit has 'Rejected' status, so it can't be Verified");
        }

        [Test]
        public void Can_not_verify_verified_online_deposit()
        {
            //Arrange
            var deposit = _paymentTestHelper.CreateOnlineDeposit(_testPlayer.Id, 300);
            _paymentTestHelper.VerifyOnlineDeposit(deposit.Id);            

            var request = new VerifyOnlineDepositRequest
            {
                Id = deposit.Id,
                Remarks = "remark"
            };
            //Act
            //Assert
            Assert.Throws<RegoException>(() => _depositCommands.Verify(request))
                .Message.Should().Be("The deposit has 'Verified' status, so it can't be Verified");
        }
    }
}
