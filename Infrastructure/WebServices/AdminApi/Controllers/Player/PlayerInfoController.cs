using System;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using System.Web.Http;
using AFT.RegoV2.AdminApi.Controllers.Base;
using AFT.RegoV2.AdminApi.Interface.Common;
using AFT.RegoV2.AdminWebsite.Common;
using AFT.RegoV2.AdminWebsite.Common.jqGrid;
using AFT.RegoV2.ApplicationServices.Report;
using AFT.RegoV2.BoundedContexts.Report.Data;
using AFT.RegoV2.Core.Auth.Interface.ApplicationServices;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Common.Data.Payment;
using AFT.RegoV2.Core.Common.Data.Player;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Payment.Interface.ApplicationServices;
using AFT.RegoV2.Core.Payment.Interface.Data;
using AFT.RegoV2.Core.Player.ApplicationServices;
using AFT.RegoV2.Core.Player.Data;
using AFT.RegoV2.Core.Player.Interface.Data;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Core.Security.Interfaces;
using AFT.RegoV2.Domain.Payment.Data;
using AFT.RegoV2.Infrastructure.Extensions;
using AFT.RegoV2.Shared.Utils;
using Newtonsoft.Json;
using OfflineWithdraw = AFT.RegoV2.Core.Payment.Interface.Data.OfflineWithdraw;
using TransactionType = AFT.RegoV2.Core.Common.Data.Player.TransactionType;

namespace AFT.RegoV2.AdminApi.Controllers.Player
{
    [Authorize]
    public class PlayerInfoController : BaseApiController
    {
        private const string DateFormat = "yyyy/MM/dd";

        private readonly IWalletQueries _walletQueries;
        private readonly PlayerCommands _commands;
        private readonly BrandQueries _brandQueries;
        private readonly IWithdrawalService _withdrawalService;
        private readonly IPaymentQueries _paymentQueries;
        private readonly ReportQueries _reportQueries;
        private readonly PlayerQueries _playerQueries;

        public PlayerInfoController(
            IWalletQueries walletQueries,
            PlayerCommands commands,
            BrandQueries brandQueries,
            IWithdrawalService withdrawalService,
            IPaymentQueries paymentQueries,
            ReportQueries reportQueries,
            PlayerQueries queries,
            IAuthQueries authQueries,
            IAdminQueries adminQueries)
            : base(authQueries, adminQueries)
        {
            _walletQueries = walletQueries;
            _commands = commands;
            _brandQueries = brandQueries;
            _withdrawalService = withdrawalService;
            _paymentQueries = paymentQueries;
            _reportQueries = reportQueries;
            _playerQueries = queries;
        }

        [HttpGet]
        [Filters.SearchPackageFilter("searchPackage")]
        [Route(AdminApiRoutes.ListDepositTransactionsInPlayerInfo)]
        public IHttpActionResult DepositTransactions([FromUri] SearchPackage searchPackage, Guid playerId)
        {
            var query = _paymentQueries.GetDeposits()
                .Where(o => o.PlayerId == playerId);

            var dataBuilder = new SearchPackageDataBuilder<DepositDto>(searchPackage, query);

            dataBuilder.Map(obj => obj.Id, od => new[]
            {
                od.DateSubmitted.ToString("yyyy/MM/dd HH:mm:ss zzz"),
                od.ReferenceCode,
                od.BankAccountNumber,
                od.PaymentMethod,
                od.Amount.ToString("0.00"),
                od.Status
            });
            return Ok(dataBuilder.GetPageData(obj => obj.DateSubmitted));         
        }

