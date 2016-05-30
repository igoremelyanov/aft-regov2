using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using AFT.RegoV2.ApplicationServices.Report;
using System.Web.Http.Results;
using AFT.RegoV2.Bonus.Api.Interface;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Brand.Interface.ApplicationServices;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Payment;
using AFT.RegoV2.Core.Common.Data.Player;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Payment.ApplicationServices;
using AFT.RegoV2.Core.Payment.Interface.ApplicationServices;
using AFT.RegoV2.Core.Payment.Interface.Data;
using AFT.RegoV2.Core.Payment.Validators;
using AFT.RegoV2.Core.Player.Interface.ApplicationServices;
using AFT.RegoV2.MemberApi.Interface.Common;
using AFT.RegoV2.MemberApi.Interface.Payment;
using AFT.RegoV2.MemberApi.Interface.Player;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Shared.Utils;
using AutoMapper;
using FluentValidation;
using ServiceStack.Common;
using CheckStatusResponse = AFT.RegoV2.MemberApi.Interface.Payment.CheckStatusResponse;
using OnlineDepositParams = AFT.RegoV2.MemberApi.Interface.Payment.OnlineDepositParams;
using OnlineDepositPayNotifyRequest = AFT.RegoV2.MemberApi.Interface.Payment.OnlineDepositPayNotifyRequest;
using OnlineDepositRequest = AFT.RegoV2.MemberApi.Interface.Payment.OnlineDepositRequest;
using PaymentGatewaySettings = AFT.RegoV2.MemberApi.Interface.Payment.PaymentGatewaySettings;
using SubmitOnlineDepositRequestResult = AFT.RegoV2.MemberApi.Interface.Payment.SubmitOnlineDepositRequestResult;
using OfflineDeposit = AFT.RegoV2.MemberApi.Interface.Payment.OfflineDeposit;
using TransactionType = AFT.RegoV2.Core.Common.Data.Payment.TransactionType;

namespace AFT.RegoV2.MemberApi.Controllers
{
    public class PaymentController : BaseApiController
    {
        private readonly IPaymentQueries _paymentQueries;
        private readonly PlayerBankAccountCommands _playerBankAccountCommands;
        private readonly IBrandQueries _brandQueries;
        private readonly IWithdrawalService _withdrawalService;
        private readonly ITransferFundCommands _transferFundCommands;
        private readonly IOfflineDepositCommands _offlineDepositCommands;
        private readonly IOfflineDepositQueries _offlineDepositQueries;
        private readonly IOnlineDepositCommands _onlineDepositCommands;
        private readonly IOnlineDepositQueries _onlineDepositQueries;
        private readonly IPaymentGatewaySettingsQueries _paymentGatewaySettingsQueries;
        private readonly IPlayerQueries _playerQueries;
        private readonly IBonusApiProxy _bonusApiProxy;
        private readonly ReportQueries _reportQueries;

        private readonly string RootToOfflineDeposit = "api/Payment/GetOfflineDeposit?Id=";

        static PaymentController()
        {
            Mapper.CreateMap<OfflineDepositConfirm, Core.Payment.Interface.Data.Commands.OfflineDepositConfirm>();
            Mapper.CreateMap
               <Core.Payment.Interface.Data.OnlineDepositParams, OnlineDepositParams>();

            Mapper
                .CreateMap
                <Core.Payment.Interface.Data.SubmitOnlineDepositRequestResult, SubmitOnlineDepositRequestResult>();

            Mapper
                .CreateMap
                <Core.Payment.Interface.Data.CheckStatusResponse, CheckStatusResponse>();
            Mapper
                .CreateMap
                <Core.Payment.Interface.Data.PaymentGatewaySettings, PaymentGatewaySettings>();
        }

