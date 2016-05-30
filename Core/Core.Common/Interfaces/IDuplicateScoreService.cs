using System;

namespace AFT.RegoV2.Core.Common.Interfaces
{
    public interface IDuplicateScoreService
    {
        int ScorePlayer(Guid playerId);

        int ScorePlayer(Guid basePlayerId, Guid secondaryPlayerId, IExactScoreConfiguration configuration = null);
    }
}