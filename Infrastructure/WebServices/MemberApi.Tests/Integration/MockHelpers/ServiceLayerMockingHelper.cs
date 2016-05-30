using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AFT.RegoV2.Bonus.Api.Interface;
using AFT.RegoV2.Bonus.Api.Interface.Responses;
using AFT.RegoV2.Bonus.Core.Models.Commands;
using AFT.RegoV2.Bonus.Core.Models.Data;
using AFT.RegoV2.Core.Brand.Interface.ApplicationServices;
using AFT.RegoV2.Core.Common.Brand.Data;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Payment;
using AFT.RegoV2.Core.Common.Data.Player;
using AFT.RegoV2.Core.Common.Data.Player.Enums;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Game.Interface.Data;
using AFT.RegoV2.Core.Game.Interface.Interfaces;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Core.Payment.Interface.ApplicationServices;
using AFT.RegoV2.Core.Payment.Interface.Data;
using AFT.RegoV2.Core.Player.Interface.ApplicationServices;
using AFT.RegoV2.Core.Security.Interface.ApplicationServices;
using AFT.RegoV2.Core.Security.Interface.Data;
using AFT.RegoV2.Tests.Common;
using AFT.UGS.Core.Messages.Products;
using AFT.UGS.Core.Messages.Products.External;

using FluentValidation.Results;
using Microsoft.Practices.Unity;
using Moq;
using Country = AFT.RegoV2.Core.Brand.Interface.Data.Country;
using Currency = AFT.RegoV2.Core.Brand.Interface.Data.Currency;
using Player = AFT.RegoV2.Core.Common.Data.Player.Player;
using WalletTemplate = AFT.RegoV2.Core.Brand.Interface.Data.WalletTemplate;

namespace AFT.RegoV2.MemberApi.Tests.Integration.MockHelpers
{
    public static class ServiceLayerMockingHelper
    {
        private static Mock<ILoggingService> mockedLoggingService;

