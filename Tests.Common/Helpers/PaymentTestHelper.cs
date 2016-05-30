using System;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AFT.RegoV2.Core.Brand;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Payment;
using AFT.RegoV2.Core.Common.Data.Player;
using AFT.RegoV2.Core.Common.Events.Notifications;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Core.Messaging;
using AFT.RegoV2.Core.Payment.Data;
using AFT.RegoV2.Core.Payment.Interface.ApplicationServices;
using AFT.RegoV2.Core.Payment.Interface.Data;
using AFT.RegoV2.Core.Payment.Interface.Data.Commands;
using AFT.RegoV2.Core.Player;
using AFT.RegoV2.Core.Payment;
using Bank = AFT.RegoV2.Core.Payment.Data.Bank;
using BankAccount = AFT.RegoV2.Core.Payment.Data.BankAccount;
using BankAccountType = AFT.RegoV2.Core.Payment.Data.BankAccountType;
using PaymentLevel = AFT.RegoV2.Core.Payment.Data.PaymentLevel;
using AFT.RegoV2.Shared.Utils;
using NUnit.Framework;
using PaymentGatewaySettings = AFT.RegoV2.Core.Payment.Data.PaymentGatewaySettings;
using PlayerBankAccount = AFT.RegoV2.Core.Payment.Data.PlayerBankAccount;
using OfflineDeposit = AFT.RegoV2.Core.Payment.Data.OfflineDeposit;
using PaymentSettings = AFT.RegoV2.Core.Payment.Data.PaymentSettings;
using Currency = AFT.RegoV2.Core.Payment.Data.Currency;
using PlayerPaymentLevel = AFT.RegoV2.Core.Payment.Data.PlayerPaymentLevel;
namespace AFT.RegoV2.Tests.Common.Helpers
{
    public class PaymentTestHelper
    {
        private readonly IOfflineDepositCommands _offlineDepositCommands;
        private readonly IOnlineDepositCommands _onlineDepositCommands;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IPlayerRepository _playerRepository;
        private readonly IMessagingRepository _messagingRepository;
        private readonly IPaymentSettingsCommands _paymentSettingsCommands;
        private readonly IBankAccountCommands _bankAccountCommands;
        private readonly IPlayerBankAccountCommands _playerBankAccountCommands;
        private readonly ITransferFundCommands _transferFundCommands;
        private readonly BrandCommands _brandCommands;
        private readonly IBrandRepository _brandRepository;
        private readonly IBankCommands _bankCommands;
        private readonly IPaymentGatewaySettingsCommands _paymentGatewaySettingsCommands;
        private readonly IGameRepository _gameRepository;
        private readonly IWalletQueries _walletQueries;
        private readonly IWithdrawalService _withdrawalService;
        public PaymentTestHelper(
            IOfflineDepositCommands offlineDepositCommands,
            IOnlineDepositCommands onlineDepositCommands,
            IPaymentRepository paymentRepository,
            IPlayerRepository playerRepository,
            IMessagingRepository messagingRepository,
            IPaymentSettingsCommands paymentSettingsCommands,
            IBankAccountCommands bankAccountCommands,
            IPlayerBankAccountCommands playerBankAccountCommands,
            ITransferFundCommands transferFundCommands,
            BrandCommands brandCommands,
            IBrandRepository brandRepository,
            IBankCommands bankCommands,
            IPaymentGatewaySettingsCommands paymentGatewaySettingsCommands,
            IGameRepository gameRepository,
            IWalletQueries walletQueries,
            IWithdrawalService withdrawalService
            )
        {
            _offlineDepositCommands = offlineDepositCommands;
            _onlineDepositCommands = onlineDepositCommands;
            _paymentRepository = paymentRepository;
            _playerRepository = playerRepository;
            _messagingRepository = messagingRepository;
            _paymentSettingsCommands = paymentSettingsCommands;
            _bankAccountCommands = bankAccountCommands;
            _playerBankAccountCommands = playerBankAccountCommands;
            _transferFundCommands = transferFundCommands;
            _brandCommands = brandCommands;
            _brandRepository = brandRepository;
            _bankCommands = bankCommands;
            _paymentGatewaySettingsCommands = paymentGatewaySettingsCommands;
            _gameRepository = gameRepository;
            _walletQueries = walletQueries;
            _withdrawalService = withdrawalService;
        }

