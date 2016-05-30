using AFT.RegoV2.Core.Fraud.Interface.ApplicationServices;
using AFT.RegoV2.Core.Fraud.Interface.Data;

namespace AFT.RegoV2.Tests.Common.Helpers
{
    public class RiskProfileCheckTestHelper
    {
        private readonly IRiskProfileCheckCommands _riskProfileCheckCommands;

        public RiskProfileCheckTestHelper(IRiskProfileCheckCommands riskProfileCheckCommands)
        {
            _riskProfileCheckCommands = riskProfileCheckCommands;
        }

        public RiskProfileConfiguration CreateConfiguration(RiskProfileCheckDTO configuration)
        {
            return _riskProfileCheckCommands.Create(configuration);
        }

        public void UpdateConfiguration(RiskProfileCheckDTO configuration)
        {
            _riskProfileCheckCommands.Update(configuration);
        }
    }
}
