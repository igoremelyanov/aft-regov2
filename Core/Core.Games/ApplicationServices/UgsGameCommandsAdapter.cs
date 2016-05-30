using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AFT.RegoV2.Core.Game.Interface.Data;
using AFT.RegoV2.Core.Game.Interface.Events;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.UGS.Core.BaseModels.Bus;

namespace AFT.RegoV2.Core.Game.ApplicationServices
{
    public interface IUgsGameCommandsAdapter
    {
        void ConsumeGameEvent(UgsGameEvent @event);
    }

    public class UgsGameCommandsAdapter : IUgsGameCommandsAdapter
    {
        private readonly IGameCommands _gameCommands;
        private readonly Dictionary<BusEventType, Action<UgsGameEvent>> _gameEventsMap;

        public UgsGameCommandsAdapter(IGameCommands gameCommands)
        {
            _gameCommands = gameCommands;

            _gameEventsMap =
                new Dictionary<BusEventType, Action<UgsGameEvent>>
                {
                    {
                        BusEventType.BetPlaced, @event =>
                        {
                            @event.amount = (@event.amount < 0) ? -@event.amount : @event.amount;
                            _gameCommands.PlaceBetAsync(GetGameActionData(@event), GetGameActionContext(@event),
                                Guid.Parse(@event.userid)).GetAwaiter().GetResult();
                        }
                    },
                    {
                        BusEventType.BetWon, @event =>
                        {
                            _gameCommands.WinBetAsync(GetGameActionData(@event), GetGameActionContext(@event))
                                .GetAwaiter()
                                .GetResult();
                        }
                    },
                    {
                        BusEventType.BetLost, @event =>
                        {
                            _gameCommands.LoseBetAsync(GetGameActionData(@event), GetGameActionContext(@event))
                                .GetAwaiter()
                                .GetResult();
                        }
                    },
                    {
                        BusEventType.BetFree, @event =>
                        {
                            _gameCommands.FreeBetAsync(GetGameActionData(@event), GetGameActionContext(@event),
                                Guid.Parse(@event.userid)).GetAwaiter().GetResult();
                        }
                    },
                    {
                        BusEventType.BetTied, @event =>
                        {
                            _gameCommands.TieBetAsync(GetGameActionData(@event), GetGameActionContext(@event))
                                .GetAwaiter()
                                .GetResult();
                        }
                    },
                    {
                        BusEventType.BetAdjusted, @event =>
                        {
                            _gameCommands.AdjustTransaction(GetGameActionData(@event), GetGameActionContext(@event));
                        }
                    },
                    {
                        BusEventType.GameActionCancelled, @event =>
                        {
                            _gameCommands.CancelTransactionAsync(GetGameActionData(@event), GetGameActionContext(@event))
                                .GetAwaiter()
                                .GetResult();
                        }
                    }
    };


        }

        public void ConsumeGameEvent(UgsGameEvent @event)
        {
            _gameEventsMap[@event.type].Invoke(@event);            
        }

        private static GameActionData GetGameActionData(GameEvent @event)
        {
            return new GameActionData
            {
                Amount = @event.amount,
                ExternalBetId = @event.externalbetid,
                ExternalGameId = @event.externalgameid,
                ExternalTransactionId = @event.externaltxid,
                TransactionReferenceId = @event.externaltxrefid,
                CurrencyCode = @event.cur,
                Description = @event.description,
                RoundId = @event.externalroundid,
                WalletTransactionId = @event.wallettxid,
                WalletTransactionReferenceId = @event.wallettxrefid                
            };
        }

        private static GameActionContext GetGameActionContext(GameEvent @event)
        {
            return new GameActionContext
            {
                GameCode = @event.gamecode,
                GameProviderCode = @event.gameprovidercode,
                PlatformType = @event.gameplatform,
                GgrContribution = @event.ggr,
                TurnoverContribution = @event.turnover,
                UnsettledBetsContribution = @event.unsettledbets,
                PlayerIpAddress = @event.playeripaddress
            };
        }

    }
}