        /// <summary>
        /// This method adds delays between offline deposit process steps
        /// This allows to process events in correct order
        /// </summary>
        public void MakeDepositSelenium(Guid playerId, decimal depositAmount = 200, string bonusCode = null)
        {
            var offlineDeposit = CreateOfflineDeposit(playerId, depositAmount, bonusCode);
            Thread.Sleep(2000);
            ConfirmOfflineDeposit(offlineDeposit);
            VerifyOfflineDeposit(offlineDeposit, true);
            ApproveOfflineDeposit(offlineDeposit, true);
            Thread.Sleep(1000);
        }

        public void MakeDeposit(string username, decimal depositAmount, bool waitForProcessing = false)
        {
            var playerId = _playerRepository.Players.Single(p => p.Username == username).Id;
            MakeDeposit(playerId, depositAmount, waitForProcessing:waitForProcessing);
        }

        public Guid MakeDeposit(Guid playerId, decimal depositAmount = 200, string bonusCode = null, Guid? bonusId = null, bool waitForProcessing = false)
        {
            var offlineDeposit = CreateOfflineDeposit(playerId, depositAmount, bonusCode, bonusId);
            ConfirmOfflineDeposit(offlineDeposit);
            VerifyOfflineDeposit(offlineDeposit, true);
            ApproveOfflineDeposit(offlineDeposit, true);

            if (waitForProcessing)
            {
                TimeSpan timeout = TimeSpan.FromSeconds(20);
                WaitHelper.WaitResult(() =>
                {
                    return _gameRepository.Wallets.SingleOrDefault(x =>
                        x.PlayerId == offlineDeposit.PlayerId &&
                        x.Transactions.Any(t => t.TransactionNumber == offlineDeposit.TransactionNumber));
                }, timeout, "Timeout waiting for deposit with id {0} to be processed".Args(offlineDeposit.Id));
            }

            return offlineDeposit.Id;
        }

        public void MakeFundIn(Guid playerId, Guid destinationWalletStructureId, decimal amount, string bonusCode = null, Guid? bonusId = null)
        {
            _transferFundCommands.AddFund(new TransferFundRequest
            {
                Amount = amount,
                BonusCode = bonusCode,
                BonusId = bonusId,
                PlayerId = playerId,
                TransferType = TransferFundType.FundIn,
                WalletId = destinationWalletStructureId.ToString()
            });
        }

        public OfflineDeposit CreateOfflineDeposit(Guid playerId, decimal depositAmount, string bonusCode = null, Guid? bonusId = null)
        {
            var player = _playerRepository.Players
                .Include(x => x.Brand)
                .Single(p => p.Id == playerId);

            var brandBankAccount =
                _paymentRepository.BankAccounts.First(
                    ba => ba.Bank.BrandId == player.BrandId
                        && ba.CurrencyCode == player.CurrencyCode
                        && ba.Status == BankAccountStatus.Active);

            var offlineDeposit = Task.Run(async () => await _offlineDepositCommands.Submit(new OfflineDepositRequest
            {
                PlayerId = playerId,
                BonusCode = bonusCode,
                BonusId = bonusId,
                Amount = depositAmount,
                BankAccountId = brandBankAccount.Id
            })).Result;
            offlineDeposit.BankReferenceNumber = offlineDeposit.TransactionNumber;
            // TODO: reference number should be populated from Submit method above

            return _paymentRepository.OfflineDeposits
                .Include(x=>x.Player)
                .Include(x => x.Brand)
                .Include(x => x.BankAccount.Bank)
                .FirstOrDefault(x=>x.Id==offlineDeposit.Id);
        }

