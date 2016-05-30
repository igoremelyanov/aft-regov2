using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Payment;
using AFT.RegoV2.MemberApi.Interface.Bonus;
using AFT.RegoV2.MemberApi.Interface.GameProvider;
using AFT.RegoV2.MemberApi.Interface.Payment;
using AFT.RegoV2.MemberApi.Interface.Player;
using AFT.RegoV2.MemberApi.Interface.Proxy;
using AFT.RegoV2.MemberApi.Interface.Security.IpFiltering;
using AFT.RegoV2.MemberWebsite.Common;
using AFT.RegoV2.MemberWebsite.Models;
using AFT.RegoV2.MemberWebsite.Resources;
using PagedList;
using System.Security.Principal;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.MemberApi.Interface.Common;
using AFT.RegoV2.MemberApi.Interface.Reports;
using AFT.RegoV2.Shared.Caching;

using static System.String;
using RegisterStep2Model = AFT.RegoV2.MemberWebsite.Models.RegisterStep2Model;

namespace AFT.RegoV2.MemberWebsite.Controllers
{
    public class HomeController : ControllerBase
    {
        private ICacheManager _cacheManager;

        #region Static fields and constants

        public const string BrandCode = "138";
        public const string BrandName = "138";

        #endregion

        public HomeController(ICacheManager cacheManager)
        {
            _cacheManager = cacheManager;
        }

        #region Public methods

        [AuthorizeIpAddress(BrandCode)]
        public ActionResult About()
        {
            return View();
        }

        public async Task<ActionResult> Acknowledgement(Guid id)
        {
            var memberApi = GetMemberApiProxy(Request);
            var acknowledgementData = await memberApi.GetAcknowledgementDataAsync(id);

            return View(new AcknowledgementModel
            {
                Date = acknowledgementData.Date
            });
        }

        public async Task<ActionResult> Activate(string token)
        {
            var result = await GetMemberApiProxy(Request).ActivateAsync(token);
            ViewBag.Activated = result.Activated;

            return View(result);
        }

        [Authorize]
        public async Task<ActionResult> BalanceInformation()
        {
            var memberApi = GetMemberApiProxy(Request);

            BalancesResponse balances = null;
            BalanceSetResponse balansesSet = null;
            try
            {
                balances = await memberApi.GetBalancesAsync(new BalancesRequest());
                balansesSet = await memberApi.GetBalancesSetAsync();
            }
            catch (MemberApiProxyException) { }

            var settings = new AppSettings();
            var brandId = settings.BrandId;

            var offlineDeposit = await memberApi.GetOfflineDepositFormDataAsync(brandId);
            var fundTransfer = await memberApi.GetFundTransferFormDataAsync(brandId);
            var withdrawal = await memberApi.GetWithdrawalFormDataAsync(brandId);
            var pedingDeposits = await memberApi.GetPendingDeposits();
            var wallets = await memberApi.GetWalletsAsync(brandId);
            var onlineDeposit = await memberApi.GetOnlineDepositFormDataAsync(brandId);
            var model = new BalanceInformationModel
            {
                Wallets = wallets,
                Balances = balances,
                OfflineDeposit = offlineDeposit,
                OnlineDeposit = onlineDeposit,
                FundTransfer = fundTransfer,
                Withdrawal = withdrawal,
                PendingDeposits = pedingDeposits,
                WalletsBalanceSet = balansesSet
            };
            return View(model);
        }

        [AuthorizeIpAddress(BrandCode)]
        public ActionResult Contact()
        {
            return View();
        }

        [AuthorizeIpAddress(BrandCode)]
        public ActionResult Disclaimer()
        {
            return View();
        }

        [Authorize]
        public async Task<ActionResult> GameList()
        {
            var result = await GetMemberApiProxy(Request).GameListAsync(new GamesRequest()
            {
                PlayerUsername = User.Identity.Name,
                IsForMobile = false, // this need to be setted properly
                PlayerIpAddress = Request.UserHostAddress,
                UserAgent = Request.UserAgent,
                LobbyUrl = Request.Url.AbsoluteUri.Split('?')[0] // like in UGS, need to get more info about it
            });
            return View(result);
        }

        [Authorize]
        public async Task<string> GetProductBalance(Guid? walletId)
        {
            var balances = await GetMemberApiProxy(Request).GetBalancesAsync(new BalancesRequest { WalletId = walletId });

            return SerializeJson(new
            {
                balances.Main,
                balances.Bonus,
                balances.Free,
                balances.Playable
            });
        }

        [AuthorizeIpAddress(BrandCode)]
        public ActionResult Index()
        {
            return RedirectPermanent(nameof(Login));
        }

        protected override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            base.OnResultExecuting(filterContext);

