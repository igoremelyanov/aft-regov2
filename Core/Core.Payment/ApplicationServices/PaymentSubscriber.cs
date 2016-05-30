using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Brand.Events;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Payment;
using AFT.RegoV2.Core.Common.Events;
using AFT.RegoV2.Core.Common.Events.Brand;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Domain.Player.Events;
using AFT.RegoV2.Core.Game.Interface.Events;
using AFT.RegoV2.Core.Game.Interface.Interfaces;
using AFT.RegoV2.Core.Payment.Data;
using AFT.RegoV2.Core.Payment.Interface.ApplicationServices;
using AFT.RegoV2.Core.Payment.Interface.Data;
using AFT.RegoV2.Core.Payment.Interface.Events;
using AFT.RegoV2.Core.Player.Events;
using AFT.RegoV2.Domain.Brand.Events;
using AFT.RegoV2.Domain.Payment.Data;
using AFT.RegoV2.RegoBus.Interfaces;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Shared.Utils;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.Core.Payment.ApplicationServices
{
    public class PaymentSubscriber : IBusSubscriber,
        IConsumes<CountryCreated>,
        IConsumes<LicenseeCreated>,
        IConsumes<PlayerVipLevelChanged>,
        IConsumes<PlayerRegistered>,
        IConsumes<BrandRegistered>,
        IConsumes<PlayerUpdated>,
        IConsumes<BetPlaced>,
        IConsumes<VipLevelUpdated>,
        IConsumes<VipLevelRegistered>,
        IConsumes<BrandCurrenciesAssigned>,
        IConsumes<PlayerActivated>,
        IConsumes<PlayerDeactivated>,
        IConsumes<PlayerSelfExcluded>,
        IConsumes<PlayerTimedOut>,
        IConsumes<PlayerCancelExclusion>,
        IConsumes<DepositSubmitted>,
        IConsumes<Interface.Commands.Deposit>,
        IConsumes<Interface.Commands.WithdrawRequestSubmit>,
        IConsumes<Interface.Commands.WithdrawRequestApprove>,
        IConsumes<Interface.Commands.WithdrawRequestCancel>,
        IConsumes<DepositConfirmed>,
        IConsumes<DepositVerified>,
        IConsumes<DepositApproved>,
        IConsumes<DepositUnverified>,
        IConsumes<DepositRejected>,
        IConsumes<PlayerPaymentLevelChanged>
    {
        private const string DepositRecordNotFoundMessage = "deposit record '{0}' was not found";
        private const string WithdrawRecordNotFoundMessage = "withdraw record '{0}' was not found";

        private readonly IUnityContainer _container;

        public PaymentSubscriber(IUnityContainer container)
        {
            _container = container;
        }

        public void Consume(CountryCreated @event)
        {
            var paymentRepository = _container.Resolve<IPaymentRepository>();

            if (paymentRepository.Countries.Any(x => x.Code == @event.Code))
                return;

            paymentRepository.Countries.Add(new Data.Country
            {
                Code = @event.Code,
                Name = @event.Name
            });

            paymentRepository.SaveChanges();
        }

        public void Consume(LicenseeCreated @event)
        {
            var paymentRepository = _container.Resolve<IPaymentRepository>();

            var licensee = paymentRepository.Licensees.SingleOrDefault(x => x.Id == @event.Id);

            if (licensee != null) return;

            paymentRepository.Licensees.Add(new Data.Licensee
            {
                Id = @event.Id,
                Name = @event.Name
            });

            paymentRepository.SaveChanges();
        }

        public void Consume(PlayerVipLevelChanged @event)
        {
            var paymentRepository = _container.Resolve<IPaymentRepository>();

            var player = paymentRepository.Players.FirstOrDefault(x => x.Id == @event.PlayerId);
            if (player == null)
                return;

            player.VipLevelId = @event.VipLevelId;
            paymentRepository.SaveChanges();
        }

        public void Consume(PlayerRegistered @event)
        {
            var paymentRepository = _container.Resolve<IPaymentRepository>();
            var brandQueries = _container.Resolve<BrandQueries>();

            var player = @event;

            var defaultPaymentLevelId = brandQueries.GetDefaultPaymentLevelId(player.BrandId, player.CurrencyCode);

            var paymentLevel = player.PaymentLevelId.HasValue
                ? paymentRepository.PaymentLevels.SingleOrDefault(l => l.Id == player.PaymentLevelId.Value)
                : paymentRepository.PaymentLevels.SingleOrDefault(l => l.Id == defaultPaymentLevelId);

            if (paymentLevel == null)
            {
                throw new RegoException("No appropriate payment level found. Brand: " + player.BrandId + " Currency: " +
                                    player.CurrencyCode);
            }
            paymentRepository.PlayerPaymentLevels.Add(new Data.PlayerPaymentLevel
            {
                PlayerId = player.PlayerId,
                PaymentLevel = paymentLevel
            });

            var newPlayer = AutoMapper.Mapper.DynamicMap<Data.Player>(player);
            newPlayer.Id = player.PlayerId;

            paymentRepository.Players.Add(newPlayer);
            paymentRepository.SaveChanges();
        }

        public void Consume(BrandRegistered @event)
        {
            var paymentRepository = _container.Resolve<IPaymentRepository>();

            var brand = paymentRepository.Brands.SingleOrDefault(x => x.Id == @event.Id);
            if (brand == null)
            {
                paymentRepository.Brands.Add(new Data.Brand
                {
                    Id = @event.Id,
                    Name = @event.Name,
                    Code = @event.Code,
                    LicenseeId = @event.LicenseeId,
                    LicenseeName = @event.LicenseeName,
                    TimezoneId = @event.TimeZoneId
                });
                paymentRepository.SaveChanges();
            }
        }

        public void Consume(PlayerUpdated @event)
        {
            var paymentRepository = _container.Resolve<IPaymentRepository>();

            var playerPaymentLevel =
                paymentRepository.PlayerPaymentLevels.FirstOrDefault(p => p.PlayerId == @event.Id);

            if (playerPaymentLevel != null)
            {
                var paymentLevel = paymentRepository.PaymentLevels.Single(p => p.Id == @event.PaymentLevelId);
                playerPaymentLevel.PaymentLevel = paymentLevel;
                paymentRepository.SaveChanges();
            }
        }

        public void Consume(BetPlaced @event)
        {
            var paymentRepository = _container.Resolve<IPaymentRepository>();

            var playerId = @event.PlayerId;
            var betAmount = @event.Amount;
            var offlineDeposits =
                paymentRepository
                    .OfflineDeposits
                    .Where(
                        x =>
                            x.Player.Id == playerId && x.DepositWagering != 0 &&
                            x.Status == OfflineDepositStatus.Approved)
                    .OrderBy(x => x.Created)
                    .ToArray();
            if (!offlineDeposits.Any())
                return;

            var count = 0;
            while (betAmount != 0 && count < offlineDeposits.Length)
            {
                var deposit = offlineDeposits[count];
                deposit.DepositWagering = deposit.DepositWagering - betAmount;

                if (deposit.DepositWagering >= 0)
                    betAmount = 0;
                else
                    betAmount = -1 * deposit.DepositWagering;

                if (deposit.DepositWagering < 0)
                    deposit.DepositWagering = 0;

                count++;
            }
            paymentRepository.SaveChanges();
        }

        public void Consume(VipLevelUpdated @event)
        {
            var paymentRepository = _container.Resolve<IPaymentRepository>();

            var vipLevel = paymentRepository.VipLevels.FirstOrDefault(x => x.Id == @event.Id);

            if (vipLevel == null)
                throw new RegoException("No appropriate Vip Level found. Brand: " + @event.BrandId + " Code: " + @event.Code);

            vipLevel.BrandId = @event.BrandId;
            vipLevel.Name = @event.Name;

            paymentRepository.SaveChanges();
        }

        public void Consume(VipLevelRegistered @event)
        {
            var paymentRepository = _container.Resolve<IPaymentRepository>();

            paymentRepository.VipLevels.Add(new Data.VipLevel
            {
                Id = @event.Id,
                Name = @event.Name,
                BrandId = @event.BrandId
            });

            paymentRepository.SaveChanges();
        }

        public void Consume(BrandCurrenciesAssigned message)
        {
            var paymentRepository = _container.Resolve<IPaymentRepository>();

            //reassign Base currency
            var paymentBrand = paymentRepository.Brands.Single(x => x.Id == message.BrandId);
            var oldBaseCurrencyCode = paymentBrand.BaseCurrencyCode;

            if (oldBaseCurrencyCode != message.BaseCurrency)
            {
                //remove old Currency Exchanges
                var oldCurrencyExchanges = paymentRepository.CurrencyExchanges
                    .Where(x => x.BrandId == message.BrandId)
                    .ToArray();

                foreach (var oldCurrencyExchange in oldCurrencyExchanges)
                {
                    paymentRepository.CurrencyExchanges.Remove(oldCurrencyExchange);
                }

                paymentRepository.SaveChanges();

                //remove old Currencies
                var oldCurrencies = paymentRepository.BrandCurrencies
                    .Where(x => x.BrandId == message.BrandId)
                    .ToArray();

                foreach (var oldCurrency in oldCurrencies)
                {
                    paymentRepository.BrandCurrencies.Remove(oldCurrency);
                }

                paymentRepository.SaveChanges();

                //add new currencies
                foreach (var newCurrency in message.Currencies)
                {
                    var brandCurrency = new Data.BrandCurrency { BrandId = message.BrandId, CurrencyCode = newCurrency };

                    paymentRepository.BrandCurrencies.AddOrUpdate(brandCurrency);
                }

                paymentRepository.SaveChanges();

                //Set Base Currency
                paymentBrand.BaseCurrencyCode = message.BaseCurrency;
                paymentRepository.SaveChanges();

                //add new base currency exchange
                var baseCurrencyExchange = new Data.CurrencyExchange
                {
                    BrandId = message.BrandId,
                    CurrencyToCode = message.BaseCurrency,
                    IsBaseCurrency = true,
                    CurrentRate = 1,
                    //todo: need to update
                    CreatedBy = "System",
                    DateCreated = DateTimeOffset.Now.ToBrandOffset(paymentBrand.TimezoneId),
                };

                paymentRepository.CurrencyExchanges.AddOrUpdate(baseCurrencyExchange);
                paymentRepository.SaveChanges();

                return;
            }

            //remove not exist Currency Exchanges
            var notExistCurrencyExchanges = paymentRepository.CurrencyExchanges
                    .Where(x => x.BrandId == message.BrandId &&
                        !message.Currencies.Contains(x.CurrencyToCode))
                    .ToArray();

            foreach (var oldCurrencyExchange in notExistCurrencyExchanges)
            {
                paymentRepository.CurrencyExchanges.Remove(oldCurrencyExchange);
            }

            paymentRepository.SaveChanges();

            //remove not exist Currencies
            var notExistCurrencies = paymentRepository.BrandCurrencies
                .Where(x => x.BrandId == message.BrandId &&
                    !message.Currencies.Contains(x.CurrencyCode))
                .ToArray();

            foreach (var oldCurrency in notExistCurrencies)
            {
                paymentRepository.BrandCurrencies.Remove(oldCurrency);
            }

            paymentRepository.SaveChanges();

            //add new currencies
            var existingCurrencies = paymentRepository.BrandCurrencies
                .Where(x => x.BrandId == message.BrandId).Select(c => c.CurrencyCode).ToArray();

            var newCurrencies =
                message.Currencies.Where(c =>
                    !existingCurrencies.Contains(c)).ToArray();

            foreach (var newCurrency in newCurrencies)
            {
                var brandCurrency = new Data.BrandCurrency { BrandId = message.BrandId, CurrencyCode = newCurrency };

                paymentRepository.BrandCurrencies.AddOrUpdate(brandCurrency);
            }

            paymentRepository.SaveChanges();
        }

        public void Consume(PlayerActivated message)
        {
            var paymentRepository = _container.Resolve<IPaymentRepository>();

            var player = paymentRepository.Players
                .Single(o => o.Id == message.PlayerId);

            player.IsActive = true;

            paymentRepository.Players.AddOrUpdate(player);
            paymentRepository.SaveChanges();
        }

        public void Consume(PlayerDeactivated message)
        {
            var paymentRepository = _container.Resolve<IPaymentRepository>();

            var player = paymentRepository.Players
                .Single(o => o.Id == message.PlayerId);

            player.IsActive = false;

            paymentRepository.Players.AddOrUpdate(player);
            paymentRepository.SaveChanges();
        }

        public void Consume(PlayerSelfExcluded message)
        {
            var paymentRepository = _container.Resolve<IPaymentRepository>();

            var player = paymentRepository.Players
                .Single(o => o.Id == message.PlayerId);

            player.IsSelfExclude = true;
            player.SelfExcludeEndDate = message.SelfExclusionEndDate;
            player.IsTimeOut = false;
            player.TimeOutEndDate = null;

            paymentRepository.Players.AddOrUpdate(player);
            paymentRepository.SaveChanges();
        }

        public void Consume(PlayerTimedOut message)
        {
            var paymentRepository = _container.Resolve<IPaymentRepository>();

            var player = paymentRepository.Players
                .Single(o => o.Id == message.PlayerId);

            player.IsTimeOut = true;
            player.TimeOutEndDate = message.TimeOutEndDate;
            player.IsSelfExclude = false;
            player.SelfExcludeEndDate = null;

            paymentRepository.Players.AddOrUpdate(player);
            paymentRepository.SaveChanges();
        }

        public void Consume(PlayerCancelExclusion message)
        {
            var paymentRepository = _container.Resolve<IPaymentRepository>();

            var player = paymentRepository.Players
                .Single(o => o.Id == message.PlayerId);

            player.IsSelfExclude = false;
            player.SelfExcludeEndDate = null;
            player.IsTimeOut = false;
            player.TimeOutEndDate = null;

            paymentRepository.Players.AddOrUpdate(player);
            paymentRepository.SaveChanges();
        }

        public void Consume(DepositSubmitted submittedEvent)
        {
            var paymentRepository = _container.Resolve<IPaymentRepository>();

            var record = paymentRepository.Deposits.SingleOrDefault(r => r.Id == submittedEvent.DepositId);
            if (record != null)
                throw new RegoException(string.Format("deposit record '{0}' already exists", submittedEvent.DepositId));

            var player = paymentRepository.Players
                .SingleOrDefault(x => x.Id == submittedEvent.PlayerId);
            if (player == null)
                throw new RegoException(string.Format("plaer '{0}' not found", submittedEvent.PlayerId));
            var brand = paymentRepository.Brands.FirstOrDefault(x => x.Id == player.BrandId);

            if (brand == null)
                throw new RegoException(string.Format("brand '{0}' not found", player.BrandId));

            record = new Deposit
            {
                Id = submittedEvent.DepositId,
                Licensee = brand.LicenseeName,
                BrandId = brand.Id,
                PlayerId = player.Id,
                ReferenceCode = submittedEvent.TransactionNumber,
                PaymentMethod = submittedEvent.PaymentMethod,
                CurrencyCode = submittedEvent.CurrencyCode,
                Amount = submittedEvent.Amount,
                UniqueDepositAmount = 0,
                Status = submittedEvent.Status,
                DateSubmitted = submittedEvent.Submitted,
                SubmittedBy = submittedEvent.SubmittedBy,
                DepositType = submittedEvent.DepositType,
                BankAccountId = submittedEvent.BankAccount
            };
            paymentRepository.Deposits.Add(record);
            paymentRepository.SaveChanges();
        }

        public void Consume(DepositConfirmed confirmedEvent)
        {
            var paymentRepository = _container.Resolve<IPaymentRepository>();

            var record = paymentRepository.Deposits.SingleOrDefault(r => r.Id == confirmedEvent.DepositId);
            if (record == null)
                throw new RegoException(string.Format(DepositRecordNotFoundMessage, confirmedEvent.DepositId));

            record.Amount = confirmedEvent.Amount;
            record.Status = OfflineDepositStatus.Processing.ToString();
            paymentRepository.SaveChanges();
        }

        public void Consume(DepositVerified verifiedEvent)
        {
            var paymentRepository = _container.Resolve<IPaymentRepository>();

            var record = paymentRepository.Deposits.SingleOrDefault(r => r.Id == verifiedEvent.DepositId);
            if (record == null)
                throw new RegoException(string.Format(DepositRecordNotFoundMessage, verifiedEvent.DepositId));

            if (record.Status != OfflineDepositStatus.Processing.ToString())
                throw new RegoException(string.Format("deposit record '{0}' was not in 'Processing' state", verifiedEvent.DepositId));

            record.Status = OfflineDepositStatus.Verified.ToString();
            record.DateVerified = verifiedEvent.Verified;
            record.VerifiedBy = verifiedEvent.VerifiedBy;
            record.BankAccountId = verifiedEvent.BankAccountId;
            paymentRepository.SaveChanges();
        }

        public void Consume(DepositApproved approvedEvent)
        {
            var paymentRepository = _container.Resolve<IPaymentRepository>();

            var record = paymentRepository.Deposits.SingleOrDefault(r => r.Id == approvedEvent.DepositId);
            if (record == null)
                throw new RegoException(string.Format(DepositRecordNotFoundMessage, approvedEvent.DepositId));
            if ((record.DepositType == DepositType.Offline && record.Status != OfflineDepositStatus.Verified.ToString()) ||
                (record.DepositType == DepositType.Online && (record.Status == OnlineDepositStatus.Approved.ToString() || record.Status == OnlineDepositStatus.Rejected.ToString()))
                )
                throw new RegoException(string.Format("deposit record '{0}' was in '{1}' state, can't be approved", approvedEvent.DepositId, record.Status));

            record.Status = OfflineDepositStatus.Approved.ToString();
            record.DateApproved = approvedEvent.Approved;
            record.ApprovedBy = approvedEvent.ApprovedBy;
            paymentRepository.SaveChanges();
        }

        public void Consume(DepositUnverified @event)
        {
            var paymentRepository = _container.Resolve<IPaymentRepository>();

            var record = paymentRepository.Deposits.SingleOrDefault(r => r.Id == @event.DepositId);
            if (record == null)
                throw new RegoException(string.Format(DepositRecordNotFoundMessage, @event.DepositId));

            record.Status = @event.Status.ToString();
            record.DateUnverified = @event.Unverified;
            record.UnverifiedBy = @event.UnverifiedBy;
            record.UnverifyReason = @event.UnverifyReason;

            paymentRepository.SaveChanges();
        }

        public void Consume(DepositRejected rejectedEvent)
        {
            var paymentRepository = _container.Resolve<IPaymentRepository>();

            var record = paymentRepository.Deposits.SingleOrDefault(r => r.Id == rejectedEvent.DepositId);
            if (record == null)
                throw new RegoException(string.Format(DepositRecordNotFoundMessage, rejectedEvent.DepositId));

            record.Status = OfflineDepositStatus.Rejected.ToString();
            record.DateRejected = rejectedEvent.Rejected;
            record.RejectedBy = rejectedEvent.RejectedBy;
            paymentRepository.SaveChanges();
        }

        public void Consume(PlayerPaymentLevelChanged @event)
        {
            var paymentRepository = _container.Resolve<IPaymentRepository>();

            var playerPaymentLevel = paymentRepository.PlayerPaymentLevels.FirstOrDefault(x => x.PlayerId == @event.PlayerId);

            if (playerPaymentLevel == null)
                return;
            var paymentLevel = paymentRepository.PaymentLevels.FirstOrDefault(x => x.Id == @event.NewPaymentLevelId);
            if (paymentLevel == null)
                return;

            playerPaymentLevel.PaymentLevel = paymentLevel;
            paymentRepository.SaveChanges();
        }

        public void Consume(Interface.Commands.Deposit command)
        {
            var brandOperations = _container.Resolve<IBrandOperations>();
            var serviceBus = _container.Resolve<IServiceBus>();

            var fundIndAmount = command.Amount + command.Fee;
            var gameBalance = brandOperations.FundIn(command.PlayerId, fundIndAmount, command.CurrencyCode, command.ReferenceCode);

            var eventCreated = DateTimeOffset.Now.ToOffset(command.Approved.Offset);

            serviceBus.PublishMessage(new DepositApproved
            {
                ReferenceCode = command.ReferenceCode,
                EventCreatedBy = command.ActorName,
                DepositId = command.DepositId,
                PlayerId = command.PlayerId,
                ActualAmount = command.Amount,
                Fee = command.Fee,
                Remarks = command.Remarks,
                DepositType = command.DepositType,
                Approved = command.Approved,
                ApprovedBy = command.ApprovedBy,
                DepositWagering = command.DepositWagering,
                Deposited = eventCreated,
                EventCreated = eventCreated,
            });
        }

        public void Consume(Interface.Commands.WithdrawRequestSubmit command)
        {
            var paymentRepository = _container.Resolve<IPaymentRepository>();
            var brandOperations = _container.Resolve<IBrandOperations>();
            var serviceBus = _container.Resolve<IServiceBus>();
            var withdrawalService = _container.Resolve<IWithdrawalService>();

            //move money out of Game service
            var player = paymentRepository.Players.FirstOrDefault(x => x.Id == command.PlayerId);
            var gameBalance = brandOperations.FundOut(command.PlayerId, command.Amount, player.CurrencyCode, command.ReferenceCode);

            var bankAccount =
            paymentRepository.PlayerBankAccounts.Include(x => x.Player)
                .Include(x => x.Bank)
                .SingleOrDefault(x => x.Id == command.PlayerBankAccountId);

            bankAccount.EditLock = true;

            var withdrawal = new Data.OfflineWithdraw();
            withdrawal.Id = command.WithdrawId;
            withdrawal.PlayerBankAccount = bankAccount;
            withdrawal.TransactionNumber = command.ReferenceCode;
            withdrawal.Amount = command.Amount;
            withdrawal.CreatedBy = command.RequestedBy;
            withdrawal.Created = command.Requested;
            withdrawal.Remarks = command.Remarks;
            withdrawal.Status = WithdrawalStatus.New;

            //lock money for withdrawal in our wallet
            var withdrawalLock = new WithdrawalLock
            {
                Id = command.LockId,
                PlayerId = command.PlayerId,
                WithdrawalId = command.WithdrawId,
                Amount = command.Amount,
                Status = Status.Active,
                LockedOn = DateTimeOffset.Now.ToOffset(command.Requested.Offset),
                LockedBy = command.RequestedBy
            };

            var eventCreated = DateTimeOffset.Now.ToOffset(command.Requested.Offset);

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                paymentRepository.OfflineWithdraws.Add(withdrawal);
                paymentRepository.WithdrawalLocks.Add(withdrawalLock);
                paymentRepository.SaveChanges();

                //raise WithdrawalCreated event
                var withdrawalCreatedEvent = new WithdrawalCreated(
                    command.WithdrawId,
                    command.Amount,
                    eventCreated,
                    command.PlayerId,
                    command.PlayerId,
                    WithdrawalStatus.New,
                    command.Remarks,
                    command.ReferenceCode,
                    command.ActorName)
                {
                    EventCreated = eventCreated,
                    EventCreatedBy = command.ActorName,
                };

                serviceBus.PublishMessage(withdrawalCreatedEvent);

                scope.Complete();

                //TODO:AFTREGO-4131 implement behavior stated below:
                //there is still as small chance that some amount will be spent in Game subdomain 
                //after we performed balance check and before money removed from Game balance
                //it means that we need to detect such situation by performing additional validations after WithdrawRequestSubmitted event
                //and automatically reject withdrawal request if some inconsistencies were found
            }
            withdrawalService.WithdrawalStateMachine(command.WithdrawId);
        }

        public void Consume(Interface.Commands.WithdrawRequestApprove command)
        {
            var paymentRepository = _container.Resolve<IPaymentRepository>();
            var serviceBus = _container.Resolve<IServiceBus>();

            var withdrawal = paymentRepository.OfflineWithdraws
                    .Include(x => x.PlayerBankAccount.Player)
                    .FirstOrDefault(x => x.Id == command.WithdrawId);

            if (withdrawal == null)
                throw new RegoException(string.Format(WithdrawRecordNotFoundMessage, command.WithdrawId));

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                if (withdrawal.Status != WithdrawalStatus.Accepted)
                    throw new InvalidOperationException(string.Format("The withdrawal has \"{0}\" status, so it can't be Approve, id:{1}", withdrawal.Status, command.WithdrawId));

                withdrawal.Remarks = command.Remarks;
                withdrawal.Status = WithdrawalStatus.Approved;
                withdrawal.Approved = command.Approved;
                withdrawal.ApprovedBy = command.ActorName;

                var eventCreated = DateTimeOffset.Now.ToOffset(command.Approved.Offset);

                //UnLock WithdrawLock
                var withdrawLock =
                    paymentRepository.WithdrawalLocks.FirstOrDefault(x => x.WithdrawalId == command.WithdrawId);
                withdrawLock.Status = Status.Inactive;
                withdrawLock.UnLockedBy = command.ActorName;
                withdrawLock.UnLockedOn = eventCreated;

                paymentRepository.SaveChanges();

                //raise WithdrawalApproved event
                var withdrawalApprovedEvent = new WithdrawalApproved(
                    command.WithdrawId,
                    withdrawal.Amount,
                    eventCreated,
                    command.ApprovedUerId,
                    withdrawal.PlayerBankAccount.Player.Id,
                    WithdrawalStatus.Approved,
                    command.Remarks,
                    withdrawal.TransactionNumber,
                    command.ActorName)
                {
                    EventCreatedBy = command.ActorName,
                    EventCreated = command.Approved,
                };

                serviceBus.PublishMessage(withdrawalApprovedEvent);

                scope.Complete();
            }
        }

        public void Consume(Interface.Commands.WithdrawRequestCancel command)
        {
            var paymentRepository = _container.Resolve<IPaymentRepository>();
            var serviceBus = _container.Resolve<IServiceBus>();
            var brandOperations = _container.Resolve<IBrandOperations>();

            var withdrawal = paymentRepository.OfflineWithdraws
                    .Include(x => x.PlayerBankAccount.Player)
                    .FirstOrDefault(x => x.Id == command.WithdrawId);

            if (withdrawal == null)
                throw new RegoException(string.Format(WithdrawRecordNotFoundMessage, command.WithdrawId));

            if (command.Status == WithdrawalStatus.Unverified)
            {
                if (withdrawal.Status != WithdrawalStatus.AutoVerificationFailed
                && withdrawal.Status != WithdrawalStatus.Reverted
                && withdrawal.Status != WithdrawalStatus.Investigation
                && withdrawal.Status != WithdrawalStatus.Documents)
                {
                    throw new InvalidOperationException(string.Format("The withdrawal has \"{0}\" status, so it can't be {1}, id:{2}", withdrawal.Status, "Unverified", command.WithdrawId));
                }
            }

            //Put the money back to game domain
            var gameBalance = brandOperations.FundIn(withdrawal.PlayerBankAccount.Player.Id, withdrawal.Amount,
                withdrawal.PlayerBankAccount.Player.CurrencyCode, withdrawal.TransactionNumber + "-C");

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                withdrawal.Remarks = command.Remarks;
                withdrawal.Status = command.Status;
                if (command.Status == WithdrawalStatus.Unverified)
                {
                    withdrawal.Unverified = command.Canceled;
                    withdrawal.UnverifiedBy = command.ActorName;
                }
                else
                {
                    withdrawal.CanceledTime = command.Canceled;
                    withdrawal.CanceledBy = command.ActorName;
                }

                var eventCreated = DateTimeOffset.Now.ToOffset(command.Canceled.Offset);

                //UnLock WithdrawLock
                var withdrawLock =
                    paymentRepository.WithdrawalLocks.FirstOrDefault(x => x.WithdrawalId == command.WithdrawId);
                withdrawLock.Status = Status.Inactive;
                withdrawLock.UnLockedBy = command.ActorName;
                withdrawLock.UnLockedOn = eventCreated;

                paymentRepository.SaveChanges();

                //raise WithdrawalCancel event
                var withdrawalCancelledEvent = new WithdrawalCancelled(
                    command.WithdrawId,
                    withdrawal.Amount,
                    eventCreated,
                    command.CanceledUserId,
                    withdrawal.PlayerBankAccount.Player.Id,
                    command.Status,
                    command.Remarks,
                    withdrawal.TransactionNumber,
                    command.ActorName)
                {
                    EventCreated = eventCreated,
                    EventCreatedBy = command.ActorName,
                };

                serviceBus.PublishMessage(withdrawalCancelledEvent);

                scope.Complete();
            }
        }
    }
}