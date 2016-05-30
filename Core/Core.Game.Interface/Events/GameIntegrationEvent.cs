using System;
using System.Threading;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Common.Utils;
using AFT.UGS.Core.BaseModels.Bus;
using Newtonsoft.Json;


namespace AFT.RegoV2.Core.Game.Interface.Events
{

    public class UgsGameEvent : GameEvent, IDomainEvent
    {
        public UgsGameEvent()
        {
            EventId = Identifier.NewSequentialGuid();
            EventCreated = Identifier.NewDateTimeOffset().LocalDateTime;
            EventCreatedBy = Thread.CurrentPrincipal.Identity.Name;
        }

        public UgsGameEvent(GameEvent @event) : base()
        {
            amount = @event.amount;
            brandcode = @event.brandcode;
            cur = @event.cur;
            description = @event.description;
            externalbetid = @event.externalbetid;
            externalgameid = @event.externalgameid;
            externalroundid = @event.externalroundid;
            externaltimestamp = @event.externaltimestamp;
            externaltxid = @event.externaltxid;
            externaltxrefid = @event.externaltxrefid;
            gamecode = @event.gamecode;
            gameplatform = @event.gameplatform;
            gameprovidercode = @event.gameprovidercode;
            gametype = @event.gametype;
            ggr = @event.ggr;
            isroundclosing = @event.isroundclosing;
            istestplayer = @event.istestplayer;
            playeripaddress = @event.playeripaddress;
            turnover = @event.turnover;
            unsettledbets = @event.unsettledbets;
            timestamp = @event.timestamp;
            type = @event.type;
            userid = @event.userid;
            username = @event.username;
            wallettxid = @event.wallettxid;
            wallettxrefid = @event.wallettxrefid;
        }


        [JsonProperty]
        public Guid EventId { get; }
        [JsonProperty]
        public Guid AggregateId { get; set; }
        [JsonProperty]
        public DateTimeOffset EventCreated { get; set; }
        [JsonProperty]
        public string EventCreatedBy { get; set; }
    }
}