        public void ConfirmOfflineDeposit(OfflineDeposit deposit)
        {
            _offlineDepositCommands.Confirm(new OfflineDepositConfirm
            {
                Amount = deposit.Amount,
                BankId = deposit.BankAccount.Bank.Id,
                OfflineDepositType = DepositMethod.CounterDeposit,
                Id = deposit.Id,
                //PlayerAccountName = deposit.BankAccount.AccountName,
                PlayerAccountName = deposit.Player.GetFullName(),
                PlayerAccountNumber = deposit.BankAccount.AccountNumber,
                ReferenceNumber = deposit.TransactionNumber,
                TransferType = TransferType.SameBank
            }, "", new byte[0], new byte[0], new byte[0]);
        }

        public void VerifyOfflineDeposit(OfflineDeposit deposit, bool success)
        {
            if (success)
            {
                _offlineDepositCommands.Verify(deposit.Id, deposit.BankAccountId, "test verification success");
            }
            else
            {
                _offlineDepositCommands.Unverify(deposit.Id, "test verification fail", UnverifyReasons.D0001);
            }
        }

        public void ApproveOfflineDeposit(OfflineDeposit deposit, bool success, decimal fee = 0)
        {
            if (success)
            {
                _offlineDepositCommands.Approve(new OfflineDepositApprove
                {
                    Id = deposit.Id,
                    Fee = fee,
                    ActualAmount = deposit.Amount - fee,
                    Remark = "test deposit approved"
                });
            }
            else
            {
                _offlineDepositCommands.Reject(deposit.Id, "test deposit rejected");
            }
        }

        public OnlineDeposit CreateOnlineDeposit(Guid playerId, decimal depositAmount, string bonusCode = "")
        {
            var player = _playerRepository.Players.Single(p => p.Id == playerId);

            var response = Task.Run(() => _onlineDepositCommands.SubmitOnlineDepositRequest(new OnlineDepositRequest
            {
                PlayerId = playerId,
                BonusCode = bonusCode,
                Amount = depositAmount,
                RequestedBy = "TestHelper",
                CultureCode = "en-US",
                NotifyUrl = "http://notifyUrl",
                ReturnUrl = "http://returnUrl"
            })).Result;
            var deposit = _paymentRepository.OnlineDeposits.SingleOrDefault(x => x.Id == response.DepositId);

            return deposit;
        }

        public OnlineDeposit VerifyOnlineDeposit(Guid id, string remarks = "remarks")
        {
            _onlineDepositCommands.Verify(new VerifyOnlineDepositRequest
            {
                Id = id,
                Remarks =remarks
            });
            var deposit = _paymentRepository.OnlineDeposits.SingleOrDefault(x => x.Id == id);

            return deposit;
        }

        public OnlineDeposit RejectOnlineDeposit(Guid id, string remarks = "remarks")
        {
            _onlineDepositCommands.Reject(new RejectOnlineDepositRequest
            {
                Id = id,
                Remarks = remarks
            });
            var deposit = _paymentRepository.OnlineDeposits.SingleOrDefault(x => x.Id == id);

            return deposit;
        }

        public OnlineDeposit ApproveOnlineDeposit(Guid id, string remarks = "remarks")
        {
            _onlineDepositCommands.Approve(new ApproveOnlineDepositRequest
            {
                Id = id,
                Remarks = remarks
            });
            var deposit = _paymentRepository.OnlineDeposits.SingleOrDefault(x => x.Id == id);

            return deposit;
        }


        public Bank CreateBank(Guid brandId, string countryCode)
        {
            var bank = _paymentRepository
                .Banks
                .SingleOrDefault(b => b.CountryCode == countryCode && b.BrandId == brandId);

            if (bank == null)
            {
                var newBankId = _bankCommands.Add(new AddBankData
                {
                    BrandId = brandId,
                    BankId = TestDataGenerator.GetRandomBankAccountNumber(20),
                    BankName = "Test Bank",
                    CountryCode = countryCode,
                    Remarks = "Remark"
                });
                bank = _paymentRepository.Banks.SingleOrDefault(b => b.Id == newBankId);
            }

            return bank;
        }