        public PaymentController(
            IPaymentQueries paymentQueries,
            PlayerBankAccountCommands playerBankAccountCommands,
            IBrandQueries brandQueries,
            IWithdrawalService withdrawalService,
            ITransferFundCommands transferFundCommands,
            IOfflineDepositCommands offlineDepositCommands,
            IOfflineDepositQueries offlineDepositQueries,
            IOnlineDepositCommands onlineDepositCommands,
            IOnlineDepositQueries onlineDepositQueries,
            IPaymentGatewaySettingsQueries paymentGatewaySettingsQueries,
            IPlayerQueries playerQueries,
            IBonusApiProxy bonusApiProxy,
            ReportQueries reportQueries)
        {
            _paymentQueries = paymentQueries;
            _playerBankAccountCommands = playerBankAccountCommands;
            _brandQueries = brandQueries;
            _withdrawalService = withdrawalService;
            _transferFundCommands = transferFundCommands;
            _offlineDepositCommands = offlineDepositCommands;
            _offlineDepositQueries = offlineDepositQueries;
            _onlineDepositCommands = onlineDepositCommands;
            _onlineDepositQueries = onlineDepositQueries;
            _paymentGatewaySettingsQueries = paymentGatewaySettingsQueries;
            _playerQueries = playerQueries;
            _bonusApiProxy = bonusApiProxy;
            _reportQueries = reportQueries;
        }

        [HttpGet]
        public OfflineDepositFormDataResponse OfflineDepositFormData([FromUri]OfflineDepositFormDataRequest request)
        {
            var bankAccountsForOfflineDepositRequest = _paymentQueries.GetBankAccountsForOfflineDepositRequest(PlayerId);
            if (bankAccountsForOfflineDepositRequest == null)
                throw new RegoException(ErrorMessagesEnum.ServiceUnavailable.ToString());

            return new OfflineDepositFormDataResponse
            {
                BankAccounts = bankAccountsForOfflineDepositRequest
            };

        }

        [HttpGet]
        public async Task<PlayerLastDepositSummaryResponse> PlayerLastDepositSummaryResponse([FromUri]PlayerLastDepositSummary request)
        {
            var playerLastDepositSummaryResponse = new PlayerLastDepositSummaryResponse();

            var deposits = _onlineDepositQueries.GetOnlineDepositsByPlayerId(PlayerId);
            if (deposits == null)
                throw new RegoException(ErrorMessagesEnum.ServiceUnavailable.ToString());

            var lastDeposit = deposits.OrderByDescending(x => x.DateApproved).First();
            if (lastDeposit == null)
                throw new RegoException("Can't find deposit");

            playerLastDepositSummaryResponse.Amount = lastDeposit.Amount;
            if (lastDeposit.BonusRedemptionId.HasValue)
            {
                var redemption = await _bonusApiProxy.GetBonusRedemptionAsync(PlayerId, lastDeposit.BonusRedemptionId.Value);
                if (redemption == null)
                    throw new RegoException(ErrorMessagesEnum.ServiceUnavailable.ToString());

                playerLastDepositSummaryResponse.BonusAmount = redemption.Amount;
                playerLastDepositSummaryResponse.BonusCode = lastDeposit.BonusCode;
            }

            return playerLastDepositSummaryResponse;
        }

        [HttpGet]
        public PaymentSettingsResponse OnlineDepositPaymentSettings([FromUri] DefaultPaymentSettingsRequest request)
        {
            var defaultVipLevel = _playerQueries.GetDefaultVipLevel(request.BrandId);
            if (defaultVipLevel == null)
                throw new RegoValidationException(ErrorMessagesEnum.ThereIsNoDefaultVipLevelForRequestedBrand.ToString());

            var paymentGetewaySettings = _paymentGatewaySettingsQueries.GetOnePaymentGatewaySettingsByPlayerId(PlayerId);
            if (paymentGetewaySettings == null)
                throw new RegoException("PaymentGatewaySettings not found");

            var method = paymentGetewaySettings.OnlinePaymentMethodName;

            var paymentSetting = _paymentQueries.GetOnlinePaymentSettings(request.BrandId, PaymentType.Deposit,
                defaultVipLevel.Id.ToString(), method, request.CurrencyCode);

            return new PaymentSettingsResponse(paymentSetting);
        }

        [HttpGet]
        public PaymentSettingsResponse OfflineDepositPaymentSettings([FromUri] DefaultPaymentSettingsRequest request)
        {
            var defaultVipLevel = _playerQueries.GetDefaultVipLevel(request.BrandId);
            if (defaultVipLevel == null)
                throw new RegoValidationException(ErrorMessagesEnum.ThereIsNoDefaultVipLevelForRequestedBrand.ToString());

            var paymentSetting = _paymentQueries.GetOfflinePaymentSettings(request.BrandId, PaymentType.Deposit,
                defaultVipLevel.Id.ToString(), request.CurrencyCode);

            return new PaymentSettingsResponse(paymentSetting);
        }

