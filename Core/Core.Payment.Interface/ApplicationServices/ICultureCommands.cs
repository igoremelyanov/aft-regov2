using AFT.RegoV2.Core.Common.Data.Admin;

namespace AFT.RegoV2.Core.Payment.Interface.ApplicationServices
{
    public interface ICultureCommands
    {
        string Save(EditCultureData model);
        string Edit(EditCultureData model);
    }
}