        public PaymentSettings CreatePaymentSettings(Core.Brand.Interface.Data.Brand brand, PaymentType type, bool enable = true)
        {
            var model = new SavePaymentSettingsCommand
            {
                Id = Guid.NewGuid(),
                Licensee = brand.Licensee.Id,
                Brand = brand.Id,
                PaymentType = type,
                PaymentGatewayMethod = PaymentMethod.OfflineBank,
                PaymentMethod = PaymentMethodDto.OfflinePayMethod,
                Currency = brand.BrandCurrencies.First().CurrencyCode,
                VipLevel = brand.DefaultVipLevelId.ToString(),
                MinAmountPerTransaction = 10,
                MaxAmountPerTransaction = 200,
                MaxTransactionPerDay = 10,
                MaxTransactionPerWeek = 20,
                MaxTransactionPerMonth = 30
            };

            var paymentSettingsId = _paymentSettingsCommands.AddSettings(model);

            var ps =
                _paymentRepository.PaymentSettings
                    .Single(x => x.Id == paymentSettingsId);

            if (enable)
                _paymentSettingsCommands.Enable(ps.Id, "remark");

            return ps;
        }

        public PaymentSettings CreatePaymentSettings(Core.Brand.Interface.Data.Brand brand, PaymentType type, string vipLevel, PaymentSettingsValues settings)
        {
            var model = new SavePaymentSettingsCommand
            {
                Id = Guid.NewGuid(),
                Licensee = brand.Licensee.Id,
                Brand = brand.Id,
                PaymentType = type,
                PaymentGatewayMethod = PaymentMethod.OfflineBank,
                PaymentMethod = PaymentMethodDto.OfflinePayMethod,
                Currency = brand.BrandCurrencies.First().CurrencyCode,
                VipLevel = vipLevel,
                MinAmountPerTransaction = settings.MinAmountPerTransaction,
                MaxAmountPerTransaction = settings.MaxAmountPerTransaction,
                MaxTransactionPerDay = settings.MaxTransactionsPerDay,
                MaxTransactionPerWeek = settings.MaxTransactionsPerWeek,
                MaxTransactionPerMonth = settings.MaxTransactionsPerMonth
            };

            var paymentSettingsId = _paymentSettingsCommands.AddSettings(model);

            var ps =
                _paymentRepository.PaymentSettings
                    .Single(x => x.Id == paymentSettingsId);

            _paymentSettingsCommands.Enable(ps.Id, "remark");
            return ps;
        }

        public BankAccountType CreateBankAccountType(Guid typeId, string name)
        {
            var data = new BankAccountType
            {
                Id = typeId,
                Name = name
            };

            var bankAccountType = _paymentRepository.BankAccountTypes.Add(data);

            _paymentRepository.SaveChanges();

            return bankAccountType;
        }

        public BankAccount CreateBankAccount(Guid brandId, string currencyCode,bool activate = true)
        {
            var bank = _paymentRepository.Banks.FirstOrDefault(b => b.BrandId == brandId);
            var bankAccountType = _paymentRepository.BankAccountTypes.FirstOrDefault();

            if (bankAccountType == null)
                bankAccountType = CreateBankAccountType(Guid.NewGuid(), "Test Type");

            var data = new AddBankAccountData
            {
                BrandId = brandId.ToString(),
                AccountId = TestDataGenerator.GetRandomString(10),
                AccountName = TestDataGenerator.GetRandomString(10),
                AccountNumber = TestDataGenerator.GetRandomString(10, "1234567890"),
                Bank = bank.Id,
                AccountType = bankAccountType.Id,
                //AccountType = Guid.NewGuid(),
                Branch = "Branch",
                Currency = currencyCode,
                Province = "Province",
                SupplierName = "Supplier Name",
                ContactNumber = "911",
                USBCode = "USB001"
            };

            var bankAccountId = _bankAccountCommands.Add(data);
            if(activate)
            _bankAccountCommands.Activate(bankAccountId, "remark");
            var bankAccount = _paymentRepository.BankAccounts.FirstOrDefault(x => x.Id == bankAccountId);
            return bankAccount;
        }

