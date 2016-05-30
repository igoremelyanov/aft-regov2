
namespace AFT.RegoV2.Core.Payment.Interface.ApplicationServices
{
    public interface IPaymentCommands
    {
        void ActivateCurrency(string code, string remarks);
     
        void DeactivateCurrency(string code, string remarks);
    }
}