            if (Request.IsAuthenticated)
            {
                var proxy = GetMemberApiProxy(Request);

                try
                {
                    var task = Task.Run(async () =>
                    {
                        var result = await proxy.GetOnSiteMessagesCountAsync();
                        ViewData["UnreadMessages"] = result.Count;
                    });

                    task.Wait();
                }
                catch (AggregateException ex)
                {
                    var exception = ex.InnerExceptions.First();

                    if (exception is MemberApiProxyException)
                    {
                        FormsAuthentication.SignOut();
                        HttpContext.User = new GenericPrincipal(new GenericIdentity(Empty), null);
                    }

                    throw exception;
                }
            }
        }

        [AuthorizeIpAddress(BrandCode)]
        public ActionResult Login()
        {
            if (Request.IsAuthenticated)
            {
                return RedirectToActionLocalized(nameof(Overview));
            }

            return View();
        }

        [Authorize]
        public async Task<ActionResult> Overview()
        {
            var model = new OverviewModel();

            var memberApi = GetMemberApiProxy(Request);

            var bonuses = await memberApi.GetQualifiedBonuses(new QualifiedBonusRequest());
            var bonus = bonuses.FirstOrDefault();

            var messages = await memberApi.GetOnSiteMessagesAsync();

            model.MessageSubject = messages.OnSiteMessages
                .FirstOrDefault()?
                .Subject;

            model.MessageId = messages.OnSiteMessages.FirstOrDefault()?.Id;

            model.Games = new GamesDataView
            {
                IsAuthenticated = Request.IsAuthenticated,
                Data = await GetGames()
            };

            if (bonus == null)
                return View(model);

            model.BonusId = bonus.Id;
            model.BonusCode = bonus.Code;
            model.BonusName = bonus.Name;

            return View(model);
        }