        public void CreatePlayerPaymentLevel(Guid playerId, PaymentLevel paymentLevel)
        {
            _paymentRepository.PlayerPaymentLevels.Add(new PlayerPaymentLevel
            {
                PlayerId = playerId,
                PaymentLevel = paymentLevel
            });
        }

        public PaymentLevel CreatePaymentLevel(Guid brandId, string currencyCode,bool isActive=true , Guid? bankAccountId=null)
        {
            var brandBankAccounts = _paymentRepository.Banks.Include(b => b.Accounts).Single(b => b.BrandId == brandId).Accounts;
            if (bankAccountId.HasValue)
            {
                brandBankAccounts = brandBankAccounts.Where(x => x.Id == bankAccountId).ToList();
            }
            var paymentGatewaySettings = _paymentRepository.PaymentGatewaySettings.Where(b => b.BrandId == brandId).ToList();

            var hasDefaultPaymentLevel = false;
            var brand = _brandRepository.Brands.Include(x => x.BrandCurrencies)
                .SingleOrDefault(o => o.Id == brandId);

            if (brand != null)
                hasDefaultPaymentLevel = brand
                    .BrandCurrencies
                    .Any(o => o.BrandId == brandId
                        && o.CurrencyCode == currencyCode
                        && o.DefaultPaymentLevelId.HasValue
                        && o.DefaultPaymentLevelId != Guid.Empty
                        );

            var paymentLevelCode = TestDataGenerator.GetRandomString(3);
            var paymentLevelId = Guid.NewGuid();
            var paymentLevel = new PaymentLevel
            {
                Id = paymentLevelId,
                BrandId = brandId,
                CurrencyCode = currencyCode,
                DateCreated = DateTimeOffset.Now,
                Code = "PL-" + paymentLevelCode,
                Name = "PaymentLevel-" + paymentLevelCode,
                EnableOfflineDeposit = true,
                EnableOnlineDeposit = true,
                Status = isActive ? PaymentLevelStatus.Active : PaymentLevelStatus.Inactive,
                BankAccounts = brandBankAccounts,
                PaymentGatewaySettings = paymentGatewaySettings
            };

            if (!hasDefaultPaymentLevel)
                _brandCommands.MakePaymentLevelDefault(paymentLevelId, brandId, currencyCode);

            PopulateRepositoriesWhichNeedANewlyCreatedPaymentLevel(paymentLevel, paymentLevelId, paymentLevelCode);

            return paymentLevel;
        }

        public PlayerBankAccount CreatePlayerBankAccount(Guid playerId, Guid brandId, bool verify = false)
        {
            var player = _paymentRepository.Players.Single(x => x.Id == playerId);
            var bankAccountId = _playerBankAccountCommands.Add(new EditPlayerBankAccountData
            {
                Id = Guid.NewGuid(),
                PlayerId = playerId,
                Bank = _paymentRepository.Banks.First(o => o.BrandId == brandId).Id,
                AccountName = player.GetFullName(),
                AccountNumber = TestDataGenerator.GetRandomString(10, "1234567890"),
                Province = TestDataGenerator.GetRandomString(7),
                City = TestDataGenerator.GetRandomString(7),
                Branch = TestDataGenerator.GetRandomString(7),
                SwiftCode = TestDataGenerator.GetRandomString(7),
                Address = TestDataGenerator.GetRandomString(7)
            });

            if (verify)
            {
                VerifyPlayerBankAccount(bankAccountId);
            }

            return _paymentRepository.PlayerBankAccounts.FirstOrDefault(x=> x.Id ==bankAccountId);
        }

