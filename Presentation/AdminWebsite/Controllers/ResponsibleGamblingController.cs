using System;
using System.Web.Mvc;
using AFT.RegoV2.AdminWebsite.Common;
using AFT.RegoV2.Core.Common;
using AFT.RegoV2.Core.Common.Data.Player;
using AFT.RegoV2.Core.Player.ApplicationServices;
using AFT.RegoV2.Core.Player.Interface.Data;
using AFT.RegoV2.Core.Security.Common;

namespace AFT.RegoV2.AdminWebsite.Controllers
{
    [Authorize]
    public class ResponsibleGamblingController : BaseController
    {
        private readonly PlayerQueries _playerQueries;
        private readonly PlayerCommands _playerCommands;

        public ResponsibleGamblingController(PlayerQueries playerQueries, PlayerCommands playerCommands)
        {
            _playerQueries = playerQueries;
            _playerCommands = playerCommands;
        }

        public string GetData(Guid playerId)
        {
            return SerializeJson(_playerCommands.GetSelfExclusionData(playerId));
        }

        public DateTimeOffset GetSelfExclusionEndDate(SelfExclusion exclusionType, DateTimeOffset startDate)
        {
            return ExclusionDateHelper.GetSelfExcusionEndDate(exclusionType, startDate);
        }

        public DateTimeOffset GetTimeOutEndDate(TimeOut timeOutType, DateTimeOffset startDate)
        {
            return ExclusionDateHelper.GetTimeOutEndDate(timeOutType, startDate);
        }

        [HttpPost]
        public ActionResult UpdateSelfExclusion(SelfExclusionData data)
        {
            if (data.IsSelfExclusionEnabled)
                _playerCommands.SelfExclude(data.PlayerId, (PlayerEnums.SelfExclusion) data.SelfExclusion);
            else if (data.IsTimeOutEnabled)
                _playerCommands.TimeOut(data.PlayerId, (PlayerEnums.TimeOut) data.TimeOut);
            else
                _playerCommands.CancelExclusion(data.PlayerId);

            return this.Success(new
            {
                Result = "success"
            });
        }

        public class SelfExclusionData
        {
            public Guid PlayerId { get; set; }

            public bool IsTimeOutEnabled { get; set; }
            public TimeOut TimeOut { get; set; }
            public DateTimeOffset? TimeOutStartDate { get; set; }

            public bool IsSelfExclusionEnabled { get; set; }
            public SelfExclusion SelfExclusion { get; set; }
            public DateTimeOffset? SelfExclusionStartDate { get; set; }
        }
    }
}