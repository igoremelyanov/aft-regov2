using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web.Mvc;
using AFT.RegoV2.AdminWebsite.Common;
using AFT.RegoV2.AdminWebsite.Common.jqGrid;
using AFT.RegoV2.Core.Auth.Interface;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Fraud.Interface;
using AFT.RegoV2.Core.Fraud.Interface.ApplicationServices;
using AFT.RegoV2.Core.Fraud.Interface.Data;
using AFT.RegoV2.Core.Game.Exceptions;
using AFT.RegoV2.Core.Payment.Interface.ApplicationServices;
using AFT.RegoV2.Core.Payment.Interface.Data;
using AFT.RegoV2.Core.Payment.Interface.Data.Commands;
using AFT.RegoV2.Core.Security.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using WithdrawalStatus = AFT.RegoV2.Domain.Payment.Data.WithdrawalStatus;

namespace AFT.RegoV2.AdminWebsite.Controllers
{
    [Authorize]
    public class OfflineWithdrawController : BaseController
    {
        private readonly IWithdrawalService _service;
        private readonly IPaymentQueries _paymentQueries;
        private readonly IAdminQueries _adminQueries;
        private readonly IWithdrawalVerificationLogsQueues _withdrawalVerificationLogsQueues;
        private readonly IActorInfoProvider _actorInfoProvider;

        public OfflineWithdrawController(
            IWithdrawalService service,
            IPaymentQueries paymentQueries,
            IAdminQueries adminQueries,
            IWithdrawalVerificationLogsQueues withdrawalVerificationLogsQueues,
            IActorInfoProvider actorInfoProvider)
        {
            _service = service;
            _paymentQueries = paymentQueries;
            _adminQueries = adminQueries;
            _withdrawalVerificationLogsQueues = withdrawalVerificationLogsQueues;
            _actorInfoProvider = actorInfoProvider;
        }

