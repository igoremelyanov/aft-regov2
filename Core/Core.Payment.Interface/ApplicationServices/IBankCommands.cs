using System;
using AFT.RegoV2.Core.Payment.Interface.Data.Commands;

namespace AFT.RegoV2.Core.Payment.Interface.ApplicationServices
{
    public interface IBankCommands
    {        
        Guid Add(AddBankData data);
     
        void Edit(EditBankData data);
    }
}