        [HttpGet]
        [Filters.SearchPackageFilter("searchPackage")]
        [Route(AdminApiRoutes.ListWithdrawTransactionsInPlayerInfo)]
        public IHttpActionResult WithdrawTransactions([FromUri] SearchPackage searchPackage, Guid playerId)
        {
            var query = _paymentQueries.GetOfflineWithdraws()
                .Where(x =>
                    x.PlayerBankAccount.Player.Id == playerId &&
                    (x.Status == WithdrawalStatus.New ||
                    x.Status == WithdrawalStatus.Approved ||
                    x.Status == WithdrawalStatus.AutoVerificationFailed ||
                    x.Status == WithdrawalStatus.Documents ||
                    x.Status == WithdrawalStatus.Investigation ||
                    x.Status == WithdrawalStatus.Accepted ||
                    x.Status == WithdrawalStatus.Unverified ||
                    x.Status == WithdrawalStatus.Rejected ||
                    x.Status == WithdrawalStatus.Verified ||
                    x.Status == WithdrawalStatus.Reverted ||
                    x.Status == WithdrawalStatus.Canceled));

            var dataBuilder = new SearchPackageDataBuilder<OfflineWithdraw>(searchPackage, query);

            dataBuilder = dataBuilder.SetFilterRule(x => x.PlayerBankAccount.Player.Id, value => p => p.PlayerBankAccount.Player.Id == new Guid(value));
            dataBuilder.Map(obj => obj.Id, od => new object[]
            {
                od.Created.ToString("yyyy/MM/dd HH:mm:ss zzz"),
                od.TransactionNumber,
                od.PlayerBankAccount.AccountNumber,
                Enum.GetName(typeof(PaymentMethod), od.PaymentMethod),
                od.Amount.ToString(CultureInfo.InvariantCulture),
                od.Status.ToString()
            });

            return Ok(dataBuilder.GetPageData(obj => obj.Created));
        }

        [HttpGet]
        [Filters.SearchPackageFilter("searchPackage")]
        [Route(AdminApiRoutes.ListTransactionsInPlayerInfo)]
        public IHttpActionResult Transactions([FromUri] SearchPackage searchPackage, Guid playerId)
        {
            var dataBuilder = new SearchPackageDataBuilder<PlayerTransactionRecord>(searchPackage, _reportQueries.GetPlayerTransactionRecords(playerId, false));
            return Ok(dataBuilder
                .Map(r => r.TransactionId, r => new object[]
                {
                    r.Type,
                    r.MainBalanceAmount + r.BonusBalanceAmount + r.TemporaryBalanceAmount 
                        + r.LockBonusAmount + r.LockFraudAmount + r.LockWithdrawalAmount,
                    r.MainBalance + r.BonusBalance + r.TemporaryBalance,
                    r.CreatedOn.ToString("yyyy/MM/dd HH:mm:ss zzz"),
                    r.Wallet,
                    r.CurrencyCode,
                    r.Description,
                    r.PerformedBy,
                    //r.TransactionNumber
                    r.TransactionId,
                    r.RelatedTransactionId
                })
                .GetPageData(r => r.TransactionId));
        }

        [HttpGet]
        [Filters.SearchPackageFilter("searchPackage")]
        [Route(AdminApiRoutes.ListTransactionsAdvInPlayerInfo)]
        public IHttpActionResult TransactionsAdv([FromUri] SearchPackage searchPackage, Guid playerId)
        {
            var dataBuilder = new SearchPackageDataBuilder<PlayerTransactionRecord>(searchPackage, _reportQueries.GetPlayerTransactionRecords(playerId));
            return Ok(dataBuilder
                .Map(r => r.TransactionId, r => new object[]
                {
                    r.Type,
                    r.MainBalanceAmount,
                    r.MainBalance,
                    r.BonusBalanceAmount,
                    r.BonusBalance,
                    r.TemporaryBalanceAmount,
                    r.TemporaryBalance,
                    r.LockBonusAmount + r.LockFraudAmount + r.LockWithdrawalAmount,
                    r.LockBonus + r.LockFraud + r.LockWithdrawal,
                    //r.MainBalance + r.BonusBalance + r.TemporaryBalance,
                    r.Balance,
                    r.CreatedOn.ToString("yyyy/MM/dd HH:mm:ss zzz"),
                    r.Wallet,
                    r.CurrencyCode,
                    r.Description,
                    r.PerformedBy,
                    //r.TransactionNumber
                    r.TransactionId,
                    r.RelatedTransactionId
                })
                .GetPageData(r => r.TransactionId));
        }