        [HttpGet]
        public bool IsDepositorsFullNameValid([FromUri]IsDepositorsFullNameValidRequest request)
        {
            var player = _playerQueries.GetPlayer(PlayerId);
            if (player == null)
                throw new RegoException(ErrorMessagesEnum.ServiceUnavailable.ToString());

            return OfflineDepositConfirmValidator.IsAccountNameValid(request.Name, player);
        }

        [HttpGet]
        public OfflineDepositBankAccountResponse GetBankAccountsForOfflineDeposit()
        {
            var player = _playerQueries.GetPlayer(PlayerId);
            var bankAccountsForAdminOfflineDepositRequest =
                _paymentQueries.GetBankAccountsForAdminOfflineDepositRequest(PlayerId);

            if (player == null || bankAccountsForAdminOfflineDepositRequest == null)
                throw new RegoException(ErrorMessagesEnum.ServiceUnavailable.ToString());

            return new OfflineDepositBankAccountResponse
            {
                BankAccounts = bankAccountsForAdminOfflineDepositRequest
                    .Select(o => new BankAccountDto
                    {
                        Id = o.Id,
                        AccountName = o.AccountName,
                        BankName = o.Bank.BankName
                    })
            };
        }

        [HttpGet]
        public PendingDepositsResponse PendingDeposits([FromUri]PendingDepositsRequest request)
        {
            var pendingDeposits = _offlineDepositQueries.GetPendingDeposits(PlayerId);
            if (pendingDeposits == null)
                throw new RegoException(ErrorMessagesEnum.ServiceUnavailable.ToString());

            return new PendingDepositsResponse
            {
                PendingDeposits = pendingDeposits
                    .Select(o => new OfflineDeposit
                    {
                        Id = o.Id,
                        Amount = o.Amount,
                        Status = o.Status.ToString(),
                        DateCreated = o.Created.ToString("yyyy/MM/dd HH:mm:ss zzz"),
                        ReferenceCode = o.TransactionNumber
                    })
            };
        }

        [HttpGet]
        public TransactionHistoryResponse GetTransactions(int page, DateTime? startDate = null, DateTime? endDate = null, TransactionType? transactionType = null)
        {
            var pageSize = 10;

            var dateTimeOffset = DateTimeOffset.Now.AddYears(-1);

            var filteredQueryable = _reportQueries.GetPlayerTransactionRecords(PlayerId, false)
                .Where(o => o.CreatedOn >= dateTimeOffset);

            if (startDate != null)
                filteredQueryable = filteredQueryable
                    .Where(o => o.CreatedOn >= startDate.Value);

            if (endDate != null)
                filteredQueryable = filteredQueryable
                    .Where(o => o.CreatedOn <= endDate.Value);

            if (transactionType != null)
                filteredQueryable = filteredQueryable
                    .Where(o => o.Type == transactionType.ToString());

            var orderedQueryable = filteredQueryable
                .OrderByDescending(o => o.CreatedOn);

            var transactions = orderedQueryable
                .Skip(pageSize * page)
                .Take(pageSize)
                .ToList();

            var totalItemsCount = orderedQueryable.Count();

            var result = transactions.Select(o => new Transaction
            {
                CreatedOn = o.CreatedOn.ToString("yyyy/MM/dd HH:mm"),
                TransactionType = o.Type.ToString(),
                TransactionNumber = o.TransactionNumber,
                Amount = o.MainBalanceAmount,
                AmountFormatted = o.MainBalanceAmount.Format(o.CurrencyCode, false),
                MainBalance = o.MainBalance,
                MainBalanceFormatted = o.MainBalance.Format(o.CurrencyCode, false)
            });

            return new TransactionHistoryResponse
            {
                Transactions = result,
                PageSize = pageSize,
                TotalItemsCount = totalItemsCount
            };
        }

