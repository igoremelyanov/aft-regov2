using System;
using AFT.RegoV2.Core.Payment.Interface.Data;
using AFT.RegoV2.Core.Payment.Interface.Data.Commands;

namespace AFT.RegoV2.Core.Payment.Interface.ApplicationServices
{
    public interface IBankAccountCommands
    {
        Guid Add(AddBankAccountData data);

        Guid AddWithFiles(AddBankAccountData data, byte[] idFrontImage, byte[] idBackImage,
            byte[] atmCardImage);

        void SaveChanges(
            EditBankAccountData bankAccountData,
            byte[] idFrontImage,
            byte[] idBackImage,
            byte[] atmCardImage
            );

        void Edit(EditBankAccountData data);

        void Activate(Guid bankAccountId, string remarks);

        void Deactivate(Guid bankAccountId, string remarks);

        Guid AddBankAccountType(BankAccountType bankAccountType);
    }
}