        [HttpGet]
        [Filters.SearchPackageFilter("searchPackage")]
        [Route(AdminApiRoutes.ListActivityLogsInPlayerInfo)]
        public IHttpActionResult ActivityLog([FromUri] SearchPackage searchPackage)
        {
            var query = _playerQueries.GetPlayerActivityLog();

            var dataBuilder = new SearchPackageDataBuilder<PlayerActivityLog>(searchPackage, query);

            dataBuilder.Map(obj => obj.Id, od => new object[]
            {
                od.Category,
                od.ActivityDone,
                od.PerformedBy,
                od.DatePerformed.ToString("yyyy/MM/dd HH:mm:ss zzz"),
                od.Remarks,
                od.UpdatedBy,
                od.DateUpdated.HasValue ? od.DateUpdated.Value.ToString("yyyy/MM/dd HH:mm:ss zzz") : null
            });

            return Ok(dataBuilder.GetPageData(obj => obj.DatePerformed));
        }

        [HttpGet]
        [Filters.SearchPackageFilter("searchPackage")]
        [Route(AdminApiRoutes.ListIdentityVerificationsInPlayerInfo)]
        public IHttpActionResult IdentityVerification([FromUri] SearchPackage searchPackage, Guid playerId)
        {
            var identityVerifications = _playerQueries.GetPlayerIdentityVerifications(playerId);

            var dataBuilder = new SearchPackageDataBuilder<IdentityVerification>(searchPackage, identityVerifications);

            dataBuilder.Map(obj => obj.Id, obj => new object[]
            {
                obj.DocumentType.ToString(),
                obj.CardNumber,
                Format.FormatDate(obj.ExpirationDate, false),
                obj.VerificationStatus.ToString(),
                obj.VerifiedBy,
                Format.FormatDate(obj.DateVerified, false),
                obj.UnverifiedBy,
                Format.FormatDate(obj.DateUnverified, false),
                obj.UploadedBy,
                Format.FormatDate(obj.DateUploaded, false),
                obj.Remarks
            });

            return Ok(dataBuilder.GetPageData(obj => obj.DocumentType));
        }

        [HttpPost]
        [Route(AdminApiRoutes.EditLogRemarkInPlayerInfo)]
        public IHttpActionResult EditLogRemark(EditLogRemarkData data)
        {
            _commands.UpdateLogRemark(data.LogId, data.Remarks);
            return Ok(new { result = "success" });
        }

