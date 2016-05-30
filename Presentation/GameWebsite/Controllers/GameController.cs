using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using AFT.RegoV2.GameWebsite.Helpers;
using AFT.RegoV2.GameWebsite.Models;
using DotNetOpenAuth.OAuth2;
using FakeUGS.Core.Classes;
using FakeUGS.Core.ServiceContracts;
using FakeUGS.Core.ServiceContracts.Base;
using ServiceStack.ServiceClient.Web;

namespace AFT.RegoV2.GameWebsite.Controllers
{

    class GameProviderData
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string ApiPathPrefix { get; set; }
    }

    public class GameController : Controller
    {

        public GameController()
        {
            GameProviderApiPathMap.Add("1814418D-BC00-43B4-AD18-BEBEF6501D7F",
                new GameProviderData { ClientId="MOCK_CASINO_CLIENT_ID", ClientSecret = "MOCK_CLIENT_SECRET", ApiPathPrefix="api/mock"});
            GameProviderApiPathMap.Add("18FB823B-435D-42DF-867E-3BA38ED92060", 
                new GameProviderData { ClientId="MOCK_SPORTS_CLIENT_ID", ClientSecret = "MOCK_CLIENT_SECRET", ApiPathPrefix="api/sports"});
            GameProviderApiPathMap.Add("2F7E5735-AD42-4945-9B72-A3954C2BE07F", 
                new GameProviderData { ClientId= "FLYCOW_CLIENT_ID", ClientSecret = "FLYCOW_CLIENT_SECRET", ApiPathPrefix= "api/mockflycow" });
            GameProviderApiPathMap.Add("602D2FDA-9C54-4EF2-9223-F287EAD4FCFB", 
                new GameProviderData { ClientId= "SUNBET_CLIENT_ID", ClientSecret = "SUNBET_CLIENT_SECRET", ApiPathPrefix= "api/mocksunbet" });
            GameProviderApiPathMap.Add("13B9378A-D78E-4EDA-88E6-4E0A525D0573", 
                new GameProviderData { ClientId= "TGP_CLIENT_ID", ClientSecret = "TGP_CLIENT_SECRET", ApiPathPrefix= "api/mocktgp" });
            GameProviderApiPathMap.Add("4CCB0717-353C-4050-9221-0667A177E224", 
                new GameProviderData { ClientId= "GOLDDELUXE_CLIENT_ID", ClientSecret = "GOLDDELUXE_CLIENT_SECRET", ApiPathPrefix= "api/mockgolddeluxe" });
        }

        private bool IsOAuth 
        {
            get { return true; }
            set { Session["IsOAuth"] = value; }
        }
        private IAuthorizationState AuthState
        {
            get { return Session["AuthState"] as IAuthorizationState; }
            set { Session["AuthState"] = value; }
        }

        private string TokenEndpoint => new AppSettings().GameApiUrl + GetApiPrefix() + "/oauth/token";

        private string GameName
        {
            get { return Session["currentGameName"] as string; }
            set { Session["currentGameName"] = value; }
        }

        private string GameId
        {
            get { return Session["currentGameId"] as string; }
            set { Session["currentGameId"] = value; }
        }

        private string GameProviderId
        {
            get { return (Session["currentGameProviderId"] as string).ToUpperInvariant(); }
            set { Session["currentGameProviderId"] = value; }
        }

        private readonly Dictionary<string, GameProviderData> GameProviderApiPathMap =
            new Dictionary<string, GameProviderData>();

        private string OAuth2ClientId => GameProviderApiPathMap[GameProviderId].ClientId;
        private string OAuth2ClientSecret => GameProviderApiPathMap[GameProviderId].ClientSecret;

        private readonly List<GameProviderBetLimit> _betLimits = new List<GameProviderBetLimit>
        {
            new GameProviderBetLimit {BetLimitId = "10", CurrencyCode = "CAD", MinValue = 10, MaxValue = 1000},
            new GameProviderBetLimit {BetLimitId = "20", CurrencyCode = "CAD", MinValue = 20, MaxValue = 2000},
            new GameProviderBetLimit {BetLimitId = "30", CurrencyCode = "CAD", MinValue = 30, MaxValue = 3000},
            new GameProviderBetLimit {BetLimitId = "10", CurrencyCode = "RMB", MinValue = 10, MaxValue = 1000},
            new GameProviderBetLimit {BetLimitId = "20", CurrencyCode = "RMB", MinValue = 20, MaxValue = 2000},
            new GameProviderBetLimit {BetLimitId = "30", CurrencyCode = "RMB", MinValue = 30, MaxValue = 3000}
        };



        public ActionResult Index(string token, bool? isOAuth = null, string gameName = null, string gameId = null, string gameProviderId = null)
        {
            if (isOAuth.HasValue) IsOAuth = isOAuth.Value;
            if (gameName != null) GameName = gameName;
            if (gameId != null) GameId = gameId;
            if (gameProviderId != null) GameProviderId = gameProviderId;

            if (token == null)
                return View(new GameViewModel {Message = "Welcome to the mock Game Server!"});

            var response = ValidateToken(token);

            return
                View(new GameViewModel
                {
                    GameName = GameName ?? "",
                    Token = token,
                    Rounds = GetRounds(token),
                    Enabled = true,
                    Balance = response.Balance,
                    PlayerName = response.PlayerDisplayName,
                    BrandCode = response.BrandCode,
                    CurrencyCode = response.CurrencyCode,
                    Language = response.Language,
                    BetLimitCode = response.BetLimitCode
                });
        }

        private ValidateTokenResponse ValidateToken(string token)
        {
            if (String.IsNullOrWhiteSpace(token))
                throw new WebServiceException("Access denied (missing token)");

            EnsureAccessToken();

            var response = GameApiUtil.CallGameApiPost<ValidateToken, ValidateTokenResponse>(
                            FormatEndpointPath("token/validate"),
                            new ValidateToken()
                            {
                                AuthToken = token,
                                PlayerIpAddress = Request.UserHostAddress
                            }, OAuthHeaderDecoration);

            if ( response.ErrorCode != 0)
                throw new WebServiceException(
                    $"Access denied (invalid token), Error Code: {response.ErrorCode}, description: {response.ErrorDescription}");

            return response;
        }

        
        #region decorate request (for OAuth)
        private void EnsureAccessToken()
        {
            if (IsOAuth)
                PrepareAccessToken();
        }

        private void PrepareAccessToken()
        {
            if (AuthState != null && AuthState.AccessTokenExpirationUtc > DateTimeOffset.UtcNow.AddSeconds(1))
                return;

            var serverDescription = new AuthorizationServerDescription
            {
                TokenEndpoint = new Uri(TokenEndpoint)
            };

            var client = new WebServerClient(serverDescription, OAuth2ClientId, OAuth2ClientSecret);

            if (AuthState?.RefreshToken == null)
            {
                var scopes = "bets players transactions".Split(' ');
                AuthState = client.GetClientAccessToken(scopes);
            }
            else
            {
                client.RefreshAuthorization(AuthState);
            }
        }
        #endregion


        public ActionResult PlaceInitialBet(GameViewModel model)
        {
            EnsureAccessToken();

            var txList = new List<BetCommandTransactionRequest>
            {
                new BetCommandTransactionRequest()
                {
                    Amount = model.Amount,
                    CurrencyCode = "CAD",
                    RoundId = Guid.NewGuid().ToString(),
                    Description = model.Description,
                    GameId = GameId,
                    Id = Guid.NewGuid().ToString(),
                    TimeStamp = DateTimeOffset.UtcNow
                }
            };

            var response = CallPlaceBet(model.Token, model.BetLimitCode, txList);

            if (response.ErrorCode != GameApiErrorCode.NoError)
                throw new GameApiException(response.ErrorDescription);

            return RedirectToAction("Index", new {token = model.Token});
        }

        private Action<WebHeaderCollection> OAuthHeaderDecoration
        {
            get
            {
                return IsOAuth 
                        ? headers => ClientBase.AuthorizeRequest(headers, AuthState.AccessToken)
                        : (Action<WebHeaderCollection>)null;
            }
        }

        #region Call GameApi methods

        private PlaceBetResponse CallPlaceBet(string token, string betLimitId, List<BetCommandTransactionRequest> txList)
        {
            ValidateBetLimits(betLimitId, txList);

            return GameApiUtil.CallGameApiPost<PlaceBet, PlaceBetResponse>(
                            FormatEndpointPath("bets/place"),
                            new PlaceBet
                            {
                                AuthToken = token,
                                PlayerIpAddress = Request.UserHostAddress,
                                Transactions = txList
                            }, OAuthHeaderDecoration);

        }

        private void ValidateBetLimits(string betLimitId, IEnumerable<BetCommandTransactionRequest> txList)
        {
            var betLimitsValidator = new GameProviderBetLimitValidator(_betLimits);

            foreach (var placeBetTransaction in txList)
            {
                betLimitsValidator.Validate(betLimitId, placeBetTransaction.CurrencyCode, placeBetTransaction.Amount);    
            }
        }


        private WinBetResponse CallWinBet(string token, List<BetCommandTransactionRequest> txList)
        {
            return GameApiUtil.CallGameApiPost<WinBet, WinBetResponse>(
                            FormatEndpointPath("bets/win"),
                            new WinBet
                            {
                                AuthToken = token,
                                PlayerIpAddress = Request.UserHostAddress,
                                Transactions = txList
                            }, OAuthHeaderDecoration);
        }


        private LoseBetResponse CallLoseBet(string token, List<BetCommandTransactionRequest> txList)
        {
            return GameApiUtil.CallGameApiPost<LoseBet, LoseBetResponse>(
                            FormatEndpointPath("bets/lose"),
                            new LoseBet
                            {
                                AuthToken = token,
                                PlayerIpAddress = Request.UserHostAddress,
                                Transactions = txList
                            }, OAuthHeaderDecoration);
        }


        private FreeBetResponse CallFreeBet(string token, string betLimitId, List<BetCommandTransactionRequest> txList)
        {
            return GameApiUtil.CallGameApiPost<FreeBet, FreeBetResponse>(
                            FormatEndpointPath("bets/freebet"),
                            new FreeBet
                            {
                                AuthToken = token,
                                PlayerIpAddress = Request.UserHostAddress,
                                Transactions = txList
                            }, OAuthHeaderDecoration);

        }

        private AdjustTransactionResponse CallAdjustTransaction(string token, List<BetCommandTransactionRequest> txList)
        {
            return GameApiUtil.CallGameApiPost<AdjustTransaction, AdjustTransactionResponse>(
                            FormatEndpointPath("transactions/adjust"),
                            new AdjustTransaction
                            {
                                AuthToken = token,
                                PlayerIpAddress = Request.UserHostAddress,
                                Transactions = txList
                            }, OAuthHeaderDecoration);
        }

        private CancelTransactionResponse CallCancelTransaction(string token, List<BetCommandTransactionRequest> txList)
        {
            return GameApiUtil.CallGameApiPost<CancelTransaction, CancelTransactionResponse>(
                            FormatEndpointPath("transactions/cancel"),
                            new CancelTransaction
                            {
                                AuthToken = token,
                                PlayerIpAddress = Request.UserHostAddress,
                                Transactions = txList
                            }, OAuthHeaderDecoration);
        }
        #endregion

        public ActionResult PlaceTonsOfBets(GameViewModel model)
        {
            return RedirectToAction("Index", new { token = model.Token });
            
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "Place")]
        public ActionResult PlaceBet(GameViewModel model)
        {

            try
            {
                EnsureAccessToken();

                var txList = new List<BetCommandTransactionRequest>
                {
                    new BetCommandTransactionRequest
                    {
                        Amount = model.OperationAmount,
                        CurrencyCode = "CAD",
                        RoundId = model.RoundId.ToString(),
                        Description = model.Description,
                        GameId = GameId,
                        Id = Guid.NewGuid().ToString(),
                        TimeStamp = DateTimeOffset.UtcNow
                    }
                };

                var response = CallPlaceBet(model.Token, model.BetLimitCode, txList);

                if (response.ErrorCode != GameApiErrorCode.NoError)
                {
                    throw new GameApiException(response.ErrorDescription);
                }

                return RedirectToAction("Index", new { token = model.Token });
            }
            catch (GameProviderBetLimitException ex)
            {
                return View("Index", new GameViewModel { Message = ex.Message });
            }
            catch (GameApiException ex)
            {
                return View("Index", new GameViewModel { Message = ex.Message });
            }
            catch (WebServiceException ex)
            {
                return View("Index", new GameViewModel { Message = ex.ResponseBody });
            }
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "Free")]
        public ActionResult FreeBet(GameViewModel model)
        {

            try
            {
                EnsureAccessToken();

                var txList = new List<BetCommandTransactionRequest>
                {
                    new BetCommandTransactionRequest
                    {
                        Amount = model.OperationAmount,
                        CurrencyCode = "CAD",
                        RoundId = model.RoundId.ToString(),
                        Description = model.Description,
                        GameId = GameId,
                        ReferenceId = model.TransactionId,
                        Id = Guid.NewGuid().ToString(),
                        TimeStamp = DateTimeOffset.UtcNow
                    }
                };

                var response = CallFreeBet(model.Token, model.BetLimitCode, txList);

                if (response.ErrorCode != GameApiErrorCode.NoError)
                {
                    throw new GameApiException(response.ErrorDescription);
                }

                return RedirectToAction("Index", new { token = model.Token });
            }
            catch (GameProviderBetLimitException ex)
            {
                return View("Index", new GameViewModel { Message = ex.Message });
            }
            catch (GameApiException ex)
            {
                return View("Index", new GameViewModel { Message = ex.Message });
            }
            catch (WebServiceException ex)
            {
                return View("Index", new GameViewModel { Message = ex.ResponseBody });
            }
        }
    

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "Win")]
        public ActionResult WinBet(GameViewModel model)
        {
            try
            {
                EnsureAccessToken();

                var txList = new List<BetCommandTransactionRequest>
                {
                    new BetCommandTransactionRequest
                    {
                        Amount = model.OperationAmount,
                        ReferenceId = model.TransactionId,
                        CurrencyCode = "CAD",
                        RoundId = model.RoundId.ToString(),
                        Description = model.Description,
                        Id = Guid.NewGuid().ToString(),
                        TimeStamp = DateTimeOffset.UtcNow
                    }
                };

                var response = CallWinBet(model.Token, txList);

                if (response.ErrorCode != GameApiErrorCode.NoError)
                {
                    throw new GameApiException(response.ErrorDescription);
                }                

                return RedirectToAction("Index", new { token = model.Token });
            }
            catch (GameApiException ex)
            {
                return View("Index", new GameViewModel { Message = ex.Message });
            }

        }
        
        [HttpPost]
        [MultipleButton(Name = "action", Argument = "Close")]
        public ActionResult CloseBet(GameViewModel model)
        {
            try
            {
                EnsureAccessToken();

                var txList = new List<BetCommandTransactionRequest>
                {
                    new BetCommandTransactionRequest
                    {
                        Amount = 0,
                        CurrencyCode = "CAD",
                        ReferenceId = model.TransactionId,
                        RoundId = model.RoundId.ToString(),
                        Description = model.Description,
                        Id = Guid.NewGuid().ToString(),
                        TimeStamp = DateTimeOffset.UtcNow,
                    }
                };

                var response = CallLoseBet(model.Token, txList);

                if (response.ErrorCode != GameApiErrorCode.NoError)
                {
                    throw new GameApiException(response.ErrorDescription);
                }                


                return RedirectToAction("Index", new { token = model.Token });
            }
            catch (GameApiException ex)
            {
                return View("Index", new GameViewModel { Message = ex.Message });
            }

        }


        [HttpPost]
        [MultipleButton(Name = "action", Argument = "Cancel")]
        public ActionResult CancelBet(GameViewModel model)
        {
            try
            {
                EnsureAccessToken();

                var txList = new List<BetCommandTransactionRequest>
                {
                    new BetCommandTransactionRequest
                    {
                        RoundId = model.RoundId.ToString(),
                        Description = model.Description,
                        Id = Guid.NewGuid().ToString(),
                        TimeStamp = DateTimeOffset.UtcNow,
                        ReferenceId = model.TransactionId
                    }
                };

                var response = CallCancelTransaction(model.Token, txList);

                if (response.ErrorCode != GameApiErrorCode.NoError)
                {
                    throw new GameApiException(response.ErrorDescription);
                }                

                return RedirectToAction("Index", new { token = model.Token });
            }
            catch (GameApiException ex)
            {
                return View("Index", new GameViewModel { Message = ex.Message });
            }

        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "Adjust")]
        public ActionResult AdjustBet(GameViewModel model)
        {
            try
            {
                EnsureAccessToken();

                var txList = new List<BetCommandTransactionRequest>
                {
                    new BetCommandTransactionRequest
                    {
                        Amount = model.OperationAmount,
                        CurrencyCode = "CAD",
                        RoundId = model.RoundId.ToString(),
                        Description = model.Description,
                        Id = Guid.NewGuid().ToString(),
                        TimeStamp = DateTimeOffset.UtcNow,
                        ReferenceId = model.TransactionId
                    }
                };

                var response = CallAdjustTransaction(model.Token, txList);

                if (response.ErrorCode != GameApiErrorCode.NoError)
                {
                    throw new GameApiException(response.ErrorDescription);
                }                

                return RedirectToAction("Index", new { token = model.Token });
            }
            catch (GameApiException ex)
            {
                return View("Index", new GameViewModel { Message = ex.Message });
            }

        }
        

        List<RoundHistoryData> GetRounds(string token)
        {
            EnsureAccessToken();

            var roundsHistoryResponse =
                GameApiUtil.CallGameApiGet<BetsHistoryResponse>(
                    FormatEndpointPath("bets/history?authtoken=" + token + "&ipaddress = " + Request.UserHostAddress + " &gameid=" + GameId), 
                    OAuthHeaderDecoration);

            if ( roundsHistoryResponse.ErrorCode != GameApiErrorCode.NoError) 
                throw new GameApiException(roundsHistoryResponse.ErrorDescription);

            return roundsHistoryResponse.Rounds.OrderByDescending(a => a.CreatedOn).ToList();
        }

        private string FormatEndpointPath(string path)
        {
            return $"{GetApiPrefix()}/{path}";
        }

        private string GetApiPrefix()
        {
            return GameProviderApiPathMap[GameProviderId].ApiPathPrefix;
        }
    }

    
}