        public static void MockPlayerQueriesForHttpStatus200(IUnityContainer underlyingContainer)
        {
            var mockedPlayerQueries = new Mock<IPlayerQueries>();

            mockedPlayerQueries.Setup(proxy => proxy.GetPlayer(It.IsAny<Guid>())).Returns(new RegoV2.Core.Common.Data.Player.Player()
            {
                Id = Guid.NewGuid(),
                BrandId = Guid.NewGuid()
            });
            mockedPlayerQueries.Setup(proxy => proxy.GetSecurityQuestion(It.IsAny<Guid>()))
                .Returns(new RegoV2.Core.Common.Data.SecurityQuestion()
                {
                    Id = Guid.NewGuid(),
                    Question = Guid.NewGuid().ToString().Substring(8)
                });

            mockedPlayerQueries.Setup(proxy => proxy.GetOnSiteMessage(It.IsAny<Guid>())).Returns(
                new RegoV2.Core.Player.Interface.Data.OnSiteMessage()
                {
                    Id = Guid.NewGuid(),
                    Subject = "random subject",
                    Content = "Content",
                    IsNew = true,
                    Received = DateTimeOffset.UtcNow
                });

            mockedPlayerQueries.Setup(proxy => proxy.GetOnSiteMessages(It.IsAny<Guid>()))
                .Returns(new List<RegoV2.Core.Player.Interface.Data.OnSiteMessage>()
                {
                    new RegoV2.Core.Player.Interface.Data.OnSiteMessage()
                });

            mockedPlayerQueries.Setup(proxy => proxy.GetOnSiteMessagesCount(It.IsAny<Guid>())).Returns(10);

            mockedPlayerQueries.Setup(proxy => proxy.GetPlayerByResetPasswordToken(It.IsAny<string>()))
                .Returns(new RegoV2.Core.Common.Data.Player.Player()
                {
                    Id = Guid.NewGuid()
                });

            mockedPlayerQueries.Setup(proxy => proxy.GetPlayerByUsername(It.IsAny<string>()))
                .Returns(new RegoV2.Core.Common.Data.Player.Player()
                {
                    FirstName = TestDataGenerator.GetRandomString(),
                    LastName = TestDataGenerator.GetRandomString()
                });

            mockedPlayerQueries.Setup(proxy => proxy.GetPlayer(It.IsAny<Guid>()))
                .Returns(new RegoV2.Core.Common.Data.Player.Player()
                {
                    IdentityVerifications = new List<IdentityVerification>()
                {
                    new IdentityVerification()
                    {
                        ExpirationDate = DateTimeOffset.UtcNow,
                        VerificationStatus = VerificationStatus.Verified
                    }
                }
                });

            underlyingContainer.RegisterInstance(mockedPlayerQueries.Object);
        }
        public static void MockBrandQueriesForHttpStatus200(IUnityContainer underlyingContainer)
        {
            var mockedBrandQueries = new Mock<IBrandQueries>();
            mockedBrandQueries.Setup(proxy => proxy.GetCountriesByBrand(It.IsAny<Guid>())).Returns(new List<Country>()
            {
                new Country() {Code = "CAN", Name = "Canada"},
                new Country() {Code = "UKR", Name = "Ukraine"}
            });
            mockedBrandQueries.Setup(proxy => proxy.GetCurrenciesByBrand(It.IsAny<Guid>())).Returns(new List<Currency>()
            {
                new Currency() {Code = "CAD", Name = "Canadian Dollar"},
                new Currency() {Code = "UKR", Name = "Ukrain Dollar"}
            });

            mockedBrandQueries.Setup(proxy => proxy.GetCulturesByBrand(It.IsAny<Guid>()))
                .Returns(new List<Culture>()
                {
                    new Culture()
                    {
                        Code = "en-US",
                        NativeName = "english"
                    }
                });

            mockedBrandQueries.Setup(proxy => proxy.GetWalletTemplates(It.IsAny<Guid>()))
                .Returns(new List<WalletTemplate>()
                {
                    new WalletTemplate()
                    {
                        Id = Guid.NewGuid(),
                        Name = TestDataGenerator.GetRandomString()
                    }
                });

            underlyingContainer.RegisterInstance(mockedBrandQueries.Object);
        }
        public static void MockWalletQueriesForHttpStatus200(IUnityContainer underlyingContainer)
        {
            var mockedWalletQueries = new Mock<IWalletQueries>();

            mockedWalletQueries.Setup(proxy => proxy.GetPlayerBalance(It.IsAny<Guid>(), null))
                .Returns(Task.FromResult(new PlayerBalance()
                {
                    Bonus = 10,
                    CurrencyCode = "CAD",
                    Main = 1000,
                    Playable = 600,
                    Total = 4500
                }));

            underlyingContainer.RegisterInstance(mockedWalletQueries.Object);
        }
        public static void MockBonusQueriesForHttpStatus200(IUnityContainer underlyingContainer)
        {
            var mockedBonusApiProxy = new Mock<IBonusApiProxy>();

            mockedBonusApiProxy.Setup(proxy => proxy.GetWageringBalancesAsync(It.IsAny<Guid>(), null))
                .Returns((Task.FromResult(new PlayerWagering()
                {
                    Completed = 10,
                    Remaining = 10,
                    Requirement = 20
                })));

            underlyingContainer.RegisterInstance(mockedBonusApiProxy.Object);
        }