        [HttpGet]
        [Route(AdminApiRoutes.GetBalancesInPlayerInfo)]
        public async Task<IHttpActionResult> GetBalances(Guid playerId)
        {
            VerifyPermission(Permissions.View, Modules.PlayerManager);
            VerifyPermission(Permissions.View, Modules.OfflineDepositRequests);

            var player = await _playerQueries.GetPlayerAsync(playerId);
            var wallet = await _walletQueries.GetPlayerBalance(playerId);
            
            var offlineDeposits = _paymentQueries.GetOfflineDeposits().Where(x => x.PlayerId == playerId);
            var offlineDepositsWithoutWageringCompleted = await offlineDeposits.Where(x => x.DepositWagering != 0).ToListAsync();
            var playerBetStatistics = await _playerQueries.GetPlayerBetStatistics().FirstOrDefaultAsync(s => s.PlayerId == playerId);

            var offlineTotalWithdrawal = _paymentQueries.GetOfflineWithdraws().Where(x => x.PlayerBankAccount.Player.Id == playerId);

            var playerTransactions = _reportQueries.GetPlayerTransactionRecords(playerId);

            decimal totalWin = 0;
            decimal totalLoss = 0;
            decimal totalAdjustments = 0;
            var approvedDeposits = _paymentQueries.GetDepositsAsQueryable().Where(x => x.PlayerId == playerId && x.Status =="Approved")
                .Select(x=> new { Amount=x.Amount ,Id =x.Id} ).ToList();

            if (playerBetStatistics != null)
            {
                totalWin = playerBetStatistics.TotalWon;
                totalLoss = playerBetStatistics.TotalLoss;
                totalAdjustments = playerBetStatistics.TotlAdjusted;
            }

            return Ok(new
            {
                Balance = new
                {
                    Currency = player.CurrencyCode,
                    MainBalance = wallet.Main,
                    BonusBalance = wallet.Bonus,
                    PlayableBalance = wallet.Playable,
                    FreeBalance = wallet.Free,
                    TotalBonus = playerTransactions.Any() ? playerTransactions.Sum(x => x.BonusBalanceAmount) : 0,
                    DepositCount = approvedDeposits.Count,
                    TotalDeposit = approvedDeposits.Count>0? approvedDeposits.Sum(x => x.Amount) : 0,
                    WithdrawalCount = offlineTotalWithdrawal.Any(s => s.Approved.HasValue) ? offlineTotalWithdrawal.Count(s => s.Approved.HasValue) : 0,
                    TotalWithdrawal = offlineTotalWithdrawal.Any(s => s.Approved.HasValue) ? offlineTotalWithdrawal.Where(s => s.Approved.HasValue).Sum(x => x.Amount) : 0,
                    TotalWin = totalWin,
                    TotalLoss = totalLoss,
                    TotalAdjustments = totalAdjustments,
                    TotalCreditsRefund = "-",
                    TotalCreditsCancellation = "-",
                    TotalChargeback = "-",
                    TotalChargebackReversals = "-",
                    TotalWager = "-",
                    AverageWagering = "-",
                    AverageDeposit = "-",
                    MaxBalance = "-"
                },

                GameBalance = new
                {
                    Product = "-",
                    Balance = "-",
                    BonusBalance = playerTransactions.Any(g => g.GameId.HasValue) ? playerTransactions.Where(g => g.GameId.HasValue).Sum(x => x.BonusBalanceAmount) : 0,
                    BettingBalance = "-",
                    TotalBonus = "-"
                },

                DepositWagering = new
                {
                    TotalWagering = offlineDepositsWithoutWageringCompleted.Any() ? offlineDepositsWithoutWageringCompleted.Select(x => x.Amount).Aggregate((x, y) => x + y) : 0,
                    WageringRequired = offlineDepositsWithoutWageringCompleted.Any() ? offlineDepositsWithoutWageringCompleted.Select(x => x.DepositWagering).Aggregate((x, y) => x + y) : 0,
                }

            });
        }

        [HttpGet]
        [Route(AdminApiRoutes.GetTransactionTypesInPlayerInfo)]
        public IHttpActionResult GetTransactionTypes()
        {
            var transactionTypeNames = Enum.GetNames(typeof(TransactionType)).ToList();

            return Ok(new
            {
                Types = transactionTypeNames.Select(name => new { Name = name })
            });
        }

        [HttpGet]
        [Route(AdminApiRoutes.GetWalletTemplatesInPlayerInfo)]
        public async Task<IHttpActionResult> GetWalletTemplates(Guid playerId)
        {
            VerifyPermission(Permissions.View, Modules.PlayerManager);

            var player = await _playerQueries.GetPlayerAsync(playerId);
            var walletTemplates = _brandQueries.GetWalletTemplates(player.BrandId);

            return Ok(walletTemplates.Select(w => new
            {
                w.Id,
                w.Name
            }));
        }

