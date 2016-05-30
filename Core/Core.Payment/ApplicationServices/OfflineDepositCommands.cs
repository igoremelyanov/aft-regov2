using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using AFT.RegoV2.Bonus.Api.Interface;
using AFT.RegoV2.Bonus.Core.Models.Commands;
using AFT.RegoV2.Core.Auth.Interface;
using AFT.RegoV2.Core.Common;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Player;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Messaging.Interface.ApplicationServices;
using AFT.RegoV2.Core.Messaging.Interface.Data;
using AFT.RegoV2.Core.Messaging.Interface.Data.MessageTemplateModels;
using AFT.RegoV2.Core.Payment.Interface.ApplicationServices;
using AFT.RegoV2.Core.Payment.Interface.Data;
using AFT.RegoV2.Core.Payment.Interface.Data.Commands;
using AFT.RegoV2.Core.Payment.Validators;
using AFT.RegoV2.Core.Player.Validators;
using AFT.RegoV2.RegoBus.Interfaces;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Shared.Utils;
using AutoMapper;

namespace AFT.RegoV2.Core.Payment.ApplicationServices
{
    public class OfflineDepositCommands : IOfflineDepositCommands
    {
        private readonly IPaymentRepository _repository;

        private readonly IPaymentQueries _paymentQueries;
        private readonly IEventBus _eventBus;
        private readonly IPaymentSettingsValidator _validator;
        private readonly IActorInfoProvider _actorInfoProvider;
        private readonly IPlayerIdentityValidator _identityValidator;
        private readonly IDocumentService _documentsService;
        private readonly IOfflineDepositQueries _offlineDepositQueries;
        private readonly IServiceBus _serviceBus;
        private readonly IBonusApiProxy _bonusApiProxy;
        private readonly IMessageTemplateService _messageTemplateService;

        static OfflineDepositCommands()
        {
            MapperConfig.CreateMap();
        }

        public OfflineDepositCommands(
            IPaymentRepository repository,
            IPaymentQueries paymentQueries,
            IEventBus eventBus,
            IPaymentSettingsValidator validator,
            IActorInfoProvider actorInfoProvider,
            IPlayerIdentityValidator identityValidator,
            IDocumentService documentsService,
            IOfflineDepositQueries offlineDepositQueries,
            IServiceBus serviceBus,
            IBonusApiProxy bonusApiProxy, 
            IMessageTemplateService messageTemplateService)
        {
            _repository = repository;
            _paymentQueries = paymentQueries;
            _eventBus = eventBus;
            _validator = validator;
            _actorInfoProvider = actorInfoProvider;
            _identityValidator = identityValidator;
            _documentsService = documentsService;
            _offlineDepositQueries = offlineDepositQueries;
            _serviceBus = serviceBus;
            _bonusApiProxy = bonusApiProxy;
            _messageTemplateService = messageTemplateService;
        }

        //[Permission(Permissions.Create, Module = Modules.OfflineDepositRequests)]
        public async Task<OfflineDeposit> Submit(OfflineDepositRequest request)
        {
            var validationResult = _offlineDepositQueries.GetValidationResult(request);
            if (validationResult.IsValid == false)
                throw new RegoException(string.Join("/n", validationResult.Errors.Select(failure => failure.ErrorMessage)));

            var player = _repository.Players
                .Include(x => x.Brand)
                .Single(p => p.Id == request.PlayerId);

            var bankAccount = _repository.BankAccounts
                .Include(x => x.Bank)
                .Single(x => x.Id == request.BankAccountId);

            _validator.Validate(request.PlayerId, bankAccount.CurrencyCode, request.Amount);
            _identityValidator.Validate(request.PlayerId, TransactionType.Deposit);

            var deposit = new Entities.OfflineDeposit(request, bankAccount, player, _actorInfoProvider.Actor.UserName);
            using (var scope = CustomTransactionScope.GetTransactionScopeAsync())
            {
                var depositEvent = deposit.Submit();
                _repository.OfflineDeposits.Add(deposit.Data);

                if (request.BonusId.HasValue || string.IsNullOrWhiteSpace(request.BonusCode) == false)
                {
                    var bonusRedemptionId = await _bonusApiProxy.ApplyForBonusAsync(new DepositBonusApplication
                    {
                        PlayerId = player.Id,
                        BonusId = request.BonusId,
                        BonusCode = request.BonusCode,
                        Amount = request.Amount,
                        DepositId = deposit.Data.Id
                    });
                    deposit.SetBonusRedemption(bonusRedemptionId);
                }

                _repository.SaveChanges();
                _eventBus.Publish(depositEvent);
                scope.Complete();
            }

            var messageModel = new OfflineDepositRequestedModel
            {
                BankName = bankAccount.Bank.BankName,
                BankAccountName = bankAccount.AccountName,
                BankAccountNumber = bankAccount.AccountNumber
            };

            _messageTemplateService.TrySendPlayerMessage(
                    player.Id,
                    MessageType.OfflineDepositRequested,
                    MessageDeliveryMethod.Email,
                    messageModel);

            _messageTemplateService.TrySendPlayerMessage(
                player.Id,
                MessageType.OfflineDepositRequested,
                MessageDeliveryMethod.Sms,
                messageModel);

            var data = Mapper.Map<OfflineDeposit>(deposit.Data);
            return data;
        }

