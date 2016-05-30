using System;

namespace AFT.RegoV2.Core.Game.Interface.Exceptions
{
    public class DuplicateGameActionException : Exception
    {
        public Guid GameActionId { get; private set; }

        public DuplicateGameActionException(Guid betTxId)
        {
            GameActionId = betTxId;
        }
    }
}
