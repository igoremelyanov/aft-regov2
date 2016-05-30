using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Web.Http;
using AFT.RegoV2.ApplicationServices.Report;
using AFT.RegoV2.Core.Common.Data.Payment;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.MemberApi.Interface.Reports;

namespace AFT.RegoV2.MemberApi.Controllers
{
    public class ReportController : BaseApiController
    {
	    private readonly ReportQueries _reportQueries;
	    private readonly IGameQueries _gameQueries;

	    public ReportController(ReportQueries reportQueries, IGameQueries gameQueries)
	    {
		    _reportQueries = reportQueries;
		    _gameQueries = gameQueries;
	    }

	    [HttpGet]
	    public TransactionListResponse PlayerTransactionItems(int page, DateTime? startDate = null, DateTime? endDate = null, Guid? gameId = null)
	    {
		    var pageSize = 10;
		    
		    var items = _reportQueries
			    .GetPlayerTransactionRecords(PlayerId)
			    .Where(x =>
				    x.Type == TransactionType.BetWon.ToString() ||
				    x.Type == TransactionType.BetLost.ToString());
		    if (startDate.HasValue)
			    items = items.Where(x => x.CreatedOn >= startDate.Value);
		    if (endDate.HasValue)
			    items = items.Where(x => x.CreatedOn <= endDate.Value);
		    if (gameId.HasValue)
			    items = items.Where(x => x.GameId == gameId.Value);
			var totalCount = items.Count();
			items = items.OrderBy(x => x.CreatedOn)
				.Skip(pageSize * page)
				.Take(pageSize);
			var transactionItemsResponses = items
			    .ToList()
			    .Select(x =>
			    {
				    TransactionType type;
				    Enum.TryParse(x.Type, out type);
				    var lastBet = _reportQueries
					    .GetPlayerTransactionRecords(PlayerId)
					    .Where(y => y.Type == TransactionType.BetPlaced.ToString())
					    .Where(y => y.CreatedOn < x.CreatedOn)
						.Where(y => y.GameId == x.GameId)
					    .OrderByDescending(y => y.CreatedOn)
					    .First();
				    var amount = x.MainBalanceAmount + x.BonusBalanceAmount + x.TemporaryBalanceAmount
				                 + x.LockBonusAmount + x.LockFraudAmount + x.LockWithdrawalAmount;
				    if (type == TransactionType.BetLost)
					    amount = amount + -1;

			        var transactionItemResponse = new TransactionItemResponse
			        {
                        Id = x.TransactionId,
                        TransactionType = type,
                        Amount = amount,
                        AmountFormatted = amount.Format(lastBet.CurrencyCode, false, DecimalDisplay.ShowNonZeroOnly),
                        GameName = _gameQueries.GetGameDto(x.GameId.Value).Name,
                        Date = x.CreatedOn,
                        RoundId = x.RoundId.ToString(),
                        Bet = (lastBet.MainBalanceAmount + lastBet.BonusBalanceAmount + lastBet.TemporaryBalanceAmount
                               + lastBet.LockBonusAmount + lastBet.LockFraudAmount + lastBet.LockWithdrawalAmount) * -1
                    };

			        transactionItemResponse.BetFormatted = transactionItemResponse.Bet.Format(
                        lastBet.CurrencyCode, 
                        false,
			            DecimalDisplay.ShowNonZeroOnly);

					return transactionItemResponse;
			    });
		    return new TransactionListResponse
		    {
			    Items = transactionItemsResponses,
			    TotalItemsCount = totalCount
			};
	    } 

		[HttpGet]
	    public IEnumerable<TransactionItemResponse> PlayerTransactionItems(Guid playerId, Guid gameId)
	    {
		    return _reportQueries
				.GetPlayerTransactionRecords(playerId)
				.Where(x => x.GameId == gameId)
				.Where(x => x.Type == TransactionType.BetWon.ToString() || x.Type == TransactionType.BetLost.ToString())
				.ToList()
				.Select(x =>
				{
					TransactionType type;
					Enum.TryParse(x.Type,out type);
					return new TransactionItemResponse
					{
						Id = x.TransactionId,
						TransactionType = type,
						Amount = x.MainBalanceAmount + x.BonusBalanceAmount + x.TemporaryBalanceAmount
							+ x.LockBonusAmount + x.LockFraudAmount + x.LockWithdrawalAmount,
					};
				});

	    } 
    }
}