        public static void MockPlayerQueriesForHttpStatus400(IUnityContainer underlyingContainer)
        {
            var mockedPlayeQueries = new Mock<IPlayerQueries>();
            mockedPlayeQueries.Setup(proxy => proxy.GetPlayer(It.IsAny<Guid>()))
                .Returns((RegoV2.Core.Common.Data.Player.Player)null);

            mockedPlayeQueries.Setup(proxy => proxy.GetSecurityQuestion(It.IsAny<Guid>()))
                .Returns((RegoV2.Core.Common.Data.SecurityQuestion)null);

            mockedPlayeQueries.Setup(proxy => proxy.GetOnSiteMessage(It.IsAny<Guid>()))
                .Returns((RegoV2.Core.Player.Interface.Data.OnSiteMessage)null);

            mockedPlayeQueries.Setup(proxy => proxy.GetPlayerByResetPasswordToken(It.IsAny<string>()))
                .Returns((RegoV2.Core.Common.Data.Player.Player)null);

            underlyingContainer.RegisterInstance(mockedPlayeQueries.Object);
        }
        public static void MockBrandQueriesForHttpStatus400(IUnityContainer underlyingContainer)
        {
            var mockedBrandQueries = new Mock<IBrandQueries>();
            mockedBrandQueries.Setup(proxy => proxy.GetWalletTemplates(It.IsAny<Guid>()))
                .Returns((IEnumerable<WalletTemplate>)null);

            underlyingContainer.RegisterInstance(mockedBrandQueries.Object);
        }

        public static void MockPlayerQueriesForHttpStatus500(IUnityContainer underlyingContainer)
        {
            var mockedPlayerQueries = new Mock<IPlayerQueries>();
            mockedPlayerQueries.Setup(proxy => proxy.GetOnSiteMessages(It.IsAny<Guid>()))
                .Returns((IEnumerable<RegoV2.Core.Player.Interface.Data.OnSiteMessage>)null);

            mockedPlayerQueries.Setup(proxy => proxy.GetPlayer(It.IsAny<Guid>()))
                .Returns((Player)null);

            underlyingContainer.RegisterInstance(mockedPlayerQueries.Object);
        }
        public static void MockWalletQueriesForHttpStatus500(IUnityContainer underlyingContainer)
        {
            var mockedWalletQueries = new Mock<IWalletQueries>();
            underlyingContainer.RegisterInstance(mockedWalletQueries.Object);
        }
        public static void MockBrandQueriesForHttpStatus500(IUnityContainer underlyingContainer)
        {
            var mockedBrandQueries = new Mock<IBrandQueries>();
            mockedBrandQueries.Setup(proxy => proxy.GetCulturesByBrand(It.IsAny<Guid>()))
                .Returns((IEnumerable<Culture>)null);

            mockedBrandQueries.Setup(proxy => proxy.GetCountriesByBrand(It.IsAny<Guid>()))
                .Returns((IEnumerable<Country>)null);

            underlyingContainer.RegisterInstance(mockedBrandQueries.Object);
        }

        public static void MockPlayerCommandsForHttpStatus201(IUnityContainer underlyingContainer)
        {
            var mockedPlayerQueries = new Mock<IPlayerCommands>();
            mockedPlayerQueries.Setup(
                proxy => proxy.VerifyMobileNumber(It.IsAny<Guid>(), It.IsAny<string>()))
                .Verifiable();

            mockedPlayerQueries.Setup(
                proxy => proxy.ChangeSecurityQuestion(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()));

            underlyingContainer.RegisterInstance(mockedPlayerQueries.Object);
        }

