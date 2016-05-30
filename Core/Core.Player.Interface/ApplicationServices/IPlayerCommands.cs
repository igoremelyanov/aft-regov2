using System;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Player;
using AFT.RegoV2.Core.Player.Interface.Data;
using FluentValidation.Results;

namespace AFT.RegoV2.Core.Player.Interface.ApplicationServices
{
    public interface IPlayerCommands
    {
        ValidationResult ValidateThatNewPasswordCanBeSent(SendNewPasswordData data);

        ValidationResult ValidateThatPlayerCanBeRegistered(RegistrationData data);

        ValidationResult ValidateThatPlayerInfoCanBeEdited(EditPlayerData data);

        void SendNewPassword(SendNewPasswordData request);
        void SendResetPasswordUrl(ResetPasswordData request);
        ValidationResult Login(string username, string password, LoginRequestContext context);
        void Edit(EditPlayerData request);
        void ChangePlayerVipLevel(VipLevelId oldVipLevelId, VipLevelId newVipLevelId);
        void AssignVip(Common.Data.Player.Player player, VipLevel vipLevel);

        bool ActivateViaEmail(string emailActivationToken);

        bool ActivateViaSms(string smsActivationToken);
        void ReferFriends(ReferralData referralData);
        
        void SetStatus(PlayerId playerId, bool active);
        void ResendActivationEmail(Guid id);
        Guid Register(RegistrationData request);

        ValidationResult ValidateRegisterInfo(RegistrationData request);

        void ChangeVipLevel(PlayerId playerId, VipLevelId vipLevelId, string remarks);
        ValidationResult ValidatePlayerPaymentLevelCanBeChanged(ChangePaymentLevelData command);
        void ChangePaymentLevel(ChangePaymentLevelData command);
        void ChangePassword(ChangePasswordData request);
        void ChangeSecurityQuestion(Guid playerId, Guid questionId, string answer);
        void SendMobileVerificationCode(Guid playerId);

        void VerifyMobileNumber(Guid playerId, string verificationCode);
        void AddPlayerInfoLogRecord(Guid playerId);
        void ResetPlayerActivityStatus(string username);

        void UpdateLogRemark(Guid id, string remarks);
        void UpdatePlayersPaymentLevel(Guid currentPaymentLevelId, Guid newPaymentLevelId);

        string GetFullName(Common.Data.Player.Player player);

        IdentityVerification UploadIdentificationDocuments(IdUploadData uploadData, Guid playerId, string userName);
        
        void VerifyIdDocument(Guid id, string userName);
        void UnverifyIdDocument(Guid id, string userName);
        void FreezeAccount(Guid playerId);

        void UnfreezeAccount(Guid playerId);
        void SelfExclude(Guid playerId, PlayerEnums.SelfExclusion exclusionType);
        void TimeOut(Guid playerId, PlayerEnums.TimeOut timeOut);
        void Unlock(Guid playerId);
        object GetSelfExclusionData(Guid playerId);
        void CancelExclusion(Guid playerId);

        Guid CreateSecurityQuestion(SecurityQuestion securityQuestion);
        void SetNewPassword(Guid playerId, string newPassword);
    }
}
