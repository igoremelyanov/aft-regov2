using System;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using AFT.RegoV2.Core.Auth.Interface;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Payment;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Core.Messaging.Interface.ApplicationServices;
using AFT.RegoV2.Core.Messaging.Interface.Data;
using AFT.RegoV2.Core.Payment.Interface.ApplicationServices;
using AFT.RegoV2.Core.Payment.Interface.Data;
using AFT.RegoV2.Core.Payment.Interface.Data.Commands;
using AFT.RegoV2.Core.Payment.Interface.Commands;
using AFT.RegoV2.Core.Payment.Interface.Events;
using AFT.RegoV2.Core.Payment.Validators;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Domain.Payment.Data;
using AFT.RegoV2.RegoBus.Interfaces;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Shared.Utils;
using Microsoft.Practices.Unity;

using AutoMapper.QueryableExtensions;
using OfflineWithdraw = AFT.RegoV2.Core.Payment.Interface.Data.OfflineWithdraw;
namespace AFT.RegoV2.Core.Payment.ApplicationServices
{
    public class WithdrawalService : IApplicationService, IWithdrawalService
    {
        private readonly BrandQueries _brandQueries;
        private readonly IAVCValidationService _avcValidationService;
        private readonly IRiskProfileCheckValidationService _riskProfileCheckService;
        private readonly IPaymentRepository _repository;
        private readonly IPaymentQueries _paymentQueries;
        private readonly IEventBus _eventBus;
        private readonly IActorInfoProvider _actorInfoProvider;
        private readonly IServiceBus _serviceBus;
        private readonly IUnityContainer _container;
        private readonly IMessageTemplateService _messageTemplateService;
	    private readonly IGameWalletOperations _walletCommands;

	    public WithdrawalService(
            IPaymentRepository repository,
            IPaymentQueries paymentQueries,
            IEventBus eventBus,
            BrandQueries brandQueries,
            IAVCValidationService avcValidationService,
            IRiskProfileCheckValidationService riskProfileCheckService,
            IActorInfoProvider actorInfoProvider,
            IUnityContainer container,
            IMessageTemplateService messageTemplateService,
			IGameWalletOperations walletCommands,
            IServiceBus serviceBus)
        {
            _repository = repository;
            _paymentQueries = paymentQueries;
            _eventBus = eventBus;
            _brandQueries = brandQueries;
            _avcValidationService = avcValidationService;
            _riskProfileCheckService = riskProfileCheckService;
            _actorInfoProvider = actorInfoProvider;
            _container = container;
            _serviceBus = serviceBus;
            _messageTemplateService = messageTemplateService;
		    _walletCommands = walletCommands;
        }

        [Permission(Permissions.View, Module = Modules.OfflineWithdrawalRequest)]
        public IQueryable<OfflineWithdraw> GetWithdrawalsForVerification()
        {
            return GetWithdrawals().Where(x => x.Status == WithdrawalStatus.AutoVerificationFailed || x.Status == WithdrawalStatus.Reverted);
        }

        [Permission(Permissions.View, Module = Modules.OfflineWithdrawalVerification)]
        public IQueryable<OfflineWithdraw> GetWithdrawalsForAcceptance()
        {
            return GetWithdrawals().Where(x => x.Status == WithdrawalStatus.Verified);
        }

        [Permission(Permissions.View, Module = Modules.OfflineWithdrawalVerification)]
        public IQueryable<OfflineWithdraw> GetWithdrawalsForVerificationQueue()
        {
            return
                GetWithdrawals()
                    .Where(x => x.Status == WithdrawalStatus.AutoVerificationFailed
                        || x.Status == WithdrawalStatus.Reverted);
        }

        [Permission(Permissions.View, Module = Modules.OfflineWithdrawalAcceptance)]
        public IQueryable<OfflineWithdraw> GetWithdrawalsForApproval()
        {
            return GetWithdrawals().Where(x => x.Status == WithdrawalStatus.Accepted);
        }

        [Permission(Permissions.View, Module = Modules.OfflineWithdrawalAcceptance)]
        public IQueryable<OfflineWithdraw> GetWithdrawalsCanceled()
        {
            return GetWithdrawals().Where(x => x.Status == WithdrawalStatus.Canceled);
        }