        [SearchPackageFilter("searchPackage")]
        public ActionResult RequestedList(SearchPackage searchPackage)
        {
            var data = SearchOfflineWithdraws(
                searchPackage,
                obj => new[]
                {
                    obj.PlayerBankAccount.Player.Username,
                    obj.PlayerBankAccount.Bank.Brand.Name,
                    obj.PlayerBankAccount.Bank.BankName,
                    obj.TransactionNumber,
                    obj.PlayerBankAccount.Province,
                    obj.PlayerBankAccount.City,
                    obj.PlayerBankAccount.Branch,
                    obj.PlayerBankAccount.SwiftCode,
                    obj.PlayerBankAccount.Address,
                    obj.PlayerBankAccount.AccountName,
                    obj.PlayerBankAccount.AccountNumber,
                    obj.Amount.ToString(CultureInfo.InvariantCulture),
                    obj.Created.ToString(CultureInfo.InvariantCulture),
                    obj.CreatedBy,
                    obj.Remarks
                },
                _service.GetWithdrawalsForVerification());

            return new JsonResult
            {
                Data = data,
                MaxJsonLength = int.MaxValue,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        [SearchPackageFilter("searchPackage")]
        public ActionResult VerifiedList(SearchPackage searchPackage)
        {
            var data = SearchOfflineWithdraws(
                searchPackage,
                obj => new[]
                {
                    obj.PlayerBankAccount.Player.Username,
                    obj.PlayerBankAccount.Bank.Brand.Name,
                    obj.PlayerBankAccount.Bank.BankName,
                    obj.TransactionNumber,
                    obj.PlayerBankAccount.Province,
                    obj.PlayerBankAccount.City,
                    obj.PlayerBankAccount.Branch,
                    obj.PlayerBankAccount.SwiftCode,
                    obj.PlayerBankAccount.Address,
                    obj.PlayerBankAccount.AccountName,
                    obj.PlayerBankAccount.AccountNumber,
                    obj.Amount.ToString(CultureInfo.InvariantCulture),
                    obj.Created.ToString(CultureInfo.InvariantCulture),
                    obj.CreatedBy,
                    obj.Remarks
                },
                _service.GetWithdrawalsForAcceptance());

            return new JsonResult
            {
                Data = data,
                MaxJsonLength = int.MaxValue,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        [SearchPackageFilter("searchPackage")]
        public ActionResult AcceptedList(SearchPackage searchPackage)
        {
            var data = SearchOfflineWithdraws(
                searchPackage,
                obj => new[]
                {
                    obj.PlayerBankAccount.Player.Username,
                    obj.PlayerBankAccount.Bank.Brand.Name,
                    obj.PlayerBankAccount.Bank.BankName,
                    obj.TransactionNumber,
                    obj.PlayerBankAccount.Province,
                    obj.PlayerBankAccount.City,
                    obj.PlayerBankAccount.Branch,
                    obj.PlayerBankAccount.SwiftCode,
                    obj.PlayerBankAccount.Address,
                    obj.PlayerBankAccount.AccountName,
                    obj.PlayerBankAccount.AccountNumber,
                    obj.Amount.ToString(CultureInfo.InvariantCulture),
                    obj.Created.ToString(CultureInfo.InvariantCulture),
                    obj.CreatedBy,
                    obj.Remarks
                },
                _service.GetWithdrawalsForApproval());

            return new JsonResult
            {
                Data = data,
                MaxJsonLength = int.MaxValue,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        [SearchPackageFilter("searchPackage")]
        public ActionResult VerificationQueueList(SearchPackage searchPackage)
        {
            var data = SearchOfflineWithdraws(
                searchPackage,
                obj => new[]
                {
                    obj.Id.ToString(),
                    obj.PlayerBankAccount.Player.Id.ToString(),
                    obj.PlayerBankAccount.Player.Username,
                    obj.PlayerBankAccount.Player.FullName,
                    obj.TransactionNumber,
                    obj.PaymentMethod.ToString(),
                    obj.PlayerBankAccount.Player.CurrencyCode,
                    obj.Amount.ToString(CultureInfo.InvariantCulture),
                    obj.Status == WithdrawalStatus.AutoVerificationFailed ? WithdrawalStatus.New.ToString() : obj.Status.ToString(),
                    obj.AutoVerificationCheckStatus == null ? "-" : obj.AutoVerificationCheckStatus.ToString(),
                    obj.RiskLevelStatus == null ? "-" : obj.RiskLevelStatus.ToString()
                },
                _service.GetWithdrawalsForVerificationQueue());

            return new JsonResult
            {
                Data = data,
                MaxJsonLength = int.MaxValue,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        [SearchPackageFilter("searchPackage")]
        public ActionResult OnHoldList(SearchPackage searchPackage)
        {
            var data = SearchOfflineWithdraws(
                 searchPackage,
                 obj => new[]
                {
                    obj.Id.ToString(),
                    obj.PlayerBankAccount.Player.Id.ToString(),
                    obj.PlayerBankAccount.Player.Username,
                    obj.PlayerBankAccount.Player.FullName,
                    obj.TransactionNumber,
                    obj.PaymentMethod.ToString(),
                    obj.PlayerBankAccount.Player.CurrencyCode,
                    obj.Amount.ToString(CultureInfo.InvariantCulture),
                    obj.Status.ToString()
                },
                 _service.GetWithdrawalsOnHold());

            return new JsonResult
            {
                Data = data,
                MaxJsonLength = int.MaxValue,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        [SearchPackageFilter("searchPackage")]
        public ActionResult AcceptList(SearchPackage searchPackage)
        {
            var data = SearchOfflineWithdraws(
                 searchPackage,
                 obj => new[]
                {
                    obj.Id.ToString(),
                    obj.PlayerBankAccount.Player.Id.ToString(),
                    obj.PlayerBankAccount.Player.Username,
                    obj.PlayerBankAccount.Player.FullName,
                    obj.TransactionNumber,
                    obj.PaymentMethod.ToString(),
                    obj.PlayerBankAccount.Player.CurrencyCode,
                    obj.Amount.ToString(CultureInfo.InvariantCulture),
                    obj.Status.ToString()
                },
                 _service.GetWithdrawalsVerified());

            return new JsonResult
            {
                Data = data,
                MaxJsonLength = int.MaxValue,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        [SearchPackageFilter("searchPackage")]
        public ActionResult ReleaseList(SearchPackage searchPackage)
        {
            var data = SearchOfflineWithdraws(
                 searchPackage,
                 obj => new[]
                {
                    obj.Id.ToString(),
                    obj.PlayerBankAccount.Player.Id.ToString(),
                    obj.PlayerBankAccount.Player.Username,
                    obj.PlayerBankAccount.Player.FullName,
                    obj.TransactionNumber,
                    obj.PaymentMethod.ToString(),
                    obj.PlayerBankAccount.Player.CurrencyCode,
                    obj.Amount.ToString(CultureInfo.InvariantCulture),
                    obj.Status.ToString()
                },
                 _service.GetWithdrawalsAccepted());

            return new JsonResult
            {
                Data = data,
                MaxJsonLength = int.MaxValue,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public ActionResult AutoVerificationStatus(Guid id)
        {
            var withdrawal = _service.GetWithdrawals().FirstOrDefault(x => x.Id == id);

            var verificationStatus = _withdrawalVerificationLogsQueues.AutoVerificationStatus(id,
                withdrawal.PlayerBankAccount.Player.Brand.Name,
                withdrawal.PlayerBankAccount.Player.Brand.LicenseeName,
                withdrawal.PlayerBankAccount.Player.Username);

            return Json(new
            {
                statuses = verificationStatus.ListOfAppliedChecks,
                verificationStatus.VerificationDialogHeaderValues.PlayerName,
                verificationStatus.VerificationDialogHeaderValues.BrandName,
                verificationStatus.VerificationDialogHeaderValues.LicenseeName,
                verificationStatus.VerificationDialogHeaderValues.StatusSuccess
            });
        }

        public ActionResult RiskProfileCheckStatus(Guid id)
        {
            var withdrawal = _service.GetWithdrawals().FirstOrDefault(x => x.Id == id);

            var verificationStatus = _withdrawalVerificationLogsQueues.RiskProfileCheckStatus(id,
                withdrawal.PlayerBankAccount.Player.Brand.Name,
                withdrawal.PlayerBankAccount.Player.Brand.LicenseeName,
                withdrawal.PlayerBankAccount.Player.Username);

            return Json(new
            {
                statuses = verificationStatus.ListOfAppliedChecks,
                verificationStatus.VerificationDialogHeaderValues.PlayerName,
                verificationStatus.VerificationDialogHeaderValues.BrandName,
                verificationStatus.VerificationDialogHeaderValues.LicenseeName,
                verificationStatus.VerificationDialogHeaderValues.StatusSuccess
            });
        }

        [HttpGet]
        public ActionResult Get(Guid id)
        {
            try
            {
                var offlineWithdraw = _paymentQueries.GetWithdrawById(id);
                return
                    this.Success(
                        new JsonSerializerSettings
                        {
                            ContractResolver = new CamelCasePropertyNamesContractResolver(),
                            PreserveReferencesHandling = PreserveReferencesHandling.Objects
                        }, offlineWithdraw);
            }
            catch (InvalidOperationException exception)
            {
                return this.Failed(exception);
            }
        }

        [HttpPost]
        public string Create(OfflineWithdrawRequest withdrawRequest)
        {
            if (ModelState.IsValid == false)
                return SerializeJson(new { Result = "failed", Data = (string)null });

            withdrawRequest.RequestedBy = System.Threading.Thread.CurrentPrincipal.Identity.Name;

            OfflineWithdrawResponse response;
            try
            {
                response = _service.Request(withdrawRequest);
            }
            catch (Exception ex)
            {
                return SerializeJson(new { Result = "failed", Error = ex.Message });
            }

            return SerializeJson(new { Result = "success", Data = "app:payment.withdraw.requestCreated", Id = response.Id });
        }

        [HttpPost]
        public ActionResult Verify(Guid requestId, string remarks)
        {
            try
            {
                _service.Verify(requestId, remarks);
                return this.Success("app:payment.withdraw.successfullyVerified");
            }
            catch (Exception exception)
            {
                return this.Failed(exception);
            }
        }

        [HttpPost]
        public ActionResult Unverify(Guid requestId, string remarks)
        {
            try
            {
                _service.Unverify(requestId, remarks);
                return this.Success("app:payment.withdraw.successfullyUnverified");
            }
            catch (Exception exception)
            {
                return this.Failed(exception);
            }
        }

        public ActionResult Documents(Guid requestId, string remarks)
        {
            try
            {
                _service.SetDocumentsState(requestId, remarks);
                return this.Success("app:payment.withdraw.successfullyTaggedDocuments");
            }
            catch (Exception exception)
            {
                return this.Failed(exception);
            }
        }

        public ActionResult Investigate(Guid requestId, string remarks)
        {
            try
            {
                _service.SetInvestigateState(requestId, remarks);
                return this.Success("app:payment.withdraw.successfullyTaggedInvestigate");
            }
            catch (Exception exception)
            {
                return this.Failed(exception);
            }
        }

        [HttpPost]
        public ActionResult Approve(Guid requestId, string remarks)
        {
            try
            {
                _service.Approve(requestId, remarks);
                return this.Success("app:payment.withdraw.successfullyReleased");
            }
            catch (InsufficientFundsException)
            {
                return this.Failed("app:payment.insufficentFund");
            }
            catch (Exception exception)
            {
                return this.Failed(exception);
            }
        }

        [HttpPost]
        public ActionResult Reject(Guid requestId, string remarks)
        {
            try
            {
                _service.Reject(requestId, remarks);
                return this.Success("app:payment.withdraw.successfullyRejected");
            }
            catch (Exception exception)
            {
                return this.Failed(exception);
            }
        }

        [HttpPost]
        public ActionResult Accept(Guid requestId, string remarks)
        {
            try
            {
                _service.Accept(requestId, remarks);
                return this.Success("app:payment.withdraw.successfullyAccepted");
            }
            catch (Exception exception)
            {
                return this.Failed(exception);
            }
        }

        [HttpGet]
        public string WithdrawalInfo(Guid id)
        {
            var withdrawal = _paymentQueries.GetWithdrawById(id);
            var verificationQueue = _paymentQueries.GetVerificationQueueRecord(id);
            var onHoldQueue = _paymentQueries.GetOnHoldQueueRecord(id);

            var response = GenerateWithdawalInfoResponse(id, withdrawal, verificationQueue, onHoldQueue);

            return SerializeJson(response);
        }

        private object GenerateWithdawalInfoResponse(Guid id, 
            OfflineWithdraw withdrawal, 
            OfflineWithdrawalHistory verificationQueue, 
            OfflineWithdrawalHistory onHoldQueue)
        {
            var response = new
            {
                BaseInfo = new
                {
                    Licensee = withdrawal.PlayerBankAccount.Bank.Brand.LicenseeName,
                    Brand = withdrawal.PlayerBankAccount.Bank.Brand.Name,
                    withdrawal.PlayerBankAccount.Player.Username,
                    ReferenceCode = withdrawal.TransactionNumber,
                    Status =
                        withdrawal.Status == WithdrawalStatus.AutoVerificationFailed
                            ? WithdrawalStatus.New.ToString()
                            : withdrawal.Status.ToString(),
                    InternalAccount = "no", //TODO: wtf
                    Currency = withdrawal.PlayerBankAccount.Player.CurrencyCode,
                    PaymentMethod = withdrawal.PaymentMethod.ToString(),
                    Amount = withdrawal.Amount.ToString(CultureInfo.InvariantCulture),
                    Submitted = withdrawal.CreatedBy,
                    DateSubmitted = Format.FormatDate(withdrawal.Created)
                },
                BankInformation = new
                {
                    withdrawal.PlayerBankAccount.Bank.BankName,
                    BankAccountName = withdrawal.PlayerBankAccount.AccountName,
                    BankAccountNumber = withdrawal.PlayerBankAccount.AccountNumber,
                    withdrawal.PlayerBankAccount.Branch,
                    withdrawal.PlayerBankAccount.SwiftCode,
                    withdrawal.PlayerBankAccount.Address,
                    withdrawal.PlayerBankAccount.City,
                    withdrawal.PlayerBankAccount.Province
                },
                ProcessInformation = new
                {
                    AutoVerification = new
                    {
                        HasAutoVerification = withdrawal.AutoVerificationCheckStatus != null,
                        Status =
                            withdrawal.AutoVerificationCheckStatus != null
                                ? withdrawal.AutoVerificationCheckStatus.ToString()
                                : "",
                        Date =
                            withdrawal.AutoVerificationCheckDate != null
                                ? Format.FormatDate(withdrawal.AutoVerificationCheckDate)
                                : ""
                    },
                    RiskLevel = new
                    {
                        HasRiskLevel = withdrawal.RiskLevelStatus != null,
                        Status =
                            withdrawal.RiskLevelStatus != null
                                ? withdrawal.RiskLevelStatus.ToString()
                                : "",
                        Date =
                            withdrawal.RiskLevelCheckDate != null
                                ? Format.FormatDate(withdrawal.RiskLevelCheckDate)
                                : ""
                    },
                    VerificationQueue = new
                    {
                        HasResult = verificationQueue != null,
                        Result = verificationQueue != null ? verificationQueue.Action.ToString() : null,
                        HandledBy = verificationQueue != null ? verificationQueue.Username : null,
                        DateHandled = verificationQueue != null ? Format.FormatDate(verificationQueue.DateCreated) : null
                    },
                    OnHoldQueue = new
                    {
                        HasResult = onHoldQueue != null,
                        Result = onHoldQueue != null ? onHoldQueue.Action.ToString() : null,
                        HandledBy = onHoldQueue != null ? onHoldQueue.Username : null,
                        DateHandled = onHoldQueue != null ? Format.FormatDate(onHoldQueue.DateCreated) : null
                    }
                },
                Remarks = new
                {
                    AdminRemarks = GetWithdrawalHistoryFormattedRemakrs(id, true),
                    PlayerRemarks = GetWithdrawalHistoryFormattedRemakrs(id)
                },
                Officer = _actorInfoProvider.Actor.UserName
            };
            return response;
        }

        [HttpPost]
        public ActionResult CancelRequest(Guid requestId, string remark)
        {
            try
            {
                _service.Cancel(requestId, remark);
                return this.Success("app:payment.withdraw.successfullyCanceled");
            }
            catch (InvalidOperationException exception)
            {
                return this.Failed(exception);
            }
        }

        [HttpPost]
        public ActionResult Revert(Guid requestId, string remarks)
        {
            try
            {
                _service.Revert(requestId, remarks);
                return this.Success("app:payment.withdraw.successfullyReverted");
            }
            catch (InvalidOperationException exception)
            {
                return this.Failed(exception);
            }
        }

        private IEnumerable<object> GetWithdrawalHistoryFormattedRemakrs(Guid id, bool remarksFromAdministrator = false)
        {
            var source = remarksFromAdministrator
                ? _paymentQueries.WithdrawalHistories(id).Where(el => el.Action != WithdrawalStatus.New)
                : _paymentQueries.WithdrawalHistories(id).Where(el => el.Action == WithdrawalStatus.New);

            return source
                .OrderByDescending(el => el.DateCreated)
                .Select(el => new
                {
                    user = el.Username,
                    remark = el.Remark
                });
        }

        private SearchPackageResult SearchOfflineWithdraws(SearchPackage searchPackage,
            Expression<Func<OfflineWithdraw, object>> cellExpression, IQueryable<OfflineWithdraw> queryable)
        {
            var brandFilterSelections = _adminQueries.GetBrandFilterSelections();

            queryable = queryable
                .Where(x => brandFilterSelections.Contains(x.PlayerBankAccount.Player.BrandId));

            var dataBuilder = new SearchPackageDataBuilder<OfflineWithdraw>(searchPackage, queryable);

            dataBuilder
                .SetFilterRule(x => x.PlayerBankAccount.Player.BrandId,
                    value => d => d.PlayerBankAccount.Player.BrandId == new Guid(value))
                .Map(obj => obj.Id, cellExpression);
            var data = dataBuilder.GetPageData(obj => obj.Created);
            return data;
        }

        private VerificationDialogHeaderValues GetVerficationDialogHeaderValues(Guid withdrawalId)
        {
            var withdrawal = _service.GetWithdrawals().FirstOrDefault(x => x.Id == withdrawalId);

            return new VerificationDialogHeaderValues
            {
                BrandName = withdrawal.PlayerBankAccount.Player.Brand.Name,
                LicenseeName = withdrawal.PlayerBankAccount.Player.Brand.LicenseeName,
                PlayerName = withdrawal.PlayerBankAccount.Player.Username
            };
        }
    }
}