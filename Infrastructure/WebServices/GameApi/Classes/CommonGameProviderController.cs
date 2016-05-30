using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using AFT.RegoV2.GameApi.ServiceContracts;
using AFT.RegoV2.GameApi.Attributes;
using AFT.RegoV2.GameApi.Services;
using AFT.RegoV2.Infrastructure.Attributes;
using AFT.RegoV2.Shared;

namespace AFT.RegoV2.GameApi.Classes
{
    [ForceJsonFormatter]
    [WebApiRequireOAuth2Scope("bets")]
    public class CommonGameProviderController  : ApiController
    {
        protected ICommonGameActionsProvider GameActions;

        public CommonGameProviderController(ICommonGameActionsProvider gameActions)
        {
            var attrs = GetType().GetCustomAttributes(true);

            var attr = attrs.SingleOrDefault(x => x is ForGameProviderAttribute);

            if (attr == null)
                throw new RegoException("Missing ForGameProvider attribute.");

            GameActions = gameActions;
            GameActions.SetGameActionContext(new GameActionContextDetails
            {
                GameProviderId = ((ForGameProviderAttribute)attr).GameProviderId,
                OptionalTxRefId = true
            });
        }

        [Route("token/validate"), ValidateTokenData, ProcessError]
        public async Task<ValidateTokenResponse> Post(ValidateToken request)
        {
            return await GameActions.ValidateTokenAsync(request);
        }
        [Route("players/balance"), ValidateTokenData, ProcessError]
        public async Task<GetBalanceResponse> Get([FromUri]GetBalance request)
        {
            return await GameActions.GetBalanceAsync(request);
        }
        [Route("bets/place"), ValidateTokenData, ProcessError]
        public async Task<PlaceBetResponse> Post(PlaceBet request)
        {
            return await GameActions.PlaceBetAsync(request);
        }
        [Route("bets/win"), ValidateTokenData, ProcessError]
        public async Task<WinBetResponse> Post(WinBet request)
        {
            return await GameActions.WinBetAsync(request);
        }
        [Route("bets/lose"), ValidateTokenData, ProcessError]
        public async Task<LoseBetResponse> Post(LoseBet request)
        {
            return await GameActions.LoseBetAsync(request);
        }
        [Route("bets/freebet"), ValidateTokenData, ProcessError]
        public async Task<FreeBetResponse> Post(FreeBet request)
        {
            return await GameActions.FreeBetAsync(request);
        }
        [Route("bets/tie"), ValidateTokenData, ProcessError]
        public async Task<TieBetResponse> Post(TieBet request)
        {
            return await GameActions.TieBetAsync(request);
        }
        [Route("transactions/adjust"), ValidateTokenData, ProcessError]
        public AdjustTransactionResponse Post(AdjustTransaction request)
        {
            return GameActions.AdjustTransaction(request);
        }
        [Route("transactions/cancel"), ValidateTokenData, ProcessError]
        public async Task<CancelTransactionResponse> Post(CancelTransaction request)
        {
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
        [Route("bets/history"), ValidateTokenData, ProcessError]
        public BetsHistoryResponse Get([FromUri]RoundsHistory request)
        {
            return GameActions.GetBetHistory(request);
        }
    }
}
