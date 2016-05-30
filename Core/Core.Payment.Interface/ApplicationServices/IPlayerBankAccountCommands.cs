using System;
using AFT.RegoV2.Core.Common.Data.Payment;
using AFT.RegoV2.Core.Common.Data.Player;
using AFT.RegoV2.Core.Payment.Interface.Data;
using FluentValidation.Results;

namespace AFT.RegoV2.Core.Payment.Interface.ApplicationServices
{
    public interface IPlayerBankAccountCommands
    {
        ValidationResult ValidateThatPlayerBankAccountCanBeEdited(EditPlayerBankAccountData data);
        ValidationResult ValidateThatPlayerBankAccountCanBeAdded(EditPlayerBankAccountData data);
        ValidationResult ValidateThatPlayerBankAccountCanBeSet(SetCurrentPlayerBankAccountData data);
     
        Guid Add(EditPlayerBankAccountData model);

        void Edit(EditPlayerBankAccountData model);

        void SetCurrent(PlayerBankAccountId playerBankAccountId);

        void Verify(PlayerBankAccountId playerBankAccountId, string remarks);

        void Reject(PlayerBankAccountId playerBankAccountId, string remarks);
    }
}
