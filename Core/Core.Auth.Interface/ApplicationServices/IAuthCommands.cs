using AFT.RegoV2.Core.Auth.Interface.Data;

namespace AFT.RegoV2.Core.Auth.Interface.ApplicationServices
{
    public interface IAuthCommands
    {
        void CreateActor(CreateActor model);
        void ChangePassword(ChangePassword model);
        void CreateRole(CreateRole model);
        void UpdateRole(UpdateRole model);
        void CreatePermission(CreatePermission model);
        void AssignRoleToActor(AssignRole model);
    }
}