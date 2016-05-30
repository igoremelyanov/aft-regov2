using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AFT.RegoV2.Core.Common.Interfaces;

using FakeUGS.Core.Interfaces;

namespace FakeUGS.Core.Providers
{
    public class ModeSwitch : IModeSwitch
    {
        private readonly ICommonSettingsProvider _commonSettingsProvider;

        public ModeSwitch(ICommonSettingsProvider commonSettingsProvider)
        {
            _commonSettingsProvider = commonSettingsProvider;
        }

        public bool IsUsingRealUgs()
        {
            var operatorApiUrl = _commonSettingsProvider.GetOperatorApiUrl();
            var gameApiUrl = _commonSettingsProvider.GetGameApiUrl();
            bool isUsingFake = operatorApiUrl.Equals(gameApiUrl, StringComparison.InvariantCultureIgnoreCase);
            return !isUsingFake;
        }
    }
}
