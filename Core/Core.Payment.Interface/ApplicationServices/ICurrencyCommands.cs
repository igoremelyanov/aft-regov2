using AFT.RegoV2.Core.Common.Data.Admin;
using AFT.RegoV2.Core.Payment.Interface.Data.Commands;

namespace AFT.RegoV2.Core.Payment.Interface.ApplicationServices
{
    public interface ICurrencyCommands
    {
        CurrencyCRUDStatus Add(EditCurrencyData model);
        CurrencyCRUDStatus Save(EditCurrencyData model);
    }
}