        [Permission(Permissions.View, Module = Modules.OfflineWithdrawalWagerCheck)]
        public IQueryable<OfflineWithdraw> GetWithdrawalsFailedAutoWagerCheck()
        {
            return GetWithdrawals().Where(x => !x.AutoWagerCheck && x.Status == WithdrawalStatus.AutoVerificationFailed);
        }

        [Permission(Permissions.View, Module = Modules.OfflineWithdrawalOnHold)]
        public IQueryable<OfflineWithdraw> GetWithdrawalsOnHold()
        {
            return
                GetWithdrawals()
                    .Where(x => x.Status == WithdrawalStatus.Documents || x.Status == WithdrawalStatus.Investigation);
        }

        [Permission(Permissions.View, Module = Modules.OfflineWithdrawalOnHold)]
        public IQueryable<OfflineWithdraw> GetWithdrawalsVerified()
        {
            return
                GetWithdrawals()
                    .Where(x => x.Status == WithdrawalStatus.Verified);
        }


        [Permission(Permissions.View, Module = Modules.OfflineWithdrawalOnHold)]
        public IQueryable<OfflineWithdraw> GetWithdrawalsAccepted()
        {
            return
                GetWithdrawals()
                    .Where(x => x.Status == WithdrawalStatus.Accepted);
        }

        public IQueryable<OfflineWithdraw> GetWithdrawals()
        {
            return _repository.OfflineWithdraws
                .Include(p => p.PlayerBankAccount.Player)
                .Include(p => p.PlayerBankAccount.Bank.Brand)
                .AsNoTracking().Project().To<OfflineWithdraw>();
        }

        [Permission(Permissions.Create, Module = Modules.OfflineWithdrawalRequest)]
        public OfflineWithdrawResponse Request(OfflineWithdrawRequest request)
        {
            return WithdrawalRequestSubmit(request);
        }

        public OfflineWithdrawResponse WithdrawalRequestSubmit(OfflineWithdrawRequest request)
        {
            var offlineWithdrawResponse = new OfflineWithdrawResponse();
            var offlineWithdrawalRequestValidator = 
                new OfflineWithdrawalRequestValidator(_container, _repository);

            var validationResult = offlineWithdrawalRequestValidator.Validate(request);
            if (!validationResult.IsValid)
                throw new RegoValidationException(validationResult);

            var bankAccount =
                        _repository.PlayerBankAccounts.Include(y => y.Player)
                            .Include(y => y.Bank)
                            .Include(y => y.Player.Brand)
                            .SingleOrDefault(y => y.Id == request.PlayerBankAccountId);

            var id = offlineWithdrawResponse.Id = Guid.NewGuid();
         
            var referenceCode = GenerateTransactionNumber();

            _serviceBus.PublishMessage(
                new WithdrawRequestSubmit
                {
                    ActorName = _actorInfoProvider.Actor.UserName,
                    WithdrawId = id,
                    PlayerId = bankAccount.Player.Id,
                    Amount = request.Amount,
                    LockId = Guid.NewGuid(),
                    PlayerBankAccountId = request.PlayerBankAccountId,
                    ReferenceCode = referenceCode,
                    RequestedBy = request.RequestedBy,
                    Requested = DateTimeOffset.Now.ToBrandOffset(bankAccount.Player.Brand.TimezoneId), 
                    Remarks = request.Remarks
                });
                     
            return offlineWithdrawResponse;
        }

        public void WithdrawalStateMachine(Guid id)
        {
            var withdrawal = _repository.OfflineWithdraws.SingleOrDefault(x => x.Id == id);
            if (withdrawal == null)
                return;

            if (withdrawal.Status == WithdrawalStatus.New)
            {
                _avcValidationService.Validate(id);
                if (_avcValidationService.Failed)
                    _riskProfileCheckService.Validate(id);
            }
        }

        private void AppendWagerCheckComments(Data.OfflineWithdraw offlineWithdraw, string checker)
        {
            var textToAppend = "Wager Checked by: " + checker + "\nDate Wager Checked: " + DateTime.Now.ToString("MM'/'dd'/'yyyy HH:mm");
            var remarks = offlineWithdraw.Remarks ?? "";
            var match = Regex.Match(remarks, "^.*$", RegexOptions.Multiline | RegexOptions.RightToLeft);
            if (match.Success)
            {
                if (match.Value.Trim().Length != 0)
                {
                    textToAppend = "\n\n" + textToAppend;
                }
                else
                {
                    match = match.NextMatch();
                    if (match.Success && match.Value.Trim().Length != 0)
                    {
                        textToAppend = "\n" + textToAppend;
                    }
                }
            }
            // potential string length issue
            offlineWithdraw.Remarks = remarks + textToAppend;
        }