        [HttpGet]
        public DepositHistoryResponse GetDeposits([FromUri]GetDepositsRequest request)
        {
            var pageSize = 10;

            var dateTimeOffset = DateTimeOffset.Now.AddYears(-1);

            var filteredQueryable = _paymentQueries.GetDeposits()
                .Where(o => o.PlayerId == PlayerId)
                .Where(o => o.DateSubmitted >= dateTimeOffset);

            if (request.StartDate != null)
                filteredQueryable = filteredQueryable
                    .Where(o => o.DateSubmitted >= request.StartDate.Value);

            if (request.EndDate != null)
                filteredQueryable = filteredQueryable
                    .Where(o => o.DateSubmitted <= request.EndDate.Value);

            if (request.DepositType != null)
                filteredQueryable = filteredQueryable
                    .Where(o => o.DepositType == request.DepositType.Value);

            var orderedQueryable = filteredQueryable
                .OrderByDescending(o => o.DateSubmitted);

            var deposits = orderedQueryable
                .Skip(pageSize * request.Page)
                .Take(pageSize)
                .ToList();

            var totalItemsCount = orderedQueryable.Count();

            var result = deposits.Select(o => new OfflineDeposit
            {
                Id = o.Id,
                Amount = o.Amount,
                AmountFormatted = o.Amount.Format(o.CurrencyCode, false),
                DateCreated = o.DateSubmitted.ToString("yyyy/MM/dd HH:mm"),
                DepositType = o.DepositType.ToString(),
                ReferenceCode = o.ReferenceCode,
                UnverifyReason = o.UnverifyReason?.ToDescription(),
                Status = o.Status
            });

            return new DepositHistoryResponse
            {
                Deposits = result,
                PageSize = pageSize,
                TotalItemsCount = totalItemsCount
            };
        }

        [HttpGet]
        public WithdrawalHistoryResponse GetWithdrawals([FromUri]GetWithdrawalsRequest request)
        {
            var pageSize = 10;

            var dateTimeOffset = DateTimeOffset.Now.AddYears(-1);

            var filteredQueryable = _paymentQueries.GetOfflineWithdraws()
                .Where(o => o.PlayerBankAccount.Player.Id == PlayerId)
                .Where(o => o.Created >= dateTimeOffset);

            if (request.StartDate != null)
                filteredQueryable = filteredQueryable
                    .Where(o => o.Created >= request.StartDate.Value);

            if (request.EndDate != null)
                filteredQueryable = filteredQueryable
                    .Where(o => o.Created <= request.EndDate.Value);

            var orderedQueryable = filteredQueryable
                .OrderByDescending(o => o.Created);

            var withdraws = orderedQueryable
                .Skip(pageSize * request.Page)
                .Take(pageSize)
                .ToList();

            var totalItemsCount = orderedQueryable.Count();

            var result = withdraws.Select(o => new OfflineWithdrawal
            {
                Id = o.Id,
                Amount = o.Amount,
                AmountFormatted = o.Amount.Format(o.PlayerBankAccount.Player.CurrencyCode, false),
                TransactionNumber = o.TransactionNumber,
                Created = o.Created.ToString("yyyy/MM/dd HH:mm"),
                Status = o.Status
            });

            return new WithdrawalHistoryResponse
            {
                Withdrawals = result,
                PageSize = pageSize,
                TotalItemsCount = totalItemsCount
            };
        }

        [HttpGet]
        public OfflineDeposit GetOfflineDeposit([FromUri]GetOfflineDepositRequest request)
        {
            var deposit = _offlineDepositQueries.GetOfflineDeposit(request.Id);
            if (deposit == null)
                throw new RegoValidationException(ErrorMessagesEnum.RequestedOfflineDepositDoesntExist.ToString());

            return new OfflineDeposit
            {
                Id = deposit.Id,
                Amount = deposit.Amount,
                Status = deposit.Status.ToString(),
                ReferenceCode = deposit.TransactionNumber,
                DateCreated = deposit.Created.ToString("yyyy/MM/dd HH:mm:ss zzz"),
                DepositType = deposit.DepositMethod.ToString(),
                TransferType = deposit.TransferType.ToString(),
                BonusRedemptionId = deposit.BonusRedemptionId/*,
                IdFront = !string.IsNullOrEmpty(deposit.IdFrontImage) ? _fileStarage.Get(deposit.IdFrontImage) : new byte[] { },
                IdBack = !string.IsNullOrEmpty(deposit.IdBackImage) ? _fileStarage.Get(deposit.IdBackImage) : new byte[] { },
                Receipt = !string.IsNullOrEmpty(deposit.ReceiptImage) ? _fileStarage.Get(deposit.ReceiptImage) : new byte[] { }*/
            };
        }

