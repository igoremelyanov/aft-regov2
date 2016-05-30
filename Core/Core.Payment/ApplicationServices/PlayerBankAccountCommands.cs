using System;
using System.Data.Entity;
using System.Linq;
using System.Transactions;
using AFT.RegoV2.Core.Auth.Interface;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Payment;
using AFT.RegoV2.Core.Common.Data.Player;
using AFT.RegoV2.Core.Common.Events;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Messaging.Interface.ApplicationServices;
using AFT.RegoV2.Core.Messaging.Interface.Data;
using AFT.RegoV2.Core.Messaging.Interface.Data.MessageTemplateModels;
using AFT.RegoV2.Core.Payment.Interface.ApplicationServices;
using AFT.RegoV2.Core.Payment.Interface.Data;
using AFT.RegoV2.Core.Payment.Interface.Events;
using AFT.RegoV2.Core.Payment.Validators;
using AFT.RegoV2.RegoBus.Interfaces;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Shared.Utils;
using FluentValidation.Results;
using PlayerBankAccount = AFT.RegoV2.Core.Payment.Data.PlayerBankAccount;

namespace AFT.RegoV2.Core.Payment.ApplicationServices
{
    public class PlayerBankAccountCommands : IPlayerBankAccountCommands
    {
        private readonly IPaymentRepository _repository;
        private readonly IPaymentQueries _queries;
        private readonly IActorInfoProvider _actorInfoProvider;
        private readonly IEventBus _eventBus;
        private readonly IMessageTemplateService _messageService;

        public PlayerBankAccountCommands(
            IPaymentRepository repository,
            IPaymentQueries queries,
            IActorInfoProvider actorInfoProvider,
            IEventBus eventBus,
            IMessageTemplateService messageService)
        {
            _repository = repository;
            _queries = queries;
            _actorInfoProvider = actorInfoProvider;
            _eventBus = eventBus;
            _messageService = messageService;
        }

        public ValidationResult ValidateThatPlayerBankAccountCanBeEdited(EditPlayerBankAccountData data)
        {
            var validator = new EditPlayerBankAccountValidator(_repository, _queries);
            return validator.Validate(data);
        }

        public ValidationResult ValidateThatPlayerBankAccountCanBeAdded(EditPlayerBankAccountData data)
        {
            var validator = new AddPlayerBankAccountValidator(_repository, _queries);
            return validator.Validate(data);
        }

        public ValidationResult ValidateThatPlayerBankAccountCanBeSet(SetCurrentPlayerBankAccountData data)
        {
            var validator = new SetCurrentPlayerBankAccountValidator(_repository);
            return validator.Validate(data);
        }

