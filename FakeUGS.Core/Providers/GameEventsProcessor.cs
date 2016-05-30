using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.RegoBus.Interfaces;
using AFT.RegoV2.Shared.Utils;
using AFT.UGS.Core.BaseModels.Bus;
using AFT.UGS.Core.BaseModels.Enums;
using AFT.UGS.Core.FlyCowClient;
using AFT.UGS.Endpoints.Games.FlyCow.Models;

using FakeUGS.Core.Data;
using FakeUGS.Core.Interfaces;

namespace FakeUGS.Core.Providers
{
    public interface IGameEventsProcessor
    {
        Task Process(IEnumerable<BusEvent> events, string playerToken, string brandCode, GameActionData actionData, string gameProviderCode);
    }

    public class GameEventsProcessor : IGameEventsProcessor
    {
        private readonly IUgsServiceBus _serviceBus;
        private readonly IFlycowApiClientProvider _flycowApiClientProvider;

        private readonly bool _useRealUgs;

        public GameEventsProcessor(
            IUgsServiceBus serviceBus,
            IFlycowApiClientProvider flycowApiClientProvider,
            IModeSwitch modeSwitch)
        {
            _serviceBus = serviceBus;
            _flycowApiClientProvider = flycowApiClientProvider;

            _useRealUgs = modeSwitch.IsUsingRealUgs();
        }

        public async Task Process(IEnumerable<BusEvent> events, string playerToken, string brandCode, GameActionData actionData, string gameProviderCode)
        {
            if (_useRealUgs)
            {
                var client = _flycowApiClientProvider.GetApiClient();
                var token = await _flycowApiClientProvider.GetApiToken(client);

                var gameEvents = events.OfType<GameEvent>();
                foreach (var ev in gameEvents)
                {
                    switch (ev.type)
                    {
                        case BusEventType.BetPlaced:
                            await client.PlaceBetAsync(GetBetRequest<PlaceBetRequest>(playerToken, ev, null), token);
                            break;
                        case BusEventType.BetFree:
                            await client.FreeBetAsync(GetBetRequest<FreeBetRequest>(playerToken, ev, (int)GameTransactionType.Free), token);
                            break;
                        case BusEventType.BetLost:
                            await client.LoseBetAsync(GetBetRequest<LoseBetRequest>(playerToken, ev, (int)GameTransactionType.Loss), token);
                            break;
                        case BusEventType.BetWon:
                            await client.WinBetAsync(GetBetRequest<WinBetRequest>(playerToken, ev, (int)GameTransactionType.Win), token);
                            break;
                        case BusEventType.BetAdjusted:
                            await client.AdjustTransactionAsync(GetBetRequest<AdjustTransactionRequest>(playerToken, ev, null), token);
                            break;
                        case BusEventType.BetTied:
                            await client.TieBetAsync(GetBetRequest<TieBetRequest>(playerToken, ev, (int)GameTransactionType.Tie), token);
                            break;
                        case BusEventType.GameActionCancelled:
                            await client.CancelTransaction(GetBetRequest<CancelTransactionRequest>(playerToken, ev, null), token);
                            break;
                    }
                }
            }
            else
            {
                events.ForEach(
                    @event =>
                    {
                        var ev = @event as GameEvent;
                        ev.brandcode = brandCode;
                        ev.externalroundid = actionData.RoundId;
                        ev.externalgameid = actionData.ExternalGameId;
                        ev.gameprovidercode = gameProviderCode;

                        _serviceBus.PublishExternalMessage(ev);
                    });
            }
        }

        private T GetBetRequest<T>(string playerToken, GameEvent ev, int? txtype) where T: BetRequest, new()
        {
            var transaction = new TransactionRequest
            {
                txid = ev.externaltxid,
                txrefid = ev.externaltxrefid,
                betrecid = ev.externalbetid,
                amt = ev.amount,
                cur = ev.cur,
                desc = ev.description,
                gameid = ev.externalgameid,
                roundclosed = ev.isroundclosing,
                roundid = ev.externalroundid,
                timestamp = ev.timestamp,
                txtype = txtype,
            };

            return new T
            {
                authtoken = playerToken,
                transactions = new[]
                {
                    transaction
                }
            };
        }

    }
}
