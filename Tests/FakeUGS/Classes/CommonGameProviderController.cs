using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

using AFT.RegoV2.Infrastructure.Attributes;
using AFT.RegoV2.Shared;

using FakeUGS.Attributes;
using FakeUGS.Core.Exceptions;
using FakeUGS.Core.ServiceContracts;
using FakeUGS.Core.Services;
using FakeUGS.Core.Interfaces;

using GetBalanceResponse = FakeUGS.Core.ServiceContracts.GetBalanceResponse;
using ValidateTokenResponse = FakeUGS.Core.ServiceContracts.ValidateTokenResponse;

namespace FakeUGS.Classes
{
    [ForceJsonFormatter]
    [WebApiRequireOAuth2Scope("bets")]
    public class CommonGameProviderController  : ApiController
    {
        protected ICommonGameActionsProvider GameActions;
        protected IFlycowApiClientSettingsProvider FlycowApiClientSettingsProvider;

        public CommonGameProviderController(
            ICommonGameActionsProvider gameActions,
            IFlycowApiClientSettingsProvider flycowApiClientSettingsProvider)
        {
            var attrs = GetType().GetCustomAttributes(true);
            var attr = attrs.SingleOrDefault(x => x is ForGameProviderAttribute) as ForGameProviderAttribute;

            //if (attr == null)
            //    throw new RegoException("Missing ForGameProvider attribute.");

            FlycowApiClientSettingsProvider = flycowApiClientSettingsProvider;
            GameActions = gameActions;

            GameActions.SetGameActionContext(new GameActionContextDetails
            {
                GameProviderCode = attr == null ? string.Empty : attr.GameProviderCode,
                OptionalTxRefId = true,
                PlayerToken = GetPlayerToken(ActionContext)
            });
        }

        private string GetPlayerToken(HttpActionContext context)
        {
            var token = string.Empty;
            foreach (var arg in context.ActionArguments.Values)
            {
                var req = arg as IAuthTokenHolder;
                if (req == null) continue;

                token = req.AuthToken;
            }
            return token;
        }

        private async Task ValidateTokenData(IAuthTokenHolder authTokenHolder)
        {
            if (authTokenHolder == null)
            {
                throw new ValidationException("Authentication token not found");
            }

            await GameActions.ValidateTokenAsync(authTokenHolder);
        }

        [Route("token/validate"), ProcessError]
        public async Task<ValidateTokenResponse> Post(ValidateToken request)
        {
            return await GameActions.ValidateTokenAsync(request);
        }
        [Route("players/balance"), ProcessError]
        public async Task<GetBalanceResponse> Get([FromUri]GetBalance request)
        {
            await ValidateTokenData(request);
            return await GameActions.GetBalanceAsync(request);
        }
        [Route("bets/place"), ProcessError]
        public async Task<PlaceBetResponse> Post(PlaceBet request)
        {
            await ValidateTokenData(request);
            return await GameActions.PlaceBetAsync(request);
        }
        [Route("bets/win"), ProcessError]
        public async Task<WinBetResponse> Post(WinBet request)
        {
            await ValidateTokenData(request);
            return await GameActions.WinBetAsync(request);
        }
        [Route("bets/lose"), ProcessError]
        public async Task<LoseBetResponse> Post(LoseBet request)
        {
            await ValidateTokenData(request);
            return await GameActions.LoseBetAsync(request);
        }
        [Route("bets/freebet"), ProcessError]
        public async Task<FreeBetResponse> Post(FreeBet request)
        {
            await ValidateTokenData(request);
            return await GameActions.FreeBetAsync(request);
        }
        [Route("bets/tie"), ProcessError]
        public async Task<TieBetResponse> Post(TieBet request)
        {
            await ValidateTokenData(request);
            return await GameActions.TieBetAsync(request);
        }
        [Route("transactions/adjust"), ProcessError]
        public async Task<AdjustTransactionResponse> Post(AdjustTransaction request)
        {
            await ValidateTokenData(request);
            return await GameActions.AdjustTransaction(request);
        }
        [Route("transactions/cancel"), ProcessError]
        public async Task<CancelTransactionResponse> Post(CancelTransaction request)
        {
            await ValidateTokenData(request);
            return await GameActions.CancelTransaction(request);
        }
        [Route("batch/bets/settle"), ProcessError]
        public async Task<BatchSettleResponse> Post(BatchSettle request)
        {
            return await GameActions.BatchSettle(request);
        }
        [Route("batch/transactions/adjust"), ProcessError]
        public BatchAdjustResponse Post(BatchAdjust request)
        {
            return GameActions.BatchAdjust(request);
        }
        [Route("batch/transactions/cancel"), ProcessError]
        public CancelTransactionsResponse Post(BatchCancel request)
        {
            return GameActions.BatchCancel(request);
        }
        [Route("bets/history"), ProcessError]
        public async Task<BetsHistoryResponse> Get([FromUri]RoundsHistory request)
        {
            await ValidateTokenData(request);
            return GameActions.GetBetHistory(request);
        }
    }
}