        public Currency CreateCurrency(string code, string name)
        {
            var currency = _paymentRepository.Currencies.SingleOrDefault(c => c.Code == code);
            if (currency == null)
            {
                currency = new Currency
                {
                    Code = code,
                    CreatedBy = "test",
                    DateCreated = DateTimeOffset.UtcNow,
                    Name = name,
                    Remarks = "remark"

                };
                _paymentRepository.Currencies.Add(currency);

                _paymentRepository.SaveChanges();
            }
            return currency;
        }

        private void VerifyPlayerBankAccount(Guid bankAccountId)
        {
            _playerBankAccountCommands.Verify(bankAccountId, "remark");
        }

        public PaymentGatewaySettings CreatePaymentGatewaySettings(Guid brandId, bool enable = true,
            string onlinePaymentMethodName ="",string paymentGatewayName="XPAY",int channel=-1,
            string entryPoint = "http://domain.com", string remarks = "some remarks")
        {
            if (string.IsNullOrEmpty(onlinePaymentMethodName))
            {
                onlinePaymentMethodName = TestDataGenerator.GetRandomString(50);
            }
            if (channel == -1)
            {
                channel = TestDataGenerator.GetRandomNumber(99999, 1);
            }
            var model = new SavePaymentGatewaysSettingsData
            {
                Brand = brandId,
                OnlinePaymentMethodName = onlinePaymentMethodName,
                Channel = channel,
                PaymentGatewayName = paymentGatewayName,
                EntryPoint = entryPoint,
                Remarks = remarks
            };

            var result = _paymentGatewaySettingsCommands.Add(model);

            var ps =
                _paymentRepository.PaymentGatewaySettings
                    .Single(x => x.Id == result.PaymentGatewaySettingsId);

            if (enable)
                _paymentGatewaySettingsCommands.Activate(
                    new ActivatePaymentGatewaySettingsData
                    {
                        Id = ps.Id,
                        Remarks = "remark"
                    });

            return ps;
        }

        #region Private Methods
        private void PopulateRepositoriesWhichNeedANewlyCreatedPaymentLevel(
            PaymentLevel paymentLevel,
            Guid paymentLevelId,
            string paymentLevelCode)
        {
            //Populate Payment repository
            _paymentRepository.PaymentLevels.Add(paymentLevel);

            _paymentRepository.SaveChanges();

            //Populate Messaging repository
            _messagingRepository.PaymentLevels.Add(new Core.Messaging.Data.PaymentLevel()
            {
                Id = paymentLevelId,
                Name = "PaymentLevel-Messaging-" + paymentLevelCode
            });

            _messagingRepository.SaveChanges();
        }

        #endregion

        public async void AssertBalance(Guid playerId, 
            decimal total=0,
            decimal playable = 0,
            decimal main=0,
            decimal bonus =0,
            decimal free=0,
            decimal bonusLock = 0,
            decimal withdrawalLock=0)
        {
            var balance = await _walletQueries.GetPlayerBalance(playerId);
            Assert.AreEqual(total, balance.Total,"Total");
            Assert.AreEqual(playable, balance.Playable,"Playable");
            Assert.AreEqual(main, balance.Main,"Main");
            Assert.AreEqual(bonus, balance.Bonus,"Bonsu");
            Assert.AreEqual(free, balance.Free,"Free");
            Assert.AreEqual(bonusLock, balance.BonusLock,"BonsuLock");
            Assert.AreEqual(withdrawalLock, balance.WithdrawalLock, "WithdrawalLock");
        }

        public OfflineWithdrawResponse MakeWithdraw(Guid playerId, string requestedby="admin", decimal amount = 1)
        {
            return _withdrawalService.Request(
                new OfflineWithdrawRequest
                {
                    Amount = amount,
                    NotificationType = NotificationType.None,
                    BankAccountTime = _paymentRepository.PlayerBankAccounts.First().Created.ToString(),
                    BankTime = _paymentRepository.Banks.First().Created.ToString(),
                    PlayerBankAccountId = _paymentRepository
                        .PlayerBankAccounts
                        .Include(x => x.Player)
                        .First(x => x.Player.Id == playerId)
                        .Id,
                    Remarks = "remraks",
                    RequestedBy = requestedby
                });
        }
    }
}