        [HttpPost]
        [ResponseType(typeof(OfflineDepositConfirmResponse))]
        public IHttpActionResult ConfirmDeposit(OfflineDepositConfirmRequest request)
        {
            var depositConfirm =
                Mapper.Map<OfflineDepositConfirm, Core.Payment.Interface.Data.Commands.OfflineDepositConfirm>(request.DepositConfirm);

            _offlineDepositCommands.Confirm(
                     depositConfirm,
                     Username,
                     request.IdFrontImage,
                     request.IdBackImage,
                     request.ReceiptImage);

            var uri = RootToOfflineDeposit + request.DepositConfirm.Id;
            return Created(uri, new OfflineDepositConfirmResponse()
            {
                UriToConfirmedDeposit = uri
            });
        }

        [HttpPost]
        public ValidationResult ValidateConfirmDeposit(OfflineDepositConfirm request)
        {
            var depositConfirm =
                Mapper.Map<OfflineDepositConfirm, Core.Payment.Interface.Data.Commands.OfflineDepositConfirm>(request);

            var errors = new Dictionary<string, string>();

            var result = _paymentQueries.ValidateOfflineDepositRequest(depositConfirm);

            if (result.Errors.Any())
                result.Errors.ForEach(x =>
                {
                    if (!errors.ContainsKey(x.PropertyName))
                        errors.Add(x.PropertyName, x.ErrorMessage);
                });

            return new ValidationResult()
            {
                Errors = errors
            };
        }

        [Authorize]
        [HttpGet]
        public WithdrawalFormDataResponse WithdrawalFormData([FromUri]WithdrawalFormDataRequest request)
        {
            var player = _paymentQueries.GetPlayerWithBank(PlayerId);
            if (player == null)
                throw new RegoException(ErrorMessagesEnum.ServiceUnavailable.ToString());

            var playerBankAccount = player.CurrentBankAccount;
            var paymentSettings = _paymentQueries.GetOfflinePaymentSettings(
                request.BrandId,
                PaymentType.Withdraw,
                player.VipLevelId.ToString(),
                player.CurrencyCode);

            if (playerBankAccount == null)
                return new WithdrawalFormDataResponse { BankAccount = null };

            return new WithdrawalFormDataResponse
            {
                BankAccount = new BankData
                {
                    BankAccountName = playerBankAccount.AccountName,
                    BankAccountNumber = playerBankAccount.AccountNumber,
                    BankName = playerBankAccount.Bank.BankName,
                    City = playerBankAccount.City,
                    Branch = playerBankAccount.Branch,
                    Province = playerBankAccount.Province,
                    SwiftCode = playerBankAccount.SwiftCode,
                    PlayerBankAccountId = playerBankAccount.Id,
                    BankAccountTime = playerBankAccount.Updated ?? playerBankAccount.Created,
                    BankTime = playerBankAccount.Bank.Updated ?? playerBankAccount.Bank.Created,
                    Status = playerBankAccount.Status,
                    Remark = playerBankAccount.Remarks
                },
                PaymentSettings = paymentSettings == null ? null : new PaymentSettingsData
                {
                    MinAmountPerTransaction = paymentSettings.MinAmountPerTransaction,
                    MinAmountPerTransactionFormatted = paymentSettings.MinAmountPerTransaction.Format(player.CurrencyCode, false),
                    MaxAmountPerTransaction = paymentSettings.MaxAmountPerTransaction,
                    MaxAmountPerTransactionFormatted = paymentSettings.MaxAmountPerTransaction.Format(player.CurrencyCode, false),
                    MaxAmountPerDay = paymentSettings.MaxAmountPerDay,
                    MaxAmountPerDayFormatted = paymentSettings.MaxAmountPerDay.Format(player.CurrencyCode, false),
                    DayMaximumDeposit = paymentSettings.MaxTransactionPerDay
                }
            };
        }

        [HttpPost]
        [ResponseType(typeof(OfflineDepositResponse))]
        public async Task<CreatedNegotiatedContentResult<OfflineDepositResponse>> OfflineDeposit(OfflineDepositRequest request)
        {
            ValidateAccountFrozenStatus();

            var offlineDepositRequest = Mapper.DynamicMap<Core.Payment.Interface.Data.Commands.OfflineDepositRequest>(request);
            offlineDepositRequest.PlayerId = PlayerId;

            var deposit = await _offlineDepositCommands.Submit(offlineDepositRequest);

            var uri = RootToOfflineDeposit + deposit.Id;
            return Created(uri, new OfflineDepositResponse
            {
                Id = deposit.Id,
                BonusRedemptionId = deposit.BonusRedemptionId,
                UriToSubmittedOfflineDeposit = string.Empty
            });
        }