        [AuthorizeIpAddress(BrandCode), HttpPost]
        [RequireHttps]
        public async Task<JsonResult> Login(LoginRequest model)
        {
            const string IPAddressServerVariableName = "REMOTE_ADDR";

            var appSettings = new AppSettings();
            var brandId = appSettings.BrandId;

            model.BrandId = brandId;
            model.IPAddress = Request.ServerVariables[IPAddressServerVariableName];
            model.RequestHeaders = Request.Headers.ToDictionary();

            var loginResult = await GetMemberApiProxy(Request).Login(model);
            return Json(loginResult, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Menu(string currentController, string currentAction)
        {
            var model = new Menu(currentController, currentAction)
            {
                Items = Request.IsAuthenticated
                    ? new List<MenuItem>
                    {
                        new MenuItem
                        {
                            Text = Labels.Menu_Home,
                            Action = nameof(PlayerProfile),
                            SubMenuItems = new List<MenuItem>
                            {
                                new MenuItem {Text = Labels.Menu_PlayerProfile_PlayGames, Action = nameof(GameList)},
                                new MenuItem {Text = Labels.Menu_PlayerProfile_Personal, Action = nameof(PlayerProfile)},
                                new MenuItem {Text = Labels.Menu_PlayerProfile_ReferFriend, Action = nameof(ReferAFriend)},
                                new MenuItem {Text = Labels.Menu_PlayerProfile_ClaimBonus, Action = nameof(ClaimBonusReward)},
                                new MenuItem
                                {
                                    Text = Labels.Menu_PlayerProfile_BalanceInformation,
                                    Action = nameof(BalanceInformation)
                                }
                            }
                        }
                    }
                    : new List<MenuItem>
                    {
                        new MenuItem {Text = Labels.Menu_Home, Action = nameof(Login)},
                        new MenuItem {Text = Labels.Menu_Register, Action = nameof(Register)},
                    }
            };
            model.Items.AddRange(new[]
            {
                new MenuItem {Text = Labels.Menu_Casino},
                new MenuItem {Text = Labels.Menu_LiveCasino},
                new MenuItem {Text = Labels.Menu_Bingo},
                new MenuItem {Text = Labels.Menu_Cashier},
                new MenuItem {Text = Labels.Menu_Promotions},
                new MenuItem {Text = Labels.Menu_News},
                new MenuItem {Text = Labels.Menu_Support}
            });
            return PartialView("_PartialMenu", model);
        }

        [Authorize]
        public async Task<ActionResult> OfflineDepositConfirm(Guid id)
        {
            var memberApi = GetMemberApiProxy(Request);
            var deposit = await memberApi.GetOfflineDeposit(id);

            return View(new OfflineDepositConfirmationModel
            {
                Deposit = deposit
            });
        }

        [Authorize]
        public async Task<ActionResult> OfflineDepositResubmit(Guid id)
        {
            var memberApi = GetMemberApiProxy(Request);
            var deposit = await memberApi.GetOfflineDeposit(id);

            return View("OfflineDepositConfirm", new OfflineDepositConfirmationModel
            {
                Deposit = deposit
            });
        }

        [Authorize]
        public ActionResult OnlineDepositForm()
        {
            return View();
        }

        [HttpPost]
        public async Task<string> OnlineDepositPayNotify(OnlineDepositPayNotifyRequest request)
        {
            var response = await GetMemberApiProxy(Request).OnlineDepositPayNotifyAsync(request);
            return response;
        }

        [Authorize]
        public ActionResult OnlineDepositPayReceive(OnlineDepositPayNotifyRequest request)
        {
            return View("OnlineDepositCompleted");
        }

        [Authorize]
        public ActionResult PlayerProfile()
        {
            return View();
        }

        [AuthorizeIpAddress(BrandCode)]
        public ActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous]
        public async Task<ActionResult> Promo(Guid? id)
        {
            if (!Request.IsAuthenticated)
                return View("PromoAnonymous");

            var memberApi = GetMemberApiProxy(Request);
            var bonuses = await memberApi
                .GetQualifiedBonuses(new QualifiedBonusRequest());

            var claimableBonuses = await memberApi.BonusRedemptionsAsync();

            var orderedBonuses = bonuses
                .OrderBy(o => o.Name);

            var selectedBonus = bonuses
                .SingleOrDefault(o => o.Id == id);

            if (selectedBonus == null)
                return RedirectToAction("Promos");

            var nextBonus = orderedBonuses
                .FirstOrDefault(o => CompareOrdinal(o.Name, selectedBonus.Name) >= 0
                    && o.Id != selectedBonus.Id);

            var previousBonus = orderedBonuses
                .FirstOrDefault(o => CompareOrdinal(o.Name, selectedBonus.Name) <= 0
                    && o.Id != selectedBonus.Id);

            return View(new PromoModel
            {
                Bonus = selectedBonus,
                PreviousId = previousBonus?.Id,
                NextId = nextBonus?.Id,
                PlayerHaveActiveBonus = claimableBonuses.Redemptions.Any()
            });
        }

        [AllowAnonymous]
        public async Task<ActionResult> Promos()
        {
            if (!Request.IsAuthenticated)
                return View();

            var memberApi = GetMemberApiProxy(Request);
            var bonuses = await memberApi.GetQualifiedBonuses(new QualifiedBonusRequest());

            if (!bonuses.Any())
                return View("PromosEmptyPage");

            return View(bonuses);
        }

        [AuthorizeIpAddress(BrandCode)]
        public ActionResult Qna()
        {
            return View();
        }

        [Authorize]
        public ActionResult Messages()
        {
            return View();
        }

        public async Task<ActionResult> Register()
        {
            if (Request.IsAuthenticated)
            {
                return RedirectToActionLocalized(nameof(Overview));
            }

            var settings = new AppSettings();
            var accessProxy = GetMemberApiProxy(Request);
            var result = await accessProxy.RegistrationFormDataAsync(new RegistrationFormDataRequest
            {
                BrandId = settings.BrandId
            });

            return View(result);
        }

        [HttpPost]
        public async Task<JsonResult> Register(RegisterRequest model)
        {
            model.IpAddress = Request.UserHostAddress;
            var registerResult = await GetMemberApiProxy(Request).RegisterAsync(model);
            return Json(registerResult, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult> RegisterStep2(string bonusCode, decimal? amount)
        {
            var bonusCodeKey = "bonusCode";
            var settings = new AppSettings();
            var brandId = settings.BrandId;
            var memberApi = GetMemberApiProxy(Request);

            var playerData = await memberApi.GetPlayerData(User.Identity.Name);
            var paymentSetting = await memberApi.GetOnlinePaymentSetting(brandId, playerData.CurrencyCode);
            var bonuses = await memberApi.GetVisibleDepositQualifiedBonuses(new QualifiedBonusRequest() { Amount = null });
            var bonusList = bonuses.ToList();

            if (!IsNullOrEmpty(bonusCode))
            {

                var codesString = System.Web.HttpContext.Current.Session[bonusCodeKey] as String;
                if (codesString == null)
                    codesString = String.Empty;

                var codes = codesString.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                if (!codes.Any(x => x.Equals(bonusCode, StringComparison.InvariantCultureIgnoreCase)))
                    codes.Add(bonusCode);

                foreach (var code in codes)
                {
                    var selectedBonus = await GetBonusByCode(code, amount.Value);
                    if (selectedBonus != null)
                        bonusList.Insert(0, selectedBonus);
                }
                System.Web.HttpContext.Current.Session[bonusCodeKey] = String.Join(",", codes);
            }

            var model = new RegisterStep2Model
            {
                Min = paymentSetting?.MinAmountPerTransaction,
                Max = paymentSetting?.MaxAmountPerTransaction,
                Bonuses = bonusList,
                Amount = amount
            };

            model.MinFormatted = model.Min?.Format(paymentSetting.CurrencyCode, false, DecimalDisplay.ShowNonZeroOnly);
            model.MaxFormatted = model.Max?.Format(paymentSetting.CurrencyCode, false, DecimalDisplay.ShowNonZeroOnly);
            model.AmountFormatted = model.Amount?.Format();

            if (model.Min.HasValue && model.Max.HasValue)
                model.QuickSelectAmounts = CreateQuickSelectAmounts(model.Min.Value, model.Max.Value, paymentSetting.CurrencyCode);

            if (amount.HasValue || !IsNullOrEmpty(bonusCode))
            {
                TempData["Model"] = model;
                return RedirectToActionLocalized(nameof(RegisterStep2));
            }

            return View(TempData["Model"] ?? model);
        }

        [HttpPost]
        public async Task<ActionResult> RegisterStep2(RegisterStep2Request request)
        {
            return await new Task<ActionResult>(() => new EmptyResult());
        }

        [Authorize]
        public async Task<ActionResult> RegisterStep4()
        {
            var lastDepositInfo = await GetMemberApiProxy(Request).PlayerLastDepositSummaryResponse();
            var model = new RegisterStep4Model
            {
                BonusAmount = lastDepositInfo.BonusAmount == null ? null : (decimal?)Math.Round(lastDepositInfo.BonusAmount.Value, 2),
                BonusCode = lastDepositInfo.BonusCode,
                DepositAmount = Math.Round(lastDepositInfo.Amount, 2),
            };

            model.BonusAmountFormatted = model.BonusAmount?.Format();
            model.DepositAmountFormatted = model.DepositAmount?.Format();
            model.TotalAmount = (model.BonusAmount ?? 0) + (model.DepositAmount ?? 0);
            model.TotalAmountFormatted = model.TotalAmount.Format();

            return View(model);
        }

        [AuthorizeIpAddress(BrandCode)]
        public ActionResult Responsibility()
        {
            return View();
        }

        [Authorize]
        public ActionResult Deposit()
        {
            return View();
        }

        [Authorize]
        public ActionResult ResponsibleGaming()
        {
            return View();
        }

        [AuthorizeIpAddress(BrandCode)]
        public ActionResult Rules()
        {
            return View();
        }

        public ActionResult SetCulture(string cultureCode, string returnPath = "/")
        {
            var cookie = new HttpCookie("CultureCode", cultureCode) { Expires = DateTime.Now.AddYears(1) };
            Response.SetCookie(cookie);
            return Redirect(returnPath);
        }

        [HttpPost]
        public async Task<ValidationResult> SubmitOnlineDepositAmount(
            ValidateOnlineDepositAmount request)
        {
            request.BrandId = new AppSettings().BrandId;
            var response = await GetMemberApiProxy(Request).ValidateOnlineDepositAmount(request);
            System.Web.HttpContext.Current.Session.Add("depositAmount", request.Amount);
            return response;
        }

        [AuthorizeIpAddress(BrandCode)]
        public ActionResult TermsConditions()
        {
            return View();
        }

        #endregion

        #region Private methods

        [Authorize]
        public ActionResult ReferAFriend()
        {
            return View();
        }

        [Authorize]
        public ActionResult ClaimBonusReward()
        {
            return View();
        }

        private async Task<QualifiedBonus> GetBonusByCode(string bonusCode, decimal amount)
        {
            var firstDepositByCodeRequest = new FirstDepositApplicationRequest
            {
                BonusCode = bonusCode,
                DepositAmount = amount
            };

            var bonus = await GetMemberApiProxy(Request).GetFirstDepositBonuseByCode(firstDepositByCodeRequest);
            return bonus;
        }

        private MemberApiProxy GetMemberApiProxy(HttpRequestBase request)
        {
            var appSettings = new AppSettings();
            return new MemberApiProxy(appSettings.MemberApiUrl.ToString(), request.AccessToken());
        }

        #endregion

        protected override void OnException(ExceptionContext filterContext)
        {
            var memberApiProxyException = filterContext.Exception as MemberApiProxyException;
            if (memberApiProxyException != null && memberApiProxyException.StatusCode == HttpStatusCode.Unauthorized)
            {
                filterContext.ExceptionHandled = true;
                FormsAuthentication.SignOut();
                FormsAuthentication.RedirectToLoginPage();
            }
        }

        [Authorize]
        public ActionResult MessagesList(string someData)
        {
            var proxy = GetMemberApiProxy(Request);
            var result = new OnSiteMessagesResponse();

            try
            {
                // Don't rewrite it in async way, there's a bug in mvc5
                var task = Task.Run(async () => result = await proxy.GetOnSiteMessagesAsync());
                task.Wait();
            }
            catch (AggregateException ex)
            {
                throw ex.InnerExceptions.First();
            }

            var onSiteMessages = result.OnSiteMessages
                .OrderByDescending(o => o.Received);

            return PartialView("_PartialMessages", onSiteMessages);
        }

        [AllowAnonymous]
        public async Task<ActionResult> Msg(Guid? id)
        {
            var proxy = GetMemberApiProxy(Request);
            var messagesResult = await proxy.GetOnSiteMessagesAsync();
            var allLoadedMessages = messagesResult.OnSiteMessages.ToList();

            OnSiteMessage selectedMessage;
            if (!id.HasValue)
            {
                selectedMessage = allLoadedMessages
                    .OrderByDescending(o => o.Received)
                    .First();

                id = selectedMessage.Id;
            }
            else
            {
                var messageResult = await proxy.GetOnSiteMessageAsync(new OnSiteMessageRequest
                {
                    OnSiteMessageId = id.Value
                });
                selectedMessage = messageResult.OnSiteMessage;
            }

            var nextMessage = allLoadedMessages
                .OrderBy(o => o.Received)
                .FirstOrDefault(o => o.Received >= selectedMessage.Received && o.Id != id);

            var previousMessage = allLoadedMessages
                .OrderByDescending(o => o.Received)
                .FirstOrDefault(o => o.Received <= selectedMessage.Received && o.Id != id);

            return View("Messages", new MessagesModel
            {
                Messages = allLoadedMessages.OrderByDescending(o => o.Received),
                SelectedMessage = selectedMessage,
                NextLink = nextMessage != null
                    ? Url.Action("Msg", new { id = nextMessage.Id })
                    : null,
                PreviousLink = previousMessage != null
                    ? Url.Action("Msg", new { id = previousMessage.Id })
                    : null
            });
        }

        [Authorize]
        [AuthorizeIpAddress(BrandCode)]
        public async Task<ActionResult> Casino()
        {
            var model = new GamesDataView
            {
                IsAuthenticated = Request.IsAuthenticated
            };

            if (model.IsAuthenticated)
            {

                model.Data = await GetGames();
            }

            return View(model);
        }

        [AuthorizeIpAddress(BrandCode)]
        public async Task<ActionResult> LiveDealer()
        {
            var model = new GamesDataView
            {
                IsAuthenticated = Request.IsAuthenticated
            };

            if (model.IsAuthenticated)
            {
                model.Data = await GetGames();
            }

            return View(model);
        }

        private async Task<GamesResponse> GetGames()
        {
            var duration = TimeSpan.FromMinutes(2);
            var key = $"games_{User.Identity.Name}_{Request.UserHostAddress}";

            return await _cacheManager.GetOrCreateObjectAsync(CacheType.GamesRequestToMemberApi, key, duration,
                async () => await GetMemberApiProxy(Request).GamesAsync(
                    new GamesRequest
                    {
                        PlayerUsername = User.Identity.Name,
                        IsForMobile = false, // this need to be setted properly
                        PlayerIpAddress = Request.UserHostAddress,
                        UserAgent = Request.UserAgent,
                        LobbyUrl = Request.Url.AbsoluteUri.Split('?')[0]
                    })
                );
        }

        [AuthorizeIpAddress(BrandCode)]
        public ActionResult ForgetPasswordStep1()
        {
            return View();
        }

        public async Task<ActionResult> ForgetPasswordStep2()
        {
            var token = Request["token"];
            var result = await GetMemberApiProxy(Request).GetPlayerByResetPasswordToken(token);
            ViewBag.PlayerId = result.PlayerId;
            ViewBag.Token = token;

            return View();
        }

        [AuthorizeIpAddress(BrandCode)]
        public async Task<ActionResult> ForgetPasswordStep3()
        {
            var result = await GetMemberApiProxy(Request).GetPlayerByResetPasswordToken(Request["token"]);
            ViewBag.PlayerId = result.PlayerId;

            return View();
        }

        [Authorize]
        public ActionResult VipLevel()
        {
            return View();
        }

        [AuthorizeIpAddress(BrandCode)]
        public ActionResult VipLevel2()
        {
            return View();
        }

        [Authorize]
        public async Task<ActionResult> ActiveBonus()
        {
            var memberApiProxy = GetMemberApiProxy(Request);

            var bonuses = await memberApiProxy.GetBonusesWithIncompleteWagering();

            return View(bonuses);
        }

        [Authorize]
        public async Task<ActionResult> BonusHistory(string startDate, string endDate)
        {
            DateTime? startDateTime = null;
            if (!IsNullOrEmpty(startDate))
                startDateTime = DateTime.Parse(startDate).StartOfTheDay();

            DateTime? endDateTime = null;
            if (!IsNullOrEmpty(endDate))
                endDateTime = DateTime.Parse(endDate).EndOfTheDay();

            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;

            var memberApiProxy = GetMemberApiProxy(Request);

            var bonuses = await memberApiProxy.GetCompleteBonuses(startDateTime, endDateTime);

            return View(bonuses);
        }

        [Authorize]
        public async Task<ActionResult> ClaimBonus(string bonusCode = null)
        {
            var memberApiProxy = GetMemberApiProxy(Request);

            var qualifiedBonuses = await memberApiProxy.BonusRedemptionsAsync();
            var bonusesList = qualifiedBonuses.Redemptions.ToList();

            return View(bonusesList);
        }

        [Authorize]
        public ActionResult ClaimBonusEligible()
        {
            return View();
        }

        [Authorize]
        public ActionResult WithdrawalFrozen()
        {
            return View();
        }

        [Authorize]
        public ActionResult WithdrawalUnverifiedBankAccount()
        {
            return View();
        }

        [Authorize]
        public ActionResult WithdrawalUnverifyingBankAccount()
        {
            return View();
        }

        [Authorize]
        public ActionResult WithdrawalRejectedBankAccount()
        {
            return View();
        }

        [Authorize]
        public ActionResult WithdrawalVerifiedBankAccount()
        {
            return View();
        }

        [Authorize]
        public async Task<ActionResult> GameSummary(string startDate, string endDate, Guid? game, int page = 1)
        {
            DateTime? startDateTime = null;
            if (!IsNullOrEmpty(startDate))
                startDateTime = DateTime.Parse(startDate).StartOfTheDay();

            DateTime? endDateTime = null;
            if (!IsNullOrEmpty(endDate))
                endDateTime = DateTime.Parse(endDate).EndOfTheDay();

            var transactionItem = await GetMemberApiProxy(Request).PlayerTransactionItems(page - 1, startDateTime, endDateTime, game);

            var gameList = await GetMemberApiProxy(Request).GameDtos();

            var selectItems =
                gameList
                    .Select(x => new SelectListItem
                    {
                        Value = x.Id.Value.ToString(),
                        Text = x.Name
                    });

            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;
            ViewBag.GameId = game?.ToString();

            var pagedList = new StaticPagedList<TransactionItemResponse>(
                transactionItem.Items,
                page,
                10,
                transactionItem.TotalItemsCount);
            return View(new GameSummaryModel()
            {
                TransactionItemResponses = pagedList,
                Games = new SelectList(selectItems, "Value", "Text")
            });
        }

        [Authorize]
        public ActionResult GameSummaryEmpty()
        {
            return View();
        }

        #region Cashier

        [Authorize]
        public ActionResult CasherHome()
        {
            return View("BalanceDetails");
        }

        [Authorize]
        public async Task<ActionResult> Withdrawal(bool isSuccess = false)
        {
            var playerProfile = await GetMemberApiProxy(Request).ProfileAsync();
            if (playerProfile.IsFrozen)
                return RedirectToActionLocalized(nameof(FrozenAccount));

            var withdrawalData = await GetMemberApiProxy(Request).GetWithdrawalFormDataAsync(new AppSettings().BrandId);

            if (withdrawalData.BankAccount == null)
                return RedirectToActionLocalized(nameof(CreateBankAccount));
            if (withdrawalData.BankAccount.Status == BankAccountStatus.Pending)
                return View("PlayerBankAcountPending");
            if (withdrawalData.BankAccount.Status == BankAccountStatus.Rejected)
                return RedirectToActionLocalized(nameof(RejectBankAccount));
            if (isSuccess)
            {
                TempData["SuccesfullyRecieved"] = true;
                return RedirectToActionLocalized(nameof(Withdrawal));
            }

            ViewBag.SuccesfullyRecieved = TempData["SuccesfullyRecieved"];
            return View(withdrawalData);
        }

        [Authorize]
        public ActionResult FrozenAccount()
        {
            return View();
        }

        [Authorize]
        public ActionResult WithdrawalSuccess()
        {
            return View();
        }

        [Authorize]
        public ActionResult CreateBankAccount()
        {
            var model = new CreateBankAccountModel()
            {
                IsRejected = false
            };

            return View(model);
        }

        [Authorize]
        public async Task<ActionResult> RejectBankAccount()
        {
            var withdrawalData = await GetMemberApiProxy(Request).GetWithdrawalFormDataAsync(new AppSettings().BrandId);
            var model = new CreateBankAccountModel()
            {
                IsRejected = true,
                Remark = withdrawalData.BankAccount.Remark
            };

            return View("CreateBankAccount", model);
        }
        [Authorize]
        public ActionResult BalanceDetails()
        {
            return View();
        }

        [Authorize]
        public async Task<ActionResult> TransactionHistory(string startDate, string endDate, TransactionType? transactionType, int page = 1)
        {
            var memberApi = GetMemberApiProxy(Request);

            DateTime? startDateTime = null;
            if (!IsNullOrEmpty(startDate))
                startDateTime = DateTime.Parse(startDate).StartOfTheDay();

            DateTime? endDateTime = null;
            if (!IsNullOrEmpty(endDate))
                endDateTime = DateTime.Parse(endDate).EndOfTheDay();

            var historyResponse = await memberApi.GetTransactions(page - 1, startDateTime, endDateTime, transactionType);

            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;
            ViewBag.TransactionType = transactionType?.ToString();

            var pagedList = new StaticPagedList<Transaction>(
                historyResponse.Transactions,
                page,
                historyResponse.PageSize,
                historyResponse.TotalItemsCount);

            return View(pagedList);
        }

        [Authorize]
        public async Task<ActionResult> OnlineDepositConfirmation()
        {
            var lastDepositInfo = await GetMemberApiProxy(Request).PlayerLastDepositSummaryResponse();
            var model = new RegisterStep4Model();

            if (lastDepositInfo.BonusAmount.HasValue)
            {
                model.BonusAmount = Math.Round(lastDepositInfo.BonusAmount.Value, 2);
                model.BonusAmountFormatted = model.BonusAmount.Value.Format();
                model.BonusCode = lastDepositInfo.BonusCode;
            }

            model.DepositAmount = Math.Round(lastDepositInfo.Amount, 2);
            model.DepositAmountFormatted = model.DepositAmount.Value.Format();

            model.TotalAmount = (model.BonusAmount ?? 0) + (model.DepositAmount ?? 0);
            model.TotalAmountFormatted = model.TotalAmount.Format();

            return View(model);
        }

        [Authorize]
        public async Task<ActionResult> OnlineDeposit()
        {
            var settings = new AppSettings();

            var memberApi = GetMemberApiProxy(Request);
            var playerData = await memberApi.GetPlayerData(User.Identity.Name);

            ViewBag.IsFrozen = playerData.IsFrozen;

            var balances = await memberApi.GetBalancesAsync(new BalancesRequest());
            var paymentSettings = await memberApi.GetOnlinePaymentSetting(settings.BrandId, balances.CurrencyCode);

            var bankAccountResponse = await memberApi.GetBankAccountsForOfflineDeposit();

            if (paymentSettings == null)
                paymentSettings = new PaymentSettingsResponse();

            return View(new OfflineDepositModel
            {
                CurrencyCode = balances.CurrencyCode,
                DepositAmountMin = paymentSettings.MinAmountPerTransaction,
                DepositAmountMinFormatted = paymentSettings.MinAmountPerTransaction.Format(balances.CurrencyCode, false),
                DepositAmountMax = paymentSettings.MaxAmountPerTransaction,
                DepositAmountMaxFormatted = paymentSettings.MaxAmountPerTransaction.Format(balances.CurrencyCode, false),
                DailyMaximumDepositAmount = paymentSettings.MaxAmountPerDay,
                DailyMaximumDepositAmountFormatted = paymentSettings.MaxAmountPerDay.Format(balances.CurrencyCode, false),
                DailyMaximumDepositCount = paymentSettings.MaxTransactionPerDay,
                BankAccounts = bankAccountResponse.BankAccounts.Select(o => new BankAccount
                {
                    Id = o.Id,
                    Description = $"{o.BankName} - {o.AccountName}"
                }),
                QuickSelectAmounts = CreateQuickSelectAmounts(paymentSettings.MinAmountPerTransaction, paymentSettings.MaxAmountPerTransaction, balances.CurrencyCode)
            });
        }

        [Authorize]
        public async Task<ActionResult> OfflineDeposit()
        {
            var settings = new AppSettings();
            var memberApi = GetMemberApiProxy(Request);
            var playerData = await memberApi.GetPlayerData(User.Identity.Name);

            ViewBag.IsFrozen = playerData.IsFrozen;

            var balances = await memberApi.GetBalancesAsync(new BalancesRequest());
            var paymentSettings = await memberApi.GetOfflinePaymentSetting(settings.BrandId, balances.CurrencyCode);
            var bankAccountResponse = await memberApi.GetBankAccountsForOfflineDeposit();

            if (paymentSettings == null)
                paymentSettings = new PaymentSettingsResponse();

            return View(new OfflineDepositModel
            {
                CurrencyCode = balances.CurrencyCode,
                DepositAmountMin = paymentSettings.MinAmountPerTransaction,
                DepositAmountMinFormatted = paymentSettings.MinAmountPerTransaction.Format(balances.CurrencyCode, false),
                DepositAmountMax = paymentSettings.MaxAmountPerTransaction,
                DepositAmountMaxFormatted = paymentSettings.MaxAmountPerTransaction.Format(balances.CurrencyCode, false),
                DailyMaximumDepositAmount = paymentSettings.MaxAmountPerDay,
                DailyMaximumDepositAmountFormatted = paymentSettings.MaxAmountPerDay.Format(balances.CurrencyCode, false),
                DailyMaximumDepositCount = paymentSettings.MaxTransactionPerDay,
                BankAccounts = bankAccountResponse.BankAccounts.Select(o => new BankAccount
                {
                    Id = o.Id,
                    Description = $"{o.BankName}"
                }),
                QuickSelectAmounts = CreateQuickSelectAmounts(paymentSettings.MinAmountPerTransaction, paymentSettings.MaxAmountPerTransaction, balances.CurrencyCode)
            });
        }

        private IEnumerable<QuickSelectAmount> CreateQuickSelectAmounts(decimal min, decimal max, string currency)
        {
            const int quickSelectCount = 4;
            const decimal quickSelectLimit = 5000;

            max = max == 0 || max >= quickSelectLimit
                ? quickSelectLimit
                : max;

            var increment = (max - min) / quickSelectCount;

            var quickSelectAmounts = new List<QuickSelectAmount>();

            for (var amount = min + increment; amount <= max; amount += increment)
            {
                var truncatedAmount = Math.Truncate(amount);
                quickSelectAmounts.Add(new QuickSelectAmount
                {
                    Amount = truncatedAmount,
                    AmountFormatted = truncatedAmount.Format(currency, true, DecimalDisplay.ShowNonZeroOnly)
                });
            }

            return quickSelectAmounts;
        }

        [Authorize]
        public async Task<ActionResult> OfflineDepositConfirmation(Guid depositId, Guid? redemptionId)
        {
            var memberApi = GetMemberApiProxy(Request);
            var deposit = await memberApi.GetOfflineDeposit(depositId);
            var balances = await memberApi.GetBalancesAsync(new BalancesRequest());
            var redemption = redemptionId != null
                ? await memberApi.GetRedemption(redemptionId.Value)
                : new ClaimableRedemption();

            return View(new DepositConfirmationModel
            {
                DepositAmount = deposit.Amount,
                DepositAmountFormatted = deposit.Amount.Format(balances.CurrencyCode, false),
                BonusAmount = redemption.Amount,
                BonusAmountFormatted = redemption.Amount.Format(balances.CurrencyCode, false),
                Total = deposit.Amount + redemption.Amount,
                TotalFormatted = (deposit.Amount + redemption.Amount).Format(balances.CurrencyCode, false),
                Bonus = redemption.BonusName,
                CurrencyCode = balances.CurrencyCode
            });
        }

        public async Task<ActionResult> DepositHistoryConfirmation(Guid depositId)
        {
            var memberApi = GetMemberApiProxy(Request);
            var deposit = await memberApi.GetOfflineDeposit(depositId);

            ViewBag.BonusOn = deposit.BonusRedemptionId != null;

            return View();
        }

        [Authorize]
        public async Task<ActionResult> DepositHistory(string startDate, string endDate, DepositType? depositType, int page = 1)
        {
            var memberApi = GetMemberApiProxy(Request);

            DateTime? startDateTime = null;
            if (!IsNullOrEmpty(startDate))
                startDateTime = DateTime.Parse(startDate).StartOfTheDay();

            DateTime? endDateTime = null;
            if (!IsNullOrEmpty(endDate))
                endDateTime = DateTime.Parse(endDate).EndOfTheDay();

            var historyResponse = await memberApi.GetDeposits(page - 1, startDateTime, endDateTime, depositType);

            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;
            ViewBag.DepositType = depositType?.ToString();

            var playerHasValidIdDocuments = await memberApi.ArePlayersIdDocumentsValid();

            var pagedList = new StaticPagedList<OfflineDeposit>(
                historyResponse.Deposits,
                page,
                historyResponse.PageSize,
                historyResponse.TotalItemsCount);

            return View(new DepositHistoryModel
            {
                PagedList = pagedList,
                PlayerHasValidIdDocuments = playerHasValidIdDocuments
            });
        }

        [Authorize]
        public async Task<ActionResult> WithdrawalHistory(string startDate, string endDate, int page = 1)
        {
            var memberApi = GetMemberApiProxy(Request);

            DateTime? startDateTime = null;
            if (!IsNullOrEmpty(startDate))
                startDateTime = DateTime.Parse(startDate).StartOfTheDay();

            DateTime? endDateTime = null;
            if (!IsNullOrEmpty(endDate))
                endDateTime = DateTime.Parse(endDate).EndOfTheDay();

            var historyResponse = await memberApi.GetWithdrawals(page - 1, startDateTime, endDateTime);

            if (!historyResponse.Withdrawals.Any())
            {
                page = 1;
                historyResponse = await memberApi.GetWithdrawals(page - 1, startDateTime, endDateTime);
            }

            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;

            var pagedList = new StaticPagedList<OfflineWithdrawal>(
                historyResponse.Withdrawals,
                page,
                historyResponse.PageSize,
                historyResponse.TotalItemsCount);

            return View(pagedList);
        }

        #endregion


        public class MessagesModel
        {
            public IOrderedEnumerable<OnSiteMessage> Messages { get; set; }
            public OnSiteMessage SelectedMessage { get; set; }
            public string NextLink { get; set; }
            public string PreviousLink { get; set; }
        }

        public class AcknowledgementModel
        {
            #region Properties

            public string Date { get; set; }

            #endregion
        }

        public class CreateBankAccountModel
        {
            public bool IsRejected { get; set; }
            public string Remark { get; set; }
        }
    }

    public class DepositHistoryModel
    {
        public StaticPagedList<OfflineDeposit> PagedList { get; set; }
        public bool PlayerHasValidIdDocuments { get; set; }
    }

    public class GameSummaryModel
    {
        public StaticPagedList<TransactionItemResponse> TransactionItemResponses { get; set; }
        public SelectList Games { get; set; }
    }
}