        public Guid Add(EditPlayerBankAccountData model)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope(IsolationLevel.RepeatableRead))
            {
                var validationResult = new AddPlayerBankAccountValidator(_repository, _queries).Validate(model);

                if (!validationResult.IsValid)
                {
                    throw new RegoValidationException(validationResult);
                }

                var player = _repository.Players
                    .Include(x => x.CurrentBankAccount)
                    .Single(x => x.Id == model.PlayerId);

                var bank = _repository.Banks
                    .Include(x => x.Brand)
                    .Single(x => x.Id == model.Bank);

                var bankAccount = new PlayerBankAccount
                {
                    Id = Guid.NewGuid(),
                    Player = player,
                    Status = BankAccountStatus.Pending,
                    Bank = bank,
                    Province = model.Province,
                    City = model.City,
                    Branch = model.Branch,
                    SwiftCode = model.SwiftCode,
                    Address = model.Address,
                    AccountName = model.AccountName,
                    AccountNumber = model.AccountNumber,
                    Created = DateTimeOffset.Now.ToBrandOffset(bank.Brand.TimezoneId),
                    CreatedBy = _actorInfoProvider.Actor.UserName
                };

                if (player.CurrentBankAccount == null || player.CurrentBankAccount.Status != BankAccountStatus.Active)
                {
                    if (player.CurrentBankAccount != null)
                        player.CurrentBankAccount.IsCurrent = false;

                    player.CurrentBankAccount = bankAccount;
                    bankAccount.IsCurrent = true;
                }

                _repository.PlayerBankAccounts.Add(bankAccount);
                _repository.SaveChanges();

                _eventBus.Publish(new PlayerBankAccountAdded
                {
                    Id = bankAccount.Id,
                    Name = bankAccount.AccountName,
                    Number = bankAccount.AccountNumber,
                    EventCreated = bankAccount.Created,
                });

                scope.Complete();
                return bankAccount.Id;
            }
        }

        public void Edit(EditPlayerBankAccountData model)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope(IsolationLevel.RepeatableRead))
            {
                var validationResult = new EditPlayerBankAccountValidator(_repository, _queries).Validate(model);

                if (!validationResult.IsValid)
                {
                    throw new RegoValidationException(validationResult);
                }

                var bank = _repository.Banks.Single(x => x.Id == model.Bank);

                var bankAccount = _repository.PlayerBankAccounts
                    .Include(x => x.Player.CurrentBankAccount)
                    .Include(x => x.Player.Brand)
                    .Include(x => x.Bank)
                    .Single(x => x.Id == model.Id.Value);

                var isModified =
                    bankAccount.Bank.Id != bank.Id ||
                    bankAccount.Province != model.Province ||
                    bankAccount.City != model.City ||
                    bankAccount.Branch != model.Branch ||
                    bankAccount.SwiftCode != model.SwiftCode ||
                    bankAccount.Address != model.Address ||
                    bankAccount.AccountName != model.AccountName ||
                    bankAccount.AccountNumber != model.AccountNumber;

                if (isModified)
                {
                    bankAccount.Status = BankAccountStatus.Pending;
                }

                bankAccount.Bank = bank;
                bankAccount.Province = model.Province;
                bankAccount.City = model.City;
                bankAccount.Branch = model.Branch;
                bankAccount.SwiftCode = model.SwiftCode;
                bankAccount.Address = model.Address;
                bankAccount.AccountName = model.AccountName;
                bankAccount.AccountNumber = model.AccountNumber;
                bankAccount.Updated = DateTimeOffset.Now.ToBrandOffset(bankAccount.Player.Brand.TimezoneId);
                bankAccount.UpdatedBy = _actorInfoProvider.Actor.UserName;

                _repository.SaveChanges();
                _eventBus.Publish(new PlayerBankAccountEdited
                {
                    Id = bankAccount.Id,
                    Name = bankAccount.AccountName,
                    Number = bankAccount.AccountNumber,
                    EventCreated = bankAccount.Updated.Value,
                });

                scope.Complete();
            }
        }

        public void SetCurrent(PlayerBankAccountId playerBankAccountId)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var setCurrentPlayerBankAccountCommand = new SetCurrentPlayerBankAccountData
                {
                    PlayerBankAccountId = playerBankAccountId
                };

                var validationResult = new SetCurrentPlayerBankAccountValidator(_repository).Validate(setCurrentPlayerBankAccountCommand);

                if (!validationResult.IsValid)
                {
                    throw new RegoValidationException(validationResult);
                }

                var bankAccount = _repository.PlayerBankAccounts
                    .Include(x => x.Player.CurrentBankAccount)
                    .Include(x => x.Player.Brand)
                    .Include(x => x.Bank)
                    .Single(x => x.Id == setCurrentPlayerBankAccountCommand.PlayerBankAccountId);

                bankAccount.Player.CurrentBankAccount.IsCurrent = false;
                bankAccount.Player.CurrentBankAccount = bankAccount;
                bankAccount.IsCurrent = true;

                _repository.SaveChanges();

                _eventBus.Publish(new PlayerBankAccountCurrentSet(
                    bankAccount.Player.Id,
                    bankAccount.Id,
                    bankAccount.AccountNumber)
                {
                    EventCreated = DateTimeOffset.Now.ToBrandOffset(bankAccount.Player.Brand.TimezoneId),
            });

                scope.Complete();
            }
        }

        public void Verify(PlayerBankAccountId playerBankAccountId, string remarks)
        {
            var validationResult =
                    new VerifyPlayerBankAccountValidator(_repository).Validate(new VerifyPlayerBankAccountData
                    {
                        Id = playerBankAccountId
                    });

            if (!validationResult.IsValid)
            {
                throw new RegoException(validationResult.Errors.First().ErrorMessage);
            }

            var bankAccount = _repository.PlayerBankAccounts
                .Include(x => x.Player)
                .Include(x => x.Bank)
				.Include(x => x.Player.Brand)
                .Single(x => x.Id == playerBankAccountId);

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                

                bankAccount.Status = BankAccountStatus.Verified;
                bankAccount.Remarks = remarks;
                bankAccount.VerifiedBy = _actorInfoProvider.Actor.UserName;

                bankAccount.Verified = DateTimeOffset.Now.ToBrandOffset(bankAccount.Player.Brand.TimezoneId);

                _repository.SaveChanges();

                _eventBus.Publish(new PlayerBankAccountVerified(
                    bankAccount.Player.Id,
                    bankAccount.Id,
                    bankAccount.AccountNumber,
                    bankAccount.Remarks)
                {
                    EventCreated = bankAccount.Verified.Value,
                });

                scope.Complete();
            }

            _messageService.TrySendPlayerMessage(
                bankAccount.Player.Id,
                MessageType.PlayerBankAccountApproved,
                MessageDeliveryMethod.Email,
                new PlayerBankAccountApprovedModel());
        }

        public void Reject(PlayerBankAccountId playerBankAccountId, string remarks)
        {
            var validationResult =
                    new RejectPlayerBankAccountValidator(_repository).Validate(new RejectPlayerBankAccountData
                    {
                        Id = playerBankAccountId
                    });

            if (!validationResult.IsValid)
            {
                throw new RegoException(validationResult.Errors.First().ErrorMessage);
            }

            var bankAccount = _repository.PlayerBankAccounts
                .Include(x => x.Player)
				.Include(x => x.Player.Brand)
                .Include(x => x.Bank)
                .Single(x => x.Id == playerBankAccountId);

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                bankAccount.Status = BankAccountStatus.Rejected;
                bankAccount.Remarks = remarks;
                bankAccount.RejectedBy = _actorInfoProvider.Actor.UserName;
                bankAccount.Rejected = DateTimeOffset.Now.ToBrandOffset(bankAccount.Player.Brand.TimezoneId);

                _repository.SaveChanges();

                _eventBus.Publish(new PlayerBankAccountRejected(
                    bankAccount.Player.Id,
                    bankAccount.Id,
                    bankAccount.AccountNumber,
                    bankAccount.Remarks)
                {
                    EventCreated = bankAccount.Rejected.Value,
                });

                scope.Complete();
            }

            _messageService.TrySendPlayerMessage(
                bankAccount.Player.Id,
                MessageType.PlayerBankAccountRejected,
                MessageDeliveryMethod.Email,
                null);
        }
    }
}