        public static void MockBonusApiProxyForHttpStatus200(IUnityContainer underlyingContainer)
        {
            var mockedBonusApiProxy = new Mock<IBonusApiProxy>();
            mockedBonusApiProxy.Setup(proxy => proxy.GetBonusRedemptionAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(Task.FromResult(new Bonus.Core.Models.Data.BonusRedemption()));

            mockedBonusApiProxy.Setup(
                proxy => proxy.GetDepositQualifiedBonusesAsync(It.IsAny<Guid>(), It.IsAny<decimal>()))
                .Returns(Task.FromResult(new List<Bonus.Core.Models.Data.DepositQualifiedBonus>()
                {
                    new Bonus.Core.Models.Data.DepositQualifiedBonus()
                    {
                        BonusAmount = 1000,
                        Code = TestDataGenerator.GetRandomString(),
                        Description = TestDataGenerator.GetRandomString(),
                        Id = Guid.NewGuid(),
                        Name = TestDataGenerator.GetRandomString()
                    }
                }));

            mockedBonusApiProxy.Setup(
                proxy =>
                    proxy.GetDepositQualifiedBonusByCodeAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<decimal>()))
                .Returns(Task.FromResult(new Bonus.Core.Models.Data.DepositQualifiedBonus()
                {
                    BonusAmount = 100,
                    Code = TestDataGenerator.GetRandomString(4),
                    Description = TestDataGenerator.GetRandomString(),
                    Id = Guid.NewGuid(),
                    Name = TestDataGenerator.GetRandomString()
                }));

            mockedBonusApiProxy.Setup(
                proxy =>
                    proxy.GetDepositBonusApplicationValidationAsync(It.IsAny<Guid>(), It.IsAny<string>(),
                        It.IsAny<decimal>())).Returns(Task.FromResult(new DepositBonusApplicationValidationResponse()
                        {
                            Success = true
                        }));

            mockedBonusApiProxy.Setup(proxy => proxy.GetCompletedBonusesAsync(It.IsAny<Guid>()))
                .Returns(Task.FromResult(new List<BonusRedemption>()));

            mockedBonusApiProxy.Setup(proxy => proxy.GetBonusesWithIncompleteWageringAsync(It.IsAny<Guid>()))
                .Returns(Task.FromResult(new List<BonusRedemption>()));

            mockedBonusApiProxy.Setup(
                proxy => proxy.GetVisibleDepositQualifiedBonuses(It.IsAny<Guid>(), It.IsAny<decimal>()))
                .Returns(Task.FromResult(new List<DepositQualifiedBonus>()));

            mockedBonusApiProxy.Setup(proxy => proxy.GetClaimableRedemptionsAsync(It.IsAny<Guid>()))
                .Returns(Task.FromResult(new List<BonusRedemption>()));

            underlyingContainer.RegisterInstance(mockedBonusApiProxy.Object);
        }
        public static void MockBonusApiProxyForHttpStatus400(IUnityContainer underlyingContainer)
        {
            var mockedBonusApiProxy = new Mock<IBonusApiProxy>();
            mockedBonusApiProxy.Setup(proxy => proxy.GetBonusRedemptionAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(Task.FromResult<BonusRedemption>(null));

            mockedBonusApiProxy.Setup(
                proxy =>
                    proxy.GetDepositQualifiedBonusByCodeAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<decimal>()))
                .Returns(Task.FromResult<DepositQualifiedBonus>(null));

            mockedBonusApiProxy.Setup(
                proxy =>
                    proxy.GetDepositQualifiedBonusByCodeAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<decimal>()))
                .Returns(Task.FromResult<DepositQualifiedBonus>(null));

            mockedBonusApiProxy.Setup(
                proxy =>
                    proxy.GetDepositBonusApplicationValidationAsync(It.IsAny<Guid>(), It.IsAny<string>(),
                        It.IsAny<decimal>()))
                .Returns(Task.FromResult(new DepositBonusApplicationValidationResponse() { Success = true }));

            mockedBonusApiProxy.Setup(
                proxy =>
                    proxy.GetVisibleDepositQualifiedBonuses(It.IsAny<Guid>(), It.IsAny<decimal>()))
                .Returns((Task.FromResult<List<DepositQualifiedBonus>>(null)));

            underlyingContainer.RegisterInstance(mockedBonusApiProxy.Object);
        }