        [HttpGet]
        public IEnumerable<BankResponse> GetBanks()
        {
            var player = _paymentQueries.GetPlayerWithBank(PlayerId);
            if (player == null)
                throw new RegoException(ErrorMessagesEnum.ServiceUnavailable.ToString());

            var banks = _paymentQueries.GetBanksByBrand(player.BrandId);
            if (banks == null)
                throw new RegoException(ErrorMessagesEnum.ServiceUnavailable.ToString());

            return banks.Select(x => new BankResponse
            {
                Id = x.Id,
                Name = x.BankName
            });
        }

        [HttpGet]
        public ValidationResult ValidatePlayerBankAccount([FromUri]PlayerBankAccountRequest request)
        {
            var data = new EditPlayerBankAccountData();
            Mapper.DynamicMap(request, data);
            data.PlayerId = PlayerId;

            var result = _paymentQueries.ValidatePlayerBankAccount(data);
            if (result == null)
                throw new RegoException(ErrorMessagesEnum.ServiceUnavailable.ToString());

            var errors = new Dictionary<string, string>();
            if (result.Errors.Any())
            {
                result.Errors.ForEach(x =>
                {
                    if (!errors.ContainsKey(x.PropertyName))
                        errors.Add(x.PropertyName, x.ErrorMessage);
                });
            }

            return new ValidationResult
            {
                Errors = errors
            };
        }

        [HttpPost]
        public PlayerBankAccountResponse CreatePlayerBankAccount(PlayerBankAccountRequest request)
        {
            var data = new EditPlayerBankAccountData();
            Mapper.DynamicMap(request, data);
            data.PlayerId = PlayerId;
            var result = _playerBankAccountCommands.Add(data);

            return new PlayerBankAccountResponse();
        }

        private void ValidateAccountFrozenStatus()
        {
            var player = _playerQueries.GetPlayer(PlayerId);
            if (player.IsFrozen == false)
                return;

            throw new RegoException("Account is frozen.");
        }

        [HttpPost]
        public OfflineWithdrawalResponse OfflineWithdraw(WithdrawalRequest request)
        {
            ValidateAccountFrozenStatus();

            var offlineWithdrawalRequest = new OfflineWithdrawRequest
            {
                PlayerId = PlayerId,
                Amount = request.Amount,
                PlayerBankAccountId = _paymentQueries.GetPlayer(PlayerId).CurrentBankAccount.Id,
                RequestedBy = Username,
                NotificationType = request.NotificationType,
            };
            _withdrawalService.Request(offlineWithdrawalRequest);

            return new OfflineWithdrawalResponse();
        }

        [HttpPost]
        public ValidationResult ValidateWithdrawalRequest(WithdrawalRequest request)
        {
            var player = _paymentQueries.GetPlayer(PlayerId);
            if (player == null)
                throw new RegoException(ErrorMessagesEnum.ServiceUnavailable.ToString());

            var offlineWithdrawalRequest = new OfflineWithdrawRequest
            {
                PlayerId = PlayerId,
                Amount = request.Amount,
                PlayerBankAccountId = player.CurrentBankAccount.Id,
                RequestedBy = Username,
                NotificationType = request.NotificationType,
            };
            var errors = new Dictionary<string, string>();
            try
            {
                var result = _paymentQueries.ValidateOfflineWithdrawalRequest(offlineWithdrawalRequest);

                if (result.Errors.Any())
                    result.Errors.ForEach(x => errors.Add(x.PropertyName, x.ErrorMessage));
            }
            catch (RegoValidationException regoValidationException)
            {
                errors.Add("Amount", regoValidationException.Message);
            }
            return new ValidationResult
            {
                Errors = errors
            };
        }

        [HttpGet]
        public FundTransferFormDataResponse FundTransferFormData([FromUri]FundTransferFormDataRequest request)
        {
            var walletTemplates = _brandQueries.GetWalletTemplates(request.BrandId);

            return new FundTransferFormDataResponse
            {
                Wallets = walletTemplates.Where(wallet => !wallet.IsMain).ToDictionary(w => w.Id, w => w.Name)
            };
        }

