using AFT.RegoV2.Core.Fraud.Interface.Data;

namespace AFT.RegoV2.Core.Fraud.Interface.ApplicationServices
{
    public interface IRiskLevelCommands
    {
        void Create(RiskLevel entity);
        void Update(RiskLevel entity);
        void Activate(RiskLevelId id, string remarks);
        void Deactivate(RiskLevelId id, string remarks);
        void Tag(PlayerId playerId, RiskLevelId riskLevel, string description);
        void Untag(PlayerId id, string description);
    }
}
