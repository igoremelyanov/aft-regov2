using System;

namespace AFT.RegoV2.Core.Messaging.Interface.Data.MassMessageCommands
{
    public class UpdateRecipientsRequest
    {
        public Guid? Id { get; set; }
        public UpdateRecipientsType UpdateRecipientsType { get; set; }
        public SearchPlayersRequest SearchPlayersRequest { get; set; }
        public Guid? PlayerId { get; set; }
        public string IpAddress { get; set; }
    }

    public enum UpdateRecipientsType
    {
        SearchResultSelectAll,
        SearchResultUnselectAll,
        RecipientsListUnselectAll,
        SelectSingle,
        UnselectSingle
    }
}