        [HttpPost]
        public async Task<FundResponse> FundIn(FundRequest request)
        {
            ValidateAccountFrozenStatus();

            var transferFundRequest = new TransferFundRequest
            {
                PlayerId = PlayerId,
                Amount = request.Amount,
                TransferType = Mapper.Map<Core.Common.Data.TransferFundType>(request.TransferFundType),
                WalletId = request.WalletId.ToString(),
                BonusCode = request.BonusCode
            };

            return new FundResponse
            {
                TransferId = await _transferFundCommands.AddFund(transferFundRequest)
            };
        }

        [HttpPost]
        public async Task<OnlineDepositResponse> SubmitOnlineDeposit(OnlineDepositRequest request)
        {
            var onlineDepositRequest = Mapper.DynamicMap<Core.Payment.Interface.Data.OnlineDepositRequest>(request);

            onlineDepositRequest.PlayerId = PlayerId;
            onlineDepositRequest.RequestedBy = Username;

            var requestResult = await _onlineDepositCommands.SubmitOnlineDepositRequest(onlineDepositRequest);
            var requestResultMapped = Mapper.Map<SubmitOnlineDepositRequestResult>(requestResult);

            return new OnlineDepositResponse
            {
                DepositRequestResult = requestResultMapped
            };
        }

        [HttpGet]
        public ValidationResult ValidateOnlineDepositAmount([FromUri]ValidateOnlineDepositAmount request)
        {
            var errors = new Dictionary<string, string>();
            try
            {
                _onlineDepositCommands.
                    ValidateOnlineDepositAmount(new ValidateOnlineDepositAmountRequest
                    {
                        BrandId = request.BrandId,
                        Amount = request.Amount,
                        PlayerId = PlayerId
                    });
            }
            catch (ValidationException ex)
            {

                errors = ex.Errors
                    .GroupBy(o => o.PropertyName)
                    .Select(o => o.First())
                    .ToDictionary(k => k.PropertyName, v => v.ErrorMessage);
            }

            return new ValidationResult() { Errors = errors };
        }

        [HttpGet]
        public BankAccountIdSettings GetBankAccountForOfflineDeposit(Guid offlineDepositId)
        {
            var bankAccount = _paymentQueries.GetBankAccountForOfflineDeposit(offlineDepositId);
            if (bankAccount == null)
                throw new RegoValidationException(ErrorMessagesEnum.BankAccountDoesnNotExistForOfflineDeposit.ToString());

            return Mapper.DynamicMap<BankAccount, BankAccountIdSettings>(bankAccount);
        }

        [HttpPost]
        [AllowAnonymous]
        public string OnlineDepositPayNotify(OnlineDepositPayNotifyRequest request)
        {
            var commandRequest = Mapper.DynamicMap<Core.Payment.Interface.Data.OnlineDepositPayNotifyRequest>(request);

            var commandResponse = _onlineDepositCommands.PayNotify(commandRequest);

            return commandResponse;
        }

        [HttpGet]
        public CheckOnlineDepositStatusResponse CheckOnlineDepositStatus([FromUri]CheckOnlineDepositStatusRequest request)
        {
            var commandRequest = Mapper.DynamicMap<CheckStatusRequest>(request);

            var commandResponse = _onlineDepositQueries.CheckStatus(commandRequest);
            if (commandResponse == null)
                throw new RegoValidationException(ErrorMessagesEnum.NoDepositRelatedToThisTransactionId.ToString());

            var requestResultMapped = Mapper.Map<CheckStatusResponse>(commandResponse);

            return new CheckOnlineDepositStatusResponse
            {
                DepositStatus = requestResultMapped
            };
        }

        [HttpGet]
        public OnlineDepositFormDataResponse OnlineDepositFormData([FromUri]OnlineDepositFormDataRequest request)
        {
            var response = _paymentGatewaySettingsQueries.GetPaymentGatewaySettingsByPlayerId(PlayerId);
            if (response == null)
                throw new RegoException(ErrorMessagesEnum.ServiceUnavailable.ToString());

            var responseMapped = Mapper.Map<IEnumerable<PaymentGatewaySettings>>(response);

            return new OnlineDepositFormDataResponse
            {
                PaymentGatewaySettings = responseMapped
            };
        }

    }
}