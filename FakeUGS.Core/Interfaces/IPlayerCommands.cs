using System;

using AFT.UGS.Core.Messages.Players;

using FakeUGS.Core.Data;

namespace FakeUGS.Core.Interfaces
{
    public interface IPlayerCommands
    {
        Player AddPlayerFromRequest(AuthorizePlayerRequest request, Guid brandId);
    }
}