        [HttpGet]
        [Route(AdminApiRoutes.GetStatusInPlayerInfo)]
        public async Task<IHttpActionResult> GetStatus(Guid playerId)
        {
            VerifyPermission(Permissions.View, Modules.PlayerManager);

            // do not lock
            using (var scope = CustomTransactionScope.GetTransactionScopeAsync(IsolationLevel.ReadUncommitted))
            {
                var player = await _playerQueries.GetPlayerAsync(playerId);
                scope.Complete();
                return Ok(player.IsOnline);
            }
        }

        [HttpGet]
        [Route(AdminApiRoutes.GetPlayerTitleInPlayerInfo)]
        public async Task<IHttpActionResult> GetPlayerTitle(Guid id)
        {
            VerifyPermission(Permissions.View, Modules.PlayerManager);

            var player = await _playerQueries.GetPlayerAsync(id);

            return Ok(new
            {
                player.Username,
                player.FirstName,
                player.LastName,
                player.IsOnline
            });
        }

        [HttpGet]
        [Route(AdminApiRoutes.GetIdentificationDocumentEditDataInPlayerInfo)]
        public IHttpActionResult GetIdentificationDocumentEditData(Guid id)
        {
            var player = _playerQueries.GetPlayer(id);

            return Ok(new
            {
                Username = player.Username,
                BrandName = player.Brand.Name,
                LicenseeName = _brandQueries.GetLicensee(player.Brand.LicenseeId).Name,
                DocumentTypes = Enum.GetNames(typeof(DocumentType))
            });
        }

        [HttpPost]
        [Route(AdminApiRoutes.UploadIdInPlayerInfo)]
        public IHttpActionResult UploadId()
        {
            var request = HttpContext.Current.Request;

            var data = request.Form["data"];
            var playerId = Guid.Parse(request.Form["playerId"]);

            var uploadData = JsonConvert.DeserializeObject<IdUploadData>(data);
            var uploadIdFront = request.Files["uploadId1"];
            var uploadIdBack = request.Files["uploadId2"];

            var frontFileName = uploadIdFront != null ? uploadIdFront.FileName : null;
            var backFileName = uploadIdBack != null ? uploadIdBack.FileName : null;

            uploadData.FrontIdFile = uploadIdFront != null ? uploadIdFront.InputStream.ToByteArray() : null;
            uploadData.BackIdFile = uploadIdBack != null ? uploadIdBack.InputStream.ToByteArray() : null;
            uploadData.FrontName = frontFileName;
            uploadData.BackName = backFileName;

            IdentityVerification identity;
            try
            {
                identity = _commands.UploadIdentificationDocuments(uploadData, playerId, Username);
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    Result = "failed",
                    Data = ex.Message
                });
            }