        [Permission(Permissions.Verify, Module = Modules.OfflineWithdrawalVerification)]
        public void Verify(OfflineWithdrawId id, string remarks)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var offlineWithdrawal = GetWithdrawalWithPlayer(id);

                if (offlineWithdrawal.Status != WithdrawalStatus.AutoVerificationFailed
                    && offlineWithdrawal.Status != WithdrawalStatus.Reverted
                    && offlineWithdrawal.Status != WithdrawalStatus.Investigation
                    && offlineWithdrawal.Status != WithdrawalStatus.Documents)
                {
                    ThrowStatusError(offlineWithdrawal.Status, WithdrawalStatus.Verified);
                }

                if (offlineWithdrawal.Status == WithdrawalStatus.Investigation)
                {
                    offlineWithdrawal.InvestigationStatus = CommonVerificationStatus.Passed;
                    offlineWithdrawal.InvestigationDate = DateTimeOffset.UtcNow;
                }

                if (offlineWithdrawal.Status == WithdrawalStatus.Documents)
                {
                    offlineWithdrawal.DocumentsCheckStatus = CommonVerificationStatus.Passed;
                    offlineWithdrawal.DocumentsCheckDate = DateTimeOffset.UtcNow;
                }

                offlineWithdrawal.Verified = DateTimeOffset.Now.ToBrandOffset(offlineWithdrawal.PlayerBankAccount.Player.Brand.TimezoneId);

                offlineWithdrawal.VerifiedBy = _actorInfoProvider.Actor.UserName;
                offlineWithdrawal.Remarks = remarks;
                offlineWithdrawal.Status = WithdrawalStatus.Verified;

                _eventBus.Publish(new WithdrawalVerified(
                    id,
                    offlineWithdrawal.Amount,
                    offlineWithdrawal.Verified.Value,
                    _actorInfoProvider.Actor.Id,
                    offlineWithdrawal.PlayerBankAccount.Player.Id,
                    WithdrawalStatus.Verified,
                    remarks,
                    offlineWithdrawal.TransactionNumber,
                    _actorInfoProvider.Actor.UserName)
                {
                    EventCreated = offlineWithdrawal.Verified.Value
                });

                _repository.SaveChanges();

