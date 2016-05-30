using AFT.RegoV2.Core.Payment.Interface.Data.Commands;

namespace AFT.RegoV2.Core.Payment.Interface.ApplicationServices
{
    public interface ICurrencyExchangeCommands
    {
        string Add(SaveCurrencyExchangeData model);
        string Save(SaveCurrencyExchangeData model);
        string Revert(SaveCurrencyExchangeData model);
    }
}