            return Ok(new
            {
                Result = "success",
                Data = new
                {
                    FrontIdFilename = identity.FrontFile,
                    BackIdFilename = identity.BackFile
                }
            });
        }

        [HttpGet]
        [Route(AdminApiRoutes.GetPlayerInPlayerInfo)]
        public async Task<IHttpActionResult> Get(Guid id)
        {
            VerifyPermission(Permissions.View, Modules.PlayerManager);

            var paymentPlayer = _paymentQueries.GetPlayer(id);

            var player = await _playerQueries.GetPlayerAsync(id);
            var paymentId = await _paymentQueries.GetPlayerPaymentLevels().SingleAsync(p => p.PlayerId == id);
            var brand = await _brandQueries.GetBrandOrNullAsync(player.BrandId);

            var brandTimeZone = TimeZoneInfo.FindSystemTimeZoneById(brand.TimezoneId);
            var now = DateTimeOffset.Now;
            var exemptFrom = paymentPlayer.ExemptWithdrawalFrom ?? now;
            var exemptTo = paymentPlayer.ExemptWithdrawalTo ?? now;
            var exemptFromText = TimeZoneInfo.ConvertTime(exemptFrom, brandTimeZone).ToString(DateFormat);
            var exemptToText = TimeZoneInfo.ConvertTime(exemptTo, brandTimeZone).ToString(DateFormat);

            var question = await _playerQueries.GetSecurityQuestions().SingleOrDefaultAsync(x => x.Id == player.SecurityQuestionId);

            return Ok(new
            {
                player.BrandId,
                player.Username,
                player.FirstName,
                player.LastName,
                DateOfBirth = Format.FormatDate(player.DateOfBirth, false),
                ParsedDateOfBirth = player.DateOfBirth.ToString("MMMM d, y"),
                Title = Enum.GetName(typeof(Title), player.Title),
                Gender = Enum.GetName(typeof(Gender), player.Gender),
                player.Email,
                player.PhoneNumber,
                player.MailingAddressLine1,
                player.MailingAddressLine2,
                player.MailingAddressLine3,
                player.MailingAddressLine4,
                player.MailingAddressCity,
                player.MailingAddressPostalCode,
                player.MailingAddressStateProvince,
                player.PhysicalAddressLine1,
                player.PhysicalAddressLine2,
                player.PhysicalAddressLine3,
                player.PhysicalAddressLine4,
                player.PhysicalAddressCity,
                player.PhysicalAddressPostalCode,
                player.PhysicalAddressStateProvince,
                player.CountryCode,
                PaymentLevel = paymentId != null ? (Guid?)paymentId.PaymentLevel.Id : null,
                ContactPreference = Enum.GetName(typeof(ContactMethod), player.ContactPreference),
                DateRegistered = player.DateRegistered.ToString(DateFormat),
                Licensee = brand.Licensee.Name,
                Brand = brand.Name,
                player.CurrencyCode,
                VipLevel = player.VipLevel == null ? (Guid?)null : player.VipLevel.Id,
                ExemptWithdrawalVerification = paymentPlayer.ExemptWithdrawalVerification.HasValue && paymentPlayer.ExemptWithdrawalVerification.Value,
                ExemptFrom = exemptFromText,
                ExemptTo = exemptToText,
                ExemptLimit = paymentPlayer.ExemptLimit ?? -1,
                SecurityQuestion = question != null ? question.Question : "",
                player.SecurityAnswer,
                player.IsOnline,
                player.AccountAlertEmail,
                player.AccountAlertSms,
                player.MarketingAlertEmail,
                player.MarketingAlertSms,
                player.MarketingAlertPhone,
                Frozen = player.IsFrozen,
                player.IsLocked,
                player.IsInactive,
                IsSelfExcluded = player.SelfExclusion.HasValue,
                IsTimedOut = player.TimeOut.HasValue
            });
        }

        [HttpGet]
        [Route(AdminApiRoutes.GetExemptionDataInPlayerInfo)]
        public async Task<IHttpActionResult> GetExemptionData(Guid id)
        {
            VerifyPermission(Permissions.View, Modules.PlayerManager);

            var paymentPlayer = _paymentQueries.GetPlayer(id);

            var player = await _playerQueries.GetPlayerAsync(id);
            var brand = await _brandQueries.GetBrandOrNullAsync(player.BrandId);

            var brandTimeZone = TimeZoneInfo.FindSystemTimeZoneById(brand.TimezoneId);
            var now = DateTimeOffset.Now;
            var exemptFrom = paymentPlayer.ExemptWithdrawalFrom ?? now;
            var exemptTo = paymentPlayer.ExemptWithdrawalTo ?? now;
            var exemptFromText = TimeZoneInfo.ConvertTime(exemptFrom, brandTimeZone).ToString(DateFormat);
            var exemptToText = TimeZoneInfo.ConvertTime(exemptTo, brandTimeZone).ToString(DateFormat);

            return Ok(new
            {
                ExemptWithdrawalVerification = paymentPlayer.ExemptWithdrawalVerification.HasValue && paymentPlayer.ExemptWithdrawalVerification.Value,
                ExemptFrom = exemptFromText,
                ExemptTo = exemptToText,
                ExemptLimit = paymentPlayer.ExemptLimit ?? -1,
            });
        }

        [HttpPost]
        [Route(AdminApiRoutes.EditPlayerInfo)]
        public IHttpActionResult Edit(EditPlayerData data)
        {
            VerifyPermission(Permissions.Update, Modules.PlayerManager);

            if (ModelState.IsValid == false)
                return Ok(ErrorResponse());

            var validationResult = _commands.ValidateThatPlayerInfoCanBeEdited(data);
            if (!validationResult.IsValid)
                return Ok(ValidationExceptionResponse(validationResult.Errors));

            _commands.Edit(data);
            return Ok(new { Result = "success" });
        }

        [HttpPost]
        [Route(AdminApiRoutes.SubmitExemptionInPlayerInfo)]
        public IHttpActionResult SubmitExemption(Exemption exemption)
        {
            VerifyPermission(Permissions.Exempt, Modules.OfflineWithdrawalExemption);

            if (!ModelState.IsValid)
                return Ok(ErrorResponse());

            _withdrawalService.SaveExemption(exemption);

            return Ok(new { Result = "success" });
        }

        [HttpPost]
        [Route(AdminApiRoutes.SetStatusInPlayerInfo)]
        public IHttpActionResult SetStatus(SetStatusData data)
        {
            VerifyPermission(Permissions.Activate, Modules.PlayerManager);
            VerifyPermission(Permissions.Deactivate, Modules.PlayerManager);

            _commands.SetStatus(data.Id, data.Active);

            return Ok(new { Result = "success", active = data.Active });
        }

        [HttpPost]
        [Route(AdminApiRoutes.SetFreezeStatusInPlayerInfo)]
        public object SetFreezeStatus(FreezeData data)
        {
            if (data.Freeze)
                _commands.FreezeAccount(data.PlayerId);
            else
                _commands.UnfreezeAccount(data.PlayerId);

            return Ok(new { Result = "success" });
        }

        [HttpPost]
        [Route(AdminApiRoutes.UnlockPlayerInPlayerInfo)]
        public object Unlock(PlayerData data)
        {
            _commands.Unlock(data.PlayerId);

            return Ok(new { Result = "success" });
        }

        [HttpPost]
        [Route(AdminApiRoutes.CancelExclusionInPlayerInfo)]
        public object CancelExclusion(PlayerData data)
        {
            _commands.CancelExclusion(data.PlayerId);

            return Ok(new { Result = "success" });
        }

        [HttpPost]
        [Route(AdminApiRoutes.VerifyIdDocumentInPlayerInfo)]
        public IHttpActionResult VerifyIdDocument(VerifyIdDocumentData data)
        {
            try
            {
                _commands.VerifyIdDocument(data.Id, Username);

            }
            catch (Exception ex)
            {
                return Ok(new { Result = "failed", Data = ex.Message });
            }

            return Ok(new { Result = "success" });
        }

        [HttpPost]
        [Route(AdminApiRoutes.UnverifyIdDocumentInPlayerInfo)]
        public IHttpActionResult UnverifyIdDocument(UnverifyIdDocumentData data)
        {
            _commands.UnverifyIdDocument(data.Id, Username);

            return Ok(new { Result = "success" });
        }

        [HttpPost]
        [Route(AdminApiRoutes.ResendActivationEmailInPlayerInfo)]
        public IHttpActionResult ResendActivationEmail(ResendActivationEmailData data)
        {
            _commands.ResendActivationEmail(data.Id);

            return Ok(new { Result = "success" });
        }
    }

    public class FreezeData
    {
        public Guid PlayerId { get; set; }
        public bool Freeze { get; set; }
    }

    public class PlayerData
    {
        public Guid PlayerId { get; set; }
    }
}