                scope.Complete();
            }
        }

        private void ThrowStatusError(WithdrawalStatus from, WithdrawalStatus to)
        {
            throw new InvalidOperationException(string.Format("The withdrawal has \"{0}\" status, so it can't be {1}", from, to));
        }

        public void SetDocumentsState(Guid requestId, string remarks)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var withdrawal = GetWithdrawalWithPlayer(requestId);
                if (withdrawal == null)
                    return;

                var eventCreated = DateTimeOffset.Now.ToBrandOffset(withdrawal.PlayerBankAccount.Player.Brand.TimezoneId);

                _eventBus.Publish(new WithdrawalDocumentsChecked(
                    withdrawal.Id,
                    withdrawal.Amount,
                    eventCreated,
                    _actorInfoProvider.Actor.Id,
                    withdrawal.PlayerBankAccount.Player.Id,
                    WithdrawalStatus.Documents,
                    remarks,
                    withdrawal.TransactionNumber)
                {
                    EventCreated = eventCreated,
                });

                withdrawal.Remarks = remarks;
                withdrawal.Status = WithdrawalStatus.Documents;
                _repository.SaveChanges();
                scope.Complete();
            }
        }

        public void SetInvestigateState(Guid requestId, string remarks)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var withdrawal = GetWithdrawalWithPlayer(requestId);
                if (withdrawal == null)
                    return;

                var eventCreated = DateTimeOffset.Now.ToBrandOffset(withdrawal.PlayerBankAccount.Player.Brand.TimezoneId);

                _eventBus.Publish(new WithdrawalInvestigated(
                    withdrawal.Id,
                    withdrawal.Amount,
                    eventCreated,
                    _actorInfoProvider.Actor.Id,
                    withdrawal.PlayerBankAccount.Player.Id,
                    WithdrawalStatus.Investigation,
                    remarks,
                    withdrawal.TransactionNumber)
                {
                    EventCreated = eventCreated,
                });

                withdrawal.Remarks = remarks;
                withdrawal.Status = WithdrawalStatus.Investigation;
                _repository.SaveChanges();
                scope.Complete();
            }
        }

        [Permission(Permissions.Unverify, Module = Modules.OfflineWithdrawalVerification)]
        public void Unverify(OfflineWithdrawId id, string remarks)
        {
            var withdrawal = _repository.OfflineWithdraws.Include(x => x.PlayerBankAccount.Player).Single(x => x.Id == id);
            if (withdrawal.Status != WithdrawalStatus.AutoVerificationFailed
                && withdrawal.Status != WithdrawalStatus.Reverted
                && withdrawal.Status != WithdrawalStatus.Investigation
                && withdrawal.Status != WithdrawalStatus.Documents)
            {
                ThrowStatusError(withdrawal.Status, WithdrawalStatus.Unverified);
            }
           
            var now = _paymentQueries.GetBrandDateTimeOffset(withdrawal.PlayerBankAccount.Player.BrandId);




            _serviceBus.PublishMessage(new WithdrawRequestCancel
            {
                WithdrawId = id,
                ActorName = _actorInfoProvider.Actor.UserName,
                CanceledUserId = _actorInfoProvider.Actor.Id,
                Status = WithdrawalStatus.Unverified,
                Remarks = remarks,
                Canceled = now
            });
        }

        [Permission(Permissions.Approve, Module = Modules.OfflineWithdrawalApproval)]
        public void Approve(OfflineWithdrawId id, string remarks)
        {
			var offlineWithdrawal = GetWithdrawalWithPlayer(id);
			if (offlineWithdrawal.Status != WithdrawalStatus.Accepted)
			{
				ThrowStatusError(offlineWithdrawal.Status, WithdrawalStatus.Approved);
			}

			var now = _paymentQueries.GetBrandDateTimeOffset(offlineWithdrawal.PlayerBankAccount.Player.BrandId);

			_serviceBus.PublishMessage(
				new WithdrawRequestApprove
				{
					ActorName = _actorInfoProvider.Actor.UserName,
					WithdrawId = id,
					Remarks = remarks,
					ApprovedUerId = _actorInfoProvider.Actor.Id,
					Approved = now
				});
			_messageTemplateService.TrySendPlayerMessage(
                offlineWithdrawal.PlayerBankAccount.Player.Id,
                MessageType.WithdrawalRequestReleased,
                MessageDeliveryMethod.Email,
                null);
        }

        [Permission(Permissions.Reject, Module = Modules.OfflineWithdrawalApproval)]
        public void Reject(OfflineWithdrawId id, string remarks)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var withdrawal = GetWithdrawalWithPlayer(id);
                if (withdrawal.Status != WithdrawalStatus.Accepted)
                {
                    ThrowStatusError(withdrawal.Status, WithdrawalStatus.Rejected);
                }
                withdrawal.Remarks = remarks;
                withdrawal.Status = WithdrawalStatus.Rejected;
                withdrawal.Rejected =
                    DateTimeOffset.Now.ToBrandOffset(withdrawal.PlayerBankAccount.Player.Brand.TimezoneId);
                withdrawal.RejectedBy = _actorInfoProvider.Actor.UserName;

                _repository.SaveChanges();
                scope.Complete();
            }
        }

        [Permission(Permissions.Pass, Module = Modules.OfflineWithdrawalWagerCheck)]
        public void PassWager(OfflineWithdrawId id, string remarks)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var offlineWithdrawal = GetWithdrawalWithPlayer(id);
                offlineWithdrawal.WagerCheck = true;
                offlineWithdrawal.Remarks = remarks;
                AppendWagerCheckComments(offlineWithdrawal, _actorInfoProvider.Actor.UserName);
                //_eventBus.Publish(new WithdrawalWagerChecked(
                //    offlineWithdrawal.Id, 
                //    offlineWithdrawal.Amount, 
                //    DateTime.Now, 
                //    _actorInfoProvider.Actor.Id, 
                //    WithdrawalStatus.Investigation, 
                //    remarks,
                //    offlineWithdrawal.TransactionNumber));
                _repository.SaveChanges();
                scope.Complete();
            }
        }

        private Data.OfflineWithdraw GetWithdrawalWithPlayer(Guid id)
        {
            return _repository.OfflineWithdraws
                .Include(x => x.PlayerBankAccount.Player)
                .Include(x => x.PlayerBankAccount.Player.Brand)
                .Single(x => x.Id == id);
        }

        [Permission(Permissions.Fail, Module = Modules.OfflineWithdrawalWagerCheck)]
        public void FailWager(OfflineWithdrawId id, string remarks)
        {
            var withdrawal = GetWithdrawalWithPlayer(id);
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                withdrawal.WagerCheck = false;
                withdrawal.Remarks = remarks;
                AppendWagerCheckComments(withdrawal, _actorInfoProvider.Actor.UserName);
                withdrawal.Status = WithdrawalStatus.Unverified;

                _serviceBus.PublishMessage(new WithdrawRequestCancel
                {
                    WithdrawId = id,
                    ActorName = _actorInfoProvider.Actor.UserName,
                    CanceledUserId = _actorInfoProvider.Actor.Id,
                    Status = WithdrawalStatus.Unverified,
                    Remarks = remarks
                });
               


                _repository.SaveChanges();
                scope.Complete();
            }
        }

        [Permission(Permissions.Pass, Module = Modules.OfflineWithdrawalInvestigation)]
        public void PassInvestigation(OfflineWithdrawId id, string remarks)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var offlineWithdrawal = GetWithdrawalWithPlayer(id);
                offlineWithdrawal.InvestigatedBy = _actorInfoProvider.Actor.UserName;
                offlineWithdrawal.InvestigationDate = DateTimeOffset.Now.ToBrandOffset(offlineWithdrawal.PlayerBankAccount.Player.Brand.TimezoneId);
                offlineWithdrawal.Remarks = remarks;
                offlineWithdrawal.Status = WithdrawalStatus.Verified;

                _eventBus.Publish(new WithdrawalInvestigated(
                    id,
                    offlineWithdrawal.Amount,
                    offlineWithdrawal.InvestigationDate.Value,
                    _actorInfoProvider.Actor.Id,
                    offlineWithdrawal.PlayerBankAccount.Player.Id,
                    WithdrawalStatus.Investigation,
                    remarks,
                    offlineWithdrawal.TransactionNumber)
                {
                    EventCreated = offlineWithdrawal.InvestigationDate.Value,
                });

                _repository.SaveChanges();
                scope.Complete();
            }
        }

        [Permission(Permissions.Fail, Module = Modules.OfflineWithdrawalInvestigation)]
        public void FailInvestigation(OfflineWithdrawId id, string remarks)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var withdrawal = GetWithdrawalWithPlayer(id);
                withdrawal.InvestigatedBy = _actorInfoProvider.Actor.UserName;
                withdrawal.InvestigationDate =
                    DateTimeOffset.Now.ToBrandOffset(withdrawal.PlayerBankAccount.Player.Brand.TimezoneId);
                withdrawal.Remarks = remarks;
                withdrawal.Status = WithdrawalStatus.Unverified;

                _serviceBus.PublishMessage(new WithdrawRequestCancel
                {
                    WithdrawId = id,
                    ActorName = _actorInfoProvider.Actor.UserName,
                    CanceledUserId = _actorInfoProvider.Actor.Id,
                    Status = WithdrawalStatus.Unverified,
                    Remarks = remarks
                });
               

                _repository.SaveChanges();
                scope.Complete();
            }
        }

        [Permission(Permissions.Accept, Module = Modules.OfflineWithdrawalAcceptance)]
        public void Accept(OfflineWithdrawId id, string remarks)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var offlineWithdrawal = GetWithdrawalWithPlayer(id);
                if (offlineWithdrawal.Status != WithdrawalStatus.Verified)
                {
                    ThrowStatusError(offlineWithdrawal.Status, WithdrawalStatus.Accepted);
                }
                offlineWithdrawal.Remarks = remarks;
                offlineWithdrawal.Status = WithdrawalStatus.Accepted;
                offlineWithdrawal.AcceptedBy = _actorInfoProvider.Actor.UserName;
                offlineWithdrawal.AcceptedTime =
                    DateTimeOffset.Now.ToBrandOffset(offlineWithdrawal.PlayerBankAccount.Player.Brand.TimezoneId);

                _eventBus.Publish(new WithdrawalAccepted(id,
                    offlineWithdrawal.Amount,
                    offlineWithdrawal.AcceptedTime.Value,
                    _actorInfoProvider.Actor.Id,
                    offlineWithdrawal.PlayerBankAccount.Player.Id,
                    WithdrawalStatus.Accepted,
                    remarks,
                    offlineWithdrawal.TransactionNumber)
                {
                    EventCreated = offlineWithdrawal.AcceptedTime.Value,
                });

                _repository.SaveChanges();

                scope.Complete();
            }
        }

        [Permission(Permissions.Revert, Module = Modules.OfflineWithdrawalAcceptance)]
        public void Revert(OfflineWithdrawId id, string remarks)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var withdrawal = GetWithdrawalWithPlayer(id);
                if (withdrawal.Status != WithdrawalStatus.Verified)
                {
                    ThrowStatusError(withdrawal.Status, WithdrawalStatus.Reverted);
                }
                withdrawal.Remarks = remarks;
                withdrawal.Status = WithdrawalStatus.Reverted;
                withdrawal.RevertedBy = _actorInfoProvider.Actor.UserName;
                withdrawal.RevertedTime =
                    DateTimeOffset.Now.ToBrandOffset(withdrawal.PlayerBankAccount.Player.Brand.TimezoneId);
                
                _eventBus.Publish(new WithdrawalReverted(
                    id,
                    withdrawal.Amount,
                    withdrawal.RevertedTime.Value,
                    _actorInfoProvider.Actor.Id,
                    withdrawal.PlayerBankAccount.Player.Id,
                    WithdrawalStatus.Reverted,
                    remarks,
                    withdrawal.TransactionNumber, 
                    _actorInfoProvider.Actor.UserName)
                {
                    EventCreated = withdrawal.RevertedTime.Value,
                });

                _repository.SaveChanges();
                scope.Complete();
            }
        }

        [Permission(Permissions.Revert, Module = Modules.OfflineWithdrawalAcceptance)]
        public void Cancel(OfflineWithdrawId id, string remarks)
        {
            var withdrawal = GetWithdrawalWithPlayer(id);
            if (withdrawal == null)
                return;

            var now = _paymentQueries.GetBrandDateTimeOffset(withdrawal.PlayerBankAccount.Player.BrandId);

            _serviceBus.PublishMessage(new WithdrawRequestCancel
            {
                WithdrawId = id,
                ActorName = _actorInfoProvider.Actor.UserName,
                CanceledUserId = _actorInfoProvider.Actor.Id,
                Status =  WithdrawalStatus.Canceled,
                Remarks = remarks,
                Canceled = now
            });
            

        }

        [Permission(Permissions.Exempt, Module = Modules.OfflineWithdrawalExemption)]
        public void SaveExemption(Exemption exemption)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var player = _repository.Players.Single(x => x.Id == exemption.PlayerId);
                var brand = _brandQueries.GetBrandOrNull(player.BrandId);
                var exemptFrom = DateTime.Parse(exemption.ExemptFrom, CultureInfo.InvariantCulture);
                var exemptTo = DateTime.Parse(exemption.ExemptTo, CultureInfo.InvariantCulture);

                if (exemptTo < exemptFrom)
                    throw new ArgumentException("ExemptTo must be greater or equal then ExemptFrom.");

                player.ExemptWithdrawalVerification = exemption.Exempt;
                player.ExemptWithdrawalFrom = exemptFrom.ToBrandDateTimeOffset(brand.TimezoneId);
                player.ExemptWithdrawalTo = exemptTo.ToBrandDateTimeOffset(brand.TimezoneId);
                player.ExemptLimit = exemption.ExemptLimit;
                _repository.SaveChanges();

                _eventBus.Publish(new PlayerAccountRestrictionsChanged(
                    player.Id,
                    player.ExemptLimit,
                    player.ExemptWithdrawalTo,
                    player.ExemptWithdrawalFrom,
                    player.ExemptWithdrawalVerification)
                {
                    EventCreated = DateTimeOffset.Now.ToBrandOffset(brand.TimezoneId),
                });

                scope.Complete();
            }
        }

        static string GenerateTransactionNumber()
        {
            var random = new Random();
            return "OW" + random.Next(10000000, 99999999);
        }
    }
}
