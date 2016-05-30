using AFT.RegoV2.Core.Fraud.Interface.Data;

namespace AFT.RegoV2.Core.Fraud.Interface.ApplicationServices
{
    public interface IRiskProfileCheckCommands
    {
        RiskProfileConfiguration Create(RiskProfileCheckDTO data);
        void Update(RiskProfileCheckDTO data);
    }
}