        public OfflineDeposit Confirm(
            OfflineDepositConfirm depositConfirm,
            string confirmedBy,
            byte[] idFrontImage,
            byte[] idBackImage,
            byte[] receiptImage)
        {
            var offlineDeposit = _repository.GetDepositById(depositConfirm.Id);

            var frontImageId = SaveFile(depositConfirm.IdFrontImage, idFrontImage, offlineDeposit.Data.PlayerId);
            var backImageId = SaveFile(depositConfirm.IdBackImage, idBackImage, offlineDeposit.Data.PlayerId);
            var receiptImageId = SaveFile(depositConfirm.ReceiptImage, receiptImage, offlineDeposit.Data.PlayerId);

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                ValidateLimits(depositConfirm);
                var confirmEvent = offlineDeposit.Confirm(
                    depositConfirm.PlayerAccountName,
                    depositConfirm.PlayerAccountNumber,
                    depositConfirm.ReferenceNumber,
                    depositConfirm.Amount,
                    depositConfirm.TransferType,
                    depositConfirm.OfflineDepositType,
                    depositConfirm.Remark,
                    confirmedBy,
                    frontImageId,
                    backImageId,
                    receiptImageId);

                _repository.SaveChanges();

                confirmEvent.EventCreated = DateTimeOffset.Now.ToBrandOffset(offlineDeposit.Data.Player.Brand.TimezoneId);

                _eventBus.Publish(confirmEvent);

                scope.Complete();
            }

            return _paymentQueries.GetDepositById(depositConfirm.Id);
        }

        private void ValidateLimits(OfflineDepositConfirm depositConfirm)
        {
            var deposit = _paymentQueries.GetDepositById(depositConfirm.Id);
            var playerId = deposit.PlayerId;
            var amount = depositConfirm.Amount;
            var currencyCode = deposit.CurrencyCode;
            _validator.Validate(playerId, currencyCode, amount);
        }

        public void Verify(OfflineDepositId id, Guid bankAccountId, string remark)
        {
            var offlineDeposit = _repository.GetDepositById(id);
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var verifyEvent = offlineDeposit.Verify(_actorInfoProvider.Actor.UserName, remark);
                offlineDeposit.ChangeBankAccount(bankAccountId);

                _repository.SaveChanges();

                verifyEvent.EventCreated = DateTimeOffset.Now.ToBrandOffset(offlineDeposit.Data.Player.Brand.TimezoneId);
                verifyEvent.BankAccountId = bankAccountId;

                _eventBus.Publish(verifyEvent);

                scope.Complete();
            }
        }

        public void Unverify(OfflineDepositId id, string remark, UnverifyReasons unverifyReason)
        {
            var offlineDeposit = _repository.GetDepositById(id);
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var unverifyEvent = offlineDeposit.Unverify(_actorInfoProvider.Actor.UserName, remark, unverifyReason);
                _repository.SaveChanges();

                unverifyEvent.EventCreated = DateTimeOffset.Now.ToBrandOffset(offlineDeposit.Data.Player.Brand.TimezoneId);

                _eventBus.Publish(unverifyEvent);

                scope.Complete();
            }
        }

        public void Approve(OfflineDepositApprove approveCommand)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var offlineDeposit = _repository.GetDepositById(approveCommand.Id);

                if (approveCommand.ActualAmount > offlineDeposit.Data.Amount)
                {
                    throw new RegoException("actualAmountGreaterThanVerified");
                }

                _validator.Validate(offlineDeposit.Data.PlayerId,
                    offlineDeposit.Data.CurrencyCode,
                    approveCommand.ActualAmount);

                var depositCommand = offlineDeposit.Approve(
                    approveCommand.ActualAmount,
                    approveCommand.Fee,
                    approveCommand.PlayerRemark,
                    _actorInfoProvider.Actor.UserName,
                    approveCommand.Remark);

                _repository.SaveChanges();

                _serviceBus.PublishMessage(depositCommand);

                scope.Complete();
            }
        }

        public void Reject(OfflineDepositId id, string remark)
        {
            var offlineDeposit = _repository.GetDepositById(id);
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var rejectedEvent = offlineDeposit.Reject(_actorInfoProvider.Actor.UserName, remark);
                _repository.SaveChanges();

                rejectedEvent.EventCreated = DateTimeOffset.Now.ToBrandOffset(offlineDeposit.Data.Player.Brand.TimezoneId);

                _eventBus.Publish(rejectedEvent);

                scope.Complete();
            }
        }

        private Guid? SaveFile(string fileName, byte[] content, Guid playerId)
        {
            if (content != null && content.Length > 0)
            {
                var player = _repository.Players
                    .Include(o => o.Brand)
                    .Single(o => o.Id == playerId);

                var streamId = _documentsService.SaveFile(fileName, content, playerId, player.BrandId, player.Brand.LicenseeId);
                return streamId;
            }
            return null;
        }
    }
}