        public static void MockBonusApiProxyForHttpStatus201(IUnityContainer underlyingContainer)
        {
            var mockedBonusApiProxy = new Mock<IBonusApiProxy>();
            mockedBonusApiProxy.Setup(proxy => proxy.ClaimBonusRedemptionAsync(It.IsAny<Bonus.Core.Models.Commands.ClaimBonusRedemption>()))
                .Returns(Task.FromResult(new ClaimBonusRedemption()));

            mockedBonusApiProxy.Setup(proxy => proxy.GetBonusRedemptionAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(Task.FromResult(new Bonus.Core.Models.Data.BonusRedemption()));

            underlyingContainer.RegisterInstance(mockedBonusApiProxy.Object);
        }

        public static void MockGameAndOperationQueriesForHttpStatus400(IUnityContainer underlyingContainer)
        {
            var mockedGameQueries = new Mock<IGameQueries>();
            mockedGameQueries.Setup(proxy => proxy.GetPlayerByUsernameAsync(It.IsAny<string>()))
                .Returns(Task.FromResult<RegoV2.Core.Game.Interface.Data.Player>(null));

            underlyingContainer.RegisterInstance(mockedGameQueries.Object);
        }
        public static void MockGameAndOperationQueriesForHttpStatus200(IUnityContainer underlyingContainer)
        {
            var mockedGameQueries = new Mock<IGameQueries>();
            mockedGameQueries.Setup(proxy => proxy.GetPlayerByUsernameAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(new RegoV2.Core.Game.Interface.Data.Player() { Id = Guid.NewGuid() }));

            mockedGameQueries.Setup(proxy => proxy.GetPlayerData(It.IsAny<Guid>())).Returns(new RegoV2.Core.Game.Interface.Data.Player()
            {
                BrandId = Guid.NewGuid()
            });

            mockedGameQueries.Setup(proxy => proxy.GetLobby(It.IsAny<Guid>(), It.IsAny<bool>())).Returns(new Lobby()
            {

            });

            underlyingContainer.RegisterInstance(mockedGameQueries.Object);

            var mockedProductOperations = new Mock<IProductOperations>();
            mockedProductOperations.Setup(
                proxy =>
                    proxy.GetProductsForPlayerAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(),
                        It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(new ProductsForPlayerResponse()
                        {
                            products = new ProductsDataResponse()
                            {
                                lobbyid = Guid.NewGuid(),
                                groups = new[]
                                {
                                    new GameGroupSummary()
                                    {
                                        code = TestDataGenerator.GetRandomString(),
                                        games = new[]
                                        {
                                            new GameSummary()
                                            {
                                                code = TestDataGenerator.GetRandomString(),
                                                id = TestDataGenerator.GetRandomString(),
                                                isactive = true,
                                                name = TestDataGenerator.GetRandomString(),
                                                order = 34,
                                                url = TestDataGenerator.GetRandomWebsiteUrl()
                                            },
                                        },
                                        name = TestDataGenerator.GetRandomString()
                                    },
                                }
                            }
                        }));

            underlyingContainer.RegisterInstance(mockedProductOperations.Object);
        }

        public static void MockBonusApiProxyForHttpStatus500(IUnityContainer underlyingContainer)
        {
            var mockedBonusApiProxy = new Mock<IBonusApiProxy>();
            mockedBonusApiProxy.Setup(proxy => proxy.GetCompletedBonusesAsync(It.IsAny<Guid>()))
                .Returns((Task<List<BonusRedemption>>)null);

            mockedBonusApiProxy.Setup(proxy => proxy.GetBonusesWithIncompleteWageringAsync(It.IsAny<Guid>()))
                .Returns((Task<List<BonusRedemption>>)null);

            mockedBonusApiProxy.Setup(proxy => proxy.GetWageringBalancesAsync(It.IsAny<Guid>(), It.IsAny<Guid?>()))
                .Returns(Task.FromResult(new PlayerWagering()
                {
                    Completed = 10,
                    Remaining = 10,
                    Requirement = 20
                }));

            underlyingContainer.RegisterInstance(mockedBonusApiProxy.Object);
        }

        public static void MockPaymentQueriesForHttpStatus200(IUnityContainer underlyingContainer)
        {
            var defaultVilLevelId = Guid.NewGuid();

            #region Payment Queries
            var mockedPaymentQueries = new Mock<IPaymentQueries>();
            mockedPaymentQueries.Setup(proxy => proxy.GetBankAccountsForOfflineDepositRequest(It.IsAny<Guid>()))
                .Returns(new Dictionary<Guid, string>());

            mockedPaymentQueries.Setup(proxy => proxy.GetPaymentSettings())
                .Returns(new List<PaymentSettings>()
                {
                    new PaymentSettings()
                    {
                        VipLevel = defaultVilLevelId.ToString(),
                        CurrencyCode = string.Empty,
                        Enabled = Status.Active
                    }
                }.AsQueryable());

            mockedPaymentQueries.Setup(proxy => proxy.GetBankAccountsForAdminOfflineDepositRequest(It.IsAny<Guid>()))
                .Returns(new List<BankAccount>()
                {
                    new BankAccount()
                    {
                        Id = Guid.NewGuid(),
                        AccountName = TestDataGenerator.GetRandomString(),
                        Bank = new Bank()
                        {
                            BankName = TestDataGenerator.GetRandomString()
                        }
                    }
                });

            mockedPaymentQueries.Setup(proxy => proxy.GetPlayerWithBank(It.IsAny<Guid>()))
                .Returns(new RegoV2.Core.Payment.Interface.Data.Player()
                {
                    VipLevelId = Guid.NewGuid(),
                    CurrencyCode = TestDataGenerator.GetRandomCurrencyCode(),
                    CurrentBankAccount = new PlayerBankAccount()
                    {
                        Bank = new Bank()
                        {
                            Updated = DateTimeOffset.UtcNow,
                            BankName = TestDataGenerator.GetRandomString()
                        }
                    }
                });

            mockedPaymentQueries.Setup(proxy => proxy.GetBanksByBrand(It.IsAny<Guid>()))
                .Returns(new List<Bank>()
                {
                    new Bank()
                    {
                        Id = Guid.NewGuid(),
                        BankName = TestDataGenerator.GetRandomString()
                    }
                });

            mockedPaymentQueries.Setup(
                proxy =>
                    proxy.GetOnlinePaymentSettings(It.IsAny<Guid>(), PaymentType.Deposit, It.IsAny<string>(),
                        It.IsAny<string>(), It.IsAny<string>()))
                        .Returns(new PaymentSettings());

            mockedPaymentQueries.Setup(
                proxy =>
                    proxy.GetOfflinePaymentSettings(It.IsAny<Guid>(), PaymentType.Deposit, It.IsAny<string>(),
                        It.IsAny<string>()))
                        .Returns(new PaymentSettings());

            mockedPaymentQueries.Setup(proxy => proxy.ValidatePlayerBankAccount(It.IsAny<EditPlayerBankAccountData>()))
                .Returns(new ValidationResult()
                {
                    Errors = { }
                });

            mockedPaymentQueries.Setup(proxy => proxy.GetBankAccountForOfflineDeposit(It.IsAny<Guid>()))
                .Returns(new BankAccount());

            underlyingContainer.RegisterInstance(mockedPaymentQueries.Object);
            #endregion

            #region Online Deposit Queries
            var mockedOnlineDepositQueries = new Mock<IOnlineDepositQueries>();
            mockedOnlineDepositQueries.Setup(proxy => proxy.GetOnlineDepositsByPlayerId(It.IsAny<Guid>()))
                .Returns(new List<DepositDto>()
                {
                    new DepositDto()
                    {
                        DateApproved = DateTimeOffset.UtcNow,
                        Amount = 100,
                        BonusCode = TestDataGenerator.GetRandomString(4),
                        BonusRedemptionId = Guid.NewGuid()
                    }
                });

            mockedOnlineDepositQueries.Setup(proxy => proxy.CheckStatus(It.IsAny<CheckStatusRequest>()))
                .Returns(new CheckStatusResponse());

            underlyingContainer.RegisterInstance(mockedOnlineDepositQueries.Object);
            #endregion

            #region Online Deposit Commands
            var mockedOnlineDepositCommands = new Mock<IOnlineDepositCommands>();
            mockedOnlineDepositCommands.Setup(
                proxy => proxy.ValidateOnlineDepositAmount(It.IsAny<ValidateOnlineDepositAmountRequest>()));

            underlyingContainer.RegisterInstance(mockedOnlineDepositCommands.Object);
            #endregion

            #region Offline Deposit Queries
            var mockedOfflineDepositQueries = new Mock<IOfflineDepositQueries>();
            mockedOfflineDepositQueries.Setup(proxy => proxy.GetPendingDeposits(It.IsAny<Guid>()))
                .Returns(new List<OfflineDeposit>()
                {
                    new OfflineDeposit()
                    {
                        Id = Guid.NewGuid(),
                        Amount = 100,
                        Status = OfflineDepositStatus.Approved

                    }
                });

            mockedOfflineDepositQueries.Setup(proxy => proxy.GetOfflineDeposit(It.IsAny<Guid>()))
                .Returns(new OfflineDeposit()
                {

                });

            underlyingContainer.RegisterInstance(mockedOfflineDepositQueries.Object);
            #endregion

            var mockedBonusApiProxy = new Mock<IBonusApiProxy>();
            mockedBonusApiProxy.Setup(proxy => proxy.GetBonusRedemptionAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(Task.FromResult(new BonusRedemption()));

            underlyingContainer.RegisterInstance(mockedBonusApiProxy.Object);

            #region Player queries
            var mockedPlayerQueries = new Mock<IPlayerQueries>();
            mockedPlayerQueries.Setup(proxy => proxy.GetDefaultVipLevel(It.IsAny<Guid>()))
                .Returns(new RegoV2.Core.Common.Data.Player.VipLevel()
                {
                    Id = defaultVilLevelId
                });

            mockedPlayerQueries.Setup(proxy => proxy.GetPlayer(It.IsAny<Guid>()))
                .Returns(new Player()
                {

                });

            underlyingContainer.RegisterInstance(mockedPlayerQueries.Object);
            #endregion

            #region Brand Queries
            var mockedBrandQueries = new Mock<IBrandQueries>();

            mockedBrandQueries.Setup(proxy => proxy.GetWalletTemplates(It.IsAny<Guid>()))
                .Returns(new List<WalletTemplate>()
                {
                    new WalletTemplate()
                    {
                        IsMain = true,
                        Id = Guid.NewGuid(),
                        Name = TestDataGenerator.GetRandomString()
                    }
                });

            underlyingContainer.RegisterInstance(mockedBrandQueries.Object);

            #endregion

            #region Payment Gateway Settings Queries
            var mockedPaymentGatewaySettingsQueries = new Mock<IPaymentGatewaySettingsQueries>();

            mockedPaymentGatewaySettingsQueries.Setup(
                proxy => proxy.GetPaymentGatewaySettingsByPlayerId(It.IsAny<Guid>()))
                .Returns(new List<PaymentGatewaySettings>());

            mockedPaymentGatewaySettingsQueries.Setup(
                proxy => proxy.GetOnePaymentGatewaySettingsByPlayerId(It.IsAny<Guid>()))
                .Returns(new PaymentGatewaySettings());

            underlyingContainer.RegisterInstance(mockedPaymentGatewaySettingsQueries.Object);

            #endregion
        }

        public static void MockLoggingServiceForHttpStatus500(IUnityContainer underlyingContainer)
        {
            mockedLoggingService = new Mock<ILoggingService>();
            mockedLoggingService.Setup(proxy => proxy.Log(It.IsAny<Error>()));

            underlyingContainer.RegisterInstance(mockedLoggingService.Object);
        }

        public static void VerifyLogginServiceWasCalledAfterInternalServerError(int numberOfExpectedInternalServerErrors)
        {
            mockedLoggingService.Verify(proxy => proxy.Log(It.IsAny<Error>()), Times.Exactly(numberOfExpectedInternalServerErrors));
        }
    }
}
