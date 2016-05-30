using System;

namespace FakeUGS.Core.Exceptions
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
