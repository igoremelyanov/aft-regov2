using System;
using System.Linq;
using System.Threading.Tasks;
using AFT.RegoV2.Bonus.Api.Interface;
using AFT.RegoV2.Bonus.Core.Models.Commands;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Payment;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Payment.Data;
using AFT.RegoV2.Core.Payment.Interface.Events;
using AFT.RegoV2.RegoBus.Interfaces;
using AFT.RegoV2.Shared.Utils;

namespace AFT.RegoV2.Core.Payment.ApplicationServices
{
    public class TransferFundCommands : ITransferFundCommands
    {
        private readonly IPaymentRepository _repository;
        private readonly ITransferFundValidationService _validationService;
        private readonly BrandQueries _brandQueries;
        private readonly IEventBus _eventBus;
        private readonly IBonusApiProxy _bonusApiProxy;

        public TransferFundCommands(
            IPaymentRepository repository,
            ITransferFundValidationService validationService,
            BrandQueries brandQueries,
            IEventBus eventBus,
            IBonusApiProxy bonusApiProxy)
        {
            _repository = repository;
            _validationService = validationService;
            _brandQueries = brandQueries;
            _eventBus = eventBus;
            _bonusApiProxy = bonusApiProxy;
        }

        public async Task<string> AddFund(TransferFundRequest request)
        {
            var validationResult = await _validationService.Validate(request);

            var transferFund = new TransferFund();
            var transactionNumber = GenerateTransactionNumber();

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var requestWalletId = new Guid(request.WalletId);
                var requestWalletTemplate = _brandQueries.GetWalletTemplate(requestWalletId);
                var mainWalletTemplate = _brandQueries.GetWalletTemplates(requestWalletTemplate.Brand.Id).Single(wt => wt.IsMain);
                var timezoneId = requestWalletTemplate.Brand.TimezoneId;

                _repository.TransferFunds.Add(transferFund);
                transferFund.Id = Guid.NewGuid();
                transferFund.TransferType = request.TransferType;
                transferFund.TransactionNumber = transactionNumber;
                transferFund.Amount = request.Amount;
                transferFund.WalletId = request.WalletId;
                transferFund.CreatedBy = request.PlayerId.ToString();
                transferFund.Created = DateTimeOffset.Now.ToBrandOffset(timezoneId);
                transferFund.Remarks = transferFund.Remarks = !validationResult.IsValid ? validationResult.ErrorMessage : string.Empty;
                transferFund.Status = validationResult.IsValid ? TransferFundStatus.Approved : TransferFundStatus.Rejected;
                transferFund.BonusCode = request.BonusCode;

                if (validationResult.IsValid)
                {
                    var sourceWalletId = request.TransferType == TransferFundType.FundIn
                        ? mainWalletTemplate.Id
                        : requestWalletId;
                    var destinationWalletId = request.TransferType == TransferFundType.FundIn
                        ? requestWalletId
                        : mainWalletTemplate.Id;

                    //_walletCommands.TransferFunds(request.PlayerId, sourceWalletId, destinationWalletId, request.Amount, transactionNumber);
                    if (request.BonusId.HasValue || string.IsNullOrWhiteSpace(request.BonusCode) == false && transferFund.Status == TransferFundStatus.Approved)
                        await _bonusApiProxy.ApplyForBonusAsync(new FundInBonusApplication
                        {
                            PlayerId = request.PlayerId,
                            BonusId = request.BonusId,
                            BonusCode = request.BonusCode,
                            Amount = request.Amount,
                            DestinationWalletTemplateId = destinationWalletId
                        });

                    transferFund.DestinationWalletId = destinationWalletId;
                }

                _eventBus.Publish(new TransferFundCreated
                {
                    PlayerId = new Guid(transferFund.CreatedBy),
                    TransactionNumber = transferFund.TransactionNumber,
                    Amount = transferFund.Amount,
                    Remarks = transferFund.Remarks,
                    BonusCode = transferFund.BonusCode,
                    DestinationWalletStructureId = transferFund.DestinationWalletId,
                    Type = transferFund.TransferType,
                    Status = transferFund.Status,
                    Description = string.Format("Transaction #{0}", transferFund.TransactionNumber)
                });
                _repository.SaveChanges();
                scope.Complete();
            }

            if (!validationResult.IsValid)
                throw new ArgumentException(validationResult.ErrorMessage);

            return transactionNumber;
        }

        //todo: Add Unique key to DB
        private static string GenerateTransactionNumber()
        {
            //alternative
            //byte[] guildBytes = Guid.NewGuid().ToByteArray();
            //var id = BitConverter.ToInt64(guildBytes, 0);
            var random = new Random();
            return "TF" + random.Next(10000000, 99999999);
        }
    }
}
